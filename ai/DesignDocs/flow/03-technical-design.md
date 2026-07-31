# 03 - Technical Design

Status: `approved`

Date approved: 2026-07-31

## 1. Architecture Goal

Thiết kế Unity 6 URP 2D, kế thừa có chọn lọc từ Recovery.

```text
Dùng AgentSimulator (RVO) cho di chuyển.
Dùng IGrid cho hit-detection logic.
Config từ Excel -> JSON (SpawnerConfig, MonsterConfig...).
Pool từ GameToolkit.
```

## 2. Dependency Direction

```text
UI -> Gameplay/Core
Spawning -> Units
Heroes -> Projectiles -> Combat
Monsters -> Base/Combat
Core -> independent
```

Không để:

```text
Core phụ thuộc UI
GameToolkit phụ thuộc _TDS
Runtime code phụ thuộc Editor-only tools
```

## 3. Runtime Components

### GameManager

Path:
```text
Assets/_TDS/Core/GameManager.cs
```

Responsibilities:

```text
- Hold game state: Playing, Paused, GameOver, Victory
- Publish state changed event
- Stop gameplay logic when not Playing
```

### Health

Path:
```text
Assets/_TDS/Combat/Health.cs
```

Responsibilities:

```text
- Store max/current HP
- TakeDamage
- Heal
- OnHpChanged
- OnDied
```

Used by:

```text
- BaseCore
- Monster
- Hero
```

### BaseCore

Path:
```text
Assets/_TDS/Units/Base/BaseCore.cs
```

Responsibilities:

```text
- Receive damage
- Trigger GameOver when HP <= 0
```

### MonsterController

Path:
```text
Assets/_TDS/Units/Monsters/MonsterController.cs
```

Responsibilities:

```text
- 3 tiers: Mob, Elite, Boss
- Initialize with Base target
- Move toward Base using RVO (AgentSimulator từ Recovery)
- Damage Base on reach
- Notify finished on death/reach
```

### HeroController

Path:
```text
Assets/_TDS/Units/Heroes/HeroController.cs
```

Responsibilities:

```text
- 5 heroes: 2 Archer, 2 Magic, 1 Buff/Control
- Find nearest Monster in range (can force attack direction)
- Respect cooldown
- Spawn Projectile
- Support Passive + Active skills
```

Force target:

```text
Player tap/click enemy -> hero forces attack direction.
```

### Projectile

Path:
```text
Assets/_TDS/Projectiles/Projectile.cs
```

Responsibilities:

```text
- Follow target
- Deal damage once (logic hit via IGrid từ Recovery)
- Destroy on hit/timeout/missing target
```

### EnemySpawner

Path:
```text
Assets/_TDS/Spawning/EnemySpawner.cs
```

Responsibilities:

```text
- Read SpawnerConfig (Excel -> JSON)
- Spawn monster prefab at configured positions
- Initialize monster with Base target
- Use Pool từ GameToolkit
```

### WaveManager

Path:
```text
Assets/_TDS/Spawning/WaveManager.cs
```

Responsibilities:

```text
- Run wave coroutine
- Spawn monsters based on config
- Track alive monsters
- Trigger Roll/Shop UI after wave
- Trigger Victory after all waves
```

### RollShopUI

Path:
```text
Assets/_TDS/UI/RollShopUI.cs
```

Responsibilities:

```text
- Show after each wave
- Roll: free 1 item + 1 free refresh
- Shop: buy items with coin, refresh available
- Display item list
```

### GameplayUI

Path:
```text
Assets/_TDS/UI/GameplayUI.cs
```

Responsibilities:

```text
- Display Base HP
- Display Wave
- Display Coin
- Display Level (Exp)
- Display GameState
```

## 4. Scene Design MVP 0.1

```text
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

## 5. Prefab Design

### BaseCore.prefab

```text
Components:
- Transform
- SpriteRenderer
- Health
- BaseCore

Serialized defaults:
- Health.maxHp = 100
```

### Archer.prefab (x2)

```text
Components:
- Transform
- SpriteRenderer
- HeroController

Config: HeroConfig (Excel -> JSON)
- attackRange, attackCooldown, damage, projectileSpeed
```

### Magic.prefab (x2)

```text
Components:
- Transform
- SpriteRenderer
- HeroController

Config: HeroConfig (Excel -> JSON)
- AoE/burst params
```

### BuffControl.prefab (x1)

```text
Components:
- Transform
- SpriteRenderer
- HeroController

Config: HeroConfig (Excel -> JSON)
- Buff/control params
```

### Mob.prefab (slime)

```text
Components:
- Transform
- SpriteRenderer
- Health
- MonsterController

Config: MonsterConfig (Excel -> JSON)
- Tier: Mob
```

### Elite.prefab

```text
Components:
- Transform
- SpriteRenderer
- Health
- MonsterController

Config: MonsterConfig (Excel -> JSON)
- Tier: Elite
```

### Boss.prefab

```text
Components:
- Transform
- SpriteRenderer
- Health
- MonsterController

Config: MonsterConfig (Excel -> JSON)
- Tier: Boss
```

### Arrow.prefab

```text
Components:
- Transform
- SpriteRenderer
- Projectile

Config: ProjectileConfig (Excel -> JSON)
```

## 6. Data Strategy

```text
All config from Excel -> JSON pipeline (Recovery style):
- SpawnerConfig
- MonsterConfig (Mob, Elite, Boss)
- HeroConfig (Archer, Magic, Buff/Control)
- WaveConfig
- ProjectileConfig
- ShopConfig (items, prices)
- RollConfig (items, refresh rates)
```

Load via ConfigManager từ Recovery/GameToolkit.
Pool từ GameToolkit.


## 7. Recovery Reference Strategy

Ưu tiên Recovery, đánh giá thêm Recovery 2.

Reference và dùng lại (đã có base):

```text
- AgentSimulator.cs (RVO movement)
- IGrid.cs (hit-detection logic)
- SpawnerConfig, MonsterConfig (Excel -> JSON pipeline)
- Pool (GameToolkit)
- Updater (GameToolkit)
- Folder separation pattern
```

Không copy:

```text
- Scene files
- Prefabs
- Plugins
- Addressables
- Meta files
```

Quy tắc:

```text
- Copy code module -> giữ namespace hiện tại (_TDS.*, _GameToolkit.*)
- Không copy .meta, .unity, .prefab
- Chỉ dùng sau khi design đã được duyệt
```

## 8. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Scope grows too fast | Prototype delayed | Keep MVP 0.1 fixed |
| FindObjectsOfType inefficient | Bad with many monsters | Accept for MVP, replace later |
| No object pool | GC/spike later | Add pool after gameplay loop works |
| Manual scene setup slow | Setup mistakes | Add Editor scene generator after spec approved |
| Recovery dependency creep | Broken refs/packages | Use source-driven reference only |

## 9. Technical Decisions (from Interview)

```text
TD01 - Unity 6 URP 2D.
TD02 - Movement using logic position + RVO (AgentSimulator).
TD03 - Hit detection using logic (IGrid).
TD04 - All config: Excel -> JSON (SpawnerConfig, MonsterConfig...).
TD05 - Pool: từ GameToolkit (có base sẵn).
```
