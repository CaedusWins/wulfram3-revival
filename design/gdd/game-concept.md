# Game Concept: Wulfram 3 (Revival)

*Created: 2026-10-26*
*Status: Approved (owner, 2026-10-26)*

> Retrofitted from existing material (adoption plan step 2a): the 2018 player manual (`README.md`),
> the 2018 code as verified by the L1 checklist (`docs/L1-LOCAL-PLAYABILITY.md`), and the decisions
> in `CLAUDE.md`. Owner direction (2026-10-26): **classic core, modern take**; **old Wulfram
> players first**; **MVP = lean team combat**; pillars **strategy over kills, hovertank feel,
> teamwork wins**.

---

## Elevator Pitch

> It's a team hovertank game where two factions fight over a planet by building and defending bases
> with dropped cargo, and win by destroying the other side's tanks and repair pads, not by having
> the best aim.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Team-based vehicle combat with light real-time strategy (base building from cargo) |
| **Platform** | PC (Windows 64-bit) |
| **Target Audience** | Returning Wulfram players first (see Target Player Profile) |
| **Player Count** | Online multiplayer, two teams (Photon Cloud; the free tier allows 20 concurrent players), plus offline practice |
| **Session Length** | 30–60 min matches (to be confirmed in playtests) |
| **Monetization** | None (fan revival) |
| **Estimated Scope** | Medium for the MVP: the systems exist; the work is completing and hardening them and adding a win condition |
| **Comparable Titles** | Classic Wulfram / Wulfram 2; Tribes; BattleZone (1998) |

---

## Core Fantasy

You are one hovertank in a small army. Alone you can shoot; with your team you can **build**: carry
turrets and repair pads into position, hold ground, and break the enemy's base. The satisfaction is
a plan your team carried out, more than a kill you scored.

---

## Unique Hook

Like a team shooter, **and also** every player is a builder. Cargo dropped on the map turns into
turrets and repair pads wherever someone carries and deploys it, so the front line is literally
built by the players. Respawning depends on your team's repair pads, which makes them the real
objective.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)
1. **Fellowship**: shared purpose with your team (primary)
2. **Challenge**: out-thinking the other team
3. **Sensation**: the weight and glide of a hovertank

### Key Dynamics (Emergent player behaviors)
- Escorting cargo carriers; defending and raiding repair pads
- Building forward bases with turrets; focus-firing a target together
- Fuel management: firing, jumping and pulses all compete for one pool

### Core Mechanics (Systems we build)
Hover movement; AutoCannon (fuel per shot); pulse cannon; fuel and health regeneration; targeting;
cargo pickup/drop/deploy; turrets (flak, gun); repair pads and respawn; self-destruct; two teams.
All of these exist and are proven offline (L1 rows 5–15). **Missing for the MVP: a win condition.**

---

## Player Motivation Profile

### Primary Psychological Needs Served
- **Relatedness**: team play and coordination (primary)
- **Competence**: piloting and aim, and winning through planning
- **Autonomy**: choose a role: fighter, builder, defender

### Player Type Appeal (Bartle Taxonomy)
Primarily **Socializers** and **Achievers**; Killers are served but not centred (anti-pillar below).

### Flow State Design
Short loops (fight, get cargo, deploy) inside a long match goal (break their pads). Death costs a
respawn trip, not progress, so players stay in the match.

---

## Core Loop

### Moment-to-Moment (30 seconds)
Hover, aim and fire (cannon/pulse) while watching fuel; target with Tab/T; dodge turret fire.

### Short-Term (5-15 minutes)
Pick up cargo → carry it to a position → deploy a turret or repair pad → defend or push from it.

