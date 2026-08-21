# 04 - Plan and Task Breakdown

Dựa trên skill `planning-and-task-breakdown`.

## 0. Current Status

```text
✅ Interview completed (flow/01-interview)
🔄 Spec updated (flow/02-spec) - cần duyệt
🔄 Technical design updated (flow/03-technical-design) - cần duyệt
✅ Plan - chốt vertical slices
❌ Quality gates - chờ spec duyệt
🔄 Build - đã có core systems trên nhánh ai/tower-defense (xem section 7)
```

## 1. Planning Principle

```text
Build vertical slices, not horizontal layers.
```

Sai:

```text
Task 1: làm toàn bộ hệ thống config
Task 2: làm toàn bộ combat
Task 3: làm toàn bộ UI
```

Đúng:

```text
Slice 1: 1 slime spawn và đi tới base
Slice 2: archer bắn chết slime
Slice 3: base mất máu/game over
```

## 2. Milestones

### M0 - Design Foundation

Status: `interview done, spec + design approved 2026-07-31`

Output:

```text
- Agent skill flow docs ✅
- Interview questions ✅ (answered)
- Spec draft ✅ (approved 2026-07-31)
- Technical design draft ✅ (approved 2026-07-31)
- Quality gates ✅ (checked 2026-07-31)
```

Exit criteria:

```text
- User answers core interview questions ✅
- Spec MVP approved ✅ (2026-07-31)
- Technical design approved ✅ (2026-07-31)
```

### M1 - Playable Prototype MVP

Output:

```text
- Gameplay scene
- 5 Heroes (2 Archer, 2 Magic, 1 Buff/Control)
- 3 Monster tiers (Mob, Elite, Boss)
- RVO movement (AgentSimulator)
- Logic hit detection (IGrid)
- WaveManager + Spawner
- Roll/Shop system
- Coin + Exp system
- Passive + Active skills
- Basic UI
```

Exit criteria:

```text
- Can press Play and reach GameOver/Victory
- No red Console errors
- Meets SC01-SC15 in spec
```

### M2 - Data and Balance

Output:

```text
- Excel -> JSON pipeline hoàn chỉnh
- Balance configs
- More waves
- More items (Roll/Shop)
```

### M3 - Architecture Upgrade

Output:

```text
- Performance optimization
- Proper object pooling refinement
- Target registry
```

## 3. Vertical Slices (cập nhật)

> Trạng thái thực tế 31/07/2025 - chi tiết ở section 7.

### Slice 1 - Core Systems 🟡 (dở - chưa có GameManager/Health/BaseCore)

Tasks:

```text
T001 - Setup Unity 6 URP 2D project
T002 - Import Recovery modules: AgentSimulator, IGrid, Pool
T003 - Create GameManager state machine
T004 - Create Health component
T005 - Create BaseCore
T006 - Manual scene with BaseCore
```

Acceptance:

```text
- Base has HP
- Calling damage can trigger GameOver
```

### Slice 2 - Spawn + Monster (3 tiers) 🟡 (spawn + RVO xong, chưa damage Base)

Tasks:

```text
T007 - Create MonsterController (Mob, Elite, Boss)
T008 - Create EnemySpawner (from SpawnerConfig)
T009 - Setup RVO movement (AgentSimulator)
T010 - Spawn monsters and move to Base
```

Acceptance:

```text
- All 3 monster types spawn
- Monsters move using RVO toward Base
- Monsters damage Base on reach
```

### Slice 3 - Heroes (5 types) ❌

Tasks:

```text
T011 - Create HeroController
T012 - Setup 5 heroes: 2 Archer, 2 Magic, 1 Buff/Control
T013 - Create Projectile (IGrid hit logic)
T014 - Force attack direction mechanic
T015 - Passive + Active skills
```

Acceptance:

```text
- All 5 heroes auto-attack
- Projectiles hit using IGrid logic
- Player can force attack direction
- Skills work
```

### Slice 4 - Waves + Roll/Shop 🟡 (spawner wave xong, chưa Roll/Shop)

Tasks:

```text
T016 - Create WaveManager (from WaveConfig)
T017 - Create Spawner/Config loader (Excel -> JSON)
T018 - Track alive monsters
T019 - Create Roll UI (free 1 item + 1 refresh)
T020 - Create Shop UI (buy with coin, refresh)
T021 - Coin + Exp system
```

Acceptance:

```text
- Wave spawns correct count
- Next wave waits
- Roll appears after wave
- Shop works with coins
- Exp increases level
```

### Slice 5 - UI ❌

Tasks:

```text
T022 - Create Canvas
T023 - Base HP, Wave, Coin, Level display
T024 - GameOver/Victory screen
```

Acceptance:

```text
- All UI elements update
- GameOver when Base HP <= 0
- Victory after all waves
```

## 4. Task Definition Template (giữ nguyên)

