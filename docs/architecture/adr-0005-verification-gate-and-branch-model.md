# ADR-0005: Verification gate, branch model and local-first ordering

## Status
Accepted

Accepted 2026-10-26 (the owner delegated the call to Claude): decision in force and proven by the gate runs in docs/QA-LOG.md.

## Date
2026-10-26 (retrofit: branch model since 2026-10-07; gate since M1; local first decided 2026-10-16)

## Last Verified
2026-10-26

## Decision Makers
Owner

## Summary
Work flows `feature/*` (or `docs/*`) → `revival/phase-0-1-bringup` → `dev` → `master` through pull
requests with merge commits, and nothing merges without green CI and a fresh-clone run of
`Tools/verify-local.sh` on the exact head commit. Gameplay must work locally (L1) before more online
features are built.

## Engine Compatibility

| Field | Value |
|-------|-------|
| **Engine** | Unity 2017.3.0f3 |
| **Domain** | Core |
| **Layer** | Foundation |
| **Knowledge Risk** | LOW |
| **References Consulted** | `Tools/verify-local.sh`, `.github/workflows/checks.yml`, `docs/QA-LOG.md`, branch protection settings |
| **Post-Cutoff APIs Used** | None |
| **Verification Required** | None beyond the gate itself |

## ADR Dependencies

| Field | Value |
|-------|-------|
| **Depends On** | ADR-0001 |
| **Enables** | All implementation stories (their Definition of Done) |
| **Blocks** | None (already in force) |
| **Ordering Note** | Local first: online feature work (join fix, real match, chat, RPC trust) waits for L1; online gate steps stay as a regression canary. **Allowed now (2026-10-26):** online tests that prove existing behaviour, such as the online match-end test that ADR-0004 needs (architecture review C2) |

## Context

### Problem Statement
The project inherits a 2018 code base with no tests, binary assets, and an unstable development PC.
Without a strict path to `master` and an automated end-to-end check, regressions and machine flakiness
would be indistinguishable from progress.

### Constraints
- Binary scenes: reviews can't see asset changes, so behaviour must be tested by running the game.
- The development PC has hardware instability (mono crashes, a bugcheck).
- The owner's desktop is in use: test runs must not take the mouse or outlive themselves.

### Requirements
- Every merge is reproducible from a clean clone.
- `master` changes only on an owner decision after a playtest.

## Decision
- **Branches**: `feature/*` and `docs/*` → `revival/phase-0-1-bringup` (integration) → `dev` → `master`. Merge commits only; `dev` and `master` are protected (PR, green `checks`, admins enforced, no force-push or deletion). Merges into `dev`/`master` need the owner's go-ahead.
- **Gate**: before a merge, clone the repository outside the VS Code workspace (`C:\Development\wulfram-gates\<run>`), check the head SHA against GitHub, run `Tools/verify-local.sh` (compile, scenes, cargo, audit, build, smoke, offline play, gameplay; `--online`, `--windowed`, `--screenshots`, `--editor` as needed), archive the logs to `qa-artifacts/gate-runs/<run>`, and record the run in `docs/QA-LOG.md`.
- **Merge**: after every check run on the head SHA has completed, `gh pr merge --merge --match-head-commit <sha>`; never `--admin`.
- **Desktop**: ask before window-opening runs; stop gates only with `Tools/stop-gate.sh`.
- **Flakiness rule**: a failure counts against the code only if it reproduces.
- **Local first**: L1 local playability before more online feature work. Online *tests* of existing behaviour are not feature work and may be added at any time (revised 2026-10-26, architecture review C2).

### Architecture Diagram
```text
feature/* ─PR─> revival/phase-0-1-bringup ─PR─> dev ─PR─> master
   gate: fresh clone @ head SHA → verify-local.sh → qa-artifacts/gate-runs/<run> → QA-LOG row
   CI:   checks.yml (C# 4 syntax, RPC check, …) on push + pull_request
```

### Key Interfaces
- `Tools/verify-local.sh [--online] [--windowed] [--screenshots] [--editor] [--stress N] --out <dir>` → `SUMMARY.txt`, "VERIFIED - all steps passed".
- `Tools/stop-gate.sh`, `Tools/check-rpc.sh`.

### Implementation Guidelines
- A story is done only when its change merged with a VERIFIED gate on its head commit and a QA-LOG row.
- New gameplay behaviour must add or update a gameplay-test phase (gate 6e) in the same PR.
- Code must never push to `dev`/`master` directly or bypass protection.

## Alternatives Considered

### Alternative 1: Trunk-based development on `master`
- **Description**: Short-lived branches straight to `master`.
- **Pros**: Less ceremony.
- **Cons**: `master` would carry unplaytested work; no integration buffer.
- **Rejection Reason**: The owner wants `master` to change only after a playtest decision.

### Alternative 2: CI-only verification
- **Description**: Rely on GitHub Actions.
- **Pros**: Automatic.
- **Cons**: No Unity license or GPU in CI; can't build or play the game.
- **Rejection Reason**: Only the local gate can run Unity; CI covers static checks.

## Consequences

### Positive
- Every merged commit has archived end-to-end evidence; regressions are caught before integration.
- Flaky hardware is separated from real failures.

### Negative
- A full gate takes 10–30 minutes and needs the owner's machine.
- Window-opening steps interrupt the owner's desktop and need their OK.

## Risks
- **The gate becomes slow as phases grow.** Mitigation: headless by default; `--stress` only for promotions.
- **One machine is a single point of failure.** Mitigation: the gate runs on any Windows machine with the pinned editor.

## GDD Requirements Addressed

| GDD System | Requirement | How This ADR Addresses It |
|------------|-------------|--------------------------|
| game-concept.md | MVP: "stable enough for a closed playtest" | The gate and promotion path are how stability is shown |

## Performance Implications
- **CPU**: None at runtime.
- **Memory**: None at runtime.
- **Load Time**: None.
- **Network**: None.

## Migration Plan
None: in force. `CLAUDE.md` remains the operational reference; this ADR is its decision record.

## Validation Criteria
Every merge in `docs/QA-LOG.md` has a VERIFIED gate on its head commit; branch protection on `dev`/`master` reports admins enforced.

## Related
- ADR-0001
- `CLAUDE.md` (branch list, desktop etiquette), `docs/QA-LOG.md`
