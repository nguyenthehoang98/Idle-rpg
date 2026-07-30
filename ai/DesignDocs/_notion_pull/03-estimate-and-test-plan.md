# 03-estimate-and-test-plan

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/03-estimate-and-test-plan.md

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

```plain text
Mở Gameplay scene không báo lỗi Console.
Camera nhìn thấy Base ở giữa.
UI hiện HP và Wave.
```

### T02 - Monster spawn

Kỳ vọng:

```plain text
Monster xuất hiện từ 4 cạnh/spawn portals.
Số lượng monster đúng theo wave.
Không spawn ở giữa base.
```

### T03 - Monster movement

Kỳ vọng:

```plain text
Monster di chuyển về Base.
Monster không bị xoay/rụng khỏi mặt phẳng 2D.
Monster dừng/gây damage khi chạm Base.
```

### T04 - Hero auto attack

Kỳ vọng:

```plain text
Hero chỉ bắn khi có monster trong attackRange.
Hero chọn target gần nhất.
Attack cooldown đúng.
```

### T05 - Projectile hit

Kỳ vọng:

```plain text
Projectile bay tới target.
Projectile gây damage đúng một lần.
Projectile tự hủy/despawn sau khi hit hoặc target mất.
```

### T06 - Monster death

Kỳ vọng:

```plain text
Monster giảm HP khi bị bắn.
Monster chết khi HP <= 0.
Monster biến mất khỏi scene.
Không tiếp tục gây damage sau khi chết.
```

### T07 - Base damage/game over

Kỳ vọng:

```plain text
Monster chạm Base thì Base mất HP.
UI HP cập nhật.
Base HP <= 0 thì Game Over.
Sau Game Over, spawner và hero attack dừng.
```

### T08 - Wave complete

Kỳ vọng:

```plain text
Wave kết thúc khi spawn đủ và monster đã xử lý xong.
Wave tiếp theo bắt đầu sau delay.
Nếu hết wave thì Victory hoặc log Complete.
```

## 5. Acceptance Criteria MVP 0.1

MVP 0.1 được xem là đạt khi:

```plain text
- Chơi được 1 scene từ đầu đến Game Over/Victory.
- Không có lỗi đỏ trong Console.
- Có ít nhất 1 hero, 1 monster, 1 base, 1 wave.
- Quái spawn -> đi vào base -> bị hero bắn -> chết hoặc gây damage base.
- Code nằm đúng namespace TDSurvivor.
- Không phụ thuộc file copy trực tiếp từ Recovery.
```

## 6. Rủi ro kỹ thuật

```plain text
- Copy file Unity có thể vỡ GUID/meta.
- Addressables/ExcelExtension có thể thiếu package.
- Recovery dùng UniTask/Pool/Updater; nếu port sớm sẽ tăng phụ thuộc.
- Scene/prefab cũ có reference bị mất khi sang project mới.
```

Cách giảm rủi ro:

```plain text
- Prototype bằng MonoBehaviour thường trước.
- Sau khi gameplay chạy mới thêm Pool/Updater.
- Nếu cần tham khảo Recovery, chỉ đọc logic rồi viết lại namespace mới.
```

## 7. Thứ tự triển khai đề xuất sau khi duyệt

```plain text
Step 1 - Tạo folder `_TDSurvivor` theo file 02.
Step 2 - Tạo script skeleton MVP 0.1.
Step 3 - Tạo Gameplay scene thủ công hoặc bằng editor script.
Step 4 - Tạo prefab placeholder bằng Sprite đơn giản.
Step 5 - Test loop spawn/move/attack/damage.
Step 6 - Sau khi ổn mới xem xét port Pool/Updater từ Recovery.
```
