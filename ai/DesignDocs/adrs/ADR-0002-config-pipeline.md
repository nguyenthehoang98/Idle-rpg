# ADR-0002 - Config Pipeline: Excel → JSON

Status: `accepted`

Date: 2026-07-31

## Context

Cần chọn pipeline để quản lý game config data (monster stats, spawner params, hero stats, skill data). Có 2 lựa chọn chính trong Unity:
- **ScriptableObject**: data asset native của Unity, inspector-friendly
- **Excel → JSON**: bảng tính → JSON text, load runtime qua ConfigManager

Dự án Recovery đã dùng pipeline Excel → JSON với ExcelExtension plugin + ConfigManager.

## Decision

Dùng **Excel → JSON** (via ExcelExtension) + `_GameToolkit.GameConfig.ConfigManager` để load runtime.

## Rationale

- **Designer-friendly**: Excel quen thuộc với designer, dễ chỉnh sửa hàng loạt, có formula
- **Version control**: JSON là text, diff được trong git
- **Đã implement**: ConfigManager hoạt động, có sẵn `MonsterConfig`, `SpawnerConfig`
- **Không cần build lại**: chỉnh config trong Excel → xuất JSON → game load runtime, không cần rebuild Unity
- **Pipeline đã verify**: Boot → ConfigManager.Load() → Get<T>() hoạt động

## Consequences

- Cần ExcelExtension plugin trong project
- JSON parse overhead nhỏ khi load (chấp nhận được, chỉ load 1 lần khi boot)
- Config schema phải map chính xác giữa C# property name và JSON field
- Cần đảm bảo không dùng UnityEditor API trong code runtime (bug hiện tại ở SpawnerConfig.OnCompleteImported)

## Alternatives Considered

### ScriptableObject

Rejected.

Lý do: cần Unity Editor để chỉnh, không diff-friendly trong git (binary .asset files), mỗi lần chỉnh cần build lại, khó automation pipeline.
