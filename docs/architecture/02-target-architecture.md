# Target Architecture — Logic / View Separation

## Principle

```
Pure Logic (C# POCO / interface) 
    ↕ interfaces 
Unity View (MonoBehaviour, DOTween, MMF)
```

Logic classes must reference **zero** Unity types (no `MonoBehaviour`, `Debug`, `GameObject`, etc.).
They are testable with any C# test framework (NUnit, xUnit) **without** Unity.

## Layer Diagram

```
┌────────────────────────────────────────────┐
│  Unity Scene / Prefabs                     │
│  (depends on both layers)                  │
└──────────────┬─────────────────────────────┘
               │ wires via inspector / Installer
┌──────────────▼─────────────────────────────┐
│  View Layer (MonoBehaviour)                │
│  - Object/Pure WeaponView, SlotView        │
│  - DiceRollController, DiceView, StarView  │
│  - BattleOwner as current scene installer  │
│  - Depends on: Logic via interfaces        │
└──────────────┬─────────────────────────────┘
               │ calls logic methods
┌──────────────▼─────────────────────────────┐
│  Logic Layer (POCO / interface)            │  ← TESTABLE
│  - WeaponLogic, DiceLogic, SlotLogic       │
│  - SpawnerLogic, BattleLoopLogic           │
│  - SkillQuery, SkillFactory                │
│  - No UnityEngine dependency               │
└────────────────────────────────────────────┘
```

## Interface Boundaries

```csharp
// === LOGIC INTERFACE ===
public interface IWeaponLogic
{
    void Tick(float deltaTime);
    bool CanFire { get; }
    void Fire(float2 targetPosition);
}

public interface IDiceLogic
{
    void Tick(float deltaTime);
    int Value { get; }
    event Action<int> OnTriggered;
}

public interface ISlotLogic
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
    void Tick(float deltaTime);
}
```

## Package Restructure

```
Assets/
  _Games/
    Battle/
      Logic/                  ← NEW: Pure C# logic, testable
        Weapons/
          WeaponLogic.cs
        Dice/
          DiceLogic.cs
        Slots/
          SlotLogic.cs
        Spawning/
          SpawnerLogic.cs
        BattleLoop/
          BattleFlow.cs
          WaveController.cs
        Interfaces/
          IWeaponLogic.cs
          IDiceLogic.cs
          ISlotLogic.cs
          ISpawnerLogic.cs
      View/                   ← NEW: MonoBehaviours only
        ObjectWeaponView.cs / PureWeaponView.cs
        ObjectSlotView.cs / PureSlotView.cs
        DiceRollController.cs
        ObjectDiceView.cs / PureDiceView.cs
        DiceControlView.cs
        Star.cs
      (existing files)        ← will be deprecated
    Battle.Unitest/           ← NEW: Unit test assembly
      Logic/
        WeaponLogicTest.cs
        DiceLogicTest.cs
        SlotLogicTest.cs
        SpawnerLogicTest.cs
        BattleFlowTest.cs
```

## Migration Strategy

| Phase | Scope | Testability |
|-------|-------|-------------|
| 1 | Extract `WeaponLogic` from `Weapon.cs` | Can test cooldown, target selection, fire rate |
| 2 | Extract `DiceLogic` from `Dice.cs` | Already mostly pure — just remove `RandomUtils` dependency |
| 3 | Extract `SlotLogic` from `Slot.cs` | Isolate state machine from view instantiation |
| 4 | Refactor `BattleTickable` → `BattleLoopLogic` | Core battle loop testable |
| 5 | Refactor `BattleOwner` → `BattleFlow` | Wave lifecycle testable |
| 6 | Clean BattleSetting → split Logic/View configs | Remove view refs from config |

## Current Implementation Notes

- Current scene entry is `BattleOwner` in `Assets/_Games/Battle/game scene.unity`.
- Current game assemblies include `Games.Battle`, `Game.Config`, and `Games.Utils`.
- The target architecture remains pending for detailed implementation; immediate work should prioritize manager/lifecycle correctness before deep Logic/View extraction.
