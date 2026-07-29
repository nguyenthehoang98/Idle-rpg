# 01 - Interview Log

Dựa trên skill `interview-me`: hỏi từng câu một, kèm giả định, cho đến khi đủ tự tin để viết spec.

## 1. Current Hypothesis

```text
HYPOTHESIS:
Bạn muốn làm game Unity 2D dạng Tower Defense / Survivor, trong đó một nhóm nhân vật ở giữa màn hình tự động bảo vệ căn cứ trước đàn quái spawn từ xung quanh.

CONFIDENCE:
~65%

Đã rõ:
- Engine: Unity
- Game: 2D
- Core fantasy: nhóm nhân vật phòng thủ trung tâm
- Enemy: đàn quái vật tấn công từ xung quanh

Chưa rõ:
- Người chơi điều khiển gì trực tiếp?
- Game thiên về idle, survivor, hay tower defense chiến thuật?
- MVP đầu tiên cần cảm giác chơi giống game nào?
- Art style và platform mục tiêu?
```

## 2. Interview Rules

```text
- Chỉ hỏi 1 câu tại một thời điểm.
- Mỗi câu phải có GUESS để người dùng phản biện nhanh.
- Không tự lấp chỗ trống nếu chưa xác nhận.
- Khi confidence >= 90%, chuyển sang spec.
```

## 3. Question Queue

### Q1 - Vai trò người chơi

```text
Q: Trong trận, người chơi chủ yếu điều khiển gì?
GUESS: Người chơi không điều khiển nhân vật di chuyển; nhân vật tự đánh, người chơi chủ yếu chọn nâng cấp/kỹ năng sau mỗi wave.
```

Status: `pending`

### Q2 - Cảm giác gameplay tham chiếu

```text
Q: Bạn muốn game gần với kiểu nào hơn?
GUESS: Gần Vampire Survivors/Survivor.io hơn Kingdom Rush, vì quái tới từ mọi hướng và nhân vật đứng trung tâm.
```

Status: `pending`

### Q3 - Căn cứ hay nhân vật là điều kiện thua?

```text
Q: Người chơi thua khi căn cứ vỡ, hay khi toàn bộ nhân vật chết?
GUESS: MVP nên thua khi Base/Core ở giữa hết máu để đơn giản.
```

Status: `pending`

### Q4 - Đội hình nhân vật

```text
Q: Ban đầu có 1 nhân vật hay một nhóm nhiều nhân vật?
GUESS: MVP bắt đầu với 1 Archer tự bắn, sau đó mở rộng thành đội hình 3-5 nhân vật.
```

Status: `pending`

### Q5 - Progression trong trận

```text
Q: Nâng cấp diễn ra khi nào?
GUESS: Sau mỗi wave hiện bảng chọn 1 trong 3 nâng cấp như tăng damage, attack speed, range.
```

Status: `pending`

### Q6 - Platform

```text
Q: Game ưu tiên PC hay mobile?
GUESS: Mobile/PC đều được, nhưng MVP làm PC trong Unity Editor trước, UI sau đó tối ưu mobile.
```

Status: `pending`

### Q7 - Recovery reference

```text
Q: Bạn muốn dùng Recovery như mức nào?
GUESS: Chỉ tham khảo kiến trúc spawn/config/pool, không copy file cho tới khi MVP thiết kế xong.
```

Status: `pending`

## 4. Decisions Confirmed

```text
D01 - Unity 2D.
D02 - Tower Defense / Survival Defense.
D03 - Nhóm nhân vật ở trung tâm bảo vệ trước đàn quái.
D04 - Phải thiết kế docs trước khi copy/code tiếp.
D05 - Recovery dùng làm cấu trúc tham khảo.
D06 - Áp dụng agent-skills cho design flow.
```

## 5. Stop Condition

Chỉ chuyển qua `02-spec.md` khi trả lời đủ:

```text
- Player role
- Core loop
- MVP scope
- Win/Lose condition
- First hero/enemy/base model
- Platform target
- What to reuse/reference from Recovery
```
