# Implementation Plan: Energy Circuit MVP

## Trạng thái

**Draft - dùng để triển khai sau khi Big Todo 0 được xác nhận.**

- Design docs: hoàn tất.
- Code implementation: circuit core C-01 đến C-05 hoàn tất; combat/shop còn lại.
- Các mục `Open` là blocker trước khi code phần phụ thuộc.

## Mục tiêu MVP

Một run 5 wave có auto-combat, Energy Circuit, Shop sau mỗi wave, Augment sau wave 3 và boss ở wave 5.

Người chơi không điều khiển hero liên tục. Quyết định chính là:

1. Đặt hero/item trên 8 slot.
2. Tạo thứ tự pulse và combo phù hợp.
3. Mua/thay đổi board sau mỗi wave.
4. Chọn Augment thay đổi hướng build.

## Tiến độ

| Big Todo | Nội dung | Trạng thái |
|---|---|---|
| 0 | Chốt contract và quyết định còn mở | Done |
| 0.5 | Remote verification foundation | Partial - compile/EditMode ready |
| 1 | Circuit simulation | In progress - 4/5 |
| 2 | Board và combat integration | In progress - B-01/B-04 complete |
| 3 | Wave result và Shop | Not started |
| 4 | Augment checkpoint | Not started |
| 5 | Content và tuning | Not started |
| 6 | Full verification và polish MVP | Not started |

**Tổng quan:** design hoàn tất; implementation 0%. Remote compile/EditMode verification đã sẵn sàng.

## Lịch triển khai đề xuất

Mỗi dòng là một increment độc lập. Chỉ chuyển sang dòng tiếp theo khi exit criteria pass và checkbox trong `tasks/todo.md` đã cập nhật.

| Thứ tự | Increment | Mục tiêu | Exit criteria | Dự kiến |
|---:|---|---|---|---|
| 0 | Verification foundation | Compile/EditMode chạy headless | `verify-unity.ps1 -Mode all` pass | Đã xong |
| 1 | Contract lock | Chốt D-01 đến D-04 | Không còn blocker gameplay cho circuit | 1 phiên |
| 2 | Circuit core | Làm C-01 đến C-05 | Circuit tests pass, không cần scene | Đã xong |
| 3 | Combat slice | Làm B-01 đến B-05 | Một wave có Overdrive hoạt động | Đang làm — B-01/B-04 xong |
| 4 | Shop slice | Làm S-01 đến S-06 | Wave → Shop → wave pass | 2 phiên |
| 5 | Augment slice | Làm A-01 đến A-05 | Wave 3 chọn Augment và ảnh hưởng build | 1-2 phiên |
| 6 | Content/tuning | Làm T-01 đến T-05 | 5 wave + boss có ít nhất 2 build | 2 phiên |
| 7 | Self-play/QA | Làm R-04, R-05 và V-01 đến V-04 | Headless self-play lặp lại ổn định | 1-2 phiên |

**Next action:** bắt đầu `B-05` bằng một wave checkpoint có hero + item + pulse và giữ combat slice nhỏ. Không mở rộng content trước khi Checkpoint B pass.

## Nguyên tắc kiến trúc

### Energy Circuit

- Circuit simulation là logic riêng, không nhét vào RVO movement.
- Simulation nhận `deltaTime`, cập nhật pulse/stack và trả ra activation events.
- Không dùng generic effect framework cho MVP; mỗi item/augment chỉ cần behavior nhỏ, rõ ràng.
- Logic core phải test được không cần scene hoặc UI.

### Combat

- Tái sử dụng `Hero`, `Monster`, `SkillFactory` và wave hiện tại.
- `Hero` giữ logic auto-target/attack.
- Circuit chỉ quyết định khi nào hero được Overdrive và hiệu ứng Overdrive là gì.
- Không sửa RVO trừ khi integration phát hiện vấn đề cụ thể.

### Shop và Run

- Run state giữ gold, board, owned hero/item và Augment đã chọn.
- Shop chỉ mở giữa wave, không cho mua trong combat.
- Board state phải giữ nguyên khi chuyển từ Shop sang wave tiếp theo.

