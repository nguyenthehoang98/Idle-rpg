# ADR-0007 - Skill System Architecture

Status: `accepted`

Date: 2026-07-31

## Context

Cần thiết kế hệ thống skill/projectile/hit-detection tái sử dụng được cho nhiều game (TDSurvivor, ARPG, Survivor...).

Recovery 2 đã có `_KITSystem.SkillSystem` battle-tested với kiến trúc tốt: Spu (Skill Processing Unit) quản lý action queue, BaseAction lifecycle, BaseTrajectory + BaseCollider pattern.

Thách thức: Recovery 2 code có coupling với `_Game` (MonoBehaviour Projectile, EntityManager, AgentManager). Cần tách ra thành module portable trong GameToolkit.

## Decision

SkillSystem làm module **portable** trong `_GameToolkit.SkillSystem`. Game-specific bridge nằm trong `_TDS/`.

### Core (GameToolkit.SkillSystem — không MonoBehaviour, không game entity)

```
_GameToolkit/SkillSystem/
├── Core/
│   ├── IAction.cs              # Interface: Start/Tick/Interrupt/Stop/IsFinished/Reason
│   ├── ActionCompleteReason.cs # Enum: Undefined, EndLifeCycle, Interrupt
│   ├── BaseAction.cs           # Abstract: lifecycle template, auto EndLifeCycle
│   ├── Spu.cs                  # Skill Processing Unit: command-queue, action management
│   ├── BaseCollider.cs         # Abstract: Tick/Collision, delay/duration
│   ├── BaseTrajectory.cs       # Abstract: EvaluatePosition/Direction
│   ├── IQuery.cs               # Interface: FindTarget/GetAllEntities (spatial query)
│   ├── IProjectileView.cs      # Interface: SetPosition/Destroy (visual bridge)
│   ├── QueryResult.cs          # Structs: QueryResult, QueryEntityData
│   ├── FindTargetType.cs       # Enum: Nearest, Farthest, HpLowest...
│   ├── HitInfo.cs              # Struct: EntityId, Position, IsLastHit
│   └── CastProjectileAction.cs # Concrete: kết hợp Trajectory + Collider → hit callback
└── Imp/
    ├── ProjectileTrajectory.cs  # Curve-based linear movement
    ├── BoomerangTrajectory.cs   # Outbound→Hang→Return
    ├── SplineTrajectory.cs      # Spline path: Windup→Execute→Recovery
    ├── StationaryTrajectory.cs  # Fixed position
    ├── CircleCollider.cs        # Circle hit detection
    ├── RectangleCollider.cs     # Rectangle hit detection (rotate-aware)
    ├── ColliderData.cs          # Serializable struct + PropertyDrawer
    ├── ColliderType.cs          # Enum: Circle, Rectangle
    ├── TrajectoryData.cs        # Serializable config + TrajectoryType enum
    └── TrajectoryType.cs
```

### Bridge (_TDS/ — game-specific)

```
_TDS/Skill/
├── SkillManager.cs       # : Spu + ITickable, orchestrator cast skill → game entities
├── EntityQuery.cs         # : IQuery, query qua AgentSimulator + EntityManager
├── SkillConfig.cs         # Game config schema (prefab, skill params)
└── SkillRuntimeData.cs    # Runtime cast data struct

_TDS/Unit/
└── Projectile.cs          # : MonoBehaviour, IProjectileView (visual: lerp, trail, pool)
```

### 2 Interface cầu nối

```csharp
// GameToolkit: skill system gọi game qua interface này để query entity
public interface IQuery
{
    void FindTarget(FindTargetType type, int totalQuery, Vector2 center, Vector2 pivot, 
                    float radius, Func<int, float2, bool> filter, out QueryResult result);
    List<int> GetAllEntities(Vector2 center, Vector2 size, Func<int, bool> filter);
}

// GameToolkit: skill system gọi game qua interface này để update visual projectile
public interface IProjectileView
{
    void SetPosition(Vector2 position, Vector2 direction, float deltaTime);
    void Destroy();
}
```

## Rationale

- **Portable**: Core logic (Spu, Action, Trajectory, Collider) thuần toán, không MonoBehaviour
- **Linh hoạt**: 2 interface `IQuery` + `IProjectileView` cho phép mỗi game tự implement cách query entity và render projectile
- **Battle-tested**: Đã hoạt động trong Recovery 2 với đầy đủ trajectory types và collider types
- **Tách biệt rõ**: GameToolkit = pure logic, _TDS = MonoBehaviour + entity bridge + config

## Consequences

- Cần implement `IQuery` cho mỗi game (dùng AgentSimulator hoặc grid riêng)
- `IProjectileView` yêu cầu mỗi game tự define visual representation (prefab, animation)
- `SplineTrajectory` cần package `UnityEngine.Splines`
- `GizmosLine` utility cần moved từ Recovery 2 `_Game` → `_GameToolkit.Utils`
- `CastProjectileAction` cần refactor: thay `Projectile` MonoBehaviour → `IProjectileView`
- `DamageEntityInfo` struct → `HitInfo` struct (generic hóa)
