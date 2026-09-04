# Shop và Augments

## Trạng thái

**MVP contract v1 - đã chốt để implement.**

## Mục đích

Shop và Augment tạo ra quyết định giữa các wave. Combat vẫn tự động và nhanh, nhưng người chơi phải chuẩn bị cho wave tiếp theo.

## Shop timing

Shop mở sau mỗi wave. Gold đến từ hoàn thành wave, elite kill và boss reward; regular kill không trực tiếp cộng gold trong MVP.

Thứ tự mặc định:

```text
Wave kết thúc
  ↓
Nhận reward
  ↓
Nếu là checkpoint: chọn Augment
  ↓
Mở Shop
  ↓
Sắp xếp board
  ↓
Bắt đầu wave mới
```

Augment xuất hiện trước Shop để người chơi có thể mua item phù hợp với Augment vừa chọn.

## Offer structure MVP

Mỗi lần mở Shop:

- 1 hero offer.
- 3 item offer.
- 1 upgrade hoặc utility offer.
- 1 refresh miễn phí.

Offer cần có tag để người chơi hiểu synergy:

```text
Energy / Fire / Lightning / Defense / Critical / AOE
```

Quy tắc phân phối đề xuất:

- 2 offer có liên quan một phần tới build hiện tại.
- 1 offer tạo hướng mới hoặc rủi ro.
- Không để toàn bộ offer là item không thể dùng.

## Mua và thay đổi slot

Người chơi có thể:

- Mua hero/item và giữ chúng đến hết run.
- Bán hero/item khỏi board.
- Đổi vị trí các slot.
- Để slot trống.
- Thay hero active bằng hero mới.

MVP giới hạn 3 hero active và không có bench. Trong prototype, bán lại 100% giá để kiểm thử build nhanh; thay đổi này chỉ được cân nhắc sau khi core loop ổn định.

## Hero trong Shop

MVP chỉ cần một số vai trò rõ ràng:

- DPS: damage đơn mục tiêu, hợp boss.
- AOE: dọn nhiều quái.
- Control: slow/freeze/knockback.
- Sustain: heal/shield/lifesteal.

Hero không nên chỉ khác nhau ở damage. Mỗi hero cần có Overdrive riêng để vị trí trên circuit có ý nghĩa.

## Augment rules

- Augment miễn phí.
- Chọn 1 trong 3.
- Tác dụng kéo dài đến hết run.
- Augment thay đổi luật hoặc mở hướng build.
- Không cho quá nhiều Augment cùng lúc trong MVP.

## Augment mẫu

### Energy

**Overcharge**

- Mỗi pulse thứ 4 cho thêm stack.

**Overflow**

- Stack dư không bị mất hoàn toàn, được lưu vào Battery pool.

**Conductive Circuit**

- Khi hero kích hoạt Overdrive, hero kế bên nhận một phần stack.

### Combat

**Chain Reaction**

- Monster chết trong Overdrive có cơ hội phát nổ.

**Execution Order**

- Tăng damage lên monster dưới 30% HP.

**Emergency Protocol**

- Hero dưới 40% HP nhận thêm stack.

### Economy

**Scavenger**

- Elite và boss cho thêm gold.

**Risky Power**

- Monster nhiều hoặc mạnh hơn, nhưng Shop cho thêm offer.

**Emergency Reroll**

- Mỗi lần kích hoạt Overdrive nhận một refresh token.

## Augment checkpoint MVP

Flow cố định là `Reward → Augment (sau wave 3) → Shop → Next wave`.

Với run 5 wave:

- Chọn Augment sau wave 3.
- Sau này có thể thêm checkpoint sau wave 6 và 9 nếu run dài hơn.

Không nên mở Augment sau mọi wave vì lựa chọn sẽ mất giá trị và người chơi phải đọc quá nhiều thông tin.

## Tiêu chí một Augment tốt

Một Augment tốt phải trả lời được ít nhất một câu hỏi:

- Nó thay đổi vị trí item như thế nào?
- Nó thay đổi thời điểm Overdrive như thế nào?
- Nó tạo combo mới nào?
- Nó buộc người chơi chấp nhận rủi ro gì?
- Nó khiến một hero/item trước đây yếu trở nên hữu dụng ra sao?

## Không làm trong MVP

- Rarity nhiều tầng.
- Augment reroll phức tạp.
- Deckbuilding có draw/discard.
- Shop refresh vô hạn.
- Meta currency và nâng cấp vĩnh viễn.
- Hàng chục loại offer chỉ khác giá trị số.
