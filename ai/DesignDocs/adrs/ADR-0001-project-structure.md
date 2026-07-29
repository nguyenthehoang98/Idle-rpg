# ADR-0001 - Project Structure

Status: `proposed`

Date: 2026-07-29

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

Dùng cấu trúc mới:

```text
Assets/
├── _DesignDocs/
└── _TDSurvivor/
    ├── Code/
    └── Content/
```

Trong đó:

```text
Code/      -> C# scripts namespace TDSurvivor
Content/   -> Scenes, Prefabs, Sprites, Config, Audio
DesignDocs -> Specs, plans, ADRs, test checklists
```

Không copy trực tiếp từ Recovery trong giai đoạn thiết kế/MVP 0.1.

## Consequences

Pros:

```text
- Sạch namespace
- Ít dependency
- Dễ hiểu MVP
- Tránh lỗi missing refs từ Unity asset copy
- Có docs dẫn đường cho AI và người dùng
```

Cons:

```text
- Ban đầu chậm hơn copy
- Phải viết lại một số logic đã có ở Recovery
- Sau này cần port có chọn lọc Pool/Updater/Config nếu cần
```

## Alternatives Considered

### A. Copy toàn bộ Recovery

Rejected.

Lý do:

```text
- Rủi ro dependency/plugin
- Không rõ module nào cần thiết
- Có thể kéo theo kiến trúc quá lớn cho MVP
```

### B. Copy `_TDS` và đổi namespace

Rejected for now.

Lý do:

```text
- Vẫn phụ thuộc `_GameToolkit`, UniTask, Pool, ConfigManager
- MVP chưa cần complexity đó
```

### C. Thiết kế mới, tham khảo Recovery

Accepted.

Lý do:

```text
- Phù hợp spec-driven workflow
- Dễ kiểm soát MVP
- Sau này port từng module khi có acceptance rõ
```

## Review Date

Review lại sau khi MVP 0.1 chạy được.
