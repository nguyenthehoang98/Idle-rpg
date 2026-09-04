# MVP Task Checklist

## Progress

- [x] Gameplay design docs
- [ ] Circuit simulation
- [ ] Combat integration
- [ ] Shop loop
- [ ] Augment checkpoint
- [ ] Content and tuning

**Current status:** Design complete; implementation chưa bắt đầu.

**Priority:** P0 = bắt buộc cho MVP, P1 = làm sau khi core loop chạy được.

## Phase 1 - Circuit contract

- [ ] **P0** Chốt layout 8 slot: vòng tròn, line hoặc grid.
- [ ] **P0** Chốt pulse interval, stack threshold và Overdrive duration ban đầu.
- [ ] **P0** Định nghĩa data cho circuit slot.
- [ ] **P0** Định nghĩa data cho hero Overdrive.
- [ ] **P0** Viết test pulse đi qua đúng thứ tự.
- [ ] **P0** Viết test slot kích hoạt khi đủ stack.
- [ ] **P0** Viết test slot không kích hoạt khi chưa đủ stack.

## Phase 2 - Combat integration

- [ ] **P0** Tạo board 8 slot và trạng thái run.
- [ ] **P0** Đặt hero hiện có vào board.
- [ ] **P0** Tạo Generator, Amplifier, Battery và Relay tối thiểu.
- [ ] **P0** Kết nối Overdrive với `Hero` và `SkillFactory`.
- [ ] **P1** Thêm placeholder VFX/SFX cho pulse và Overdrive.
- [ ] **P0** Chạy một wave hoàn chỉnh với circuit.

## Phase 3 - Shop

- [ ] **P0** Tạo gold reward trong run.
- [ ] **P0** Hiển thị Shop sau mỗi wave.
- [ ] **P0** Hiển thị hero/item offers.
- [ ] **P0** Mua hero/item.
- [ ] **P0** Bán hoặc thay hero/item.
- [ ] **P0** Đổi vị trí slot.
- [ ] **P1** Thêm một refresh miễn phí.
- [ ] **P0** Kiểm tra board state được giữ sang wave tiếp theo.

## Phase 4 - Augments

- [ ] **P0** Tạo checkpoint sau wave 3.
- [ ] **P0** Hiển thị 3 Augment choices.
- [ ] **P0** Lưu Augment đến hết run.
- [ ] **P0** Implement `Overcharge`.
- [ ] **P0** Implement `Conductive Circuit`.
- [ ] **P1** Implement `Chain Reaction`.
- [ ] **P1** Implement `Scavenger`.

## Phase 5 - Content và tuning

- [ ] Thêm ít nhất 2 hero có Overdrive khác nhau.
- [ ] Thêm hero AOE hoặc control.
- [ ] Thêm boss wave 5.
- [ ] Tuning pulse frequency.
- [ ] Tuning gold và giá Shop.
- [ ] Tuning monster density và HP.
- [ ] Kiểm tra player hiểu stack/Overdrive mà không cần debug UI.
- [ ] Chạy một full MVP run từ wave 1 đến boss.

## Definition of Done

- [ ] 5 wave chạy hoàn chỉnh.
- [ ] Shop mở sau mỗi wave.
- [ ] Augment xuất hiện sau wave 3.
- [ ] Hero/item thay đổi được trên circuit.
- [ ] Overdrive tạo power spike nhìn thấy rõ.
- [ ] Có ít nhất một build DPS và một build defense khả dụng.
- [ ] Unity compile thành công.
- [ ] Test circuit và các test hiện có đều pass.
