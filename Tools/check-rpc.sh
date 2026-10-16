#!/usr/bin/env bash
# Photon RPC check (no Unity needed; runs in CI and locally from Git Bash):
#
#   1. rpc-param         every [PunRPC] method's parameters must be types PUN Classic can send over
#                        the network. PUN's offline mode runs RPCs locally WITHOUT serializing their
#                        arguments, so a component or GameObject argument works offline and only fails
#                        online - this is how the cargo RPCs (CargoManager argument) hid until 2026-10-16.
#   2. rpc-call-unknown  every photonView.RPC("Name", ...) must name an existing [PunRPC] method. A
#                        typo or a removed method only shows up at runtime ("RPC method not found").
#
# Findings are compared with Tools/rpc-baseline.txt (known, not yet fixed). A new finding fails, and
# so does a baseline entry that is no longer found (it was fixed - delete the line), so the baseline
# always tells the truth. Comments (// and /* */) are ignored.
#
#   Tools/check-rpc.sh [--root <project dir>] [--list]
#
# --list also prints every [PunRPC] signature and RPC call site it read, to audit the parser.
set -u
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
LIST=0
while [ $# -gt 0 ]; do
  case "$1" in
    --root) ROOT="$(cd "$2" && pwd)"; shift 2 ;;
    --list) LIST=1; shift ;;
    *) echo "unknown argument: $1"; exit 2 ;;
  esac
done
BASELINE="$ROOT/Tools/rpc-baseline.txt"
cd "$ROOT" || exit 2
# gawk where available (Git Bash has it; Ubuntu runners default to mawk) so both parse alike.
AWK="$(command -v gawk || command -v awk)"

t=$(mktemp -d)
trap 'rm -rf "$t"' EXIT

# One pass over every C# file under Assets: strip comments, then report [PunRPC] signatures
# ("sig <file> <method> <params>") and string-literal RPC calls ("call <file> <name>").
find Assets -name '*.cs' -print0 | sort -z | xargs -0 "$AWK" '
  FNR == 1 { inblock = 0; pending = 0; sig = "" }
  {
    line = $0
    sub(/\r$/, "", line)
    out = ""
    while (length(line) > 0) {
      if (inblock) {
        p = index(line, "*/")
        if (p == 0) { line = ""; break }
        line = substr(line, p + 2); inblock = 0
      } else {
        b = index(line, "/*"); c = index(line, "//")
        if (c > 0 && (b == 0 || c < b)) { out = out substr(line, 1, c - 1); line = ""; break }
        if (b > 0) { out = out substr(line, 1, b - 1); line = substr(line, b + 2); inblock = 1; continue }
        out = out line; line = ""
      }
    }
    code = out

    # RPC calls with a literal method name.
    s = code
    while (match(s, /\.RPC[ \t]*\([ \t]*"[A-Za-z_][A-Za-z0-9_]*"/)) {
      m = substr(s, RSTART, RLENGTH)
      sub(/^\.RPC[ \t]*\([ \t]*"/, "", m); sub(/"$/, "", m)
      print "call\t" FILENAME "\t" m
      s = substr(s, RSTART + RLENGTH)
    }

    # [PunRPC] then the method signature (same line or following lines, up to the closing paren).
    # Strings are blanked first: PUN itself has "[PunRPC]" inside string literals.
    gsub(/"([^"\\]|\\.)*"/, "\"\"", code)
    # Also inside an attribute list: [PunRPC, Obsolete].
    if (code ~ /\[([^\]]*[ \t,])?PunRPC([ \t,(][^\]]*)?\]/) {
      pending = 1; sig = ""
      sub(/.*\[([^\]]*[ \t,])?PunRPC([ \t,(][^\]]*)?\]/, "", code)
    }
    if (pending) {
      # Other attributes may sit between [PunRPC] and the method, with parentheses in their
      # arguments ([Obsolete("Use X(PhotonMessageInfo)")]): blank out strings, drop leading attributes.
      c2 = code
      gsub(/"([^"\\]|\\.)*"/, "\"\"", c2)
      if (sig ~ /^[ \t]*$/) {
        while (c2 ~ /^[ \t]*\[/) { if (!sub(/^[ \t]*\[[^\]]*\]/, "", c2)) break }
      }
      sig = sig " " c2
      if (sig ~ /\(/ && sig ~ /\)/) {
        pre = sig; sub(/\(.*/, "", pre)
        n = split(pre, w, /[ \t]+/); name = w[n]
        params = sig; sub(/^[^(]*\(/, "", params); sub(/\).*$/, "", params)
        print "sig\t" FILENAME "\t" name "\t" params
        pending = 0; sig = ""
      }
    }
  }
