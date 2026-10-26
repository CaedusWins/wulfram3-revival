# Review Log: game-concept.md

## 2026-10-26 — /design-review (lean, first review)

Specialists consulted: none (lean review mode: main review only)
Re-review: No — first review
**Verdict: APPROVED** (recommended revisions, nothing blocking)

### Completeness
Concept document: every template section is present (pitch, core identity, fantasy, hook, MDA,
motivation, core loop, pillars and anti-pillars, Visual Identity Anchor, references, player profile,
technical, risks, MVP and tiers, next steps). The system-GDD checklist (Formulas, Edge Cases,
Acceptance Criteria) does not apply to a concept document.

### Dependency Graph
- ✓ systems-index.md — exists (approved 2026-10-26)
- System GDDs — none yet (adoption step 4a)

### Registry Values
No registry entries or related-GDD values: values not cross-checked.

### Required Before Implementation
(none)

### Recommended Revisions
1. **Win condition vs the no-pad respawn fallback.** The concept says a team loses when all its tanks
   and repair pads are gone. Today, with no pad left, `GameManager.Respawn` respawns the tank at the
   arena centre, so a team never runs out of respawns. Resolve this in the win-condition GDD.
2. **Team size for the first playtest.** Not stated. The Photon free tier caps at 20 concurrent
   players, so ≤ 10 v 10. Decide it before M3.
3. **Two balance calls that touch Pillar 1** (strategy over kills): a flak hit that one-shots a full
   tank, and self-destruct damaging your own base. Both are open questions, to be decided after the
   playtest.

### Nice-to-Have
- A "Readable Battlefield" check in the art bible once one exists.

### Senior Verdict
Consistent with its sources (the 2018 manual, the proven L1 systems, the decisions in `CLAUDE.md`)
and with the owner's direction. The pillars are concrete enough to judge features against. Approved
as the base for system design.
