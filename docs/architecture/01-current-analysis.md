# Architecture Analysis — Current State

## Problem: Logic & View Mixed

The codebase mixes pure logic with Unity-specific rendering, making unit testing impossible without a running Unity instance.

### Pattern Analysis per File

| File | Logic | View | Mixed? |
|------|-------|------|--------|
| `BattleOwner.cs` | Entity lifecycle, wave control, system wiring | Canvas setup, OnGUI debug HUD, Gizmos | ❌ Heavy |
| `BattleTickable.cs` | Dice/Slot state machine | Coroutine view sequencing, DrawCircle (Debug.DrawLine) | ❌ |
| `Slot.cs` | Slot activation/state, owns `Weapon` | Instantiates `ISlotView`, debug shape drawing | ❌ |
| `Weapon.cs` | Cooldown, target find, skill build | Calls `IWeaponView`, Debug.DrawLine (triangle, square) | ❌ |
| `ObjectWeaponView.cs` / `PureWeaponView.cs` | _none/minimal_ | DOTween/Animancer/view rendering | ✅ |
| `DiceRollController.cs` | **Pure View** | DOTween roll animation | ✅ |
| `ObjectSlotView.cs` / `PureSlotView.cs` | _none/minimal_ | DOTween/MMF/stars/view rendering | ✅ |
| `SpawnerTickable.cs` | Wave spawn, budget, random bag | _none_ | ✅ (pure logic) |
| `Dice.cs` | Cooldown/trigger state machine | Calls `IDiceView` directly | ⚠️ |
| `BattleSetting.cs` | Config data | View interface/prefab references | ⚠️ (view refs in config) |

### Root Cause

- **No stable interface boundaries** between logic and Unity engine yet
- **BattleSetting** holds both logic fields (`totalSlot`, `slotCooldownTime`, `weaponCooldown`) and view references (`slot`, `dice`, `weapon`, `attractor`, `diceControl`)
- **Slot** new's up `Weapon` (logic) AND instantiates `ISlotView` — a single class doing both
- **BattleTickable** manages state AND draws gizmos
- **Weapon** calculates cooldown AND draws debug triangles
- **BattleOwner** extends `TickSystemOwner` directly — hard to test in isolation

### Current Runtime Risks

- Wave progression depends on entity cleanup order in `BattleOwner`.
- `SpawnerTickable` must guard missing/finished wave data before reading `batches[0]`.
- `SkillFactory`/`SPU` use queued commands, so skill instance ids must be allocated independently from pending action maps.
- `BattleOwner` owns static subscriptions and native-backed systems, so scene teardown must unsubscribe and dispose.
