# Idle RPG — Tiến trình dự án

Cập nhật: 2026-09-04
Branch: `feature/ai-1`
Unity: `6000.3.21f1`

## Đã hoàn thành

### Battle core

- Spawn monster theo level/wave/portal từ `SpawnConfig`.
- Movement và avoidance bằng RVO2.
- Monster tấn công hero theo attack range và cooldown.
- Hero tự tìm target và cast projectile skill.
- Object pool cho monster, projectile và VFX.
- Wave clear, win và lose event/log cơ bản.

### Chỉ số hero

- Attack, attack speed, attack range, max health.
- Critical chance.
- Critical damage: `1.0` là thêm 100%, tổng damage crit = 2x.
- Lifesteal: phần trăm damage thực nhận hồi lại cho hero.
- Config được đồng bộ trong `HeroConfig.xlsx` và `HeroConfig.json`.

### Skill modifiers

`ModifierSkillAction` hỗ trợ các hiệu ứng:

- `Slow`: giảm tốc độ di chuyển theo `value`.
- `Bleed`: gây damage theo `value` và `tickInterval`.
- `Silence`: monster tạm thời không tấn công.
- `Stun`: monster dừng di chuyển và không tấn công.

Mỗi skill có thể có nhiều modifier trong `SkillConfigData.modifiers`:

- `weight`: trọng số chọn modifier.
- `successRate`: xác suất modifier được áp dụng.
- `duration`: thời gian hiệu ứng.
- `value`: cường độ slow hoặc damage mỗi tick.
- `tickInterval`: chu kỳ bleed.

Skill `2001` hiện có ví dụ slow 40%, thời gian 2 giây, tỉ lệ thành công 50%.

## Verification

- Unity script compilation: pass.
- `git diff --check`: pass.
- Đã thêm test logic cho crit, lifesteal và weighted modifier selection.
- Full Windows Player build hiện chưa pass do các lỗi tồn tại trước đó:
  - `ExcelImporter`/NPOI đang nằm trong runtime assembly.
  - `RigidbodyComponents.cs` import `UnityEditor.XR` trong runtime code.

## Commit gần nhất

- `181e5e19 feat: add skill status modifiers`
- `a888e074 feat: add critical hit and lifesteal stats`

## Việc tiếp theo

1. Sửa blocker build Player: tách Excel Editor assembly và loại `UnityEditor.XR` khỏi runtime.
2. Chạy battle runtime và kiểm tra slow/bleed/silence/stun trên prefab thật.
3. Thêm UI icon, thời gian còn lại và feedback khi modifier thành công.
4. Thêm EXP/gold, reward và màn hình win/lose thật.
5. Tạo thêm prefab projectile cho các skill còn lại.
6. Thêm boss, elite, resistance và stacking rule cho status.
7. Thêm save game và offline progress.

## Trạng thái working tree

`eoe/eoe.sln` đang có thay đổi chưa commit từ trước và được cố ý giữ ngoài các commit battle gần đây.