' > "$t/raw"

if [ "$LIST" -eq 1 ]; then
  awk -F'\t' '$1 == "sig" { print "PunRPC  " $2 "  " $3 "(" $4 ")" }' "$t/raw" | sed -E 's/\( +/(/; s/ +\)/)/; s/  +/  /g'
  awk -F'\t' '$1 == "call" { print "call    " $2 "  " $3 }' "$t/raw"
fi

# PUN Classic can send: the Photon protocol types, PUN's Vector2/Vector3/Quaternion/PhotonPlayer,
# and arrays of them. PhotonMessageInfo is filled in by PUN, not sent.
allowed=' bool byte short int long float double string object Hashtable Vector2 Vector3 Quaternion PhotonPlayer PhotonMessageInfo '
known=$(awk -F'\t' '$1 == "sig" { print $3 }' "$t/raw" | sort -u)

: > "$t/found"
while IFS=$'\t' read -r kind file name params; do
  [ "$kind" = "sig" ] || continue
  IFS=',' read -r -a list <<< "$params"
  for p in "${list[@]}"; do
    p="$(echo "$p" | sed -E 's/=.*$//; s/^[[:space:]]+//; s/[[:space:]]+$//; s/[[:space:]]+/ /g')"
    [ -z "$p" ] && continue
    # "[modifiers] Type name" -> Type; ref/out can't be sent at all.
    words=($p)
    if [ "${#words[@]}" -lt 2 ]; then continue; fi
    type="${words[$((${#words[@]} - 2))]}"
    modifier=""
    [ "${#words[@]}" -ge 3 ] && modifier="${words[0]}"
    base="$(echo "$type" | sed -E 's/\[\]$//; s/^(ExitGames\.Client\.Photon|UnityEngine|System)\.//')"
    case "$base" in
      Boolean) base=bool ;; Byte) base=byte ;; Int16) base=short ;; Int32) base=int ;; Int64) base=long ;;
      Single) base=float ;; Double) base=double ;; String) base=string ;; Object) base=object ;;
    esac
    if [ "$modifier" = "ref" ] || [ "$modifier" = "out" ] || [[ "$allowed" != *" $base "* ]]; then
      echo "rpc-param $file $name $modifier${modifier:+ }$type" >> "$t/found"
    fi
  done
done < "$t/raw"

awk -F'\t' '$1 == "call" { print $2 "\t" $3 }' "$t/raw" | sort -u | while IFS=$'\t' read -r file name; do
  if ! grep -qxF "$name" <<< "$known"; then
    echo "rpc-call-unknown $file $name" >> "$t/found"
  fi
done

sort -u "$t/found" -o "$t/found"
if [ -f "$BASELINE" ]; then
  grep -vE '^(#|$)' "$BASELINE" | tr -d '\r' | sort -u > "$t/baseline"
else
  : > "$t/baseline"
fi
new=$(comm -13 "$t/baseline" "$t/found")
gone=$(comm -23 "$t/baseline" "$t/found")

echo "RPC check: $(grep -c '^sig' "$t/raw") [PunRPC] methods, $(grep -c '^call' "$t/raw") RPC call sites, $(wc -l < "$t/found") finding(s), $(wc -l < "$t/baseline") in the baseline"
status=0
if [ -n "$new" ]; then
  echo "New findings (not in Tools/rpc-baseline.txt) - fix them:"
  echo "$new" | sed 's/^/  /'
  status=1
fi
if [ -n "$gone" ]; then
  echo "Baseline entries no longer found (fixed? delete them from Tools/rpc-baseline.txt):"
  echo "$gone" | sed 's/^/  /'
  status=1
fi
[ "$status" -eq 0 ] && echo "RPC check passed"
exit "$status"
