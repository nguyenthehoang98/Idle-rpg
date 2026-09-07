# UI Design System và quy trình dựng lại

## Mục tiêu

Dựng lại UI theo bộ reference trong `C:/Users/Hoang PC/Documents/Idle-rpg/ref-ui` với độ tương đồng thị giác khoảng 80–90%, nhưng giữ UI dưới dạng **design system có thể mở rộng**.

Đây không phải quy trình clone từng screenshot thành một asset cố định. Reference chỉ mô tả ngôn ngữ thị giác, hierarchy và behavior; code/prefab mới là hệ thống nguồn để tạo các màn hình.

## Baseline

- Canvas chuẩn: `1080 x 2400`.
- Thiết kế mobile portrait trước.
- Tất cả màn hình nằm trong safe area.
- Không sửa trực tiếp UI prototype cũ để đạt visual mới.
- UI mới sống trong namespace và thư mục riêng; chỉ chuyển scene sang UI mới sau khi từng màn hình pass.
- Không thêm package UI mới nếu UGUI hiện tại đáp ứng được.

## Phạm vi màn hình

1. Home / level select.
2. Gameplay HUD.
3. Wave reward.
4. Shop.
5. Augment selection.
6. Victory / defeat.

Mỗi màn hình phải có đủ state: normal, loading, disabled/locked, selected, pressed, empty và error/recovery nếu có.

## Phân lớp kiến trúc

```text
UI Theme / Tokens
        ↓
Primitives: Panel, Button, Text, Badge, Icon, Divider
        ↓
Components: Header, Card, Slot, OfferCard, Modal, RewardRow
        ↓
Screen shells: Home, Gameplay, Reward, Shop, Augment, Result
        ↓
Data binding: RunState, CircuitBoard, RunShop, rewards
```

### 1. Theme / Tokens

Chứa các giá trị dùng chung:

- Colors semantic: background, surface, elevated surface, primary, accent, warning, danger, muted.
- Typography levels: display, heading, body, label, caption.
- Spacing scale theo bội số 4.
- Border width, radius, outline, shadow/glow.
- Button hierarchy: primary, secondary, ghost, destructive.
- Animation durations và easing.

Không để screen tự định nghĩa lại màu, font size hoặc spacing nếu token tương ứng đã tồn tại.

### 2. Primitives

Các primitive không biết gameplay:

- `UiPanel`
- `UiButton`
- `UiText`
- `UiBadge`
- `UiIcon`
- `UiDivider`

Primitive chỉ nhận visual state và content. Không đặt logic mua item, chọn level hoặc pause combat vào primitive.

### 3. Components

Component là nhóm UI có ý nghĩa sản phẩm:

- `UiHeader`
- `UiLevelCard`
- `UiCircuitSlot`
- `UiOfferCard`
- `UiRewardRow`
- `UiModal`
- `UiSegmentedControl`

Component nhận data contract rõ ràng và phát event UI đơn giản. Logic thật nằm ở gameplay/run state.

### 4. Screen shells

Mỗi screen là một composition độc lập của components. Screen mới được thêm bằng cách compose base components, không sửa layout của screen đã final.

```text
ScreenRoot
 ├── SafeArea
 ├── Background
 ├── Header
 ├── Content
 └── BottomAction
```

## Cấu trúc thư mục dự kiến

```text
Assets/_TDS/UI/
  Theme/
    UiTheme.cs
    UiTheme.asset
  Primitives/
  Components/
  Screens/
    Home/
    Gameplay/
    Reward/
    Shop/
    Augment/
    Result/
  Binding/
  Tests/
```

UI prototype hiện tại (`GameplayHud`, `WaveUpgradePanel`, `HomeScene`) được giữ nguyên trong giai đoạn chuyển đổi. Không trộn class mới vào các file prototype này ngoài adapter/cutover cần thiết.

## Quy trình một màn hình

### Bước 1 - Reference audit

- Gắn reference vào đúng canvas `1080 x 2400`.
- Đánh dấu vùng: header, content, CTA, secondary actions, modal.
- Đo khoảng cách tương đối, không hard-code từng pixel ngay từ đầu.
- Ghi typography, màu semantic và trạng thái tương tác.

