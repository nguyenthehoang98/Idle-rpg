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
| Big 0 - Contract | 4/4 | Done |
| Big 0.5 - Remote verification | 3/5 | Partial - compile/EditMode ready |
| Big 1 - Circuit simulation | 4/5 | In progress |
| Big 2 - Combat integration | 1/6 | In progress |
| Big 3 - Shop loop | 0/6 | Not started |
| Big 4 - Augments | 0/5 | Not started |
| Big 5 - Content/tuning | 0/5 | Not started |
| Big 6 - Verification | 0/4 | Not started |

**Current status:** Design docs đã commit; Energy Circuit core 4/5; hero combat engagement đã fix và verify.

---

# Big Todo 0 - Chốt contract trước khi code ✅

**Mục tiêu:** loại bỏ các quyết định có thể làm thay đổi toàn bộ implementation.

## D-01 - Chốt layout circuit `[P0]` `[x]`

- [x] Chọn layout logic MVP là vòng một chiều; UI có thể vẽ vòng tròn hoặc line.
- [x] Chốt thứ tự pulse `0 → 1 → ... → 7 → 0`.
- [x] Chốt số slot mặc định là 8.

**Acceptance:** Có một sơ đồ hoặc mô tả không còn mơ hồ về slot kế tiếp của mỗi slot.

**Verify:** Cập nhật `docs/game-design/energy-circuit.md` và `mvp.md`.

## D-02 - Chốt luật stack/activation `[P0]` `[x]`

- [x] Chốt pulse interval ban đầu là `0.5s`.
- [x] Chốt activation threshold là `3`.
- [x] Chốt Overdrive duration là `5s`.
- [x] Stack không tích trong lúc Overdrive active.
- [x] Activation trong lúc Overdrive active bị bỏ qua; pulse vẫn tiếp tục chạy.

**Acceptance:** Có bảng luật cho empty slot, hero slot, item slot, threshold và reset.

**Verify:** Viết lại phần luật MVP trong `energy-circuit.md` nếu quyết định thay đổi.

## D-03 - Chốt board và ownership `[P0]` `[x]`

- [x] Hero/item có thể đặt vào mọi slot; không có slot type riêng.
- [x] Tối đa 3 hero active.
- [x] Không có bench trong MVP.
- [x] Bán/thay hero/item trả 100% giá trong prototype; content bị gỡ khỏi board.

**Acceptance:** Có state diagram cho board trước và sau Shop.

**Verify:** Cập nhật `mvp.md` và `shop-and-augments.md`.

## D-04 - Chốt cadence Shop/Augment `[P0]` `[x]`

- [x] Shop mở sau mọi wave.
- [x] Augment checkpoint diễn ra sau wave 3.
- [x] Chốt thứ tự `Reward → Augment → Shop → Next wave`.
- [x] Gold đến từ hoàn thành wave, elite kill và boss; regular kill không cộng gold trực tiếp.

**Acceptance:** Có flow hoàn chỉnh cho run 5 wave.

**Verify:** Cập nhật `mvp.md` và `shop-and-augments.md`.

---

# Big Todo 0.5 - Remote verification foundation

**Dependency:** Big Todo 0 chỉ cần chốt command contract; không phụ thuộc gameplay code.

**Mục tiêu:** Pi agent có thể compile và chạy test mà không mở Unity Editor bằng UI.

## R-01 - Chốt verification command contract `[P0]` `[x]`

- [x] Chuẩn hóa `compile`, `editmode`, `playmode` và `all` theo từng tầng.
- [x] Chọn artifact directory `Temp/Verification/`.
- [x] Quy định exit code khác 0 khi compile/test fail.
- [x] Ghi command và workflow trong `docs/engineering/remote-verification.md`.

**Acceptance:** Một agent mới đọc doc có thể chạy compile/EditMode từ terminal.

**Verify:** Đọc lại doc và chạy command trên máy remote/local.

## R-02 - Tạo headless compile runner `[P0]` `[x]`

- [x] Tạo `tools/verify-unity.ps1`.
- [x] Tự tìm Unity hoặc nhận `UNITY_PATH`.
- [x] Chạy Unity bằng `-batchmode -nographics -quit`.
- [x] Lưu log theo timestamp.
- [x] Fail khi Unity exit code khác 0 hoặc log có compile error.

**Acceptance:** Compile không cần mở Unity UI và trả status rõ ràng.

**Verify:** `powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode compile`.

## R-03 - Tạo headless EditMode runner `[P0]` `[x]`

- [x] Chạy Unity Test Framework bằng `-runTests -testPlatform editmode`.
- [x] Lưu test result XML.
- [x] Fail nếu result không phải `Passed` hoặc có failed test.
- [x] Hiển thị artifact path trong output.

**Acceptance:** Agent đọc được số test pass/fail từ terminal hoặc XML.

**Verify:** `powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode editmode`.

## R-04 - Headless PlayMode smoke test `[P0]`

- [ ] Tạo test scene tối thiểu hoặc entry point không cần thao tác UI.
- [ ] Chạy flow start run → wave → Shop/checkpoint bằng `-testPlatform playmode`.
- [ ] Ghi summary: current wave, gold, activation count, monster killed, win/fail.
- [ ] Fail test khi có exception, timeout hoặc state không hợp lệ.

