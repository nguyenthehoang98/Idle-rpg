# MVP Todo: Energy Circuit

## Cách theo dõi

- `[ ]` Chưa làm.
- `[~]` Đang làm.
- `[x]` Hoàn tất và đã verify.
- `P0` Bắt buộc cho MVP.
- `P1` Làm sau khi core loop chạy được.
- Chỉ tick một sub-task khi acceptance criteria và verification đều pass.
- Mỗi sub-task là một đơn vị có thể hoàn thành độc lập, không gom nhiều feature không liên quan vào cùng một task.

## Progress

| Big Todo | Done | Trạng thái |
|---|---:|---|
| Big 0 - Contract | 0/4 | In review |
| Big 1 - Circuit simulation | 0/5 | Not started |
| Big 2 - Combat integration | 0/5 | Not started |
| Big 3 - Shop loop | 0/6 | Not started |
| Big 4 - Augments | 0/5 | Not started |
| Big 5 - Content/tuning | 0/5 | Not started |
| Big 6 - Verification | 0/4 | Not started |

**Current status:** Design docs đã commit; code implementation chưa bắt đầu.

---

# Big Todo 0 - Chốt contract trước khi code

**Mục tiêu:** loại bỏ các quyết định có thể làm thay đổi toàn bộ implementation.

## D-01 - Chốt layout circuit `[P0]`

- [ ] Chọn một layout MVP: vòng tròn, line hoặc grid.
- [ ] Chốt thứ tự pulse trong layout.
- [ ] Chốt số slot mặc định là 8.

**Acceptance:** Có một sơ đồ hoặc mô tả không còn mơ hồ về slot kế tiếp của mỗi slot.

**Verify:** Cập nhật `docs/game-design/energy-circuit.md` và `mvp.md`.

## D-02 - Chốt luật stack/activation `[P0]`

- [ ] Chốt pulse interval ban đầu.
- [ ] Chốt activation threshold.
- [ ] Chốt Overdrive duration.
- [ ] Chốt stack có tích trong lúc Overdrive hay không.
- [ ] Chốt behavior khi activation xảy ra lúc Overdrive đang active.

**Acceptance:** Có bảng luật cho empty slot, hero slot, item slot, threshold và reset.

**Verify:** Viết lại phần luật MVP trong `energy-circuit.md` nếu quyết định thay đổi.

## D-03 - Chốt board và ownership `[P0]`

- [ ] Chốt hero/item có thể đặt vào mọi slot hay slot có type riêng.
- [ ] Chốt số hero active.
- [ ] Chốt có bench hay không.
- [ ] Chốt item có bị mất khi thay thế hay bán được.

**Acceptance:** Có state diagram cho board trước và sau Shop.

**Verify:** Cập nhật `mvp.md` và `shop-and-augments.md`.

## D-04 - Chốt cadence Shop/Augment `[P0]`

- [ ] Chốt Shop mở sau mọi wave.
- [ ] Chốt Augment checkpoint sau wave 3.
- [ ] Chốt thứ tự `Reward → Augment → Shop → Next wave`.
- [ ] Chốt gold đến từ wave, kill hoặc cả hai.

**Acceptance:** Có flow hoàn chỉnh cho run 5 wave.

**Verify:** Cập nhật `mvp.md` và `shop-and-augments.md`.

---

# Big Todo 1 - Circuit simulation

**Dependency:** Big Todo 0.

**Mục tiêu:** Circuit chạy đúng độc lập với UI và scene.

## C-01 - Tạo contract/state cho circuit `[P0]`

- [ ] Tạo type cho slot state.
- [ ] Tạo type cho slot content: empty, hero, item.
- [ ] Tạo type cho activation event.
- [ ] Tạo state cho pulse index, stack, active duration.
- [ ] Không để state phụ thuộc `MonoBehaviour` nếu không cần.

**Acceptance:** Có thể tạo một circuit 8 slot trong test bằng code thuần.

**Verify:** Compile Unity; test khởi tạo circuit với slot trống.

**Files dự kiến:** `Assets/_TDS/Battle/EnergyCircuit.cs`, test tương ứng.

## C-02 - Implement pulse traversal `[P0]`

- [ ] Nhận `deltaTime`.
- [ ] Pulse đi đúng một slot theo mỗi interval.
- [ ] Pulse quay lại slot đầu sau slot cuối.
- [ ] Hỗ trợ `deltaTime` lớn hơn một interval mà không mất pulse.
- [ ] Empty slot không làm pulse dừng.

**Acceptance:** Pulse traversal deterministic và không phụ thuộc frame rate.

**Verify:** Unit test với nhiều `deltaTime` và nhiều vòng lặp.

**Files dự kiến:** `EnergyCircuit.cs`, `EnergyCircuitTests.cs`.

