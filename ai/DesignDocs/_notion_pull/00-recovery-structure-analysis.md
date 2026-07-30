# 00-recovery-structure-analysis

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/00-recovery-structure-analysis.md

# 00 - Phân tích cấu trúc tham khảo từ Recovery

Nguồn tham khảo: `C:\Users\Hoang PC\Documents\Idle-rpg\Recovery`

Đích dự án: `C:\Users\Hoang PC\Documents\Idle-rpg\tdsurvivor\Assets`

> Giai đoạn này chỉ phân tích và thiết kế cấu trúc. Không copy file từ Recovery.

## 1. Các nhóm thư mục chính trong Recovery

```plain text
Assets/
├── AddressableAssetsData/
├── Excels/
├── Plugins/
├── Scenes/
├── Settings/
├── _GameToolkit/
├── _TDS/
└── _TDS assets/
```

## 2. Nhóm nên học theo

### `_GameToolkit/`

Bộ toolkit dùng chung, tách khỏi game cụ thể.

Các module đáng tham khảo:

```plain text
_GameToolkit/
├── Avoidance/      # né va chạm / mô phỏng agent
├── Collision/      # collision custom
├── GameConfig/     # load config data
├── Resource/       # asset loading, pooling
├── Startup/        # boot scene
├── Statistics/     # hệ chỉ số
├── Updater/        # update loop tập trung
└── Utils/          # tiện ích chung
```

Ý tưởng tốt:

- Tách code dùng chung khỏi code gameplay.
- Có `UpdaterOwner` để quản lý Tick thay vì mỗi object tự Update quá nhiều.
- Có `Pool` để spawn/despawn monster hiệu quả.
- Có `ConfigManager` để đọc dữ liệu monster/spawner từ config.
### `_TDS/`

Code gameplay riêng cho Tower Defense Survival.

```plain text
_TDS/
├── Boot/
├── GameConfig/
├── Gameplay/
├── Statistics/
└── Unit/
```

Ý tưởng tốt:

- `Boot`: khởi tạo game.
- `GameConfig`: định nghĩa dữ liệu monster/spawner.
- `Gameplay`: quản lý flow, wave, spawner.
- `Unit`: entity như monster, movement.
- `Statistics`: chỉ số dùng trong game.
### `_TDS assets/`

Asset riêng của game.

```plain text
_TDS assets/
├── Config/
├── Prefabs/
├── Scenes/
└── Textures/
```

Ý tưởng tốt:

- Tách asset game khỏi code.
- Config JSON nằm riêng.
- Prefab monster nằm theo nhóm.
## 3. Các thành phần không nên copy ngay

Không copy trực tiếp ở bước đầu:

```plain text
AddressableAssetsData/
Plugins/
Settings/
Scenes/*.unity
_TDS/*.cs
_GameToolkit/*.cs
_TDS assets/*.prefab
```

Lý do:

- Có thể lệch package/version Unity.
- File `.unity`, `.prefab`, `.meta` phụ thuộc GUID.
- Code cũ có namespace `_TDS`, cần đổi/thiết kế lại cho `TDSurvivor`.
- Cần xác định yêu cầu MVP trước khi đưa code vào.
## 4. Hướng áp dụng cho dự án mới

Dự án mới nên lấy kiến trúc, không copy nguyên file:

```plain text
Assets/
├── _TDSurvivor/
│   ├── Code/
│   │   ├── Boot/
│   │   ├── Core/
│   │   ├── Config/
│   │   ├── Gameplay/
│   │   ├── Units/
│   │   ├── Combat/
│   │   ├── Spawning/
│   │   ├── Stats/
│   │   ├── UI/
│   │   └── Utils/
│   └── Content/
│       ├── Config/
│       ├── Prefabs/
│       ├── Scenes/
│       ├── Sprites/
│       ├── Animations/
│       ├── Audio/
│       └── Materials/
├── _GameToolkit/       # chỉ tạo khi thật sự cần module dùng chung
└── _DesignDocs/
```

## 5. Mapping từ Recovery sang dự án mới

| Recovery | TDSurvivor mới | Ghi chú |

|---|---|---|

| `_TDS/Boot` | `_TDSurvivor/Code/Boot` | Khởi tạo game |

| `_TDS/GameConfig` | `_TDSurvivor/Code/Config` | Data class/config loader |

| `_TDS/Gameplay` | `_TDSurvivor/Code/Gameplay`, `Spawning` | Wave/spawn/game loop |

| `_TDS/Unit` | `_TDSurvivor/Code/Units` | Monster, Hero, Base |

| `_TDS/Statistics` | `_TDSurvivor/Code/Stats` | HP, damage, speed... |

| `_TDS assets/Config` | `_TDSurvivor/Content/Config` | JSON/ScriptableObject |

| `_TDS assets/Prefabs` | `_TDSurvivor/Content/Prefabs` | Monster/Hero/Projectile/Base |

| `_TDS assets/Scenes` | `_TDSurvivor/Content/Scenes` | Boot, Gameplay |

| `_GameToolkit/Resource/Pool` | `_GameToolkit/Resource` hoặc `Code/Core/Pooling` | Dùng sau MVP nếu cần |

| `_GameToolkit/Updater` | `_GameToolkit/Updater` hoặc `Code/Core/UpdateLoop` | Dùng sau khi prototype ổn |

## 6. Kết luận

Recovery có kiến trúc tốt theo hướng data-driven, pooling và update loop tập trung. Tuy nhiên dự án mới nên bắt đầu bằng bản thiết kế sạch, tạo requirement/test/estimate trước, sau đó mới quyết định module nào viết lại hoặc port sang.