**Acceptance:** Gameplay flow chính chạy được trong headless PlayMode.

**Verify:** Chạy filter smoke test bằng PowerShell runner.

## R-05 - Deterministic self-play bot `[P0]`

- [ ] Tạo seed cố định cho run test.
- [ ] Bot mua build tối thiểu theo rule đã biết.
- [ ] Bot chọn Augment theo tag/strategy cố định.
- [ ] Bot chạy đến boss hoặc fail bằng timeout rõ ràng.
- [ ] Xuất summary dễ đọc cho Pi agent.

**Acceptance:** Một test lặp lại cho cùng kết quả và không cần chuột/keyboard.

**Verify:** Chạy self-play ít nhất 3 lần cùng seed; kết quả không đổi.

## Remote verification checkpoint

- [x] Compile headless pass trên project hiện tại.
- [x] EditMode tests headless pass trên project hiện tại.
- [ ] PlayMode smoke test pass.
- [ ] Deterministic self-play pass.

---

# Big Todo 1 - Circuit simulation

**Dependency:** Big Todo 0.

**Mục tiêu:** Circuit chạy đúng độc lập với UI và scene.

## C-01 - Tạo contract/state cho circuit `[P0]` `[x]`

- [x] Tạo type cho slot state.
- [x] Tạo type cho slot content: empty, hero, item.
- [x] Tạo type cho activation event.
- [x] Tạo state cho pulse index, stack, active duration.
- [x] Không để state phụ thuộc `MonoBehaviour` nếu không cần.

**Acceptance:** Có thể tạo một circuit 8 slot trong test bằng code thuần.

**Verify:** `tools/verify-unity.ps1 -Mode all` — compile pass, 11 EditMode tests pass.

**Files dự kiến:** `Assets/_TDS/Battle/EnergyCircuit.cs`, test tương ứng.

## C-02 - Implement pulse traversal `[P0]` `[x]`

- [x] Nhận `deltaTime`.
- [x] Pulse đi đúng một slot theo mỗi interval.
- [x] Pulse quay lại slot đầu sau slot cuối.
- [x] Hỗ trợ `deltaTime` lớn hơn một interval mà không mất pulse.
- [x] Empty slot không làm pulse dừng.

**Acceptance:** Pulse traversal deterministic và không phụ thuộc frame rate.

**Verify:** `tools/verify-unity.ps1 -Mode all` — compile pass, 15 EditMode tests pass.

**Files dự kiến:** `EnergyCircuit.cs`, `EnergyCircuitTests.cs`.

## C-03 - Implement stack/threshold `[P0]` `[x]`

- [x] Slot có content nhận stack khi pulse đi qua.
- [x] Slot trống không nhận stack.
- [x] Đủ threshold tạo một activation event.
- [x] Stack reset đúng theo contract.
- [x] Không tạo activation event lặp sai trong cùng một pulse.

**Acceptance:** Event có đúng slot, loại content và số stack tại thời điểm activation.

**Verify:** `tools/verify-unity.ps1 -Mode all` — compile pass, 18 EditMode tests pass.

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

## C-05 - Checkpoint circuit `[P0]` `[x]`

- [x] Test circuit với 8 slot.
- [x] Test nhiều hero/item trong cùng circuit.
- [x] Test circuit chạy liên tục ít nhất 3 vòng.
- [x] Test circuit reset; circuit hiện chưa giữ resource native hoặc pooled state.

**Acceptance:** Circuit test suite pass và không có allocation/resource leak mới.

**Verify:** `tools/verify-unity.ps1 -Mode all` — compile pass, 21 EditMode tests pass; `git diff --check` pass.

---

# Big Todo 2 - Board và combat integration

**Dependency:** Big Todo 1.

## B-00 - Khôi phục hero/monster engagement `[P0]` `[x]`

- [x] Xác định monster dừng ở `attackRange = 2`, ngoài `hero.attackRange = 1`.
- [x] Tính stop distance bằng khoảng giao nhau giữa monster range và hero range.
- [x] Giữ khoảng cách tối thiểu theo radius để không overlap.
- [x] Thêm regression tests cho stop distance.

**Acceptance:** Monster dừng trong tầm hero và hero có thể tìm target để tấn công.

**Verify:** Headless gameplay log có `SkillFactory.CastSkillAsync` và `[Monster:monster.1001] Death`; 23 EditMode tests pass.


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

**Verify:** `powershell -ExecutionPolicy Bypass -File tools/verify-unity.ps1 -Mode compile`.

## V-03 - Full run verification `[P0]`

- [ ] Bắt đầu run mới.
- [ ] Chơi qua wave 1-5.
- [ ] Mua và thay đổi hero/item.
- [ ] Chọn Augment sau wave 3.
- [ ] Đánh boss.
- [ ] Retry tạo run sạch.
- [ ] Chạy lại cùng flow bằng headless self-play.

**Acceptance:** Full loop không cần debug intervention hoặc thao tác UI trong test.

**Verify:** Headless PlayMode smoke test + deterministic self-play; manual Play Mode chỉ dùng để review visual/feeling.

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
