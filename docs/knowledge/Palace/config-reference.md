# Config Reference

## Battle Config

| Config | Type | Fields |
|--------|------|--------|
| BattleLogicConfig | ScriptableObject | totalDice, diceCooldown, diceDelayTrigger, weaponCooldown, weaponAttackRange, center |
| BattleViewConfig | ScriptableObject | coneView, skillFrameConfig |

## Logic Interfaces

| Interface | Methods | Events |
|-----------|---------|--------|
| IWeaponLogic | Tick, Activate, Deactivate, RotateTo | OnFire |
| IDiceLogic | Tick | OnTriggered |
| IConeLogic | Tick, Activate, Deactivate | OnActivated, OnDeactivated |
| ISpawnerLogic | Tick, WaveSpawn | OnWaveCompleted |

## View Components

| MonoBehaviour | Dependencies | Purpose |
|--------------|--------------|---------|
| WeaponMono | IWeaponLogic | DOTween rotate, Animancer attack, MMF feedback |
| ConeMono | IConeLogic | Stars, highlight, weapon rotation |
| DiceRollController | none | Pure visual dice roll animation |
| ArcMove | none | Arrow arc movement tween |
| BattleLevel | IConeLogic[] | Manages cone MonoBehaviours |
