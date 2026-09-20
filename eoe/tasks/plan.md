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
| 3 | Wave result và Shop | In progress - S-01 đến S-05 complete |
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
| 4 | Shop slice | Làm S-01 đến S-06 | Wave → Shop → wave pass | Đang làm — S-01 đến S-05 xong |
| 5 | Augment slice | Làm A-01 đến A-05 | Wave 3 chọn Augment và ảnh hưởng build | 1-2 phiên |
| 6 | Content/tuning | Làm T-01 đến T-05 | 5 wave + boss có ít nhất 2 build | 2 phiên |
| 7 | Self-play/QA | Làm R-04, R-05 và V-01 đến V-04 | Headless self-play lặp lại ổn định | 1-2 phiên |

**Next action:** hoàn tất `S-06` bằng cách nối RunState/Shop vào flow wave result hiện tại. Chưa mở rộng Augment UI trước khi Wave → Shop → Wave pass.

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

## Phase 7: UI, content và entry flow

### Mục tiêu

Tạo vertical slice người chơi có thể vào `Entry → Home → chọn level → Gameplay`, đồng thời mở rộng dữ liệu prototype lên 5 hero, 3 monster và 5 level. UI dùng UGUI native với shape/màu placeholder; không thêm package khi chưa cần.

### Thứ tự triển khai

1. Entry load config rồi mở Home.
2. Home dựng UI chọn level 1-5 và lưu lựa chọn run.
3. Gameplay đọc level đã chọn và hiển thị HUD/Circuit board.
4. Mở rộng Excel/config và prefab placeholder, cập nhật Addressables.
5. Compile, EditMode và manual PlayMode flow.

### Quyết định scope

- Dùng UGUI runtime-generated để tránh một scene YAML lớn và dễ thay placeholder sau này.
- Asset mới tạm dùng prefab/sprite shape hiện có với tint khác nhau; SVG chỉ thêm khi cần visual identity thật.
- Mỗi hero dùng một skill ID khác nhau từ `2001` đến `2005`; projectile prefab placeholder được nhân bản theo address.
- Mỗi level có 5 wave; boss, shop và augment chưa nằm trong increment này.

### Exit criteria

- Boot không vào thẳng Gameplay; config load xong mới vào Home.
- Home có 5 nút level và click level mở đúng Gameplay.
- Gameplay dùng level đã chọn và có HUD hiển thị wave/circuit/status.
- Config có 5 hero, 3 monster, 5 level × 5 wave; prefab/address tồn tại.
- Unity compile và EditMode pass.

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

---

# Feature Plan: Energy Roll Power Loop

## Trạng thái

**Implementation in progress.** Phần này thay thế contract pulse/stack hiện tại của Energy Circuit trong gameplay; không triển khai song song hai luật kích hoạt. Core/UI code đã cập nhật; EditMode/PlayMode còn chờ Unity Editor đang mở được đóng.

## Mục tiêu

Đổi Energy Circuit sang một chu kỳ duy nhất:

```text
Quái chết + thời gian chậm → Energy 0..100
Đủ 100 → Auto Roll hoặc chờ Manual Roll
Roll → random số bước → highlight nhảy đúng số bước
Ô đích có nội dung → phát Power cho đúng slot đó
Ô đích Empty → bỏ qua, không phát Power
Power kết thúc → mới bắt đầu tích Energy lại
```

Không có tích trữ hai lượt Power cùng lúc. Khi đang `READY` hoặc `POWER ACTIVE`, Energy không tăng.

## Quyết định kiến trúc

1. `EnergyCircuit` tiếp tục là state machine thuần C#, nhưng bỏ việc tự chạy pulse theo interval và stack threshold. Nó sở hữu Energy, highlight index, trạng thái Power và xử lý một lượt Roll.
2. `CircuitTickRunner` là owner của thời gian gameplay, chọn số bước random ở runtime và phát event Roll/Activation. Core nhận số bước đã chọn để test deterministic.
3. `GameplayScene` chỉ bơm kill energy từ `Monster.OnMonsterRewarded` và tiếp tục map activation Hero → `Hero.TryStartOverdrive`.
4. `GameplayHud` dùng uGUI runtime-generated hiện có. Thêm Energy, mode Auto/Manual, nút Roll và text tạm `ROLL +N`; không tạo prefab/asset UI mới khi code hiện tại đã có canonical runtime HUD.
5. `HeroSlotManager` không còn coroutine tự chạy highlight; chỉ cập nhật cell highlight khi `HighlightIndex` đổi sau Roll.
6. Item/hero là slot có nội dung hợp lệ để landing. Empty không phát activation và không làm highlight bị dừng. Behavior item cụ thể giữ ở activation layer hiện có; không tạo generic effect framework cho MVP.

## Luật gameplay MVP

