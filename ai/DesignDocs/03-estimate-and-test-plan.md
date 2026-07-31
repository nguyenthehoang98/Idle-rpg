# 03 - Estimate và Test Plan

## 1. Giả định

- Unity 2D project đã mở được.
- Dự án hiện tại `Assets` gần như trống.
- Chưa copy code/prefab từ Recovery.
- Bước tiếp theo là duyệt thiết kế, sau đó mới tạo folder/code skeleton.

## 2. Estimate MVP 0.1

| Hạng mục | Việc cần làm | Ước lượng |
|---|---|---:|
| Thiết kế cấu trúc | Docs, folder proposal, requirement | 0.5 ngày |
| Setup folder | Tạo `_TDSurvivor`, thư mục code/content | 0.25 ngày |
| Core gameplay | GameManager, Health, BaseCore | 0.5 ngày |
| Monster | MonsterController, HP, move to base | 0.5 ngày |
| Hero attack | Auto target, shoot projectile | 0.5 ngày |
| Projectile | Bay tới target, gây damage | 0.25 ngày |
| Spawner/Wave | Spawn 4 hướng, wave đơn giản | 0.5 ngày |
| UI | HP base, wave text, game over | 0.5 ngày |
| Scene setup | Gameplay scene + prefab cơ bản | 0.5 ngày |
| Test/fix | Chạy thử, fix lỗi | 0.5 ngày |

Tổng MVP 0.1: khoảng `4 - 5 ngày` nếu làm cẩn thận.

## 3. Estimate MVP 0.2

| Hạng mục | Ước lượng |
|---|---:|
| Object Pool | 0.5 ngày |
| Config data Hero/Monster/Wave | 1 ngày |
| Nhiều Monster | 0.5 ngày |
| Nhiều Hero | 1 ngày |
| Upgrade sau wave | 1 - 1.5 ngày |
| Pause/Resume | 0.25 ngày |
| Polish basic | 1 ngày |

Tổng MVP 0.2: khoảng `5 - 6 ngày`.

## 4. Test Plan MVP 0.1

### T01 - Scene load

Kỳ vọng:
```text
Mở Gameplay scene không báo lỗi Console.
Camera nhìn thấy Base ở giữa.
UI hiện HP và Wave.
```

### T02 - Monster spawn

Kỳ vọng:
```text
Monster xuất hiện từ 4 cạnh/spawn portals.
Số lượng monster đúng theo wave.
Không spawn ở giữa base.
```

### T03 - Monster movement

Kỳ vọng:
```text
Monster di chuyển về Base.
Monster không bị xoay/rụng khỏi mặt phẳng 2D.
Monster dừng/gây damage khi chạm Base.
```

### T04 - Hero auto attack

Kỳ vọng:
```text
Hero chỉ bắn khi có monster trong attackRange.
Hero chọn target gần nhất.
Attack cooldown đúng.
```

### T05 - Projectile hit

Kỳ vọng:
```text
Projectile bay tới target.
Projectile gây damage đúng một lần.
Projectile tự hủy/despawn sau khi hit hoặc target mất.
```

### T06 - Monster death

Kỳ vọng:
```text
Monster giảm HP khi bị bắn.
Monster chết khi HP <= 0.
Monster biến mất khỏi scene.
Không tiếp tục gây damage sau khi chết.
```

### T07 - Base damage/game over

Kỳ vọng:
```text
Monster chạm Base thì Base mất HP.
UI HP cập nhật.
Base HP <= 0 thì Game Over.
Sau Game Over, spawner và hero attack dừng.
```

### T08 - Wave complete

Kỳ vọng:
```text
Wave kết thúc khi spawn đủ và monster đã xử lý xong.
Wave tiếp theo bắt đầu sau delay.
Nếu hết wave thì Victory hoặc log Complete.
```

## 5. Automated Tests (Edit Mode)

Bổ sung từ 2026-07-31. Unity Test Framework (`com.unity.test-framework`) + NUnit.

