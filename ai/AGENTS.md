# AGENTS.md - TDSurvivor Agent Workflow

Project: Unity 2D Tower Defense / Survival Defense

This project uses a lightweight workflow inspired by Addy Osmani's `agent-skills`:
https://github.com/addyosmani/agent-skills

## Operating Rule

Do not jump from idea directly to code.

Required lifecycle:

```text
INTERVIEW -> SPEC -> DESIGN -> PLAN -> TASKS -> BUILD -> TEST -> REVIEW
```

## Active Skills for Design Phase

Use these agent-skills concepts:

```text
using-agent-skills              -> choose correct workflow
interview-me                    -> ask one focused question at a time
idea-refine                     -> explore gameplay alternatives
spec-driven-development         -> write spec before implementation
planning-and-task-breakdown     -> split into small vertical slices
doubt-driven-development        -> challenge assumptions before building
documentation-and-adrs          -> record architecture decisions
source-driven-development       -> use Recovery only as reference source
```

## Project Boundaries

Always:
- Write/update Markdown design docs before implementation.
- Surface assumptions before deciding.
- Keep reusable engine code under `Assets/_GameToolkit`.
- Keep game-specific code under `Assets/_TDS`.
- Keep game content under `Assets/_TDS assets`.
- Use namespace `_GameToolkit.*` (reusable) and `_TDS.*` (game-specific).
- Keep Recovery as reference, not direct source of truth.
- Write automated tests for pure logic (SkillSystem, Spu, Stats, timers).
- Run Edit Mode tests before considering a task complete.

Ask first:
- Copying code, prefab, scene, plugin, or `.meta` from Recovery.
- Adding external Unity packages.
- Changing project settings.
- Replacing folder structure.
- Introducing Addressables, UniTask, ExcelExtension, custom Pool, or custom Updater.

Never:
- Copy Recovery files blindly.
- Commit secrets or local machine paths into runtime code.
- Create large architecture before MVP requirements are approved.
- Implement features without acceptance criteria.

## Commands

Unity commands are not finalized yet. Until the Unity project is confirmed, verification is manual:

```text
Open Unity project
Check Console has no red errors
Enter Play Mode
Run MVP gameplay test checklist
```

### Automated Tests (Edit Mode)

Unity Test Framework (com.unity.test-framework) + NUnit. Test files live next to code:

```text
Assets/_GameToolkit/SkillSystem/Tests/   <- SpuTests.cs, BaseActionTests.cs
```

Run from CLI (batch mode):

```text
# Edit Mode tests (nhanh, không cần graphics)
"C:\Program Files\Unity\Hub\Editor\6000.5.5f1\Editor\Unity.exe" \
  -batchmode -nographics -runTests -projectPath "tdsurvivor" \
  -testPlatform EditMode \
  -testResults "$COMMANDCODE_SCRATCHPAD\test-results.xml"

# Hoặc trong Unity Editor: Window > General > Test Runner > EditMode > Run All
```

Rules:

- Test code nằm trong asmdef riêng (`*.Tests.asmdef`) với `defineConstraints: UNITY_INCLUDE_TESTS`.
- Không test MonoBehaviour/visual trực tiếp trong Edit Mode — chỉ test pure logic (Spu, BaseAction, timers, math).
- Play Mode tests (scene, prefab) để sau, khi gameplay loop hoàn thiện.
- Trước khi coi task xong: chạy Edit Mode tests, không được có test fail.

## Design Docs Location

```text
ai/DesignDocs/
```

Main flow docs:

```text
ai/DesignDocs/flow/00-agent-skills-integration.md
ai/DesignDocs/flow/01-interview.md
ai/DesignDocs/flow/02-spec.md
ai/DesignDocs/flow/03-technical-design.md
ai/DesignDocs/flow/04-plan.md
ai/DesignDocs/flow/05-quality-gates.md
```
