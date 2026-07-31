# 000 - Design Backlog

Status legend:

```text
todo
in-progress
done
blocked
```

## Design Tasks

### D001 - Integrate agent-skills design flow

Status: `done`

Output:

```text
- AGENTS.md
- flow/00-agent-skills-integration.md
- flow/01-interview.md
- flow/02-spec.md
- flow/03-technical-design.md
- flow/04-plan.md
- flow/05-quality-gates.md
```

### D002 - Complete interview

Status: `done` (kết quả ngày 30/07/2025 -> todo.md)

Acceptance:

```text
- All core gameplay questions answered
- Confidence >= 90%
- 01-interview.md updated
```

### D003 - Finalize MVP 0.1 spec

Status: `todo`

Acceptance:

```text
- 02-spec.md has no unresolved critical open questions
- Scope/non-scope approved
- Success criteria approved
```

### D004 - Finalize technical design

Status: `todo`

Acceptance:

```text
- 03-technical-design.md approved
- Scene/prefab design confirmed
- Recovery boundary confirmed
```

### D005 - Create implementation task list

Status: `todo`

Acceptance:

```text
- Tasks split by vertical slices
- Each task has files, steps, acceptance, test
```

### D006 - Decide what to do with existing skeleton code

Status: `done` - chọn giữ và phát triển tiếp skeleton theo hướng dẫn source-driven (Recovery chỉ là tham chiếu)

Context:

```text
Initial skeleton code already exists under Assets/_TDSurvivor/Code.
After design is finalized, decide whether to keep, revise, or replace pieces.
```

Acceptance:

```text
- Decision documented
- Any changes aligned with spec
```

## Future Build Tasks Placeholder

```text
B001 - Setup Gameplay scene
B002 - Setup placeholder sprites/prefabs
B003 - Verify Base + GameManager
B004 - Verify Monster spawn/move
B005 - Verify Hero attack/projectile
B006 - Verify WaveManager
B007 - Verify UI
```
