# 04 - Plan and Task Breakdown

Dựa trên skill `planning-and-task-breakdown`.

## 0. Current Status

```text
✅ Interview completed (flow/01-interview)
🔄 Spec updated (flow/02-spec) - cần duyệt
🔄 Technical design updated (flow/03-technical-design) - cần duyệt
⏳ Plan - đang cập nhật
❌ Quality gates - chờ spec duyệt
❌ Build - chưa bắt đầu
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

Status: `interview done, chờ duyệt spec + design`

Output:

```text
- Agent skill flow docs ✅
- Interview questions ✅ (answered)
- Spec draft ✅ (updated)
- Technical design draft ✅ (updated)
- Quality gates 📝
```

Exit criteria:

```text
- User answers core interview questions ✅
- Spec MVP approved ⏳
- Technical design approved ⏳
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

### Slice 1 - Core Systems

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

### Slice 2 - Spawn + Monster (3 tiers)

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

### Slice 3 - Heroes (5 types)

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

### Slice 4 - Waves + Roll/Shop

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

### Slice 5 - UI

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

## 4. Task Definition Template

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
🔄 Spec - updated, chờ duyệt (flow/02-spec)
🔄 Technical design - updated, chờ duyệt (flow/03-technical-design)
⏳ Quality gates - cần cập nhật sau spec duyệt
⏳ Build - chưa bắt đầu

Next step:
- Duyệt spec + technical design
- Cập nhật quality gates
- Bắt đầu Slice 1
```
