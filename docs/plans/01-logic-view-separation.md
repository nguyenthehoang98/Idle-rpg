---
date: 2026-05-23
status: draft
agent: plan
---

# Plan: Tách Logic & View để Unit Test được

## Goal

- Mọi logic code (Weapon, Dice, Cone, Spawner, BattleFlow) có thể chạy unit test **không cần Unity**
- View layer (MonoBehaviour, DOTween, MMF) chỉ gọi logic qua interfaces
- Không thay đổi behavior hiện tại

---

## Phase 1: Extract WeaponLogic (3 ngày)

### Current State

`Weapon.cs` (`Assets/_Games/Battle/Weapon.cs`) chứa cả:
- Cooldown timer, rotation direction (logic)
- `Debug.DrawLine` triangle/square, `DrawTriangle`, `DrawSquare` (view)

### Step 1.1: Create `IWeaponLogic` interface

File: `Assets/_Games/Battle/Logic/Interfaces/IWeaponLogic.cs`

```csharp
using Unity.Mathematics;

namespace _Games.Battle.Logic
{
    public interface IWeaponLogic
    {
        void Tick(float deltaTime);
        void Activate();
        void Deactivate();
        void RotateTo(float2 targetPosition);
        bool IsActive { get; }
        float CooldownProgress { get; }
        event Action<float2> OnFire;
    }
}
```

### Step 1.2: Create `WeaponLogic` pure class

File: `Assets/_Games/Battle/Logic/Weapons/WeaponLogic.cs`

```csharp
using Unity.Mathematics;

namespace _Games.Battle.Logic
{
    public class WeaponLogic : IWeaponLogic
    {
        public bool IsActive { get; private set; }
        public float CooldownProgress => elapsedTime / cooldown;
        public event Action<float2> OnFire;

        private readonly float cooldown;
        private readonly IQuery query;
        private readonly float attackRange;
        private readonly float2 center;
        private float elapsedTime;
        private float2 position;
        private float2 direction;

        public WeaponLogic(WeaponConfig config, IQuery query)
        {
            this.cooldown = config.cooldown;
            this.attackRange = config.attackRange;
            this.center = config.center;
            this.query = query;
            this.direction = config.defaultDirection;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive) return;
            
            elapsedTime += deltaTime;
            if (elapsedTime >= cooldown)
            {
                elapsedTime = 0f;
                if (query.FindNearestTargetPosition(center, attackRange, out float2 target))
                {
                    OnFire?.Invoke(target);
                }
            }
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void RotateTo(float2 targetPosition)
        {
            direction = math.normalizesafe(targetPosition - position);
        }
    }
}
```

### Step 1.3: Rewrite `Weapon` → `WeaponView` (MonoBehaviour)

The existing `Weapon.cs` becomes `WeaponView.cs` — only handles visualization.

**Files changed:**
- NEW: `Assets/_Games/Battle/Logic/Interfaces/IWeaponLogic.cs`
- NEW: `Assets/_Games/Battle/Logic/Weapons/WeaponLogic.cs`
- RENAME: `Weapon.cs` → `View/WeaponView.cs`
- CREATE: `Assets/_Games/Battle/Battle.Unitest/WeaponLogicTest.cs`

### Test: WeaponLogicTest

```csharp
[Test]
public void Fire_AfterCooldown_CallsOnFire()
{
    var query = new MockQuery(hasTarget: true);
    var weapon = new WeaponLogic(config, query);
    weapon.Activate();
    
    weapon.Tick(config.cooldown + 0.01f);
    
    Assert.IsTrue(weapon.OnFireWasCalled);
}

[Test]
public void Deactivate_StopsTicking()
{
    var weapon = new WeaponLogic(config, query);
    weapon.Activate();
    weapon.Deactivate();
    
    weapon.Tick(config.cooldown + 1f);
    
    Assert.IsFalse(weapon.OnFireWasCalled);
}
```