## C-03 - Implement stack/threshold `[P0]`

- [ ] Slot có content nhận stack khi pulse đi qua.
- [ ] Slot trống không nhận stack.
- [ ] Đủ threshold tạo một activation event.
- [ ] Stack reset đúng theo contract.
- [ ] Không tạo activation event lặp sai trong cùng một pulse.

**Acceptance:** Event có đúng slot, loại content và số stack tại thời điểm activation.

**Verify:** Unit test threshold dưới, bằng và vượt ngưỡng.

**Files dự kiến:** `EnergyCircuit.cs`, `EnergyCircuitTests.cs`.

## C-04 - Implement item modifiers `[P0]`

- [ ] Generator tăng stack hoặc pulse theo contract.
- [ ] Amplifier tăng stack nhận được.
- [ ] Battery lưu stack dư theo contract.
- [ ] Relay chuyển stack sang slot kế tiếp.
- [ ] Item modifier không phá thứ tự pulse.

**Acceptance:** Mỗi item có một test behavior; không dùng reflection để test private state.

**Verify:** Unit test từng item và một test combo Generator → Hero.

**Files dự kiến:** `EnergyCircuit.cs`, item data/config, tests.

## C-05 - Checkpoint circuit `[P0]`

- [ ] Test circuit với 8 slot.
- [ ] Test nhiều hero/item trong cùng circuit.
- [ ] Test circuit chạy liên tục ít nhất 3 vòng.
- [ ] Test circuit dispose/reset nếu có resource native hoặc pooled state.

**Acceptance:** Circuit test suite pass và không có allocation/resource leak mới.

**Verify:** Unity EditMode tests và `git diff --check`.

---

# Big Todo 2 - Board và combat integration

**Dependency:** Big Todo 1.

**Mục tiêu:** Circuit thật sự ảnh hưởng đến combat hiện tại.

## B-01 - Tạo board runtime `[P0]`

- [ ] Tạo board state cho 8 slot.
- [ ] Khởi tạo board khi Gameplay bắt đầu.
- [ ] Map hero hiện tại vào board.
- [ ] Cho phép slot trống.

**Acceptance:** Scene gameplay chạy mà không cần Shop vẫn có board hợp lệ.

**Verify:** Manual Play Mode test với hero hiện tại.

**Files dự kiến:** `Assets/_TDS/Gameplay/`, `Assets/_TDS/Battle/`.

## B-02 - Kết nối circuit với Tick runner `[P0]`

- [ ] Chọn một owner duy nhất để tick circuit.
- [ ] Tick circuit trước/đúng thời điểm combat cần activation.
- [ ] Không tick hai lần trong một frame.
- [ ] Reset circuit khi bắt đầu run mới.

**Acceptance:** Pulse tốc độ ổn định khi thay đổi frame rate/time scale.

**Verify:** Debug log hoặc test instrumentation trong một Play Mode run.

## B-03 - Thêm Hero Overdrive contract `[P0]`

- [ ] Thêm API kích hoạt Overdrive cho hero.
- [ ] Overdrive có start/end state rõ ràng.
- [ ] Không để nhiều Overdrive chồng sai lên nhau.
- [ ] Hero bình thường vẫn attack khi chưa Overdrive.

**Acceptance:** Hero nhận activation event và trở lại trạng thái bình thường sau duration.

**Verify:** Unit/integration test state; manual test với hero thật.

**Files dự kiến:** `Assets/_TDS/Battle/Hero.cs`, config/stat liên quan.

## B-04 - Tạo 3 Overdrive identity `[P0]`

- [ ] DPS: tăng tốc đánh hoặc projectile.
- [ ] AOE: tạo damage diện rộng hoặc chain.
- [ ] Defense: shield/heal/damage reduction.
- [ ] Mỗi identity có feedback khác nhau.

**Acceptance:** Có thể phân biệt ba Overdrive chỉ bằng gameplay/VFX.

**Verify:** Manual Play Mode test trong wave có nhiều quái.

## B-05 - Circuit combat checkpoint `[P0]`

- [ ] Chạy một wave có hero + item + pulse.
- [ ] Kiểm tra Overdrive thật sự làm thay đổi damage/behavior.
- [ ] Kiểm tra monster chết bình thường ngoài Overdrive.
- [ ] Kiểm tra không có lỗi JobHandle/NativeContainer.

**Acceptance:** Checkpoint B pass; không mở Shop trước khi pass.

**Verify:** Unity compile, EditMode tests và một manual wave.

---

# Big Todo 3 - Run state và Shop

**Dependency:** Big Todo 2.

**Mục tiêu:** Người chơi chỉnh build giữa các wave.

## S-01 - Tạo run state và gold `[P0]`