### Nguyên tắc

```text
- Test pure logic: Spu, BaseAction, Trajectory, SpawnTimer, Stat.
- Không test MonoBehaviour/visual trong Edit Mode.
- Test file đặt cạnh code: Assets/_GameToolkit/SkillSystem/Tests/.
- Mỗi test assembly dùng asmdef riêng với defineConstraints UNITY_INCLUDE_TESTS.
```

### Hiện có

| Test file | Nội dung | Trạng thái |
|---|---|---|
| `SkillSystem/Tests/BaseActionTests.cs` | Lifecycle: Start/Tick/Interrupt/Stop/IsFinished/Reason (22 cases) | Sẵn sàng |
| `SkillSystem/Tests/SpuTests.cs` | Command queue, add/remove action, skill grouping, id reuse (9 cases) | Sẵn sàng |
| `SkillSystem/Tests/TrajectoryTests.cs` | Projectile/Boomerang/Stationary vị trí theo thời gian (7 cases) | Sẵn sàng |
| `SkillSystem/Tests/CastProjectileActionTests.cs` | maxHitCount, hit cooldown, DoT interval, OnComplete (7 cases) | Sẵn sàng |
| `_TDS/Tests/SpawnTimerTests.cs` | Spawn count trong window, IsFinished (8 cases) | Sẵn sàng |
| `_TDS/Tests/StatTests.cs` | Modifier stacking Flat/PercentAdd/PercentMult (11 cases) | Sẵn sàng |

### Cần bổ sung (khi code ổn định)

```text
- HealthTests: TakeDamage/Heal/death edge cases.
- EntityQueryTests: query circle/rect, filter, nearest (cần mock AgentSimulator).
```

### Cách chạy

```text
CLI (không cần mở Editor):
Unity.exe -batchmode -nographics -runTests -projectPath "tdsurvivor" \
  -testPlatform EditMode -testResults "<path>/test-results.xml"

Unity Editor: Window > General > Test Runner > EditMode > Run All
```

Gate: không có test fail trước khi coi task xong.

## 6. Acceptance Criteria MVP 0.1

MVP 0.1 được xem là đạt khi:

```text
- Chơi được 1 scene từ đầu đến Game Over/Victory.
- Không có lỗi đỏ trong Console.
- Có ít nhất 1 hero, 1 monster, 1 base, 1 wave.
- Quái spawn -> đi vào base -> bị hero bắn -> chết hoặc gây damage base.
- Code nằm đúng namespace _GameToolkit.* / _TDS.*.
- Edit Mode tests pass (SkillSystem + Spu + BaseAction).
- Không phụ thuộc file copy trực tiếp từ Recovery.
```

## 7. Rủi ro kỹ thuật

```text
- Copy file Unity có thể vỡ GUID/meta.
- Addressables/ExcelExtension có thể thiếu package.
- Recovery dùng UniTask/Pool/Updater; nếu port sớm sẽ tăng phụ thuộc.
- Scene/prefab cũ có reference bị mất khi sang project mới.
- SplineTrajectory phụ thuộc package com.unity.splines (đã thêm 2.9.0).
```

Cách giảm rủi ro:

```text
- Prototype bằng MonoBehaviour thường trước.
- Sau khi gameplay chạy mới thêm Pool/Updater.
- Nếu cần tham khảo Recovery, chỉ đọc logic rồi viết lại namespace mới.
```

## 8. Thứ tự triển khai đề xuất sau khi duyệt

```text
Step 1 - Giữ folder _GameToolkit/_TDS/_TDS assets theo file 02.
Step 2 - Tạo script skeleton MVP 0.1.
Step 3 - Tạo Gameplay scene thủ công hoặc bằng editor script.
Step 4 - Tạo prefab placeholder bằng Sprite đơn giản.
Step 5 - Test loop spawn/move/attack/damage (manual T01-T08).
Step 6 - Chạy Edit Mode automated tests (SkillSystem).
```
