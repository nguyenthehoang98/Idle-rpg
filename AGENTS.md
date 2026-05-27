# AGENTS.md — Idle-rpg / light_fight

## Project

- **Engine**: Unity 6000.0.60f1 (Unity 6)
- **Language**: C#
- **Root**: `light_fight/` — open this folder in Unity Editor.
- `.csproj` / `.sln` are Unity-generated. Never edit manually.

## Directory Layout

| Path | Purpose |
|---|---|
| `Assets/_Games/` | Game-specific code (currently `Battle/`) |
| `Assets/_KITSystem/` | Reusable framework systems |
| `Assets/Plugins/` | Third-party (UniTask, DOTween, Odin) |
| `Assets/Feel/` | MoreMountains Feedbacks & MMTools |

## Battle Entry Point

- Current scene entry point is `BattleOwner` (`Assets/_Games/Battle/Logic/BattleOwner.cs`) — wired in `Assets/_Games/Battle/game scene.unity`.
- Flow: `Start()` → load Excel configs → resolve `ITickable` systems → initialize movement/agent/spawner/battle → initialize dice UI → `StartGame()`.
- Uses `UniTask` for async config/prefab loading, plus existing coroutine-based view sequencing in `BattleTickable`.

## KITSystem Subsystems

Each has its own `.asmdef` under `Assets/_KITSystem/`:

- **SkillSystem** — Config/Runtime/Unitest split. ScriptableObject skill configs (`SkillConfig`, `BaseProjectileConfig`). Runtime: `ISkillAction`, trajectory actions (Bullet, Parabolic, Boomerang, Blend), shape actions (Circle, Square, Capsule, Cone), `SkillFactory`.
- **Movement** — MPU (Movement Processing Unit) is wrapped by `MovementTickable`. Pluggable `IMovementAction` (Run, KnockBack, Lock, Teleport). Uses RVO2 for avoidance.
- **Schedule** — Tick-based update (`TickSystem`, `TickGroup`, `ITickable`, `TickSystemOwner`). MPU and other systems register as `ITickable` here.
- **Grid** — `FixedUniformGrid` / `IGridManager` for spatial queries.
- **Utils** — Shared helpers (`CollectionUtils`, `MathUtils`, `RandomUtils`, `TextFormatter`, `TypeUtils`).
- **TimeScaleToolbar** — Editor toolbar extension.

## Third-Party Dependencies

- **UniTask** — Primary async/await library. Prefer `UniTask` over `IEnumerator` coroutines.
- **DOTween / DOTweenPro** — Tweening/animations.
- **Sirenix Odin Inspector** — Editor-only serialization/inspector.
- **MoreMountains Feedbacks** — Haptic/visual/audio feedback.
- **RVO2** — Crowd avoidance (used by Movement).
- **Unity Collections / Burst** — Installed via packages (`com.unity.collections` 2.6.6, `com.unity.burst` 1.8.29).

## Testing

- **Framework**: Unity Test Framework 1.6.0.
- **Test assemblies**: `.Unitest.asmdef` per subsystem (Grid, Movement, SkillSystem).
- Run via Unity Editor > Window > General > Test Runner. No CLI test runner.

## Conventions

- **`_` prefix** distinguishes first-party code (`_Games`, `_KITSystem`) from third-party.
- **`.asmdef`** is the unit of compilation. Place new code in an existing asmdef or create one for standalone modules.
- **No root namespace** in EditorSettings — namespaces are explicit per file.
- Game code has asmdefs for current modules: `Games.Battle`, `Game.Config`, and `Games.Utils`. KITSystem code has its own asmdefs.

## Docs

| Path | Purpose |
|------|---------|
| `docs/plans/` | Implementation plans (Logic/View separation, Free Agent Group) |
| `docs/architecture/` | Architecture decisions & target design |
| `docs/knowledge/` | AI knowledge base (KG facts, Palace config reference) |

## Architecture Direction (2026-05-23)

Separating Logic & View for testability. See `docs/architecture/`.

- **Target Logic**: Pure C# POCO, no `UnityEngine` dependency. Lives under `Assets/_Games/Battle/Logic/` after refactor.
- **Current Logic**: `BattleOwner`, `BattleTickable`, `Slot`, `Weapon`, `Dice`, `SpawnerTickable`, `Monster` still mix some Unity/view responsibilities.
- **View**: MonoBehaviours with DOTween/MMF/Animancer. Lives in `Assets/_Games/Battle/View/`.
- Logic/View separation is in progress. Current view contracts are `ISlotView`, `IDiceView`, `IWeaponView`, `IAttractorView`, `IDiceControlView`.

## Verify

No CI/lint/typecheck. Verify in Unity Editor:
1. Open project — check Console for compile errors.
2. Run tests: Window > General > Test Runner.
3. Play-mode test: open `Assets/_Games/Battle/game scene.unity`.

## Gotchas

- `Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `*.csproj` are gitignored.
- Demo folders for third-party assets are gitignored.
- Enter Play Mode Options enabled but defaults (no domain reload skip).