- [ ] Tạo state chứa gold hiện tại.
- [ ] Tạo state chứa board hiện tại.
- [ ] Tạo state chứa owned hero/item trong run.
- [ ] Reset state khi bắt đầu run.
- [ ] Không lưu meta progression trong MVP.

**Acceptance:** Start/retry run tạo state sạch, không giữ dữ liệu run cũ.

**Verify:** Test reset và manual retry.

## S-02 - Tạo offer model `[P0]`

- [ ] Định nghĩa hero offer.
- [ ] Định nghĩa item offer.
- [ ] Định nghĩa direct upgrade/utility offer.
- [ ] Offer có price và tag.
- [ ] Offer có purchased/unavailable state.

**Acceptance:** UI có thể render offer từ data mà không biết logic random bên trong.

**Verify:** Unit test price, purchase và duplicate offer handling.

## S-03 - Tạo shop generation `[P0]`

- [ ] Sinh đúng số lượng offer.
- [ ] Có ít nhất hai offer liên quan một phần tới build.
- [ ] Có một offer mở hướng build mới.
- [ ] Không sinh offer không hợp lệ.
- [ ] Hỗ trợ refresh theo giới hạn.

**Acceptance:** Cùng seed cho kết quả deterministic trong test.

**Verify:** Unit test với seed và build tags khác nhau.

## S-04 - Implement buy/sell `[P0]`

- [ ] Mua hero khi đủ gold.
- [ ] Mua item khi đủ gold.
- [ ] Từ chối mua khi không đủ gold.
- [ ] Bán/trả item theo contract.
- [ ] Không mua cùng offer hai lần.

**Acceptance:** Gold và ownership luôn nhất quán sau mọi giao dịch.

**Verify:** Unit tests cho success/failure/refund.

## S-05 - Implement board editing trong Shop `[P0]`

- [ ] Đặt hero vào slot.
- [ ] Đặt item vào slot.
- [ ] Đổi chỗ hai slot.
- [ ] Xóa slot về empty.
- [ ] Chặn board state không hợp lệ.

**Acceptance:** Sau khi đóng Shop, board runtime phản ánh đúng lựa chọn.

**Verify:** Test board mutation và manual drag/click flow.

## S-06 - Kết nối wave → Shop → wave `[P0]`

- [ ] Pause combat khi Shop mở.
- [ ] Ghi reward khi wave kết thúc.
- [ ] Mở Shop đúng một lần cho mỗi wave.
- [ ] Chỉ resume sau khi người chơi đóng Shop.
- [ ] Giữ board/gold sang wave tiếp theo.

**Acceptance:** Không thể spawn hoặc thay board trong lúc Shop đang mở.

**Verify:** Manual run từ wave 1 đến wave 2.

---

# Big Todo 4 - Augment checkpoint

**Dependency:** Big Todo 3.

**Mục tiêu:** Sau một số wave, người chơi chọn buff thay đổi hướng build.

## A-01 - Tạo Augment data/state `[P0]`

- [ ] Định nghĩa Augment id, name, description, tags.
- [ ] Định nghĩa một Augment chỉ được chọn một lần trong run.
- [ ] Lưu selected Augments vào run state.
- [ ] Chống duplicate nếu contract không cho phép.

**Acceptance:** Augment state sống qua các wave nhưng reset khi retry run.

**Verify:** Unit test add/reset/duplicate.

## A-02 - Tạo checkpoint flow `[P0]`

- [ ] Trigger sau wave 3.
- [ ] Hiển thị đúng 3 choices.
- [ ] Chỉ được chọn một choice.
- [ ] Sau khi chọn mới mở Shop.
- [ ] Không trigger lại khi wave result được xử lý nhiều lần.

**Acceptance:** Flow đúng `Wave → Reward → Augment → Shop → Next wave`.

**Verify:** Manual run đến wave 3 và test event idempotency.

## A-03 - Implement Energy Augments `[P0]`

- [ ] `Overcharge`.
- [ ] `Overflow`.
- [ ] `Conductive Circuit`.

**Acceptance:** Mỗi Augment làm thay đổi circuit behavior, không chỉ thay text/stat.

**Verify:** Mỗi Augment có ít nhất một test behavior.

## A-04 - Implement Combat/Economy Augments `[P1]`

- [ ] `Chain Reaction`.
- [ ] `Execution Order`.
- [ ] `Scavenger`.

**Acceptance:** Có ít nhất một Augment tác động combat và một Augment tác động economy.

**Verify:** Unit/integration tests và manual wave.

## A-05 - Augment UX checkpoint `[P0]`

- [ ] Hiển thị title, mô tả và tag.
- [ ] Highlight choice đang hover/selected.
- [ ] Không cho đóng mà không chọn nếu MVP yêu cầu bắt buộc chọn.
- [ ] Cho người chơi hiểu Augment áp dụng đến hết run.

