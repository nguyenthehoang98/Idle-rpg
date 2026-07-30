# flow/01-interview

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/01-interview.md

# 01 - Interview Log

Dựa trên skill interview-me: hỏi từng câu một, kèm giả định, cho đến khi đủ tự tin để viết spec.

## 1. Current Hypothesis

```plain text
HYPOTHESIS:
Game là Hero Defense / Tower Defense 2D theo hướng PVE, trong đó người chơi chọn 4 hero vào trận để phòng thủ Sacred Core ở trung tâm trước các đợt quái plant spawn từ xung quanh.

CONFIDENCE:
~90%

Đã rõ:
- Engine: Unity
- Game: 2D
- Mode: PVE
- Core fantasy: đội hero cố thủ ở trung tâm bảo vệ Sacred Core
- Enemy: quái plant spawn từ xung quanh
- Player brings 4 heroes into battle

Chưa rõ:
- Tên chính thức / chỉ số / data model của từng hero MVP
- Tên chính thức và rule chi tiết của Sacred Core
- Chi tiết resource, upgrade, drop và economy
- Skill activation method cho từng hero
- Chi tiết monster loại thường / elite / boss
```

## 2. Interview Rules

```plain text
- Chỉ hỏi 1 câu tại một thời điểm.
- Mỗi câu phải có GUESS để người dùng phản biện nhanh.
- Không tự lấp chỗ trống nếu chưa xác nhận.
- Khi confidence >= 90%, chuyển sang spec.
```

## 3. Question Queue

### Q1 - Vai trò người chơi

```plain text
Q: Trong trận, người chơi chủ yếu điều khiển gì?
GUESS: Người chơi không điều khiển nhân vật di chuyển; 4 hero tự động chiến đấu, người chơi chủ yếu chọn nâng cấp/buff/item giữa wave và trong combat.
```

Status: confirmed

### Q2 - Cảm giác gameplay tham chiếu

```plain text
Q: Bạn muốn game gần với kiểu nào hơn?
GUESS: Gần Hero Defense / Survivor Defense hơn tower building truyền thống.
```

Status: confirmed

### Q3 - Căn cứ hay nhân vật là điều kiện thua?

```plain text
Q: Người chơi thua khi căn cứ vỡ, hay khi toàn bộ nhân vật chết?
GUESS: MVP nên thua khi Sacred Core ở giữa hết máu.
```

Status: confirmed

### Q4 - Đội hình nhân vật

```plain text
Q: Ban đầu có 1 nhân vật hay một nhóm nhiều nhân vật?
GUESS: Player mang 4 hero vào trận theo 4 slot active.
```

Status: confirmed

### Q5 - Progression trong trận

```plain text
Q: Nâng cấp diễn ra khi nào?
GUESS: Có 2 nhịp chính: sau mỗi wave chọn buff/chỉ số, và trong combat dùng energy để roll item ngẫu nhiên có thời hạn.
```

Status: confirmed

### Q6 - Platform

```plain text
Q: Game ưu tiên PC hay mobile?
GUESS: MVP làm trong Unity trước, chưa chốt target platform cụ thể.
```

Status: pending

### Q7 - Recovery reference

```plain text
Q: Bạn muốn dùng Recovery như mức nào?
GUESS: Chỉ tham khảo cấu trúc spawn/config/pool, không copy file cho tới khi MVP thiết kế xong.
```

Status: confirmed

## 4. Decisions Confirmed

```plain text
D01 - Unity 2D.
D02 - PVE / Hero Defense.
D03 - Player brings 4 heroes into battle.
D04 - Heroes stay fixed around center; do not move.
D05 - Enemies move toward heroes/core.
D06 - Player protects Sacred Core.
D07 - Player focuses on selecting, upgrading, and using animal heroes.
D08 - Game world has no humans; world consists of animals and plants.
D09 - Mutation event changed the ecosystem.
D10 - Some animals evolved into Animal Heroes.
D11 - Mutated plants became monsters.
D12 - Recovery is for reference only.
D13 - Use agent-skills design flow.
```

## 5. Stop Condition

Chỉ chuyển qua 02-spec.md khi trả lời đủ:

```plain text
- Player role
- Core loop
- MVP scope
- Win/Lose condition
- First hero/enemy/base model
- Platform target
- What to reuse/reference from Recovery
```

## 6. Latest Interview Decisions Update

### Game Identity

```plain text
- Game name: Animal Tower Defense.
- Genre direction: Hero-based Tower Defense / Hero Defense.
- World has no humans.
- World consists of animals and plants.
```

### World Setting

```plain text
- A mutation event changed the ecosystem.
- Some animals evolved into heroes.
- Mutated plants became monsters.
- Animal Heroes protect the Sacred Core (Thân Quý).
```

### Player Role Update

```plain text
Player does not build towers.

Player role:
- Select animal hero team.
- Enter battle.
- Defend the Sacred Core.
- Collect resources.
- Upgrade heroes.
- Continue fighting harder battles.
```

### Battle Direction Update

```plain text
Confirmed:
- Heroes stay fixed in the center area.
- Heroes do not move.
- Enemies move toward the defense point.
- Heroes automatically attack.
- Each hero has unique skills.
- Active team contains 4 hero slots.
```

### Hero Direction Update

```plain text
Total planned heroes:
- Around 30 heroes.

MVP:
- 5 heroes.

Main classes:
1. Archer
   - Ranged physical damage.

2. Mage
   - Ranged magic damage.
   - Area damage focus.

3. Support Mage
   - CC.
   - Buff.
   - Debuff.
   - Team support.
```

### Confirmed Open Questions

```plain text
- Official name/design of Sacred Core.
- Define MVP 5 heroes.
- Define hero roles.
- Define hero skills and activation.
- Define plant monster types.
- Define resource system.
- Define upgrade/evolution system.
```
