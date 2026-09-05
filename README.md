# Idle RPG

## Tóm tắt hiện trạng

Đây là game idle RPG 2D chạy trên Unity `6000.3.21f1`. Battle hiện dùng dữ liệu JSON được sinh từ Excel, Addressables để tải prefab và một tick loop 30 Hz để xử lý spawn, di chuyển, va chạm và projectile.

## Battle hiện có

### Luồng trận đấu

1. `EntryScene` tải config `Hero`, `Monster`, `Skill`, `Spawn`, `Exp`.
2. `GameplayScene` khởi tạo movement, skill runner và các hero trong slot.
3. `SpawnMonsterRunner` spawn quái theo level/wave từ `SpawnConfig`.
4. `AgentMovementRunner` đưa quái tới hero, dùng avoidance/RVO2 và xử lý đòn đánh theo cooldown.
5. Hero tự tìm mục tiêu trong tầm rồi bắn skill qua `SkillFactory`.
6. Khi hết quái của wave thì chuyển wave; hết wave cuối phát event win. Hero chết hết thì phát log thua.

### Dữ liệu hiện tại

- 1 hero: `101`, máu 20, attack 5, attack speed 0.5 đòn/giây, crit chance 50%, crit damage +100%, lifesteal 0%.
- 1 monster: `1001`, máu 10, attack 2.
- Level 1 có 5 wave: lần lượt 10, 15, 20, 25, 30 quái.
- 12 portal được chia theo từng wave.
- Có 10 skill config `2001`–`2010`, mỗi skill đã có prefab projectile tương ứng; coverage test kiểm tra prefab có `Projectile` và `CollisionDetector`.

### Cơ chế đã hỗ trợ trong code

- Object pool cho monster, projectile và death VFX.
- Chọn mục tiêu: gần nhất, xa nhất, HP thấp/cao nhất, attack thấp/cao nhất.
- Projectile bay thẳng hoặc spawn tại mục tiêu.
- Nhiều projectile theo spread góc hoặc parallel lane.
- Hit count, hit interval, collision delay và collision window.
- Critical hit và lifesteal: `critDamage = 1` nghĩa là đòn crit gây 2x damage; lifesteal là phần trăm damage thực nhận hồi cho hero.
- Modifier skill chạy bằng `ModifierSkillAction`: weighted selection + success rate cho slow, bleed, silence và stun.
- Slow/stun/silence đã tác động lên movement/đòn đánh của monster; bleed gây damage theo tick.
- `SkillModifierData`: `weight` dùng để chọn weighted modifier, `successRate` là xác suất áp dụng; `duration` tính bằng giây, `value` là % slow hoặc damage mỗi tick, `tickInterval` dùng cho bleed.
- Damage over time của projectile ở mức framework, cần kiểm tra lại config/implementation trước khi dùng production.
- Sự kiện wave clear, win, hero death và monster hit/death.

## Battle còn có thể làm

### P0 — Để battle chạy ổn định

- Sửa blocker build Player: tách `ExcelImporter`/NPOI khỏi runtime assembly và bỏ `using UnityEditor.XR` khỏi runtime code.
- Đồng nhất tên scene `GameplayScene`/`GamePlayScene`.
- Chờ `ConfigManager.Load()` hoàn tất trước khi activate gameplay scene.
- Sửa event death bị gọi lặp, validate thiếu level/wave/portal và thêm log lỗi có ngữ cảnh.

### P1 — Hoàn thiện một vertical slice chơi được

- Hiển thị thanh HP, damage number, animation hit/death và màn hình win/lose thay vì chỉ log.
- Trao EXP/gold khi monster chết; dùng `ExpConfig` để level up hero.
- Hỗ trợ đủ hero trong 4 slot; hiện config chỉ có một hero.
- Thêm pause, speed 1x/2x/4x và reset/replay battle.
- Thêm test cho damage, target selection, spawn timer, wave transition và điều kiện win/lose.

### P2 — Mở rộng chiến đấu

- Kiểm tra behavior thực tế của shotgun, rifle, sniper, magic bolt, AOE, chain lightning, poison cloud, heal và fireball.
- Nối các field skill đang có nhưng chưa dùng: cooldown, explosion, execute dưới ngưỡng HP, skill/ultimate riêng.
- Mở rộng trạng thái: poison, burn, knockback, resistance, stacking và icon/UI thời gian còn lại.
- Thêm nhiều loại monster: tank, ranged, healer, elite và boss; mỗi loại có behavior/target priority riêng.
- Thêm wave modifier và boss wave để tránh 5 wave hiện tại chỉ tăng số lượng quái.

### P3 — Đúng chất idle RPG

- Lưu đội hình, level, EXP, gold và tiến trình campaign.
- Offline progress: tính phần thưởng khi người chơi quay lại, có giới hạn thời gian và chống âm thời gian.
- Auto-advance campaign, reward chest, equipment/stat modifier và lựa chọn nâng cấp giữa các wave.
- Cân bằng battle theo DPS, thời gian clear wave, tỉ lệ sống sót và tốc độ tăng EXP/gold.

## Lưu ý kỹ thuật

`_TDS/Battle` là battle runtime đang được `GamePlayScene` dùng. `Gameplay2` cùng nhiều prefab trong `_Temp` là hệ weapon/UI cũ hoặc thử nghiệm; không nên mở rộng song song trước khi quyết định hệ nào là chính.
