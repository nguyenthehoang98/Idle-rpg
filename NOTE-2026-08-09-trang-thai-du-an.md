# NOTE — Trạng thái dự án Idle-rpg (2026-08-09)

> Ghi chú tổng quan cho phiên làm việc tiếp theo. Đọc nhanh: phần 2 và 3.

## 1. Bức tranh tổng thể

| Thành phần | Vai trò |
|---|---|
| `tdsurvivor/` | **Unity project chính** (TDSurvivor — 2D top-down Tower Defense / Survival, hero tự bắn, monster spawn 4 cạnh, wave, base HP). Unity 6 URP 2D. |
| `ai/DesignDocs/` | Pipeline thiết kế: interview → spec → design → plan → quality gates → ADR → notion-sync. |
| `Recovery 2/` + `notion/` | Unity project cũ (chỉ để **tham chiếu**) + tool sync Notion. |
| `ai/AGENTS.md` | Quy tắc làm việc: **không nhảy thẳng từ ý tưởng sang code**, phải qua flow đầy đủ; Recovery chỉ là reference. |

- Nhánh đang làm: **`ai/tower-defense`** (đã push origin). Repo có nhiều branch cũ khác (develop, feature/new-system, echo/*...) — chưa rõ còn dùng không.
- Commit gần nhất: `941b792af weapon config` (2026-08-09) — thêm hệ thống WeaponConfig/Weapon.
- GitHub Actions không thấy; test Edit Mode nói trong AGENTS.md nhưng **chưa có test nào trong dự án** (chỉ có test của plugin LitMotion).

## 2. Đã làm được gì (đang chạy được)

Theo `ai/DesignDocs/flow/04-plan.md` — flow đã chạy được: Boot → Load config → Spawn wave → Monster RVO di chuyển.

| Hệ thống | Trạng thái |
|---|---|
| T001 Setup Unity 6 URP 2D project | ✅ |
| T002 Import core modules (AgentSimulator, IGrid, Pool, AssetManager) | ✅ |
| T008 EnemySpawner từ SpawnerConfig (spawn theo wave) | ✅ |
| T009 RVO movement (AgentSimulator + MonsterMoveUpdater) | ✅ |
| T017 Config loader Excel → JSON (MonsterConfig, SpawnerConfig) | ✅ |
| Weapon/Skill hệ thống lớn (WeaponConfig 341 dòng, Weapon 676 dòng, SkillManager 582 dòng, Projectile, cast skill, crit, upgrade) | ✅/🟡 đang phát triển — commit 09/08 |

Scene đã có: `_TDS assets/Scenes/BootScene.unity`, `GamePlayScene.unity`.
Config thật: `Excels/MonsterConfig.xlsx`, `SpawnerConfig.xlsx` → JSON trong `_TDS assets/Config/`.

## 3. Còn thiếu / cần làm tiếp

### Theo plan (slice còn dở)
- **BaseCore + GameManager state machine + Health → GameOver** (T003-T005) — đã có `HealthComponent` struct (int Max/Current/Predicted) nhưng **chưa có BaseCore, chưa có GameManager, monster chưa damage Base, chưa có GameOver/Victory**.
- **Monster 3 tier tách riêng** (Mob/Elite/Boss) — hiện dùng 1 Monster + scale trong config.
- **Hero 5 loại** (2 Archer, 2 Magic, 1 Buff/Control) — mới có Weapon auto-attack, chưa thấy HeroController, chưa có force attack direction, chưa có Passive/Active skills hoàn chỉnh.
- **Roll/Shop UI + Coin/Exp runtime** — Stat framework (StatId/Stats/StatModifier) có, chưa có Coin/Exp.
- **Track alive monsters + Victory flow.**
- **UI cơ bản** (Base HP, Wave, Coin, Level, GameOver/Victory screen) — chưa thấy Canvas/UI gameplay trong Assets.

### Theo spec (SC01-SC15, approved 2026-07-31)
Đạt một phần: SC02, SC03, SC06 (monster chết). Chưa đạt: SC04 (force direction), SC05 (projectile hit qua IGrid — mới có SkillSystem projectile riêng), SC07-SC15 (damage Base, GameOver, Victory, Roll/Shop, Coin/Exp, skills, 3 tier).

### Theo AGENTS.md (chất lượng)
- **0 test** mặc dù AGENTS.md yêu cầu chạy Edit Mode tests (Spu, BaseAction, timers) trước khi coi task xong.
- `ai/DesignDocs/SKILL.md` là **dagster-expert bị copy nhầm** — không liên quan Unity, nên xóa/thay (đã ghi trong `note-2025-skills-assessment.md`).
- Không có Gameplay scene được verify thủ công theo checklist trong AGENTS.md.

## 4. Đề xuất thứ tự làm tiếp

```text
1. Hoàn thiện Slice 2: BaseCore + Health + monster damage Base → GameOver (T003-T005, SC07/SC08/SC09)
2. Track alive monsters + WaveManager victory → Victory screen (SC10)
3. Slice 3 Heroes: HeroController + 5 loại + force direction (SC04)
4. Projectile IGrid hit (SC05) — đối chiếu với SkillSystem đã có
5. Roll/Shop + Coin/Exp (SC11-SC13)
6. UI hoàn chỉnh (SC08) + 3 monster tier (SC15)
7. Bổ sung Edit Mode tests theo AGENTS.md
```

## 5. Bản đồ tài liệu (chỗ nào có gì)

| File | Nội dung |
|---|---|
| `README.md` | Chỉ có tiêu đề "Idle-rpg" (chưa có nội dung) — **nên viết lại** |
| `ai/AGENTS.md` | Quy tắc làm việc + lệnh chạy test (bắt buộc đọc trước khi sửa code) |
| `ai/DesignDocs/flow/00-agent-skills-integration.md` | Cách tích hợp agent-skills vào làm game |
| `ai/DesignDocs/flow/01-interview.md` | Kết quả interview (quyết định gameplay) |
| `ai/DesignDocs/flow/02-spec.md` | **Spec MVP 0.1** (approved) — SC01-SC15, scope, rules |
| `ai/DesignDocs/flow/03-technical-design.md` | **Thiết kế kỹ thuật** (approved) — GameManager, Health, BaseCore, HeroController... |
| `ai/DesignDocs/flow/04-plan.md` | **Plan + trạng thái slice** (cập nhật mỗi lần làm) |
| `ai/DesignDocs/flow/05-quality-gates.md` | Các cổng chất lượng (interview/spec/design/build) |
| `ai/DesignDocs/flow/06-notion-sync.md` | Sync tài liệu với Notion |
| `ai/DesignDocs/adrs/ADR-0001→0005, 0007` | Quyết định kiến trúc (project structure, config pipeline, pool, asset loading, platform, skill system) |
| `ai/DesignDocs/todo.md` | **Danh sách đã chốt + còn chờ** (nơi duy nhất ghi trạng thái tổng) |
| `ai/DesignDocs/tasks/000-design-backlog.md` | Backlog thiết kế (D001-D006) + chỗ trống cho task build (B001-B007) |
| `ai/DesignDocs/01-requirements.md` | Yêu cầu chi tiết MVP 0.1 (R01-R14) / 0.2 / Alpha |
| `ai/DesignDocs/02-proposed-folder-structure.md` | Cấu trúc folder đề xuất |
| `ai/DesignDocs/03-estimate-and-test-plan.md` | Ước lượng + kế hoạch test |
| `ai/DesignDocs/note-2025-skills-assessment.md` | Đánh giá bộ skill Pi cho làm game + phát hiện SKILL.md bị copy nhầm |
| `ai/DesignDocs/_notion_pull/` | Bản pull từ Notion (bản sao, đừng sửa trực tiếp) |
| `Recovery 2/_KITSystem/SkillSystem/REVIEW_SkillSystem.md` | Đánh giá SkillSystem của project cũ (để tham khảo thiết kế) |

## 6. Lưu ý khi làm tiếp

- Đọc `ai/AGENTS.md` trước khi sửa code — phải chạy Edit Mode tests sau mỗi task.
- Recovery/Recovery 2 chỉ là **tham chiếu**, không copy file `.prefab/.unity/.meta` mù.
- Trước khi thêm package/plugin mới hoặc đổi project settings → hỏi trước.
- Cập nhật `flow/04-plan.md` + `todo.md` sau mỗi slice.
- Câu hỏi mở: nhánh `master` lạc hậu so với `ai/tower-defense` — cần quyết định merge strategy; README trống.

---

## 7. CẬP NHẬT: Discord Control + Statistics (2026-08-09)

Thay vì cải thiện Unity, đã xây **quy trình điều khiển pi qua Discord + thống kê** (bỏ phương án Telegram).

### Cấu trúc (đặt tại `~/.pi/agent/extensions/`)

| File | Vai trò |
|---|---|
| `pi-stats.ts` | Extension pi: lắng nghe `turn_end`/`tool_call`/`agent_settled` → ghi `~/.pi/agent/extensions/pi-stats.json` (token, cost, thời gian, feature, summary). Thêm lệnh `/stats` trong TUI. |
| `discord_bot/discord_bot.py` | Bot Discord: nhận tin nhắn → gọi pi qua RPC (`pi --mode rpc` `prompt`) → gửi kết quả stream về Discord. |
| `discord_bot/.env.example` | Mẫu cấu hình: `DISCORD_BOT_TOKEN`, `DISCORD_ALLOWED_CHANNEL_IDS`, `PI_RPC_CMD`, `PI_PROJECT_DIR`. |
| `.gitignore` | Chặn commit `.env` (bot) |
| `start-discord-bot.bat` | Khởi động bot |

### Lệnh trong Discord

```
!prompt <tin>  gửi tin vào pi
!stats         token/cost/thời gian/feature
!summary       tóm tắt phiên làm việc
!status        trạng thái bot
!bash <cmd>    chạy lệnh local (không qua pi)
!cd <dir>      đổi project dir
!help          danh sách lệnh
```

### Feature auto-tag (trong pi-stats.ts)

Từ khóa → tag: `basecore-gamemanager`, `monster-wave-system`, `hero-weapon-skill`, `roll-shop-economy`, `ui`, `statistics`, `discord-control`... Mỗi turn ghi `features[]`, tổng hợp ở `!summary`.

### Cách chạy

1. Tạo bot Discord: https://discord.com/developers/applications → New Application → Bot → Reset Token → copy token.
2. Tạo `discord_bot/.env` từ `.env.example`, điền `DISCORD_BOT_TOKEN`.
3. Cài `pip install discord.py`.
4. Chạy `start-discord-bot.bat` (hoặc `python discord_bot.py`).
5. Pi phải chạy với extension `pi-stats` (tự động load từ `~/.pi/agent/extensions/`) → mở `/stats` trong TUI để xem thống kê.