---

## Phase 2: Extract DiceLogic (1 ngày)

### Current State

`Dice.cs` gần như pure logic rồi — chỉ phụ thuộc `RandomUtils`.

### Step 2.1: Create `IDiceLogic`

```csharp
public interface IDiceLogic
{
    void Tick(float deltaTime);
    int Value { get; }
    event Action<int> OnTriggered;
    bool IsReady { get; }
}
```

### Step 2.2: Create `DiceLogic` — inject `IRandomProvider`

```csharp
public interface IRandomProvider
{
    int Range(int min, int max);
}

public class DiceLogic : IDiceLogic
{
    // Pure logic — no UnityEngine.Random dependency
}
```

### Step 2.3: Test DiceLogic
- Test cooldown timing
- Test value generation (mock RandomProvider)
- Test event triggering
- Test delay trigger timing

---

## Phase 3: Extract ConeLogic (2 ngày)

### Current State

`Cone.cs` vừa quản lý `Weapon` (logic) vừa `Instantiate(ConeView)` (view).

### Step 3.1: Interface

```csharp
public interface IConeLogic
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
    void Tick(float deltaTime);
    event Action OnActivated;
    event Action OnDeactivated;
}
```

### Step 3.2: Refactor `Cone.cs`

- `Cone` cũ → rename `ConeViewInstaller` — chỉ chịu trách nhiệm wire view
- `ConeLogic` mới — quản lý state machine + weapon logic
- `ConeView` (existing) — không đổi

---

## Phase 4: Refactor BattleFlow (3 ngày)

### Current State

`BattleTickable.cs` + `BattleOwner.cs` + `BattleManager.cs` đều có mixed logic/view.

### Step 4.1: Extract `BattleFlow`

```csharp
public class BattleFlow
{
    private readonly IDiceLogic[] dices;
    private readonly IConeLogic[] cones;
    private readonly ISpawnerLogic spawner;
    
    public void Tick(float deltaTime) { ... }
    public void OnDiceTriggered(int diceIndex) { ... }
    public void StartNextWave() { ... }
}
```

### Step 4.2: BattleOwner → chỉ còn View orchestration

`BattleOwner` chỉ wire các components, subscribe events, không chứa logic game.

---

## Phase 5: Split BattleSetting (1 ngày)

### Current Problem

`BattleSetting.cs` chứa cả logic config lẫn view reference (`ConeView`).

### Solution

```csharp
// Logic config — pure data
[CreateAssetMenu]
public class BattleLogicConfig : ScriptableObject
{
    public int totalDice;
    public float diceCooldown;
    public float diceDelayTrigger;
    public float weaponCooldown;
    public float weaponAttackRange;
    public float2 center;
}

// View config — references Unity objects
[CreateAssetMenu]
public class BattleViewConfig : ScriptableObject
{
    public ConeView coneView;
    public SkillFrameConfig skillFrameConfig;
}
```

---

## Timeline

| Phase | Task | Days | Files Changed | Tests Added |
|-------|------|------|---------------|-------------|
| 1 | WeaponLogic | 3 | 3 new, 1 rename | 15+ |
| 2 | DiceLogic | 1 | 2 new, 1 edit | 10+ |
| 3 | ConeLogic | 2 | 2 new, 1 edit | 10+ |
| 4 | BattleFlow | 3 | 3 new, 2 edit | 20+ |
| 5 | BattleSetting | 1 | 2 new, 1 edit | 0 |
| 6 | Remove old files | 1 | delete 3 files | 0 |
| **Total** | | **11 days** | | **55+ tests** |

## Knowledge Impact

| File | Action |
|------|--------|
| `docs/architecture/01-current-analysis.md` | **Created** — baseline analysis |
| `docs/architecture/02-target-architecture.md` | **Created** — target design |
| `AGENTS.md` | **Update** — add Logic/View subdirectories |