**Acceptance:** Người chơi mới đọc được khác biệt giữa ba lựa chọn.

**Verify:** Manual UX review với build không có debug text.

---

# Big Todo 5 - Content và tuning

**Dependency:** Big Todo 4.

**Mục tiêu:** Tạo đủ content để kiểm tra build và boss.

## T-01 - Hero content `[P0]`

- [ ] Có ít nhất hai hero active.
- [ ] Có ít nhất một hero DPS.
- [ ] Có ít nhất một hero AOE/control/defense.
- [ ] Mỗi hero có Overdrive identity khác nhau.

**Acceptance:** Hai hero tạo ra hai cách xếp circuit hợp lý.

**Verify:** Chạy hai build khác nhau qua wave 1-4.

## T-02 - Item content `[P0]`

- [ ] Generator.
- [ ] Amplifier.
- [ ] Battery.
- [ ] Relay.
- [ ] Mỗi item có tag và description.

**Acceptance:** Item không chỉ là stat bonus; ít nhất ba item làm thay đổi pulse/stack.

**Verify:** Test item behavior và manual circuit comparison.

## T-03 - Wave/boss content `[P0]`

- [ ] Điều chỉnh mật độ wave 1-4.
- [ ] Thêm elite hoặc modifier ở wave 3/4.
- [ ] Tạo boss wave 5.
- [ ] Boss có ít nhất một mechanic khiến build quan trọng.

**Acceptance:** Auto-combat không thể thắng ổn định nếu bỏ qua circuit.

**Verify:** Manual run với build yếu và build hợp lý.

## T-04 - Economy/shop tuning `[P0]`

- [ ] Chốt gold reward mỗi wave.
- [ ] Chốt giá hero/item.
- [ ] Chốt refund khi bán.
- [ ] Chốt số offer và refresh.
- [ ] Kiểm tra người chơi đủ tài nguyên cho ít nhất một build.

**Acceptance:** Người chơi có quyết định kinh tế, không bị thiếu hoặc thừa gold rõ rệt.

**Verify:** Chạy tối thiểu 3 run và ghi lại gold/offer/board state.

## T-05 - Feeling và readability tuning `[P1]`

- [ ] Tuning pulse speed.
- [ ] Tuning stack feedback.
- [ ] Thêm VFX/SFX Overdrive.
- [ ] Hiển thị combo trigger.
- [ ] Giảm debug log không cần thiết.

**Acceptance:** Người chơi biết khi nào pulse đến, slot sắp đầy và Overdrive vừa kích hoạt.

**Verify:** Manual UX pass không dùng inspector/debug tool.

---

# Big Todo 6 - Verification và MVP sign-off

**Dependency:** Big Todo 5.

## V-01 - Automated tests `[P0]`

- [ ] Circuit tests pass.
- [ ] Shop/run state tests pass.
- [ ] Augment tests pass.
- [ ] Existing tests pass.

**Acceptance:** Không test nào bị skip/disable để làm xanh build.

**Verify:** Unity EditMode test command.

## V-02 - Unity compile `[P0]`

- [ ] Compile script assemblies thành công.
- [ ] Không có compile error mới.
- [ ] Review warning mới nếu có.

**Acceptance:** Unity batch compile exit code 0.

**Verify:** Unity batchmode compile command của project.

## V-03 - Full manual run `[P0]`

- [ ] Bắt đầu run mới.
- [ ] Chơi qua wave 1-5.
- [ ] Mua và thay đổi hero/item.
- [ ] Chọn Augment sau wave 3.
- [ ] Đánh boss.
- [ ] Retry tạo run sạch.

**Acceptance:** Full loop không cần debug intervention.

**Verify:** Play Mode checklist và ghi lại lỗi phát sinh.

## V-04 - MVP review `[P0]`

- [ ] So với success criteria trong `docs/game-design/mvp.md`.
- [ ] Cập nhật tài liệu nếu quyết định gameplay thay đổi.
- [ ] Đánh dấu các hạng mục bị cắt khỏi MVP.
- [ ] Tạo commit riêng cho MVP sign-off.

**Acceptance:** Có quyết định rõ: ship prototype, tuning tiếp hoặc quay lại design.

**Verify:** Review cùng người dùng trước khi mở rộng scope.

---

# Definition of Done

Một Big Todo chỉ được đánh dấu `[x]` khi:

- Các sub-task bắt buộc đã hoàn thành.
- Acceptance criteria của sub-task pass.
- Verification đã chạy và ghi nhận kết quả.
- Không có file debug/test tạm không cần thiết.
- Không thay đổi scope ngoài plan mà chưa cập nhật docs.
- Có commit nhỏ, mô tả đúng một logical increment.