```markdown
## Txxx - [Task name]

Purpose:
- ...

Files:
- ...

Steps:
1. ...
2. ...

Acceptance Criteria:
- ...

Test:
- ...

Rollback:
- ...
```

## 5. Build Rules

```text
- Build one slice at a time.
- Do not start next slice until current slice passes acceptance.
- If Unity Console has red errors, stop and fix before adding features.
- Keep tasks small enough to review.
```

## 6. Current Status

```text
✅ Interview - completed (flow/01-interview)
✅ Spec - approved 2026-07-31 (flow/02-spec)
✅ Technical design - approved 2026-07-31 (flow/03-technical-design)
✅ Quality gates - checked 2026-07-31 (flow/05-quality-gates)
✅ ADRs - 6/6 created (ADR-0001→0005, ADR-0007)
🔄 Build - đang phát triển trên nhánh ai/tower-defense

Next step:
- Bắt đầu build Slice 2: BaseCore + Health → GameOver
- Cập nhật quality gates
- Hoàn thiện Slice 2: BaseCore + Health + monster damage Base -> GameOver
- Bắt đầu Slice 3 (Heroes)
```

## 7. Build Progress (cập nhật 31/07/2025)

Trạng thái thực tế code trên nhánh `ai/tower-defense` (đã chạy được flow: Boot -> Load config -> Spawn wave -> Monster RVO di chuyển).

### Đã xây dựng

```text
T001 ✅ Unity 6 URP 2D project setup
T002 ✅ Import core modules: AgentSimulator, IGrid, Pool, AssetManager (GameToolkit)
T008 ✅ EnemySpawner từ SpawnerConfig (SpawnerUpdater + SpawnTimer, spawn theo wave)
T009 ✅ RVO movement: AgentSimulator + MonsterMoveUpdater
T010 🟡 Spawn + di chuyển tới Base - thiếu BaseCore nên monster chưa tấn công Base
T016 🟡 WaveManager - SpawnerUpdater có wave loop + OnWaveSpawned event, chưa có victory/lose flow
T017 ✅ Config loader Excel -> JSON (MonsterConfig, SpawnerConfig)
T021 🟡 Stat framework (StatId, Stats, StatModifier) - chưa có Coin/Exp runtime
```

Code chính:

```text
tdsurvivor/Assets/_TDS/Boot/GameBootScene.cs
     /_TDS/Gameplay/SpawnerUpdater.cs, SpawnTimer.cs, GameplayStartup.cs
     /_TDS/Unit/Monster.cs, MonsterMoveUpdater.cs, MonsterRuntimeData.cs
     /_TDS/GameConfig/MonsterConfig.cs, SpawnerConfig.cs
     /_TDS/Statistics/StatGlobal.cs, Stats.cs
```

### Chưa làm

```text
T003 ✅ GameManager state machine (đã có: Assets/_TDS/Core/GameManager.cs)
T004 ✅ Health component (HealthComponent struct + BaseCore)
T005 ✅ BaseCore + monster damage Base -> GameOver
T007 🟡 3 monster tiers tách riêng (hiện dùng 1 Monster + scale trong SpawnerConfig)
T011 ✅ HeroController (+ Weapon auto-attack đã wire damage)
T012 🟡 5 heroes: đã đủ dữ liệu HeroConfig.xlsx/json; scene setup menu clone đủ 5 hero từ hero mẫu
T013 ❌ Projectile + IGrid hit (đang dùng SkillSystem projectile riêng)
T014 ✅ Force attack direction (SC04): ForceTargetInput tap/click chọn quái gần nhất,
     Weapon ưu tiên forced target; setup qua menu Tools > Setup > Setup Gameplay Scene
T015 ❌ Passive + Active skills hoàn chỉnh
T018 ✅ Track alive monsters (WaveManager.AliveMonsters)
T019-T020 ❌ Roll/Shop UI
T021 ❌ Coin/Exp runtime
T022-T024 ✅ UI cơ bản (GameplayUI: Base HP/Wave/Alive + GameOver/Victory)
```

Scene setup: menu **Tools > Setup > Setup Gameplay Scene** (`Assets/_TDS/Editor/GameplaySceneSetup.cs`)
- Idempotent: thêm GameManager/BaseCore/WaveManager/GameplayUI/ForceTargetInput nếu thiếu.
- Đọc `HeroConfig.json`, clone hero mẫu dưới container `Heros` đủ 5 hero, gán stats (range/cooldown/damage/crit/spread/parallel/explosive) + xếp vòng quanh Base.

### Kiến trúc đang dùng

```text
- Config: Excel -> JSON -> ConfigManager (Addressables local)
- Pool + Addressables: AssetManager, Pool
- Updater: UpdaterOwner + BaseUpdatable (Tick loop)
- Movement: AgentSimulator (RVO) - logic position, Monster visual lerp theo
- Hit detection: IGrid + BaseCollision (chưa dùng trong combat)
```
