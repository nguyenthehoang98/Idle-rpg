# 01-requirements

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/01-requirements.md

# 01 - Yêu cầu dự án TDSurvivor

## 1. Tổng quan

Tên tạm: `TDSurvivor`

Thể loại:

- Unity 2D
- Tower Defense / Survival Defense
- Top-down
Ý tưởng:

- Nhóm nhân vật đứng gần trung tâm màn hình.
- Quái vật xuất hiện từ nhiều hướng xung quanh.
- Quái tiến vào trung tâm để tấn công căn cứ/nhóm nhân vật.
- Nhân vật tự động tấn công quái trong tầm.
- Người chơi sống sót qua các wave, nâng cấp nhân vật và mở thêm nội dung.
## 2. MVP 0.1 - Bắt buộc có trước

Mục tiêu: có vòng gameplay chạy được trong Unity.

Yêu cầu:

```plain text
R01 - Có scene Gameplay 2D.
R02 - Có Base/Core ở giữa màn hình.
R03 - Có 1 Hero đứng gần Base.
R04 - Có 1 loại Monster.
R05 - Monster spawn từ 4 cạnh màn hình.
R06 - Monster tự di chuyển về Base.
R07 - Hero tự tìm Monster gần nhất trong range.
R08 - Hero bắn Projectile vào Monster.
R09 - Projectile gây damage.
R10 - Monster có HP, chết khi HP <= 0.
R11 - Monster chạm Base thì gây damage cho Base và biến mất.
R12 - Base có HP, Game Over khi HP <= 0.
R13 - Có UI hiển thị HP Base, Wave hiện tại.
R14 - Có WaveManager đơn giản: spawn N quái theo thời gian.
```

## 3. MVP 0.2 - Sau khi 0.1 ổn định

```plain text
R15 - Có nhiều loại Monster: Normal, Fast, Tank.
R16 - Có nhiều Hero: Archer, Mage.
R17 - Có upgrade sau mỗi wave.
R18 - Có data config cho Monster/Wave/Hero.
R19 - Có object pool cho Monster/Projectile.
R20 - Có Pause/Resume.
```

## 4. Alpha

```plain text
R21 - Boss wave.
R22 - Skill chủ động.
R23 - EXP/Level cho đội hình.
R24 - Loot/vàng.
R25 - Meta upgrade ngoài trận.
R26 - Save/load progress.
R27 - Main menu, stage select, result screen.
```

## 5. Entity chính

### Base/Core

```plain text
Fields:
- maxHp
- currentHp
- defense

Functions:
- TakeDamage(amount)
- Die()
- OnHpChanged event
```

### Hero

```plain text
Fields:
- id
- name
- maxHp
- damage
- attackRange
- attackCooldown
- projectileSpeed

Functions:
- FindTarget()
- Attack(target)
- Upgrade(stat)
```

### Monster

```plain text
Fields:
- id
- name
- maxHp
- currentHp
- attack
- moveSpeed
- expReward
- goldReward
- radius

Functions:
- Initialize(config, runtimeData)
- MoveToBase()
- TakeDamage(amount)
- Die()
- ReachBase()
```

### Projectile

```plain text
Fields:
- damage
- speed
- target
- lifetime

Functions:
- Initialize(target, damage, speed)
- Move()
- Hit()
```

## 6. Data config cần có

Ưu tiên ban đầu dùng ScriptableObject hoặc JSON đơn giản.

```plain text
HeroConfig:
- id
- assetPath/prefab
- name
- hp
- damage
- attackRange
- attackCooldown
- projectileSpeed

MonsterConfig:
- id
- assetPath/prefab
- name
- hp
- attack
- moveSpeed
- exp
- radius

WaveConfig:
- level
- wave
- monsterId
- totalMonster
- spawnStartTime
- spawnEndTime
- spawnAreaRadius
- spawnPortals
- healthScale
- attackScale
- speedScale
```

## 7. Điều kiện thắng/thua

MVP 0.1:

```plain text
Thua: Base HP <= 0.
Thắng: Hoàn thành tất cả wave được cấu hình trong scene.
```

## 8. Nguyên tắc kỹ thuật

```plain text
- Không copy nguyên code Recovery ở bước đầu.
- Thiết kế namespace mới: TDSurvivor.
- Tạo cấu trúc thư mục sạch trước.
- Code MVP càng ít phụ thuộc plugin càng tốt.
- Chỉ thêm Addressables/Pool/Updater sau khi prototype chạy ổn.
- Mọi file cấu hình phải có mô tả rõ.
```
