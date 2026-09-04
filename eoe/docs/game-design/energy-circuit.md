# Energy Circuit

## Mục đích

Energy Circuit là hệ thống tạo quyết định chiến thuật chính của game. Hero và item được đặt trên slot cố định; một pulse năng lượng đi qua từng slot theo thứ tự.

## Khái niệm

```text
[Hero] → [Generator] → [Item] → [Hero] → [Amplifier] → ...
                                      ↑
                               Energy pulse
```

Mỗi slot có:

- Nội dung hiện tại: hero, item hoặc trống.
- Số stack năng lượng.
- Ngưỡng kích hoạt.
- Trạng thái active/Overdrive.
- Hiệu ứng khi pulse đi qua.

## Luật đề xuất cho MVP

- Pulse chạy tuần tự qua 8 slot.
- Sau slot cuối, pulse quay lại slot đầu.
- Slot trống không nhận stack nhưng không chặn pulse.
- Slot có nội dung nhận stack khi pulse đi qua.
- Ngưỡng mặc định: 3 stack.
- Khi đủ ngưỡng, slot kích hoạt hiệu ứng.
- Hero kích hoạt Overdrive trong thời gian giới hạn.
- Stack được reset sau khi kích hoạt.
- Overdrive không cộng dồn; chỉ có một trạng thái Overdrive trên mỗi hero.

Các giá trị trên là giá trị tuning ban đầu, không phải hằng số thiết kế:

```text
slotCount = 8
pulseInterval = 0.5s
activationThreshold = 3
baseOverdriveDuration = 5s
```

Mục tiêu tuning: một hero thông thường có cơ hội kích hoạt Overdrive khoảng mỗi 8-15 giây, tùy vị trí và item hỗ trợ.

## Hero slot

Hero hoạt động bình thường khi chưa đủ stack. Khi Overdrive:

- Tăng tốc đánh hoặc damage.
- Có thể thêm projectile/AOE.
- Có thể thay đổi hành vi skill.
- Có hiệu ứng hình ảnh và âm thanh riêng.

Mỗi hero nên có một Overdrive identity rõ ràng:

- DPS hero: bắn nhanh hoặc nhiều projectile.
- AOE hero: explosion hoặc chain.
- Tank hero: shield, heal, damage reduction.
- Control hero: freeze, slow, knockback.

## Item slot

### Generator

Tạo thêm stack cho slot kế tiếp hoặc tăng tốc pulse trong phạm vi nhỏ.

### Amplifier

Tăng số stack hero nhận khi pulse đi qua.

### Battery

Giữ stack dư và dùng cho lần kích hoạt tiếp theo.

### Relay

Chuyển một phần stack sang slot kế bên.

### Converter

Đổi năng lượng thành một loại hiệu ứng khác:

- Damage.
- Shield.
- Heal.
- Cooldown reduction.

### Risk item

Đổi sức mạnh lấy rủi ro:

- Pulse nhanh hơn nhưng monster mạnh hơn.
- Overdrive mạnh hơn nhưng kéo dài ngắn hơn.
- Tăng reward nhưng spawn thêm elite.

## Combo

Combo nên dựa trên tag và thứ tự, không dựa trên hàng trăm điều kiện đặc biệt.

### Ví dụ

```text
[Mark] → [Lightning Hero] → [Chain Amplifier]
```

Mục tiêu bị Mark giúp chain ưu tiên hoặc gây thêm damage.

```text
[Oil] → [Fire Hero] → [Explosion]
```

Quái bị Oil và Fire có thể phát nổ khi chết.

```text
[Generator] → [Hero DPS] → [Relay] → [Hero AOE]
```

Hai hero nhận năng lượng theo các thời điểm khác nhau, tạo hai power window nối tiếp.

## UX bắt buộc

Người chơi phải đọc được hệ thống mà không cần mở menu:

- Pulse có màu và chuyển động rõ ràng.
- Stack hiển thị ngay trên slot.
- Slot sắp đầy có trạng thái báo trước.
- Overdrive có VFX, SFX và icon thời gian còn lại.
- Khi combo xảy ra, hiển thị tên combo ngắn.
- Không dùng quá nhiều màu cho các trạng thái khác nhau.

## Các rủi ro cần kiểm tra

### Chờ quá lâu mới có power

Nếu pulse quá chậm hoặc circuit quá dài, người chơi sẽ không cảm nhận được tác dụng của slot.

### Một build luôn tối ưu

Nếu Generator + Amplifier luôn tốt hơn mọi lựa chọn, Shop sẽ mất ý nghĩa. Cần có build boss, build dọn quái và build sống sót.

### Vị trí chỉ là hình thức

Nếu đổi vị trí không làm thay đổi thời điểm hoặc hiệu ứng, slot không tạo ra quyết định thực sự.

### Overdrive không nhìn thấy

Nếu buff chỉ là tăng stat ẩn, game sẽ mất feeling. Mỗi hero cần một thay đổi dễ quan sát.

## Chưa chốt

- Có cho phép đổi chiều pulse không.
- Có cho phép item tác động nhiều slot hay chỉ slot kế bên.
- Có cho phép stack tiếp tục tích trong lúc Overdrive đang active không.
- Có giới hạn số item cùng loại trong một circuit không.
