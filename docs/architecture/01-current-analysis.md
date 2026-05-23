# Architecture Analysis — Current State

## Problem: Logic & View Mixed

The codebase mixes pure logic with Unity-specific rendering, making unit testing impossible without a running Unity instance.

### Pattern Analysis per File

| File | Logic | View | Mixed? |
|------|-------|------|--------|
| `BattleManager.cs` | Wave spawn, dice trigger | UI init, ArcMove, Debug.Log | ❌ Heavy |
| `BattleTickable.cs` | Dice/Cone state machine | DrawCircle (Debug.DrawLine) | ❌ |
| `BattleOwner.cs` | Entity lifecycle, wave control | OnGUI (Debug HUD), Gizmos | ❌ |
| `Cone.cs` | Weapon lifecycle, active state | ConeView instantiation, Debug.DrawLine | ❌ |
| `Weapon.cs` | Cooldown, target find, skill build | Debug.DrawLine (triangle, square) | ❌ |
| `WeaponMono.cs` | **Pure View** | DOTween, Animancer, MMF_Player, sorting | ✅ |
| `DiceRollController.cs` | **Pure View** | DOTween roll animation | ✅ |
| `ArcMove.cs` | **Pure View** | DOTween arc movement | ✅ |
| `ConeMono.cs` | **Pure View** | DOTween, MMF_Player, stars | ✅ |
| `SpawnerTickable.cs` | Wave spawn, budget, random bag | _none_ | ✅ (pure logic) |
| `Dice.cs` | Cooldown/trigger state machine | _none_ | ✅ (pure logic) |
| `BattleSetting.cs` | Config data | ConeView reference | ⚠️ (view ref in config) |

### Root Cause

- **No interface boundaries** between logic and Unity engine
- **BattleSetting** holds both logic fields (`totalDice`, `diceCooldown`) and view references (`coneView`)
- **Cone** class new's up `Weapon` (logic) AND instantiates `ConeView` (view) — a single class doing both
- **BattleTickable** manages state AND draws gizmos
- **Weapon** calculates cooldown AND draws debug triangles
- **BattleOwner** extends `TickSystemOwner` directly — hard to test in isolation