### Bước 2 - Static shell

Dựng background, panel, hierarchy và spacing bằng base components. Chưa nối gameplay data.

Acceptance:

- Không overlap ở `1080 x 2400`.
- Safe area không che content.
- Primary CTA dễ nhận biết trong 2–3 giây.
- Không có hơn 5–7 action areas cạnh tranh trên một screen.

### Bước 3 - Component states

Hoàn thiện normal, pressed, selected, disabled, locked và empty state trước khi binding data.

### Bước 4 - Data binding

Bind screen vào state hiện có:

- Home → `GameProgress`, `RunSelection`.
- Gameplay → `CircuitBoard`, `EnergyCircuit`, `BattleRunRewards`.
- Shop → `RunState`, `RunShop`, `RunOffer`.
- Augment → augment state.

Screen không tự random offer, tự trừ gold hoặc sửa board trực tiếp.

### Bước 5 - Runtime verification

- PlayMode smoke test cho hierarchy và state chính.
- Chạy ở canvas `1080 x 2400`.
- Kiểm tra text dài, locked state, zero gold, empty slot và modal mở/đóng.
- Kiểm tra không có missing script, exception hoặc click dead-end.

### Bước 6 - Cutover

Chỉ thay prototype screen bằng screen mới sau khi acceptance pass. Cutover là thay root/prefab của screen, không sửa ngược design system để chiều prototype cũ.

## Nguyên tắc mở rộng

- Muốn thêm screen mới: dùng token + primitive + component hiện có trước.
- Muốn thêm variant: thêm variant vào component, không copy toàn bộ prefab.
- Muốn thêm theme: tạo theme asset mới, không sửa từng screen.
- Muốn thêm card type: mở rộng data contract và `UiOfferCard`, không hard-code trong `ShopScreen`.
- Không copy screenshot làm background gameplay trừ khi đó là asset được xác nhận là production asset.

## Testing strategy

- Unit/EditMode: tokens, layout data, component state mapping, data binding thuần.
- PlayMode: screen root tồn tại, button state, modal flow, không có runtime error.
- Manual visual check: từng screen ở `1080 x 2400`, có screenshot before/after trong `Temp/Verification/`.
- Full campaign PlayMode chỉ chạy sau khi screen đã pass smoke test riêng.

## Definition of done

Một screen chỉ được gọi là final khi:

- Có screen shell riêng.
- Không phụ thuộc magic number riêng lẻ ngoài token/layout data.
- Có đủ interactive states chính.
- Có test runtime tối thiểu.
- Pass compile, EditMode và focused PlayMode.
- Có visual review so với reference ở `1080 x 2400`.
- Thêm một component/variant mới không cần sửa ngược các screen final khác.

## Ranh giới

### Luôn làm

- Dùng base component và token trước khi viết UI mới.
- Giữ data binding tách khỏi visual layout.
- Test focused trước, full verification sau mỗi checkpoint.
- Giữ reference ngoài code/runtime nếu chưa được xác nhận là production asset.

### Hỏi trước

- Đổi font/asset production.
- Thêm package UI mới.
- Thay đổi canvas orientation hoặc target resolution.
- Xóa prototype UI cũ.

### Không làm

- Không clone từng screenshot thành prefab riêng.
- Không nhúng gameplay logic vào button/card primitive.
- Không sửa một screen final để phục vụ một màn hình khác.
- Không chấp nhận layout đúng ở một resolution nhưng vỡ ở baseline.

## Reference input hiện có

- `000217c5818a01d4589b.jpg`
- `04b24272d43d54630d2c.jpg`
- `356ef8916edeee80b7cf.jpg`
- `3ce60719915611084847.jpg`
- `4e56809616d99687cfc8.jpg`
- `5c87b742210da153f81c.jpg`
- `6810a7d5319ab1c4e88b.jpg`
- `82433384a5cb25957cda.jpg`
- `8cbbc67a5035d06b8924.jpg`
- `a97974bfe2f062ae3be1.jpg`
- `Capture.PNG`

Các file reference không được sửa hoặc đưa vào commit nếu chưa có quyết định asset production riêng.
