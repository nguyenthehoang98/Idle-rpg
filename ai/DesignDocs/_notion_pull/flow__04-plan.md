# flow/04-plan

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/04-plan.md

# 04 - Plan and Task Breakdown

Dựa trên skill `planning-and-task-breakdown`.

## 1. Planning Principle

```plain text
Build vertical slices, not horizontal layers.
```

Sai:

```plain text
Task 1: làm toàn bộ hệ thống config
Task 2: làm toàn bộ combat
Task 3: làm toàn bộ UI
```

Đúng:

```plain text
Slice 1: 1 slime spawn và đi tới base
Slice 2: archer bắn chết slime
Slice 3: base mất máu/game over
```

## 2. Milestones

### M0 - Design Foundation

Status: `in progress`

Output:

```plain text
- Agent skill flow docs
- Interview questions
- Spec draft
- Technical design draft
- Quality gates
```

Exit criteria:

```plain text
- User answers core interview questions
- Spec MVP 0.1 approved
- Technical design approved
```

### M1 - Playable Prototype MVP 0.1

Output:

```plain text
- Gameplay scene
- Base/Hero/Monster/Projectile loop
- WaveManager
- Basic UI
```

Exit criteria:

```plain text
- Can press Play and reach GameOver/Victory
- No red Console errors
- Meets SC01-SC10 in spec
```

### M2 - Data and Balance MVP 0.2

Output:

```plain text
- ScriptableObject configs
- Multiple waves
- 2-3 monster variants
- Simple upgrade choice
```

### M3 - Architecture Upgrade

Output:

```plain text
- Object pool
- Target registry
- Optional updater loop
- Optional Recovery-inspired config pipeline
```

## 3. MVP 0.1 Vertical Slices

### Slice 1 - Base + Game State

Tasks:

```plain text
T001 - Create GameManager state machine
T002 - Create Health component
T003 - Create BaseCore component
T004 - Manual scene with BaseCore
```

Acceptance:

```plain text
- Base has HP
- Calling damage can trigger GameOver
```

### Slice 2 - Spawn + Monster Move

Tasks:

```plain text
T005 - Create MonsterController
T006 - Create EnemySpawner
T007 - Add 4 spawn portals
T008 - Spawn 1 slime and move to Base
```

Acceptance:

```plain text
- Slime appears outside center
- Slime moves toward Base
- Slime damages Base when reaching it
```

### Slice 3 - Hero Attack

Tasks:

```plain text
T009 - Create Projectile
T010 - Create HeroAutoAttack
T011 - Assign Arrow prefab
T012 - Archer shoots nearest slime
```

Acceptance:

```plain text
- Archer fires only in range
- Projectile hits slime
- Slime HP decreases and dies
```

### Slice 4 - Waves

Tasks:

```plain text
T013 - Create WaveManager
T014 - Configure WaveData in Inspector
T015 - Track alive monsters
T016 - Trigger Victory after final wave
```

Acceptance:

```plain text
- Wave spawns correct count
- Next wave waits until current monsters resolved
- Victory triggers after all waves
```

### Slice 5 - UI

Tasks:

```plain text
T017 - Create Canvas
T018 - Add Base HP text/slider
T019 - Add Wave text
T020 - Add GameState text
```

Acceptance:

```plain text
- UI updates HP
- UI updates wave
- UI shows GameOver/Victory
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

```plain text
- Build one slice at a time.
- Do not start next slice until current slice passes acceptance.
- If Unity Console has red errors, stop and fix before adding features.
- Keep tasks small enough to review.
```

## 6. Current Status

```plain text
Done:
- Initial docs
- Initial folder structure
- Initial script skeleton

Need before more code:
- User answers interview questions
- Spec approval
- Decide whether to keep current skeleton or revise after design
```
