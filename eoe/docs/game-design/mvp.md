# MVP Gameplay Spec

## Trạng thái

**MVP contract v1 - đã chốt để implement.**

## Mục tiêu

Tạo một run ngắn trong đó người chơi:

1. Xếp hero/item trên Energy Circuit.
2. Quan sát hero tự động dọn lượng quái lớn.
3. Nhận gold và upgrade sau mỗi wave.
4. Chọn một Augment có tác động lớn ở checkpoint.
5. Điều chỉnh build để chuẩn bị cho boss.

## Game loop

```text
Start run
  ↓
Đặt hero/item vào circuit
  ↓
Wave combat
  ↓
Nhận gold và reward
  ↓
Nếu là checkpoint: chọn 1 trong 3 Augment
  ↓
Mở Shop
  ↓
Mua/bán/thay đổi hero hoặc item trên slot
  ↓
Wave tiếp theo
```

## Nhịp MVP

| Giai đoạn | Nội dung |
|---|---|
| Wave 1 | Quái cơ bản, giới thiệu pulse và stack |
| Sau wave 1 | Shop |
| Wave 2 | Tăng mật độ quái, kiểm tra Overdrive |
| Sau wave 2 | Shop |
| Wave 3 | Có elite hoặc modifier đơn giản |
| Sau wave 3 | Chọn 1 trong 3 Augment, sau đó Shop |
| Wave 4 | Kiểm tra synergy của build |
| Sau wave 4 | Shop, chuẩn bị boss |
| Wave 5 | Boss và kết thúc run |

## Board

- Tổng cộng 8 slot theo vòng logic một chiều.
- Mỗi slot chứa tối đa một hero, một item hoặc để trống; không có slot type riêng.
- Tối đa 3 hero active trên board trong MVP; không có bench.
- Hero/item không nằm trên board không tham gia combat.
- Slot trống vẫn cho pulse đi qua nhưng không tạo hiệu ứng.
- Thứ tự slot cố định trong run; người chơi chỉ thay đổi nội dung slot tại Shop.
- Bán hero/item trả 100% giá trong prototype và xóa content khỏi board.
- Thay content tại một slot phải hoàn tất trước khi wave mới bắt đầu.

## Combat

- Hero tự động tìm mục tiêu và tấn công.
- Monster spawn từ các portal và di chuyển về hero.
- Hero vẫn hoạt động bình thường khi chưa có Overdrive.
- Overdrive là power spike, không phải điều kiện để hero có thể gây damage.
- Wave kết thúc khi hết thời gian spawn và toàn bộ monster đã bị tiêu diệt.

## Progression trong run

### Gold

Gold nhận được từ:

- Hoàn thành wave.
- Elite kill.
- Boss reward.

Gold chỉ dùng trong run ở MVP. Chưa có currency meta giữa các run.

### Shop

Shop xuất hiện sau mỗi wave. Offer mặc định:

- 1 hero.
- 3 item.
- 1 upgrade trực tiếp hoặc utility offer.
- 1 lần refresh miễn phí.

MVP nên hoàn tiền 100% khi bán/thay item để dễ test build. Giá bán có thể giảm xuống 70% sau khi gameplay ổn định.

## Augment

- Xuất hiện sau wave 3 trong MVP.
- Cho 3 lựa chọn.
- Chọn 1 Augment và giữ hiệu lực đến hết run.
- Augment thay đổi luật hoặc tạo synergy; không chỉ là `+5% damage`.

Ví dụ:

- `Overcharge`: mỗi pulse thứ 4 cho thêm stack.
- `Conductive Circuit`: Overdrive truyền một phần stack sang hero kế bên.
- `Chain Reaction`: monster chết trong Overdrive có cơ hội phát nổ.
- `Scavenger`: elite cho thêm gold.

## Success criteria

- Người chơi hiểu pulse đang chạy qua slot nào.
- Người chơi biết hero sẽ Overdrive khi đủ stack.
- Một lựa chọn Shop hoặc Augment có thể thay đổi cách build.
- Wave combat tạo được cảm giác quái đông và chết nhanh trong power window.
- Boss buộc người chơi phải chuẩn bị build, không chỉ chờ auto-combat.
- Một run hoàn thành được trong khoảng 5-10 phút sau khi tuning.

## Ngoài phạm vi

- Điều khiển hero bằng joystick.
- PvP/multiplayer.
- Meta progression giữa nhiều run.
- Crafting hoặc merge item.
- Deck draw/discard đầy đủ.
- Branching map lớn.

## Contract v1 đã chốt

- Circuit hiển thị có thể là vòng tròn hoặc đường thẳng ở UI, nhưng thứ tự logic luôn là vòng 8 slot.
- Pulse chỉ chạy một chiều trong MVP.
- Hero và item có thể đặt vào mọi slot.
- Hero/item đã mua được giữ đến hết run hoặc bị bán; Shop không reset board giữa các wave.
