# Config Reference

## Battle Config

| Config | Type | Fields |
|--------|------|--------|
| BattleSetting | SerializedScriptableObject | totalSlot, slotCooldownTime, slotRecoveryTime, weaponCooldown, weaponAttackRange, worldCenter, slot, dice, weapon, attractor, diceControl, skillFrameConfig |
| BattleLogicConfig | Planned ScriptableObject | totalSlot, slotCooldownTime, weaponCooldown, weaponAttackRange, center |
| BattleViewConfig | Planned ScriptableObject | slotView, diceView, weaponView, attractorView, diceControlView, skillFrameConfig |

## Logic Interfaces

| Interface | Methods | Events |
|-----------|---------|--------|
| IWeaponLogic | Tick, Activate, Deactivate, RotateTo | OnFire |
| IDiceLogic | Tick | OnTriggered |
| ISlotLogic | Tick, Activate, Deactivate | OnActivated, OnDeactivated |
| ISpawnerLogic | Tick, WaveSpawn | OnWaveCompleted |

## View Components

| MonoBehaviour | Dependencies | Purpose |
|--------------|--------------|---------|
| ObjectWeaponView / PureWeaponView | IWeaponView | DOTween/Animancer weapon visuals |
| ObjectSlotView / PureSlotView | ISlotView | Slot highlight, stars, stack visuals |
| DiceRollController | none | Pure visual dice roll animation |
| DiceControlView | IDiceControlView | Dice speed/control UI |
| BattleOwner | TickSystemOwner | Current scene entry and system installer |