## Dependency graph

```text
D-01..D-04: Contract decisions
        ↓
R-01..R-05: Remote compile/test/self-play foundation
        ↓
C-01..C-05: Circuit simulation + tests
        ↓
B-01..B-05: Board + Hero Overdrive + playable wave
        ↓
S-01..S-06: Run state + Shop + board editing
        ↓
A-01..A-05: Augment checkpoint + effects
        ↓
T-01..T-05: Content + tuning
        ↓
V-01..V-04: Full verification + MVP sign-off
```

## Phases

### Phase 0.5: Remote verification foundation

- Có script compile headless: `tools/verify-unity.ps1 -Mode compile`.
- Có script chạy EditMode tests headless: `tools/verify-unity.ps1 -Mode editmode`.
- Log/XML được lưu trong `Temp/Verification/`.
- Bổ sung headless PlayMode smoke test và deterministic self-play sau khi combat loop có thể chạy.

### Phase 1: Contract và simulation

## Checkpoints

### Checkpoint R - Remote verification foundation

- Compile headless pass.
- EditMode tests headless pass.
- Script trả exit code khác 0 khi compile/test fail.
- Artifact log/XML có thể đọc lại từ terminal.
- Headless PlayMode smoke test còn là task sau khi combat loop chạy được; B-01 đã có board contract verification.

### Checkpoint A - Circuit isolated

Sau Big Todo 1:

- Pulse đi đúng thứ tự.
- Stack chỉ tăng đúng slot.
- Threshold kích hoạt đúng một lần.
- Item modifier có test.
- Không cần Unity scene để test logic.

### Checkpoint B - Playable combat

Sau Big Todo 2:

- Có thể chạy một wave từ đầu đến cuối.
- Hero vẫn đánh bình thường khi chưa Overdrive.
- Overdrive tạo khác biệt nhìn thấy được.
- Circuit không làm hỏng wave/RVO hiện tại.

### Checkpoint C - Shop loop

Sau Big Todo 3:

- Wave kết thúc mở Shop.
- Người chơi mua/bán/thay đổi board.
- Wave mới dùng đúng state vừa chỉnh.
- Không thể thay đổi board giữa combat.

### Checkpoint D - Full MVP run

Sau Big Todo 4-6:

- Chạy được 5 wave và boss.
- Augment sau wave 3 hoạt động.
- Có ít nhất một build DPS và một build defense khả dụng.
- Headless PlayMode smoke test và deterministic self-play pass.
- Test, compile và manual playtest đều pass.

## Quy tắc triển khai từng task

- Mỗi sub-task phải hoàn thành trong một phiên tập trung.
- Mỗi sub-task có acceptance criteria và verification riêng trong `tasks/todo.md`.
- Không bắt đầu task có dependency chưa pass.
- Sau mỗi Big Todo, cập nhật checkbox và progress trong `tasks/todo.md`.
- Nếu quyết định gameplay thay đổi, sửa docs game design trước rồi mới sửa code.

## Rủi ro và giảm thiểu

| Rủi ro | Tác động | Giảm thiểu |
|---|---|---|
| Pulse quá chậm | Không có power feeling | Tuning để hero có Overdrive khoảng 8-15 giây |
| Circuit chỉ là trang trí | Người chơi không cần suy nghĩ | Vị trí phải thay đổi thời điểm/hiệu ứng activation |
| Shop random quá tệ | Thua vì may rủi | 2 offer liên quan build, 1 offer mở hướng mới |
| Một build luôn tối ưu | Mất giá trị lựa chọn | Có build DPS, AOE, defense và boss |
| Overdrive chỉ tăng số | Thiếu feeling | Thay đổi projectile/behavior + VFX/SFX |
| Scope phình to | Không hoàn thành MVP | Chưa làm meta progression, PvP, crafting, deck đầy đủ |
| Logic phụ thuộc UI | Khó test và sửa | Circuit/Shop/Run state tách khỏi MonoBehaviour khi có thể |