- `EnergyCost = 100`.
- Kill và thời gian chỉ tăng Energy khi circuit đang `CHARGING`.
- Khi Energy đạt 100, chuyển sang `READY` và giữ ở 100.
- Auto mode tự Roll ngay khi vào `READY`.
- Manual mode giữ `READY` cho tới khi player bấm Roll.
- Một Roll trừ 100 Energy, chọn step trong `1..SlotCount - 1`, rồi cập nhật highlight theo modulo slot count.
- Roll phát đúng một event kết quả, bao gồm số bước và slot đích.
- Slot Hero/Item phát activation một lần; slot Empty chỉ kết thúc Roll và quay về `CHARGING`.
- Khi slot đang Power, Energy không tăng và Roll mới bị từ chối.
- Khi Power hết, circuit quay về `CHARGING` với Energy bằng 0.
- Nếu landing vào slot đang active, không kích hoạt lại; Roll vẫn bị tiêu thụ và circuit quay về `CHARGING`.

Giá trị tuning ban đầu cần để riêng và dễ sửa: kill energy, passive energy/second, power duration và giới hạn thời gian Manual. Không thêm config asset cho đến khi gameplay loop được playtest.

## Dependency graph

```text
ER-01 contract + design docs
        ↓
ER-02 EnergyCircuit state/roll
        ↓
ER-03 CircuitTickRunner + kill/time energy
        ↓
ER-04 GameplayScene + Hero activation
        ↓
ER-05 GameplayHud Auto/Manual/Roll feedback
        ↓
ER-06 EditMode + PlayMode tests
        ↓
ER-07 manual tuning + review
```

## Implementation slices

### Slice 1 - Core simulation

- Thay stack/interval bằng Energy, `READY`, `POWER ACTIVE` và highlight index đứng yên khi chưa Roll.
- Thêm API deterministic `TryRoll(int steps, ...)` để test số bước, modulo và Empty.
- Giữ event activation tương thích tối thiểu để GameplayScene vẫn gọi Hero.

**Exit criteria:** Core test chứng minh không tích Energy khi READY/ACTIVE, không có hai Power cùng lúc, Empty không activation.

### Slice 2 - Gameplay integration

- Tick passive Energy trong `CircuitTickRunner`.
- Feed kill Energy từ `GameplayScene.OnMonsterRewarded`.
- Chọn random step ở runner, không ở UI.
- Auto Roll mặc định; Manual Roll là mode có thể bật.

**Exit criteria:** kill/time đưa Energy tới 100; Auto tự Roll; Manual chỉ Roll khi được gọi; highlight nhảy đúng step.

### Slice 3 - HUD/runtime asset setup

- Dùng `GameplayHud.BuildUi()` hiện có để tạo Energy text, mode button, Roll button và text Roll tạm.
- Nút Manual chỉ enabled khi READY; Auto không cần thao tác.
- Text `ROLL +N` hiển thị sau cả Auto và Manual trong thời gian ngắn.
- Cập nhật slot highlight theo index mới; `HeroSlotManager` không còn coroutine tự chạy highlight.
- Không còn hiển thị stack/`PULSE` cũ.

**Exit criteria:** Không cần sửa prefab/scene YAML; UI test tìm thấy các node và manual test đọc được Energy, mode, số step và slot đích.

### Slice 4 - Verification/tuning

- Chạy EditMode core/integration tests.
- Chạy PlayMode HUD + một flow Energy → Roll → Power → recharge.
- Manual playtest Auto/Manual, Empty landing và Power duration.
- Code review kiểm tra state transition, event ordering và không đụng file user đang sửa.

## Rủi ro và giảm thiểu

| Rủi ro | Tác động | Giảm thiểu |
|---|---|---|
| Roll rơi vào Empty quá thường xuyên | Player thấy không có Power | Hiển thị landing rõ; tuning step/board sau playtest, không tự roll lại |
| Manual luôn tốt hơn Auto | Auto trở nên vô nghĩa | Manual chỉ đổi thời điểm; nếu thêm Focus thì giới hạn thời gian và không tăng thêm lượt |
| Random khó test | Test flaky | Random ở runner, core nhận step cụ thể |
| UI còn hiển thị stack cũ | Người chơi đọc sai luật | Xóa binding stack khỏi HUD trong cùng slice UI |
| Activation event cũ bị gọi nhiều lần | Power chồng hoặc sai duration | Circuit state chặn Roll khi ACTIVE/READY và test invariant một Power |

## Open questions cần chốt khi bắt đầu implementation

- Kill Energy ban đầu là bao nhiêu cho Normal/Elite/Boss?
- Passive Energy/second ban đầu là bao nhiêu?
- Manual có Focus/Critical ngay MVP không, hay chỉ làm Auto/Manual + Roll trước?
- Landing vào Item trong MVP sẽ có effect riêng hay chỉ phát activation feedback trước?
- Nếu landing Empty, Energy reset về 0 và recharge ngay — xác nhận đây là hành vi mong muốn?
