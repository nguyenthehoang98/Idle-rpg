# Game Design

## Trạng thái

**Draft - làm nguồn tham chiếu trước khi implement.**

Các mục ghi là đề xuất hoặc **Open questions** vẫn cần được xác nhận trước khi code.

## Tầm nhìn

Một game **Auto-Survivor RPG**: người chơi không cần điều khiển hero liên tục, nhưng phải xây dựng đội hình và mạch năng lượng đúng cách để tạo ra các đợt **Overdrive** đủ mạnh tiêu diệt lượng quái rất lớn.

Trải nghiệm cốt lõi:

> Chuẩn bị build → xem hero tự động chiến đấu → thấy power spike rõ ràng → đưa ra quyết định mới sau mỗi wave.

## Các trụ cột gameplay

1. **Horde destruction** - spawn nhiều quái, projectile và hiệu ứng phải tạo cảm giác dọn quái rõ ràng.
2. **Meaningful decisions** - mỗi lần mua, đổi slot hoặc chọn Augment phải thay đổi hướng build.
3. **Energy Circuit** - hero/item nằm trên các slot cố định; pulse chạy qua và tích stack.
4. **Run-based progression** - Shop sau wave và Augment tại các checkpoint tạo khác biệt giữa các run.
5. **Readable power** - Overdrive cần có hiệu ứng, âm thanh và kết quả nhìn thấy ngay.

## Phạm vi MVP

- 5 wave, wave 5 là boss.
- Shop mở sau mỗi wave.
- Sau wave 3 chọn 1 trong 3 Augment.
- 8 Energy Circuit slot.
- 2 hero active.
- Item có thể thay đổi vị trí trên circuit.
- Pulse chạy tuần tự qua các slot.
- Slot đủ stack sẽ kích hoạt Overdrive tạm thời.
- Có gold trong run để mua hero/item.
- Có một nhóm nhỏ skill, item và Augment có synergy.

## Tài liệu

- [MVP gameplay spec](mvp.md)
- [Energy Circuit](energy-circuit.md)
- [Shop và Augments](shop-and-augments.md)
- [Implementation plan](../../tasks/plan.md)
- [Task checklist](../../tasks/todo.md)
- [Remote verification](../engineering/remote-verification.md)

## Không làm trong MVP

- PvP hoặc multiplayer.
- Meta progression ngoài run.
- Crafting, rarity phức tạp, deckbuilding đầy đủ.
- Map branching lớn.
- Điều khiển hero trực tiếp.
- Hàng chục loại hero/item chỉ để tăng số lượng content.
