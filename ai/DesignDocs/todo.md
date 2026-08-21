# Todo - Các phần đã chốt

> File này được tạo từ kết quả interview ngày 30/07/2025.
> Pulled từ Notion page "Todo - Chốt các phần chưa rõ".

## ✅ ĐÃ CHỐT

### Gameplay
- Người chơi không điều khiển trực tiếp hero
- Hero tự động tấn công kẻ địch
- Người chơi có thể **force hướng** hero tấn công
- Quái vật spawn từ **SpawnerConfig** (các vị trí cấu hình)
- Thua khi **Base hết máu**

### Heroes
- **5 hero** trong MVP: 2 Archer, 2 Magic, 1 Buff/Control
- Có cả **Passive + Active skills**

### Enemies
- **3 loại monster**: Mob (số lượng), Elite (mạnh), Boss (cực mạnh)

### Progression
- Sau mỗi wave: **Roll** hoặc **Shop**
- **Roll**: chọn miễn phí 1 item, refresh 1 lần miễn phí
- **Shop**: dùng coin mua vật phẩm, có thể mua nhiều, refresh làm mới
- 2 resources: **Coin** (mua đồ) + **Exp** (tăng level)

### Platform & Engine
- **Mobile**
- **Unity 6 URP 2D**

### Kỹ thuật
- **Movement**: logic position + RVO (AgentSimulator từ Recovery)
- **Hit detection**: logic (IGrid từ Recovery)
- **Config**: Excel -> JSON (SpawnerConfig, MonsterConfig, HeroConfig...)
- **Pool**: từ GameToolkit (đã có base)
- **Tham khảo**: ưu tiên Recovery, đánh giá thêm Recovery 2

## ⏳ CHỜ XỬ LÝ

- [x] Duyệt spec (`flow/02-spec.md`) — approved 2026-07-31
- [x] Duyệt technical design (`flow/03-technical-design.md`) — approved 2026-07-31
- [x] Cập nhật quality gates (`flow/05-quality-gates.md`) — gates 1-4,7 checked 2026-07-31
- [x] Tạo các ADR còn thiếu — ADR-0002→0005 + ADR-0007 created 2026-07-31
- [x] Thiết kế SkillSystem module (portable GameToolkit + bridge _TDS) — xem ADR-0007
- [x] Build Slice 2: BaseCore + Health → GameOver (xong 2026-08-21: GameManager, BaseCore, WaveManager victory, GameplayUI)
- [x] SC04 Force attack direction (ForceTargetInput + Weapon forced target + scene setup menu)
- [ ] Slice 3 còn lại: Projectile IGrid hit (SC05), Passive/Active skills hoàn chỉnh (SC14)
- [ ] Slice 4: Coin/Exp runtime + Roll/Shop UI (SC11-SC13) — cần thêm ItemConfig/RollConfig excel
