# Gameplay TODO implementation plan

## Overview
Complete the highest-impact missing gameplay pieces without touching the held character-selection work or replacing the approved UI asset package.

## Order
1. Fix runtime/player build blockers so the battle code has a shippable assembly boundary.
2. Add per-run EXP and gold reward accounting from monster death through win/defeat state.
3. Add result-state data and minimal runtime feedback; keep the approved UI asset package as the visual source of truth until UI approval.
4. Add Normal/Elite/Boss monster ranks and reuse projectile skills for elite/boss attacks.
5. Add status resistance and explicit stacking rules for Slow, Bleed, Silence and Stun.
6. Add projectile/VFX coverage and status feedback only for configured assets.
7. Add persistence/offline progress after the in-run model is stable.
8. After each feature, add and run the regression test set before starting the next feature.

## Post-wave upgrade plan
After each cleared wave, show exactly one of two upgrade paths:

1. **Gold shop**: show 3 purchasable items and let the player buy with gold.
2. **Upgrade roll**: roll 3 upgrade cards and let the player choose exactly 1.

Upgrade cards may target any supported stat. This requires explicit config for:

- item/card id, display data, cost and stat target;
- value/range and upgrade scaling per card;
- a separate upgrade pool keyed by `skillId` for every skill;
- no shared skill upgrade pool between different skills.

The post-wave flow should pause combat progression, resolve the choice, persist the result in the run state, then resume the next wave.

## Acceptance criteria
- Monster EXP/gold are granted exactly once on death.
- A completed run exposes total EXP, total gold, wave, and outcome.
- Existing battle and editor tests remain green.
- Normal monsters keep basic attacks; Elites have one configured skill; Bosses rotate multiple configured skills.
- Status resistance is applied before a modifier starts; Slow/Silence/Stun refresh instead of stacking; Bleed is capped at three stacks.
- Post-wave shop/upgrade cards use config data, and each skill resolves upgrades only from its own `skillId` pool.
- Player compilation no longer depends on editor-only code.
- Character/weapon catalog selection remains untouched.

## Quality and regression test plan

Testing is a release gate after every feature, not a final cleanup task. A feature is not done until its focused tests and the regression suite pass.

### 1. Logic tests (EditMode)

Cover pure behavior without scenes or Addressables:

- damage, crit, lifesteal, stat modifiers and clamping;
- reward accumulation, gold spending, run-state persistence and progression thresholds;
- status resistance, refresh, stacking and modifier lifecycle;
- target selection, spawn timers, wave transitions and win/lose conditions;
- upgrade card application and skill-specific pool isolation.

### 2. Config tests (EditMode)

Validate every config boundary before runtime:

- JSON/Excel data loads and maps to the expected model;
- IDs are unique and required references exist;
- all spawn levels have valid waves, portals and monster IDs;
- every configured hero/monster skill has a valid prefab and target type;
- every upgrade card targets a supported stat and skill cards resolve only to their own `skillId` pool.

### 3. Common/integration tests

Verify shared application plumbing:

- Boot loads all required configs before Home/Gameplay activation;
- Addressables resolve all runtime prefabs and config assets;
- scene references, EventSystem, runners and required components exist;
- no editor-only assembly is included in a Player build.

### 4. Gameplay PlayMode tests

Run the real game flow and fail on hangs or runtime errors:

- Boot → Home → Gameplay loads successfully;
- Level 1 progresses through every wave without stuck timers;
- shop and upgrade roll both pause/resume combat correctly;
- rewards are granted once, upgrade selection is applied, and the run reaches Victory or Defeat within a timeout;
- no unexpected `NullReferenceException`, missing script, missing Addressable or compiler error is logged.

### Required commands before merging

1. Unity EditMode suite.
2. Unity PlayMode smoke/regression suite.
3. Windows Player build validation.
4. `git diff --check`.

Current baseline: **59 EditMode tests**, **5 TDS PlayMode tests**, Level 1/2 clearability, Level 3 upgrade-gated clear, result-loop smoke, 20-level config validation, hero/monster/skill prefab coverage, Level 20 startup smoke, and Windows Player build passing. Explicit runtime error/stuck assertions and full progression-loop coverage remain backlog items.

## Risks
- Pooled monsters can emit duplicate death events; reward emission must be guarded at the source.
- Monster skills reuse hero projectile prefabs; target filtering and Hero collider coverage must stay valid.
- The project has two gameplay implementations; changes stay in the active `_TDS.Battle` path.
- Particle VFX and status telegraphs are deferred until matching assets are available.
- Post-wave choices are recorded in `BattleRunRewards.SelectedUpgradeIds`; save/offline persistence remains a separate follow-up.
