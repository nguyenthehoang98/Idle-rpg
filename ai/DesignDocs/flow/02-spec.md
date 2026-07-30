# 02 - Game Spec Draft

Status: `updated - interview answers applied, chờ duyệt`

Dựa trên skill `spec-driven-development`.

## 1. Objective

Xây dựng prototype Unity 2D cho game Tower Defense / Survival Defense.

Người chơi bảo vệ điểm trung tâm bằng một hoặc nhiều nhân vật tự động tấn công quái vật xuất hiện từ các hướng xung quanh.

## 2. Target Player

TBD sau interview.

Gợi ý lựa chọn:

```text
A. Người chơi thích game idle nhẹ, nâng cấp là chính.
B. Người chơi thích survivor action, nhịp nhanh, nhiều quái.
C. Người chơi thích tower defense chiến thuật, build đội hình/tower.
```

## 3. Core Fantasy

```text
Một nhóm anh hùng cố thủ ở trung tâm, chống lại làn sóng quái vật ngày càng đông và mạnh.
```

## 4. Core Loop

Draft:

```text
Start wave
-> Monsters spawn from screen edges
-> Monsters move toward Base/Core
-> Heroes auto-target and attack
-> Monsters die or reach Base
-> Player survives wave
-> Player chooses upgrade
-> Next wave starts harder
```

## 5. MVP 0.1 Scope

In scope:

```text
- 1 gameplay scene
- 1 Base/Core
- 1 Hero: Archer
- 1 Monster: Slime
- 1 Projectile: Arrow
- Monster spawn from 4 portals
- Hero auto attack nearest monster in range
- Health/damage/death flow
- Base damage and Game Over
- Simple waves
- Basic UI: Base HP, Wave, Game State
```

Out of scope MVP 0.1:

```text
- Multiple heroes
- Skills
- Upgrades
- Boss
- Save/load
- Shop/meta progression
- Addressables
- Excel import
- Object pool
- Advanced avoidance
- Real art/audio
```

## 6. Gameplay Rules

### Base/Core

```text
- Base is placed at center.
- Base has HP.
- If Base HP <= 0, game state becomes GameOver.
```

### Hero

```text
- Hero does not move in MVP 0.1.
- Hero scans for monsters in attack range.
- Hero attacks nearest valid monster.
- Hero uses cooldown between attacks.
```

### Monster

```text
- Monster spawns from portal.
- Monster moves directly toward Base.
- Monster has HP.
- Monster dies when HP <= 0.
- Monster damages Base when reaching Base.
- Monster is removed after damaging Base.
```

### Projectile

```text
- Projectile is spawned by Hero.
- Projectile follows target.
- Projectile deals damage once.
- Projectile is destroyed after hit, timeout, or target missing.
```

## 7. Data Strategy

```text
- All config data from Excel -> JSON (SpawnerConfig, MonsterConfig, HeroConfig...)
- Use Recovery's Excel pipeline as reference.
- Pool từ GameToolkit.
- Movement: RVO via AgentSimulator (Recovery).
- Hit detection: logic check via IGrid (Recovery).
```

## 8. Project Structure

```text
Assets/_TDSurvivor/Code
Assets/_TDSurvivor/Content
ai/DesignDocs
```

Detailed structure: see `02-proposed-folder-structure.md`.

## 9. Commands / Verification

TBD after Unity version/package setup confirmed.

Manual verification for now:

```text
1. Open Unity project.
2. Open Gameplay scene.
3. Check Console no red errors.
4. Press Play.
5. Confirm monsters spawn from SpawnerConfig positions.
6. Confirm heroes auto-attack (2 Archer, 2 Magic, 1 Buff/Control).
7. Confirm RVO movement (AgentSimulator) works.
8. Confirm hit detection (IGrid) works.
9. Confirm monsters die or damage Base.
10. Confirm Roll/Shop appears after wave.
11. Confirm GameOver when Base HP <= 0.
```

## 10. Boundaries

Always:

```text
- Update spec before changing design.
- Keep MVP focused on 5 heroes + 3 monster types.
- Use namespace TDSurvivor.
- Keep code and content separated.
- Config from Excel -> JSON pipeline.
```

Ask first:

```text
- Copy files from Recovery/Recovery 2.
- Add plugins/packages.
- Introduce Addressables/UniTask/ExcelExtension.
- Change Unity project settings.
```

Never:

```text
- Copy `.meta`, `.prefab`, `.unity` blindly from Recovery.
- Implement unapproved scope.
- Hide assumptions.
```

## 11. Success Criteria

```text
SC01 - Gameplay scene runs without red Console errors.
SC02 - Monsters spawn from SpawnerConfig positions.
SC03 - Monsters move toward Base using RVO (AgentSimulator).
SC04 - Heroes auto-attack, can force attack direction.
SC05 - Projectile damages Monster via IGrid logic.
SC06 - Monster dies at 0 HP.
SC07 - Monster damages Base if reaching Base.
SC08 - Base HP UI updates.
SC09 - GameOver when Base HP <= 0.
SC10 - Victory when all configured waves complete.
SC11 - Roll appears after wave: free 1 item + 1 refresh.
SC12 - Shop appears: buy items with coin, refresh available.
SC13 - Coin and Exp system works (Exp = level, Coin = mua đồ).
SC14 - Passive + Active skills functional.
SC15 - 3 monster tiers: Mob, Elite, Boss behave correctly.
```

## 12. Closed Questions (từ Interview)

```text
CQ01 - Người chơi không điều khiển gì, có thể force hướng hero.
CQ02 - Game cho Mobile.
CQ03 - 5 Hero: 2 Archer, 2 Magic, 1 Buff/Control.
CQ04 - Progression: Roll (free 1 + 1 refresh) + Shop (coin).
CQ05 - Enemy: 3 loại Mob, Elite, Boss.
CQ06 - Hệ thống: Coin + Exp, Passive + Active skill.
CQ07 - Unity 6 URP 2D, Excel -> JSON config.
CQ08 - Movement: RVO (AgentSimulator), Hit: logic (IGrid).
CQ09 - Pool: từ GameToolkit.
CQ10 - Tham khảo: ưu tiên Recovery.
```
