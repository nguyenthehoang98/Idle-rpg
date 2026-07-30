# 01 - Interview Log

Dựa trên skill `interview-me`: hỏi từng câu một, kèm giả định, cho đến khi đủ tự tin để viết spec.

## 1. Current Hypothesis

```text
HYPOTHESIS:
Bạn muốn làm game Unity 2D dạng Tower Defense / Survivor, trong đó một nhóm nhân vật ở giữa màn hình tự động bảo vệ căn cứ trước đàn quái spawn từ xung quanh.

CONFIDENCE:
~95%

Đã rõ:
- Engine: Unity 6 URP 2D
- Game: 2D Tower Defense / Survivor
- Core fantasy: nhóm 5 hero ở trung tâm tự động bảo vệ căn cứ
- Enemy: quái vật spawn từ SpawnerConfig, di chuyển theo RVO
- Người chơi: không điều khiển trực tiếp, có thể force hướng tấn công
- Platform: Mobile
- Progression: Roll (miễn phí 1 item/refresh 1 lần) + Shop (dùng coin mua)
- Recovery: ưu tiên Recovery, dùng AgentSimulator, IGrid, Config, Pool từ GameToolkit
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
Q: Trong trận, người chủ yếu điều khiển gì?
GUESS: Người chơi không điều khiển nhân vật di chuyển; nhân vật tự đánh, người chơi chủ yếu chọn nâng cấp/kỹ năng sau mỗi wave.

ANSWER: Người chơi không điều khiển gì cả. Hero tự tấn công kẻ địch.
Người chơi có thể force hướng hero tấn công.
```

Status: `answered ✅`

### Q2 - Cảm giác gameplay tham chiếu

```text
Q: Bạn muốn game gần với kiểu nào hơn?
GUESS: Gần Vampire Survivors/Survivor.io hơn Kingdom Rush, vì quái tới từ mọi hướng và nhân vật đứng trung tâm.

ANSWER: Quái vật xuất hiện dựa theo các vị trí của SpawnerConfig.
```

Status: `answered ✅`

### Q3 - Căn cứ hay nhân vật là điều kiện thua?

```text
Q: Người chơi thua khi căn cứ vỡ, hay khi toàn bộ nhân vật chết?
GUESS: MVP nên thua khi Base/Core ở giữa hết máu để đơn giản.

ANSWER: Hết máu -> Thua.
```

Status: `answered ✅`

### Q4 - Đội hình nhân vật

```text
Q: Ban đầu có 1 nhân vật hay một nhóm nhiều nhân vật?
GUESS: MVP bắt đầu với 1 Archer tự bắn, sau đó mở rộng thành đội hình 3-5 nhân vật.

ANSWER: MVP phải có tối thiểu 5 hero. 1 gameplay đã dùng tới 4 hero rồi.
Phân bố: 2 Archer, 2 Magic, 1 Buff/Control.
```

Status: `answered ✅`

### Q5 - Progression trong trận

```text
Q: Nâng cấp diễn ra khi nào?
GUESS: Sau mỗi wave hiện bảng chọn 1 trong 3 nâng cấp như tăng damage, attack speed, range.

ANSWER: Mỗi khi hoàn thành 1 wave sẽ hiện ra Roll hoặc Shop:
- **Roll**: chọn miễn phí 1 item, refresh 1 lần miễn phí/roll.
- **Shop**: dùng coin để mua vật phẩm, có thể mua nhiều, refresh làm mới.
```

Status: `answered ✅`

### Q6 - Platform

```text
Q: Game ưu tiên PC hay mobile?
GUESS: Mobile/PC đều được, nhưng MVP làm PC trong Unity Editor trước, UI sau đó tối ưu mobile.

ANSWER: Game cho Mobile.
```

Status: `answered ✅`

### Q7 - Recovery reference

```text
Q: Bạn muốn dùng Recovery như mức nào?
GUESS: Chỉ tham khảo kiến trúc spawn/config/pool, không copy file cho tới khi MVP thiết kế xong.

ANSWER: Đánh giá module của Recovery và Recovery 2. Ưu tiên Recovery.
Dùng AgentSimulator.cs (RVO), IGrid.cs (hit-detection logic),
Config files (SpawnerConfig, MonsterConfig...), Pool từ GameToolkit.
```

Status: `answered ✅`

## 4. Decisions Confirmed

```text
D01 - Unity 6 URP 2D.
D02 - Tower Defense / Survival Defense.
D03 - Nhóm 5 hero ở trung tâm bảo vệ Base.
D04 - Phải thiết kế docs trước khi copy/code tiếp.
D05 - Recovery ưu tiên làm tham khảo chính.
D06 - Áp dụng agent-skills cho design flow.
D07 - Platform: Mobile.
D08 - Người chơi không điều khiển trực tiếp, chỉ force hướng.
D09 - Quái spawn theo SpawnerConfig.
D10 - Thua khi Base hết máu.
D11 - 5 hero: 2 Archer, 2 Magic, 1 Buff/Control.
D12 - Progression: Roll (free 1 item + 1 refresh) + Shop (coin).
D13 - Coin + Exp. Exp = level, Coin = mua đồ.
D14 - Enemy: mob, elite, boss.
D15 - Có cả Passive + Active skill.
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
