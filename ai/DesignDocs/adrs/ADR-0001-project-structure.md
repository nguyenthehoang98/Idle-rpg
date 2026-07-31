# ADR-0001 - Project Structure

Status: `accepted`

Date: 2026-07-29
Date accepted: 2026-07-31

## Context

Dự án `TDSurvivor` là Unity 2D Tower Defense / Survival Defense.

Có folder tham khảo `Recovery` với cấu trúc:

```text
_GameToolkit/
_TDS/
_TDS assets/
Plugins/
AddressableAssetsData/
Excels/
```

Recovery có nhiều ý tưởng tốt: tách toolkit/gameplay/assets, config data, pooling, updater. Tuy nhiên copy trực tiếp có rủi ro do Unity GUID, package dependency, scene/prefab refs, namespace cũ.

## Decision

Dùng cấu trúc đã có, giữ nguyên từ Recovery (đã được chọn lọc module):

```text
Assets/
├── _GameToolkit/       # Engine/framework dùng chung
│   ├── Avoidance/      # RVO movement
│   ├── Collision/      # Spatial collision detection
│   ├── GameConfig/     # Config loader (Excel → JSON)
│   ├── Resource/       # Pool + Asset loader
│   ├── Startup/        # BootScene base class
│   ├── Statistics/     # Stat + StatModifier system
│   ├── Updater/        # Fixed-timestep game loop
│   └── Utils/          # SafeArea, Timing, GizmosLine
├── _TDS/               # Gameplay code (TDSurvivor specific)
│   ├── Boot/           # GameBootScene
│   ├── GameConfig/     # MonsterConfig, SpawnerConfig
│   ├── Gameplay/       # GameplayStartup, SpawnerUpdater
│   ├── Statistics/     # StatId, Stats
│   └── Unit/           # Monster, MonsterMoveUpdater
├── _TDS assets/        # Scenes, Prefabs, Config JSON, Textures
├── Excels/             # Source config (xlsx)
└── Plugins/            # Third-party libs
```

Rationale: giữ nguyên cấu trúc hiện tại để tránh refactor không cần thiết trong MVP. Cấu trúc này đã được verify hoạt động với Boot → Config → Spawn → RVO pipeline.

## Consequences

Pros:

```text
- Kiến trúc đã được verify (Boot → Config → Spawn → RVO pipeline hoạt động)
- Tách biệt rõ GameToolkit (reusable) và _TDS (game-specific)
- Không cần refactor namespace, tránh lỗi missing refs
- Module boundary đã được xác định rõ (xem 02-proposed-folder-structure.md)
```

Cons:

```text
- Giữ lại namespace cũ (_TDS.* thay vì TDSurvivor.*)
- Một số module trong GameToolkit còn coupling với Recovery patterns (cần refactor dần)
```

## Alternatives Considered

### A. Copy toàn bộ Recovery

Rejected.

Lý do: rủi ro dependency/plugin, không rõ module nào cần thiết.

### B. Thiết kế mới với `_TDSurvivor/Code/` + `Content/`

Rejected (đã từng proposed trong v1 ADR).

Lý do: cấu trúc `_TDS/` + `_GameToolkit/` hiện tại đã hoạt động ổn, refactor sang cấu trúc mới không cần thiết cho MVP.

### C. Giữ nguyên cấu trúc hiện tại (_TDS/ + _GameToolkit/)

Accepted.

Lý do:

```text
- Đã có code hoạt động (Boot, Config, Spawn, RVO)
- Module boundary rõ ràng (GameToolkit = reusable, _TDS = game-specific)
- Tránh refactor không cần thiết trong giai đoạn MVP
- Tập trung effort vào việc bổ sung gameplay thay vì tổ chức lại folder
```

## Review Date

Review lại sau khi MVP 0.1 chạy được.