### Session-Level (30-120 minutes)
Two teams contest the map; a match ends when one side has lost all its tanks and repair pads
(the manual's win condition — **to be built**).

### Long-Term Progression
None in the MVP (no accounts or stats; the 2018 backend is dead). Mastery and the community are the
progression for now.

### Retention Hooks
Team play with the returning community; scheduled match nights; map knowledge.

---

## Game Pillars

### Pillar 1: Strategy over kills
Matches are won by bases, cargo and positioning. Any feature that rewards solo kill-chasing over the
team's plan is suspect.

### Pillar 2: Hovertank feel
The hover, the strafing, the jump and the two cannons must feel right moment to moment. The classic
feel is the reference; modernize only where it plays better.

### Pillar 3: Teamwork wins
Coordination beats individual skill. Systems should create reasons to stick together (escorting,
shared bases, repair pads).

### Anti-Pillars (What This Game Is NOT)
- Not a deathmatch shooter: kill counts don't win matches.
- Not a persistent MMO economy for the MVP: SupplyShips, SkyPumps, PowerCells and the UpLink are deferred to M4.
- Not a modern-engine remake right now: Unity 2017.3 until a deliberate engine step.

---

## Visual Identity Anchor

**Direction: Readable Battlefield** (owner's choice, 2026-10-26)

**Visual rule:** team and function read at 100 m.

**Supporting principles:**
1. **Classic silhouettes, modern light.** Keep the chunky 2018 vehicle and structure shapes; improve
   lighting and materials, not shapes. *Design test:* a returning player names every unit type
   from its silhouette alone.
2. **Team colour is the loudest thing on every unit.** Red and blue markings dominate each tank,
   turret and pad; nothing else on a unit competes with them. *Design test:* in a grayscale
   screenshot with only team colours kept, every unit's side is clear.
3. **Deployables are recognisable by shape.** Turrets, repair pads and cargo boxes each have one
   unmistakable outline, because strategy depends on reading the base layout. *Design test:* from
   a tank at 100 m, a player can tell a flak turret from a gun turret from a repair pad.

**Colour philosophy:** a muted planet (terrain, sky, neutral props) so that the two team colours,
red and blue, and the critical states (low health, low fuel, incoming fire) are the only saturated
colours on screen.

---

## Inspiration and References
- Wulfram (Bolt Action Software) and Wulfram 2: the source design (manual in `README.md`)
- Tribes, BattleZone: team vehicle combat with building

---

## Target Player Profile
Returning Wulfram players: they know the game, care most about the classic feel, tolerate a rough
early build, and can be reached directly for closed playtests (M3). New players come after the
veterans are happy.

---

## Technical Considerations
Unity 2017.3 / C# 4 (pinned); Photon PUN Classic on Photon Cloud (App ID kept out of git; free tier
20 CCU); offline play for practice and testing; binary scenes and prefabs. Known online gaps: RPC
trust model, splash damage hitting only the master's own tank, the same-instant join race (fix
parked). See `CLAUDE.md`.

---

## Risks and Open Questions

### Design Risks
- The classic feel is remembered, not specified: playtests with veterans are the measure.
- Balance: one flak hit currently kills a full tank (uncapped splash).

### Technical Risks
- Old engine and networking stack; the dev PC is unstable under load (see `docs/QA-LOG.md`).
- No server authority yet (clients trust each other's RPCs).

### Market Risks
- Small audience; reaching returning players depends on community channels.

### Scope Risks
- Pulling in the economy (M4) before the combat MVP is solid.

### Open Questions
- Self-destruct: should it destroy your own team's repair pad?
- Flak: should one hit kill a full tank?
- What should the `BlueCargo` box deploy as?
- Match length and team size for the first playtests.

---

## MVP Definition

Lean team combat, online: two teams of tanks; cannons, pulse, fuel, regen; cargo pickup, drop and
deploy of turrets and repair pads; death and respawn at repair pads; **a win condition**; offline
practice. Stable enough for a closed playtest with returning players.

### Scope Tiers (if budget/time shrinks)
1. Must: everything above except the win condition, as a working online sandbox.
2. Should: the win condition and the team/vehicle selection screen.
3. Could: scouts (the prefabs exist, but nothing spawns them); minimap.
4. Later (M4): the economy layer.

---

## Next Steps
1. Owner review of this draft (pillars, MVP, open questions).
2. `/map-systems`: the systems index from the L1 rows (adoption plan step 2b).
3. `/gate-check production` to set `project.stage` (step 2c).
4. First sprint: the rest of L1 and a design for the win condition.
