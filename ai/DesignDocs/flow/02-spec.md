# 02 - Game Spec Draft

Status: `draft - waiting for interview answers`

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

## 7. Data Defaults MVP 0.1

```text
Base:
- maxHp: 100

Archer:
- damage: 10
- attackRange: 5
- attackCooldown: 1.0
- projectileSpeed: 10

Slime:
- maxHp: 30
- moveSpeed: 2
- attackDamage: 5

Wave 1:
- totalMonster: 10
- spawnInterval: 1.0
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
5. Confirm monsters spawn.
6. Confirm hero attacks.
7. Confirm monsters die or damage Base.
8. Confirm GameOver/Victory appears.
```

## 10. Boundaries

Always:

```text
- Update spec before changing design.
- Keep MVP small.
- Use namespace TDSurvivor.
- Keep code and content separated.
```

Ask first:

```text
- Copy files from Recovery.
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

## 11. Success Criteria MVP 0.1

```text
SC01 - Gameplay scene runs without red Console errors.
SC02 - Monster spawns from outside center area.
SC03 - Monster moves toward Base.
SC04 - Hero attacks Monster automatically.
SC05 - Projectile damages Monster.
SC06 - Monster dies at 0 HP.
SC07 - Monster damages Base if reaching Base.
SC08 - Base HP UI updates.
SC09 - GameOver when Base HP <= 0.
SC10 - Victory when all configured waves complete.
```

## 12. Open Questions

```text
OQ01 - Player controls movement, skills, upgrades, or placement?
OQ02 - Is the game more idle, survivor, or tower-defense strategy?
OQ03 - Target platform: PC, mobile, web?
OQ04 - Should MVP include upgrade choice after wave?
OQ05 - Should heroes stand fixed or arranged around Base?
OQ06 - Is Base a building, crystal, portal, or group HP?
```
