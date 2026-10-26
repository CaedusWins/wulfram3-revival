# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | 2017.3.0f3 |
| **Installed at pin time** | 2017.3.0f3 (a9f86dcd79df), `C:\Program Files\Unity\Editor\Unity.exe` — read from the editor log header of every gate run (`Version is '2017.3.0f3 (a9f86dcd79df) revision 11139181'`) |
| **Release Date** | December 2017 |
| **Project Pinned** | 2026-10-26 (re-pinned from the template's Unity 6.3 LTS; Wulfram3 has been on 2017.3 since its 2018 code) |
| **Last Docs Verified** | 2026-10-26 |
| **LLM Knowledge Cutoff** | well after this version |

## Knowledge Gap Warning — REVERSED for this project

The usual warning is that the model knows *less* than the pinned engine. Here it is the opposite:
the model knows Unity versions **eight years newer** than this project, and newer APIs do **not**
compile here. Before suggesting any Unity API, confirm it existed in **Unity 2017.3** and compiles
as **C# 4.0**.

**The other files in this directory (`breaking-changes.md`, `current-best-practices.md`,
`deprecated-apis.md`, `modules/`, `plugins/`, `PLUGINS.md`) describe Unity 6.3 LTS — the template's
default — and do NOT apply to this project.** They are kept unchanged so suite upgrades stay clean.

## What this project is (and is not) on

| Area | Unity 2017.3 / this project | Not available — do not suggest |
|------|------------------------------|--------------------------------|
| Language | C# 4.0 (Mono `mcs`), .NET 3.5-era scripting runtime | `?.`, `$"..."`, `nameof`, `async`/`await`, tuples, `out var`, expression-bodied members (CI rejects the first three) |
| Scripts | C# only | UnityScript `.js`, Boo (CI rejects) |
| Rendering | Built-in pipeline | URP, HDRP, Shader Graph, SRP Batcher |
| UI | UGUI (`UnityEngine.UI`), legacy `OnGUI` | UI Toolkit / UI Builder |
| Input | `UnityEngine.Input` (`GetAxis`, `GetKey`, `GetMouseButton`) | Input System package |
| Content | `Resources.Load`, AssetBundles | Addressables |
| ECS | — | DOTS / Entities / Burst / Jobs-based gameplay |
| Packages | Unity 2017.3 Package Manager (minimal) | most UPM packages |
| Networking | Photon PUN Classic 1.88 (`PhotonView`, `[PunRPC]`, `PhotonNetwork`); UNet existed but is unused here | PUN2 (needs 2017.4.7+), Netcode for GameObjects |
| Assets | Scenes and prefabs are **binary** (diff with `Assets/Editor/WulframSceneDump.cs`) | text-YAML diffs |
| Batch mode | `-batchmode -quit -nographics -projectPath -logFile <file> -executeMethod -buildWindows64Player` — **exit codes are not trusted**; read the log (`compilationhadfailure: True`) | `-logFile -` to stdout, `-version` (may start the editor) |

## Where the real rules live

`CLAUDE.md` (project facts, decisions, still-broken list), `.claude/docs/technical-preferences.md`
(forbidden patterns), `Tools/verify-local.sh` + `.github/workflows/checks.yml` (what every change must pass).
