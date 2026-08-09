# Note — Đánh giá bộ skill Pi vs quy trình làm game (2025)

> Ngày: 2025-08-06
> Phạm vi: đánh giá đọc-only, không sửa gì trong dự án.

## 1. Bức tranh dự án Idle-rpg

Repo chứa 3 thứ trộn chung:

- `tdsurvivor/` — **Unity project chính**: TDSurvivor, 2D top-down Tower Defense / Survival
  (hero tự bắn, monster spawn từ 4 cạnh, wave, base HP).
  ~22k files, 538 file C# (đa số là plugins: GUPS AntiCheat, LitMotion, UniTask, RVO2, ExcelExtension).
- `ai/DesignDocs/` — pipeline agent-skills riêng: interview → spec → technical design → plan →
  quality gates → notion-sync, kèm 6 ADRs và backlog tasks.
- `Recovery 2/` + `notion/` — project Unity cũ + tools sync.

Điểm đáng chú ý: dự án đã tham khảo **addyosmani/agent-skills** (không phải Superpowers)
và đã tự viết flow + quality gates riêng cho game.

## 2. Đánh giá bộ 28 skill theo giai đoạn làm game

| Giai đoạn | Skill có | Đánh giá trên dự án này |
|---|---|---|
| Làm rõ ý tưởng | interview-me, idea-refine, spec-driven-development | ✅ Rất tốt — flow `01-interview` + Gate 1 (7 câu hỏi: player role, core loop, MVP scope, thắng/thua) đúng chuẩn |
| Spec | spec-driven-development | ✅ Có `01-requirements.md` (MVP 0.1 với R01–R14 rõ ràng) |
| Thiết kế kỹ thuật | api-and-interface-design | ✅ ADR-0001→0007 đã dùng |
| Lập kế hoạch | planning-and-task-breakdown, incremental-implementation | ✅ Có `tasks/000-design-backlog.md` |
| **Code C# / Unity** | **KHÔNG CÓ** | ❌ **Lỗ hổng lớn nhất** — `dignified-python` (chỉ Python) và `dagster-expert` (data pipeline) đều không dùng được. Không skill nào hướng dẫn Unity API, asmdef, MonoBehaviour lifecycle, Addressables patterns |
| Test | test-driven-development | ⚠️ Có skill nhưng **dự án hiện có 0 test** (chỉ có test của LitMotion). TDD cứng nhắc khó áp dụng cho Unity |
| Debug | debugging-and-error-recovery | ✅ Cần thiết — commit history có "fixed", "refactor script 1/2" |
| Review | code-review-and-quality, code-simplification | ✅ Git có nhiều branch (ai/tower-defense, develop, feature/new-system) |
| Git | git-workflow-and-versioning | ✅ Dùng tốt |
| Performance | performance-optimization | ✅ Rất cần — game đã có object pool, avoidance grid, Burst |
| Security | security-and-hardening | ✅ Project có GUPS AntiCheat |
| UI/UX | frontend-ui-engineering, ux-review | ⚠️ frontend-ui-engineering thiên web (WCAG) — Unity uGUI/TMP không được cover; ux-review OK về nguyên tắc |
| Build/release mobile | ci-cd-and-automation, shipping-and-launch | ⚠️ Thiên web — Addressables bundles, Android/iOS build, store release không có hướng dẫn cụ thể |
| Docs | documentation-and-adrs | ✅ Dùng rất tốt |
| **Game design / balance** | **KHÔNG CÓ** | ❌ Không skill nào cover DPS math, economy, difficulty curve, progression |

**Kết luận: bộ skill đáp ứng ~65–70% quy trình làm game full.**

## 3. Ba phát hiện quan trọng

1. **Thủ phạm nghiêm trọng**: `ai/DesignDocs/SKILL.md` là **dagster-expert bị copy nhầm vào**
   — hoàn toàn không liên quan Unity (nội dung về `dg CLI`, Dagster Plus...).
   Đây là skill sai chỗ, khiến agent có thể bị nhiễu khi đọc docs của dự án.

2. **Dự án tự có thứ Superpowers không có**: flow `00-agent-skills-integration.md` đã chuyển thể đúng cho game:
   > "Gameplay intent before prefab/code. Player experience before system design.
   > Acceptance criteria before implementation. **Prototype slice before full architecture.
   > Unity Play Mode proof before expanding scope.**"

   Phần này hay hơn Superpowers — Superpowers generic, không hiểu game,
   còn đây đã có nguyên tắc game-first.

3. **Điểm chết của Superpowers khi áp vào đây**: skill `test-driven-development` của Superpowers
   **bắt buộc RED→GREEN, xóa code viết trước test**. Dự án này đang ở giai đoạn build core systems
   (branch `ai/tower-defense`) với 0 test — nếu cài Superpowers vào lúc này, nó sẽ đòi viết test trước
   mọi thứ, trong khi phần lớn code Unity (prefab wiring, Play Mode feel) không test được kiểu đó.
   **Cài bây giờ sẽ phá quy trình đang chạy.**

## 4. Khuyến nghị

- **Không cài Superpowers vào dự án này** — đã có flow riêng tốt hơn;
  Superpowers chỉ lợi nếu làm dự án code thuần không phải game.
- **Nên thêm** 1–2 skill tùy biến riêng:
  - `unity-csharp` (Unity API + asmdef + patterns: Addressables, object pool, Editor tests)
  - `game-balance` (DPS/economy/wave math)
  - Đây là 2 lỗ hổng thật sự.
- **Xóa/thay** `ai/DesignDocs/SKILL.md` (dagster nhầm) bằng skill Unity nói trên.
