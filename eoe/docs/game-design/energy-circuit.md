# Energy Circuit — Energy Roll MVP

## Trạng thái

**MVP contract v2 — Energy Roll.** Contract này thay thế pulse/stack loop cũ.

## Mục đích

Energy Circuit tạo ra một lượt Power duy nhất sau khi người chơi tích đủ Energy từ combat. Highlight không tự chạy trong lúc chờ; nó chỉ di chuyển sau khi một lượt Roll được xử lý.

```text
Quái chết + thời gian chậm
        ↓
Energy 0..100
        ↓
Auto Roll hoặc Manual READY
        ↓
Random số bước
        ↓
Highlight nhảy đúng số bước
        ↓
Slot đích được xử lý
        ↓
Power kết thúc mới tích Energy lại
```

## State machine

```text
CHARGING → READY → RESOLVING → POWER ACTIVE → CHARGING
                 └────────────→ CHARGING (nếu slot Empty)
```

### CHARGING

- Energy tăng từ kill và passive time.
- Highlight giữ nguyên ở slot hiện tại.
- Không có Roll đang chờ.

### READY

- Energy đạt 100 và giữ ở 100.
- Không tích thêm Energy.
- Auto mode chuyển ngay sang `RESOLVING`.
- Manual mode chờ người chơi bấm Roll.
- Không thể tạo thêm lượt Power.

### RESOLVING

- Roll tiêu thụ 100 Energy.
- Runtime chọn số bước trong `1..SlotCount - 1`.
- Highlight cập nhật theo modulo:

```text
newIndex = (oldIndex + steps) % slotCount
```

- UI hiện text tạm, ví dụ `ROLL +5`.
- Mỗi Roll chỉ có một slot đích.

### POWER ACTIVE

- Slot đích có nội dung sẽ phát một activation event.
- Circuit chỉ cho phép một Power active tại một thời điểm.
- Energy không tăng.
- Roll mới bị từ chối.
- Khi duration kết thúc, Energy vẫn là 0 và circuit quay lại `CHARGING`.

### Empty landing

- Highlight vẫn đổi vị trí.
- Không phát activation event.
- Không tự Roll lại.
- Energy reset về 0 và bắt đầu tích lại ở tick kế tiếp.

## Nguồn Energy

Energy là một cooldown progress, không phải kho chứa nhiều lượt.

- Kill là nguồn chính.
- Passive time là nguồn phụ và chậm.
- Energy bị clamp trong `0..100`.
- Khi `READY` hoặc `POWER ACTIVE`, mọi nguồn Energy đều bị bỏ qua.
- Khi Roll xảy ra, Energy về 0.

Giá trị tuning ban đầu nằm trong code để prototype nhanh; chỉ đưa sang config asset sau khi playtest có dữ liệu:

```text
Energy capacity: 100
Kill energy: 20 mỗi monster reward
Passive energy: 1 mỗi giây khi CHARGING
Overdrive duration: 5 giây mặc định
```

## Roll mode

### Auto

- Khi Energy đạt 100, circuit tự Roll.
- Player không cần thao tác.
- Step vẫn random.

### Manual

- Khi Energy đạt 100, circuit dừng ở `READY`.
- Player bấm nút `ROLL` để tiêu lượt.
- Trong MVP không thêm Focus/Critical; trước tiên kiểm tra core loop và feeling của việc chọn thời điểm Roll.
- Có timeout an toàn tùy UI tuning sau playtest; không cho Energy tích thêm trong lúc chờ.

## Slot behavior

| Slot đích | Kết quả |
|---|---|
| Hero | Phát activation; GameplayScene gọi `Hero.TryStartOverdrive` |
| Item | Phát activation/feedback theo item layer hiện có |
| Empty | Không activation, không Roll lại, recharge lại |
| Slot đang active | Không activation lại, Roll vẫn bị tiêu thụ, recharge lại |

Không tạo generic effect framework cho MVP. Item effect nâng cao là task riêng sau khi Energy Roll core ổn định.

## Highlight và UI

- `HeroSlotManager` không chạy coroutine tự di chuyển highlight.
- Highlight cell và slot tint chỉ cập nhật khi `HighlightIndex` thay đổi sau Roll.
- `GameplayHud` dùng uGUI runtime-generated hiện có:
  - `ENERGY 0/100`
  - `AUTO` / `MANUAL`
  - Nút `ROLL` khi Manual + READY
  - Text tạm `ROLL +N`
- Không tạo prefab/asset UI mới cho prototype; dùng `LegacyRuntime.ttf` và pattern BuildUi hiện có.

## Code boundaries

- `EnergyCircuit`: state machine thuần C#, không biết Unity random/UI.
- `CircuitTickRunner`: tick thời gian, chọn random step, Auto/Manual và phát event.
- `GameplayScene`: nhận kill reward, bơm kill Energy, map activation Hero.
- `GameplayHud`: hiển thị Energy/mode/Roll feedback và gửi input Manual.
- `HeroSlotManager`: hiển thị highlight tĩnh theo `PulseIndex`/highlight index.

## Testing contract

### EditMode

- Energy tăng theo thời gian khi `CHARGING`.
- Kill Energy bị clamp ở 100.
- READY không tăng Energy.
- Roll dưới 100 bị từ chối.
- Roll đúng số bước cập nhật highlight theo modulo.
- Empty không tạo activation.
- Hero/Item tạo tối đa một activation.
- POWER ACTIVE chặn Energy và Roll.
- Power hết thì recharge mới hoạt động.
- Reset trả Energy/highlight/active về trạng thái ban đầu.

### PlayMode

- HUD tạo Energy/mode/Roll/RollResult.
- Manual chỉ enable Roll khi READY.
- Roll feedback hiển thị số bước.
- Highlight không tự chạy trước Roll.
- Flow `kill/time → 100 → roll → landing → power/empty → recharge` chạy được.

## Ngoài MVP

- Focus/Critical khi chờ Manual.
- Player chọn giữa nhiều step preview.
- Item effect nâng cao.
- Config asset cho Energy tuning.
- Animation highlight chạy theo từng ô trong lúc Resolving.
