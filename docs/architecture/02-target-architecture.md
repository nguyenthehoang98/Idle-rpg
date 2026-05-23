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
│  - WeaponMono, ConeMono, BattleLevel       │
│  - DiceRollController, ArcMove, Star       │
│  - UIBattleControlDiceSpeed, UIBattleDiceSlot│
│  - Depends on: Logic via interfaces        │
└──────────────┬─────────────────────────────┘
               │ calls logic methods
┌──────────────▼─────────────────────────────┐
│  Logic Layer (POCO / interface)            │  ← TESTABLE
│  - WeaponLogic, DiceLogic, ConeLogic       │
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

public interface IConeLogic
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
        Cones/
          ConeLogic.cs
        Spawning/
          SpawnerLogic.cs
        BattleLoop/
          BattleFlow.cs
          WaveController.cs
        Interfaces/
          IWeaponLogic.cs
          IDiceLogic.cs
          IConeLogic.cs
          ISpawnerLogic.cs
      View/                   ← NEW: MonoBehaviours only
        WeaponMono.cs
        ConeMono.cs
        DiceRollController.cs
        ArcMove.cs
        BattleLevel.cs
        UIBattleControlDiceSpeed.cs
        UIBattleDiceSlot.cs
        Star.cs
      (existing files)        ← will be deprecated
    Battle.Unitest/           ← NEW: Unit test assembly
      Logic/
        WeaponLogicTest.cs
        DiceLogicTest.cs
        ConeLogicTest.cs
        SpawnerLogicTest.cs
        BattleFlowTest.cs
```

## Migration Strategy

| Phase | Scope | Testability |
|-------|-------|-------------|
| 1 | Extract `WeaponLogic` from `Weapon.cs` | Can test cooldown, target selection, fire rate |
| 2 | Extract `DiceLogic` from `Dice.cs` | Already mostly pure — just remove `RandomUtils` dependency |
| 3 | Extract `ConeLogic` from `Cone.cs` | Isolate state machine from view instantiation |
| 4 | Refactor `BattleTickable` → `BattleLoopLogic` | Core battle loop testable |
| 5 | Refactor `BattleOwner` → `BattleFlow` | Wave lifecycle testable |
| 6 | Clean BattleSetting → split Logic/View configs | Remove view refs from config |
