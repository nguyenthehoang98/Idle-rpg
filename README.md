# Idle-rpg

Unity 6 (6000.0.60f1) idle RPG project with a modular battle system.

## Structure

| Path | Purpose |
|------|---------|
| `light_fight/` | Unity project root |
| `Assets/_Games/` | Game-specific code (Battle, Config, Utils) |
| `Assets/_KITSystem/` | Reusable framework (Grid, Movement, Schedule, SkillSystem, EventBus, Resource) |
| `Assets/Plugins/` | Third-party (UniTask, DOTween, Odin) |
| `Assets/Feel/` | MoreMountains Feedbacks & MMTools |
| `docs/` | Plans, architecture docs, knowledge base |

## Current Goal

Separate **Logic** (pure C#) from **View** (MonoBehaviour) for unit testability.
See `docs/plans/01-logic-view-separation.md` and `docs/architecture/`.

## Quick Start

1. Open `light_fight/` in Unity Editor
2. Open `Assets/_Games/Battle/test scene.unity` or `ui scene.unity`
3. Run Tests: Window > General > Test Runner

## Docs

- `docs/plans/` — Implementation plans
- `docs/architecture/` — Architecture decisions & target design
- `docs/knowledge/` — AI agent knowledge base
