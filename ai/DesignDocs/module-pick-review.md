# Module Pick & Review - _GameToolkit

> Đánh giá sau khi pick module từ Recovery/_GameToolkit và Recovery2/_KITSystem
> Ngày: 30/07/2025

## Nguyên tắc pick

1. Pick module tái sử dụng từ 2 nguồn
2. Conflict → ưu tiên **Recovery**
3. Tính năng đã có → skip

---

## Kết quả pick

### ✅ Modules đã pick thành công

| Module | Nguồn | Mô tả |
|--------|-------|-------|
| **Avoidance/** | Recovery | AgentSimulator (RVO), IGrid, FixedUniformGrid |
| **Collision/** | Recovery | BaseCollision, BoxCollision2D, CapsuleCollision2D, CircleCollision2D, Collision3D, ICollision |
| **GameConfig/** | Recovery | ConfigManager, IGameConfig, ConfigPath |
| **Resource/** | Cả 2 | Pool, PoolInternal, AssetManager (Recovery), AssetBundleManager (Recovery2), CloudBundleLoader, LocalBundleLoader |
| **Startup/** | Recovery | BootScene |
| **Statistics/** | Recovery | Stat, StatModifier |
| **Updater/** | Recovery | UpdaterOwner, BaseUpdatable |
| **Utils/** | Recovery2 | SafeArea (Recovery), Timing (Recovery), MathUtils, CollectionUtils, CoroutineUtils, RandomUtils, KitEntryScene, UnityMainThreadDispatcher |
| **Grid/RVO2/** | Recovery2 | RVO2 library (Simulator, Agent, KdTree, Obstacle...) |
| **Schedule/** | Recovery2 | TickSystemOwner, ITickable |
| **SystemSkills/** | Recovery2 | Spu, BaseAction, IAction, IQuery, FindTargetType + Implementations (CastProjectile, ProjectileTrajectory, BoomerangTrajectory...) |
| **Entity/** | Recovery2 | EntityManager, ComponentManager, Parameter, EntityChangedEvent |
| **TimeScaleToolbar/** | Recovery2 | Editor toolbar utility |

### ⚠️ Conflicts resolved (ưu tiên Recovery)

| File | Recovery version | Recovery2 version | Action |
|------|-----------------|-------------------|--------|
| **AgentSimulator.cs** | `Avoidance/` (205 lines) | ~~`Grid/` (256 lines)~~ | ✅ Giữ **Avoidance/** |
| **IGrid.cs** | `Avoidance/` | ~~`Grid/`~~ | ✅ Giữ **Avoidance/** |
| **FixedUniformGrid.cs** | `Avoidance/` | ~~`Grid/`~~ | ✅ Giữ **Avoidance/** |

### 🔧 Code updates kèm theo

| File | Thay đổi |
|------|---------|
| `_TDS/Gameplay/Manager/AgentManager.cs` | `using _KITSystem.Grid` → `using _GameToolkit.Avoidance` |
| `_TDS/Gameplay/EntityQuery.cs` | `using _KITSystem.Grid` → `using _GameToolkit.Avoidance` |

---

## Cấu trúc _GameToolkit sau pick

```
Assets/_GameToolkit/
├── Avoidance/         ← AgentSimulator (RVO), IGrid, FixedUniformGrid
├── Collision/         ← Collision utilities
├── Entity/            ← EntityManager, ComponentManager (Recovery2)
├── GameConfig/        ← ConfigManager, IGameConfig, ConfigPath
├── Grid/
│   └── RVO2/          ← RVO2 library (Recovery2)
├── Resource/          ← Pool, AssetManager, AssetBundleManager
├── Schedule/          ← TickSystemOwner, ITickable (Recovery2)
├── Startup/           ← BootScene
├── Statistics/        ← Stat, StatModifier
├── SystemSkills/      ← Spu, BaseAction, Skills (Recovery2)
│   ├── Core/
│   └── Implement/
├── TimeScaleToolbar/  ← Editor utility
├── Updater/           ← UpdaterOwner, BaseUpdatable
└── Utils/             ← MathUtils, CollectionUtils, etc.
```

---

## Các thay đổi đã thực hiện

### 1. ✅ Đồng nhất namespace
Tất cả `_KITSystem.*` → `_GameToolkit.*`:

| Cũ | Mới | Files ảnh hưởng |
|----|-----|----------------|
| `_KITSystem.Utils` | `_GameToolkit.Utils` | 8 files (MathUtils, CollectionUtils...)
| `_KITSystem.Entity` | `_GameToolkit.Entity` | 5 files (EntityManager...)
| `_KITSystem.SkillSystem.Core` | `_GameToolkit.SkillSystem.Core` | 8 files (Spu, BaseAction...)
| `_KITSystem.SkillSystem.Imp` | `_GameToolkit.SkillSystem.Implement` | 11 files (CastProjectile...)
| `_KITSystem.Config.Editor` | `_GameToolkit.GameConfig.Editor` | 2 files
| `_KITSystem.TimeScaleToolbar` | `_GameToolkit.TimeScaleToolbar` | 3 files
| `_KITSystem.Schedule` | `_GameToolkit.Updater` | TickSystemOwner, ITickable
| `_KITSystem.Resource` | `_GameToolkit.Resource` | AssetBundleManager → xoá
| `_KITSystem.Config` | `_GameToolkit.GameConfig` | _TDS config usings

### 2. ✅ Dùng AssetManager thay thế
- `AssetBundleManager` (`_KITSystem.Resource`) đã được **xoá**
- Toàn bộ code chuyển sang dùng `AssetManager` (`_GameToolkit.Resource`)
- API tương thích: `GetAsset<T>()`, `GetAssetCached<T>()`, `UnCache()`

### 3. ✅ Dùng Updater thay thế Schedule
- `Schedule/` folder đã xoá (rỗng)
- `TickSystemOwner` + `ITickable` chuyển vào `Updater/` với namespace `_GameToolkit.Updater`
- `UpdaterOwner` + `BaseUpdatable` giữ nguyên
- Cả 2 hệ thống cùng nằm trong module `_GameToolkit.Updater`

## Kết quả cuối cùng

```
Assets/_GameToolkit/
├── Avoidance/         ← AgentSimulator (RVO), IGrid, FixedUniformGrid
├── Collision/         ← BaseCollision, BoxCollision2D...
├── Entity/            ← EntityManager, ComponentManager
├── GameConfig/        ← ConfigManager, IGameConfig
│   └── Editor/
├── Grid/
│   └── RVO2/          ← RVO2 library
├── Resource/          ← Pool, AssetManager, IBundleLoader
├── Startup/           ← BootScene
├── Statistics/        ← Stat, StatModifier
├── SystemSkills/      ← Spu, BaseAction, Skills
│   ├── Core/
│   └── Implement/
├── TimeScaleToolbar/  ← Editor utility
│   └── Editor/
├── Updater/           ← UpdaterOwner, BaseUpdatable, TickSystemOwner, ITickable
│   └── Editor/
└── Utils/             ← MathUtils, CollectionUtils, SafeArea...
```

## Đánh giá

### Điểm mạnh
- ✅ **Namespace đồng nhất**: 100% `_GameToolkit.*`
- ✅ **Core system đầy đủ**: Pool, Config (Excel→JSON), RVO movement, Update loop
- ✅ **Skill system**: Spu + nhiều implementation (Projectile, Boomerang, Spline...)
- ✅ **Entity system**: Quản lý monster/hero
- ✅ **Asset loading**: Thống nhất dùng AssetManager

### Tồn tại còn lại
1. **Updater kép**: `UpdaterOwner+BaseUpdatable` (Recovery) và `TickSystemOwner+ITickable` (Recovery2) cùng tồn tại. Cần thống nhất sau này.
2. **Asset loading strategy**: AssetManager dùng Resources vs AssetBundle. Cần quyết định sau.\n3. **Code namespace**: `_Game.*`, `_TDS.*` chưa đổi → sẽ đổi khi tạo `_TDSurvivor`
