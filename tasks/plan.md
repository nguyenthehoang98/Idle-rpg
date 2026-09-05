# Gameplay TODO implementation plan

## Overview
Complete the highest-impact missing gameplay pieces without touching the held character-selection work or replacing the approved SVG UI concept.

## Order
1. Fix runtime/player build blockers so the battle code has a shippable assembly boundary.
2. Add per-run EXP and gold reward accounting from monster death through win/defeat state.
3. Add result-state data and minimal runtime feedback; keep the SVG as the visual source of truth until UI approval.
4. Add Normal/Elite/Boss monster ranks and reuse projectile skills for elite/boss attacks.
5. Add projectile/VFX coverage and status feedback only for configured assets.
6. Add persistence/offline progress after the in-run model is stable.

## Acceptance criteria
- Monster EXP/gold are granted exactly once on death.
- A completed run exposes total EXP, total gold, wave, and outcome.
- Existing battle and editor tests remain green.
- Normal monsters keep basic attacks; Elites have one configured skill; Bosses rotate multiple configured skills.
- Player compilation no longer depends on editor-only code.
- Character/weapon catalog selection remains untouched.

## Risks
- Pooled monsters can emit duplicate death events; reward emission must be guarded at the source.
- Monster skills reuse hero projectile prefabs; target filtering and Hero collider coverage must stay valid.
- The project has two gameplay implementations; changes stay in the active `_TDS.Battle` path.
- Particle VFX and status telegraphs are deferred until matching assets are available.
