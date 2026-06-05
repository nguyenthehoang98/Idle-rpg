# ConfigRelations — Hướng dẫn

File này định nghĩa các quan hệ (foreign key) giữa các field trong config JSON.
Dùng bởi **Config Validator** (`Tools > Config/Validator`) để kiểm tra dữ liệu.

---

## Format

```json
"SourceConfig.SourceList.SourceField": "TargetConfig.TargetList.TargetField"
```

Mỗi dòng có nghĩa:
> **SourceField** trong mỗi item của **SourceList** (ở file **SourceConfig.json**)
> → phải tồn tại trong **TargetField** của **TargetList** (ở file **TargetConfig.json**)

### Ví dụ

```json
"MonsterConfig.Overview.ClassID": "MonsterConfig.Class_1.ID"
```

→ Load `MonsterConfig.json` → lấy list `Overview` → mỗi item có `ClassID`
→ Giá trị `ClassID` đó phải có trong list `Class_1` (field `ID`)

```json
"MonsterConfig.Overview.ActiveSkillID": "SkillConfig.Overview.ID"
```

→ Load `MonsterConfig.json` + `SkillConfig.json`
→ `MonsterConfig.Overview[].ActiveSkillID` phải tồn tại trong `SkillConfig.Overview[].ID`

---

## Cấu trúc key

Mỗi key gồm 3 phần cách nhau bởi dấu `.`:

| Phần | Ý nghĩa | Ví dụ |
|---|---|---|
| `ConfigType` | Tên class C# (cũng là tên file `.json`) | `MonsterConfig`, `SkillConfig` |
| `ListField` | Tên field list chứa dữ liệu | `Overview`, `Class_1`, `Spawn_1` |
| `Field` | Tên field bên trong mỗi item | `ClassID`, `ActiveSkillID`, `ID` |

Field có thể là:
- `int` → kiểm tra equality (giá trị có tồn tại trong target)
- `int[]` → kiểm tra từng phần tử, chỉ khi mảng không rỗng

---

## Field alias (parsed fields)

Một số field trong JSON là raw string (`[SerializeField, HideInInspector]`) được parse thành `int[]` khi load.
Nếu key ghi tên field raw, validator tự động map sang field đã parse:

| Raw field (ghi trong relations) | Parsed field (thực tế) |
|---|---|
| `EquipmentsPool` | `EquipmentsID` |
| `SkillBuffsPool` | `SkillBuffsID` |
| `PortalsID` | `SpawnPortalsID` |

Ví dụ:
```json
"LevelConfig.Overview.EquipmentsPool": "EquipmentConfig.Overview.ID"
```
→ Validator sẽ đọc `LevelData.EquipmentsID` (int[]) thay vì `EquipmentsPool` (string).

---

## Các config type hiện tại

| Tên (ConfigType) | File JSON | Các list field chính |
|---|---|---|
| `MonsterConfig` | `MonsterConfig.json` | `Overview` (MonsterData), `Class_1` (MonsterClassData), `Level_2` (MonsterLevelData) |
| `SkillConfig` | `SkillConfig.json` | `Overview` (SkillData), `SkillTriggers_1`, `FindTargets_2`, `DamageTickets_3`, `Shapes_4`, `CollisionTickets_5`, `Trajectory_6`, `Curve_7` |
| `EquipmentConfig` | `EquipmentConfig.json` | `Overview` (EquipmentData), `UpgradeConfigs_1` (EquipmentUpgradeData) |
| `LevelConfig` | `LevelConfig.json` | `Overview` (LevelData), `Spawn_1` (SpawnData), `Environment_2`, `Portal_3` |

---

## Khi thêm relation mới

1. Xác định **Source**: config nào, list nào, field nào đang chứa ID tham chiếu
2. Xác định **Target**: config nào, list nào, field nào là khoá chính (ID, Level, SpawnGroupID,...)
3. Thêm dòng vào file:
   ```json
   "SourceConfig.SourceList.SourceField": "TargetConfig.TargetList.TargetField"
   ```
4. Nếu source field là raw string nhưng thực tế là parsed int[], thêm alias vào `FieldAlias` trong `ConfigValidatorWindow.cs`
5. Chạy **Tools > Config/Validator** để kiểm tra
