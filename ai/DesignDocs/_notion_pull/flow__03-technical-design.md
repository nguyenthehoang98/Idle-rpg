# flow/03-technical-design

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/03-technical-design.md

# 03 - Technical Design Draft

Status: `draft`

## 1. Architecture Goal

Thiết kế Unity 2D đơn giản, dễ test, chưa phụ thuộc Recovery/plugin.

```plain text
Scene MonoBehaviours first -> verify gameplay -> then consider Pool/Config/Updater.
```

## 2. Dependency Direction

```plain text
UI -> Gameplay/Core
Spawning -> Units
Heroes -> Projectiles -> Combat
Monsters -> Base/Combat
Core -> independent
```

Không để:

```plain text
Core phụ thuộc UI
GameToolkit phụ thuộc TDSurvivor
Runtime code phụ thuộc Editor-only tools
```

## 3. Runtime Components MVP 0.1

### GameManager

Path:

```plain text
Assets/_TDSurvivor/Code/Core/GameManager.cs
```

Responsibilities:

```plain text
- Hold game state: Playing, Paused, GameOver, Victory
- Publish state changed event
- Stop gameplay logic when not Playing
```

### Health

Path:

```plain text
Assets/_TDSurvivor/Code/Combat/Health.cs
```

Responsibilities:

```plain text
- Store max/current HP
- TakeDamage
- Heal
- OnHpChanged
- OnDied
```

Used by:

```plain text
- BaseCore
- Monster
- Later: Hero
```

### BaseCore

Path:

```plain text
Assets/_TDSurvivor/Code/Units/Base/BaseCore.cs
```

Responsibilities:

```plain text
- Receive damage
- Trigger GameOver when dead
```

### MonsterController

Path:

```plain text
Assets/_TDSurvivor/Code/Units/Monsters/MonsterController.cs
```

Responsibilities:

```plain text
- Initialize with Base target
- Move toward Base
- Damage Base on reach
- Notify finished on death/reach
```

### HeroAutoAttack

Path:

```plain text
Assets/_TDSurvivor/Code/Units/Heroes/HeroAutoAttack.cs
```

Responsibilities:

```plain text
- Find nearest Monster in range
- Respect cooldown
- Spawn Projectile
```

MVP limitation:

```plain text
Uses FindObjectsOfType for simplicity.
Optimization later: TargetRegistry or spatial query.
```

### Projectile

Path:

```plain text
Assets/_TDSurvivor/Code/Projectiles/Projectile.cs
```

Responsibilities:

```plain text
- Follow target
- Deal damage once
- Destroy on hit/timeout/missing target
```

### EnemySpawner

Path:

```plain text
Assets/_TDSurvivor/Code/Spawning/EnemySpawner.cs
```

Responsibilities:

```plain text
- Spawn monster prefab
- Pick portal/random position
- Initialize monster with Base target
```

### WaveManager

Path:

```plain text
Assets/_TDSurvivor/Code/Spawning/WaveManager.cs
```

Responsibilities:

```plain text
- Run wave coroutine
- Spawn N monsters
- Track alive monsters
- Trigger Victory after all waves
```

### GameplayUI

Path:

```plain text
Assets/_TDSurvivor/Code/UI/GameplayUI.cs
```

Responsibilities:

```plain text
- Display Base HP
- Display Wave
- Display GameState
```

## 4. Scene Design MVP 0.1

```plain text
Gameplay.unity
├── Main Camera
├── GameManager
├── BaseCore
├── Heroes
│   └── Archer
├── SpawnPortals
│   ├── Top
│   ├── Bottom
│   ├── Left
│   └── Right
├── EnemySpawner
├── WaveManager
└── Canvas
    └── GameplayUI
```

## 5. Prefab Design MVP 0.1

### BaseCore.prefab

```plain text
Components:
- Transform
- SpriteRenderer
- Health
- BaseCore

Serialized defaults:
- Health.maxHp = 100
```

### Archer.prefab

```plain text
Components:
- Transform
- SpriteRenderer
- HeroAutoAttack

Serialized defaults:
- attackRange = 5
- attackCooldown = 1
- damage = 10
- projectileSpeed = 10
- projectilePrefab = Arrow
```

### Slime.prefab

```plain text
Components:
- Transform
- SpriteRenderer
- Health
- MonsterController

Serialized defaults:
- Health.maxHp = 30
- moveSpeed = 2
- attackDamage = 5
- reachDistance = 0.25
```

### Arrow.prefab

```plain text
Components:
- Transform
- SpriteRenderer
- Projectile

Serialized defaults:
- speed = 10
- hitDistance = 0.15
- lifeTime = 3
```

## 6. Data Strategy

MVP 0.1:

```plain text
Use serialized fields in MonoBehaviour.
```

MVP 0.2:

```plain text
Introduce ScriptableObject configs:
- HeroConfig
- MonsterConfig
- WaveConfig
```

Later, if needed:

```plain text
Excel/JSON pipeline inspired by Recovery.
```

## 7. Recovery Reference Strategy

Reference only:

```plain text
- Spawner structure
- Monster config fields
- Pool idea
- Updater idea
- Folder separation
```

Do not copy yet:

```plain text
- Scene files
- Prefabs
- Plugins
- Addressables
- ExcelExtension
- Meta files
```

## 8. Risks

| Risk | Impact | Mitigation |

|---|---|---|

| Scope grows too fast | Prototype delayed | Keep MVP 0.1 fixed |

| FindObjectsOfType inefficient | Bad with many monsters | Accept for MVP, replace later |

| No object pool | GC/spike later | Add pool after gameplay loop works |

| Manual scene setup slow | Setup mistakes | Add Editor scene generator after spec approved |

| Recovery dependency creep | Broken refs/packages | Use source-driven reference only |

## 9. Technical Open Questions

```plain text
TOQ01 - Use Built-in 2D or URP 2D?
TOQ02 - Need Rigidbody2D/Collider2D in MVP, or transform movement enough?
TOQ03 - Should hit detection be target-follow or physics collision?
TOQ04 - Should wave data be serialized on WaveManager or ScriptableObject immediately?
TOQ05 - Need object pooling in MVP 0.1 or MVP 0.2?
```
