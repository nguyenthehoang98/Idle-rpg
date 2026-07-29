# 02 - Cấu trúc thư mục đề xuất

Đích: `C:\Users\Hoang PC\Documents\Idle-rpg\tdsurvivor\Assets`

## 1. Cấu trúc Assets giai đoạn thiết kế

```text
Assets/
├── _GameToolkit/          # Toolkit dùng chung (từ Recovery + Recovery2)
├── _TDS/                  # Scripts gameplay (từ Recovery + Recovery2)
├── _TDS Source/           # Assets (prefab, texture, audio, scenes...)
└── _DesignDocs/
```

## 2. Cấu trúc dự án đề xuất sau khi duyệt (đã cập nhật từ Recovery & Recovery2)

```text
Assets/
├── _GameToolkit/
│   ├── Avoidance/
│   │   ├── AgentSimulator.cs
│   │   ├── FixedUniformGrid.cs
│   │   └── IGrid.cs
│   ├── Collision/
│   │   ├── BaseCollision.cs
│   │   ├── BoxCollision2D.cs
│   │   ├── CapsuleCollision2D.cs
│   │   ├── CircleCollision2D.cs
│   │   ├── Collision3D.cs
│   │   ├── CollisionUpdater.cs
│   │   └── ICollision.cs
│   ├── Entity/
│   │   ├── ComponentManager.cs
│   │   ├── EntityChangedEvent.cs
│   │   ├── EntityManager.cs
│   │   ├── Parameter.cs
│   │   └── Editor/
│   │       └── EntityCheckerWindow.cs
│   ├── GameConfig/
│   │   ├── ConfigManager.cs
│   │   ├── ConfigPath.cs
│   │   ├── IGameConfig.cs
│   │   └── Editor/
│   │       ├── ConfigDownloaderWindow.cs
│   │       └── ConfigValidatorWindow.cs
│   ├── Grid/
│   │   ├── AgentSimulator.cs
│   │   ├── FixedUniformGrid.cs
│   │   ├── IGrid.cs
│   │   └── RVO2/
│   │       ├── Agent.cs
│   │       ├── KdTree.cs
│   │       ├── Line.cs
│   │       ├── Obstacle.cs
│   │       ├── RVOMath.cs
│   │       └── Simulator.cs
│   ├── Resource/
│   │   ├── AssetBundleManager.cs
│   │   ├── AssetManager.cs
│   │   ├── CloudBundleLoader.cs
│   │   ├── IBundleLoader.cs
│   │   ├── LocalBundleLoader.cs
│   │   ├── Pool.cs
│   │   ├── PoolInternal.cs
│   │   └── PoolViewerWindow.cs
│   ├── Startup/
│   │   └── BootScene.cs
│   ├── Statistics/
│   │   ├── Stat.cs
│   │   └── StatModifier.cs
│   ├── SystemSkills/
│   │   ├── BaseActionTests.cs
│   │   ├── Core/
│   │   │   ├── BaseAction.cs
│   │   │   ├── BaseCollider.cs
│   │   │   ├── BaseTrajectory.cs
│   │   │   ├── FindTargetType.cs
│   │   │   ├── IAction.cs
│   │   │   ├── IQuery.cs
│   │   │   ├── QueryResult.cs
│   │   │   └── Spu.cs
│   │   └── Implement/
│   │       ├── BoomerangTrajectory.cs
│   │       ├── CastProjectileAction.cs
│   │       ├── CircleCollider.cs
│   │       ├── ColliderData.cs
│   │       ├── ColliderType.cs
│   │       ├── ProjectileTrajectory.cs
│   │       ├── RectangleCollider.cs
│   │       ├── SplineTrajectory.cs
│   │       ├── StationaryTrajectory.cs
│   │       └── TrajectoryData.cs
│   ├── TimeScaleToolbar/
│   │   └── Editor/
│   │       ├── TimeScaleToolbar.cs
│   │       ├── ToolbarCallback.cs
│   │       └── ToolbarExtender.cs
│   ├── Updater/
│   │   ├── BaseUpdatable.cs
│   │   ├── ITickable.cs
│   │   ├── TickSystemOwner.cs
│   │   ├── UpdaterOwner.cs
│   │   └── Editor/
│   │       └── TickSystemOwnerEditor.cs
│   └── Utils/
│       ├── CollectionUtils.cs
│       ├── CoroutineUtils.cs
│       ├── KitEntryScene.cs
│       ├── MathUtils.cs
│       ├── RandomUtils.cs
│       ├── SafeArea.cs
│       ├── Timing.cs
│       └── UnityMainThreadDispatcher.cs
│
├── _TDS/ (scripts only)
│   ├── Boot/
│   │   ├── GameBootScene.cs
│   │   └── GameEntry.cs
│   ├── Home/
│   │   └── HomeScene.cs
│   ├── Config/
│   │   ├── LevelConfig.cs
│   │   ├── LinearFormula.cs
│   │   ├── MonsterConfig.cs
│   │   ├── PlayerConfig.cs
│   │   ├── PowerFormula.cs
│   │   ├── SkillConfig.cs
│   │   ├── SpawnerConfig.cs
│   │   └── WeaponConfig.cs
│   ├── Gameplay/
│   │   ├── Data/
│   │   │   ├── HealthData.cs
│   │   │   ├── MonsterRuntimeData.cs
│   │   │   ├── PlayerRuntimeData.cs
│   │   │   ├── SkillRuntimeData.cs
│   │   │   └── StatData.cs
│   │   ├── Manager/
│   │   │   ├── AgentManager.cs
│   │   │   ├── GameManager.cs
│   │   │   ├── MonsterEntityManager.cs
│   │   │   ├── SkillManager.cs
│   │   │   ├── SoundManager.cs
│   │   │   └── SpawnManager.cs
│   │   ├── Monsters/
│   │   │   ├── Monster.cs
│   │   │   ├── MonsterMoveUpdater.cs
│   │   │   └── MonsterRuntimeData.cs
│   │   ├── Projectiles/
│   │   │   └── Projectile.cs
│   │   ├── Views/
│   │   │   ├── BaseAnimation.cs
│   │   │   ├── BottomPanel.cs
│   │   │   ├── CardItem.cs
│   │   │   ├── Energy.cs
│   │   │   ├── EquipmentActivation.cs
│   │   │   ├── EquipmentQueue.cs
│   │   │   ├── EquipmentSlot.cs
│   │   │   ├── IViewTick.cs
│   │   │   ├── ImageAnimation.cs
│   │   │   ├── PowerItem.cs
│   │   │   ├── SpriteRendererAnimation.cs
│   │   │   └── UpgradeCardUIPicker.cs
│   │   ├── Utils/
│   │   │   ├── Const.cs
│   │   │   ├── Formula.cs
│   │   │   ├── GizmosLine.cs
│   │   │   └── Path.cs
│   │   ├── BaseWeapon.cs
│   │   ├── DelayDestroyObject.cs
│   │   ├── DependencyResetAttack.cs
│   │   ├── EntityQuery.cs
│   │   ├── EventDestroyObject.cs
│   │   ├── EventSfx.cs
│   │   ├── FlyWeapon.cs
│   │   ├── GameplayStartup.cs
│   │   ├── MonsterSortingLayer.cs
│   │   ├── ProjectileWeapon.cs
│   │   ├── SpawnTimer.cs
│   │   ├── SpawnerUpdater.cs
│   │   └── TextDamage.cs
│   ├── Statistics/
│   │   ├── StatGlobal.cs
│   │   └── Stats.cs
│   └── Utils/
│       └── LocalizeManager.cs
│
├── _TDS Source/ (assets only - không scripts)
│   ├── Gameplay/
│   │   ├── Config/
│   │   │   ├── LevelConfig.json
│   │   │   ├── MonsterConfig.json
│   │   │   ├── PlayerConfig.json
│   │   │   ├── SkillConfig.json
│   │   │   ├── SpawnerConfig.json
│   │   │   └── WeaponConfig.json
│   │   ├── Audio/
│   │   │   ├── Gun.ogg
│   │   │   ├── Impact [Source].mp3
│   │   │   ├── Knife.ogg
│   │   │   ├── Laze.ogg
│   │   │   ├── button.wav
│   │   │   ├── energy.ogg
│   │   │   ├── entergy_full.ogg
│   │   │   ├── hammer.ogg
│   │   │   ├── impact_wood_01.wav
│   │   │   ├── level_up.ogg
│   │   │   └── power_select.ogg
│   │   ├── Monsters/
│   │   │   ├── Materials/
│   │   │   │   ├── Default Monster Material.mat
│   │   │   │   ├── Image_Dissolve Material.mat
│   │   │   │   ├── Sprite_Lit_Brightness_Material.mat
│   │   │   │   ├── Sprite_Lit_Default_Material.mat
│   │   │   │   └── Sprite_Outline_Material.mat
│   │   │   ├── Prefabs/
│   │   │   │   ├── Bee.Monster.prefab
│   │   │   │   └── monster.1001.prefab
│   │   │   └── Textures/
│   │   │       └── icon3001.png
│   │   ├── Weapons/
│   │   │   ├── Animation/
│   │   │   │   ├── default animator.controller
│   │   │   │   ├── default attack.anim
│   │   │   │   ├── default idle.anim
│   │   │   │   ├── none attack.anim
│   │   │   │   └── weapon none attack animator.overrideController
│   │   │   ├── Prefabs/
│   │   │   │   ├── Blaster.Equipment.prefab
│   │   │   │   ├── Blaster.Projectile.prefab
│   │   │   │   ├── Famas.Equipment.prefab
│   │   │   │   ├── Famas.Projectile.prefab
│   │   │   │   ├── FlameThrower.Equipment.prefab
│   │   │   │   ├── FlameThrower.Projectile.prefab
│   │   │   │   ├── HammerThunder.Equipment.prefab
│   │   │   │   ├── HammerThunder.Projectile.prefab
│   │   │   │   ├── Katana.Equipment.prefab
│   │   │   │   ├── Katana.Spline.prefab
│   │   │   │   ├── Nunchaku.Equipment.prefab
│   │   │   │   └── Rapier.Equipment.prefab
│   │   │   └── Textures/
│   │   │       ├── Icon1001.png
│   │   │       ├── Icon1002.png
│   │   │       ├── Icon1004.png
│   │   │       ├── Icon1006.png
│   │   │       ├── icon1003.png
│   │   │       ├── icon1005.png
│   │   │       └── icon1007.png
│   │   ├── Projectiles/
│   │   │   └── Textures/
│   │   │       ├── icon2001.png
│   │   │       ├── icon2003.png
│   │   │       ├── icon2006.png
│   │   │       └── icon2007.png
│   │   ├── Scenes/
│   │   │   ├── 1.background.prefab
│   │   │   ├── BootScene.unity
│   │   │   └── GamePlayScene.unity
│   │   ├── Textures/
│   │   │   ├── circle.png
│   │   │   └── grid.png
│   │   ├── Heroes/
│   │   │   ├── Diamond/
│   │   │   │   ├── diamond Idle.anim
│   │   │   │   ├── diamond animator.controller
│   │   │   │   ├── diamond.hero.prefab
│   │   │   │   └── diamond.png
│   │   │   └── w9/
│   │   │       ├── 9.wing.prefab
│   │   │       ├── wing9 animator.controller
│   │   │       ├── wing9 idle.anim
│   │   │       └── wing9.png
│   │   └── VFX/
│   │       ├── fx.death.prefab
│   │       ├── ui.hammer.equipment.prefab
│   │       └── ui.smoke.equipment.prefab
│   ├── Fonts/
│   │   ├── EmojiOne Attribution.txt
│   │   ├── EmojiOne.json
│   │   ├── EmojiOne.png
│   │   ├── Minecraft SDF.asset
│   │   ├── Minecraft SDF.mat
│   │   └── Minecraft.ttf
│   ├── UI/
│   │   ├── Animations/
│   │   │   ├── slot animator.controller
│   │   │   ├── slot.idle.anim
│   │   │   ├── slot.push.anim
│   │   │   ├── slot.release.anim
│   │   │   ├── textdamage animator.controller
│   │   │   └── textdamage.play.anim
│   │   ├── Prefabs/
│   │   │   ├── Equipment.Slot.prefab
│   │   │   ├── TextDamageCritical.prefab
│   │   │   ├── TextDamageNormal.prefab
│   │   │   ├── UI.BarEnergy.prefab
│   │   │   ├── UI.BarHealth.prefab
│   │   │   ├── UI.BottomPanel.prefab
│   │   │   ├── UI.Button.prefab
│   │   │   ├── UI.CardItem.prefab
│   │   │   ├── UI.EquipmentActivation.prefab
│   │   │   ├── UI.EquipmentQueue.prefab
│   │   │   ├── UI.PowerItem.prefab
│   │   │   └── UI.UpgradeCardUIPicker.prefab
│   │   └── Textures/
│   │       ├── Icons/
│   │       │   ├── T_Powerup.Arrow.1.png
│   │       │   ├── T_Powerup.Arrow.2.png
│   │       │   ├── T_Powerup.Arrow.3.png
│   │       │   ├── T_Powerup.Panel.1.png
│   │       │   ├── T_Powerup.Panel.2.png
│   │       │   ├── T_Powerup.Skill.1001.*.png
│   │       │   └── T_Powerup.Skill.1002.*.png
│   │       ├── Vfxs/
│   │       │   ├── fx_death.png
│   │       │   ├── fx_hammer.png
│   │       │   └── fx_smoke.1.png
│   │       ├── button.hammer.png
│   │       ├── frame_1.png
│   │       ├── frame_2.png
│   │       ├── frame_3.png
│   │       ├── tex_bar.png
│   │       ├── tex_circle.png
│   │       ├── tex_decor.png
│   │       ├── tex_diamond.png
│   │       └── tex_glow.png
│   └── ...
│
└── _DesignDocs/
    ├── 00-recovery-structure-analysis.md
    ├── 01-requirements.md
    ├── 02-proposed-folder-structure.md
    └── 03-estimate-and-test-plan.md
```

## 3. Assembly Definition đề xuất

Chưa bắt buộc ở MVP 0.1, nhưng nên chuẩn bị sau khi code bắt đầu nhiều.

```text
_TDS.Code.asmdef
_TDS.Runtime.asmdef
_GameToolkit.Core.asmdef
```

Quy tắc:
- `_TDS` có thể phụ thuộc `_GameToolkit`.
- `_GameToolkit` không phụ thuộc `_TDS`.
- UI phụ thuộc Gameplay/Core, nhưng Core không phụ thuộc UI.

## 4. Namespace đề xuất

```text
TDS.Boot
TDS.Home
TDS.Config
TDS.Gameplay
TDS.Gameplay.Data
TDS.Gameplay.Manager
TDS.Gameplay.Monsters
TDS.Gameplay.Projectiles
TDS.Gameplay.Views
TDS.Statistics
TDS.Utils
```

## 5. Danh sách file script MVP 0.1 cần tạo

Danh sách ban đầu:
```text
_TDS/Code/Core/GameManager.cs
_TDS/Code/Combat/Health.cs
_TDS/Code/Units/Base/BaseCore.cs
_TDS/Code/Units/Heroes/HeroAutoAttack.cs
_TDS/Code/Units/Monsters/MonsterController.cs
_TDS/Code/Projectiles/Projectile.cs
_TDS/Code/Spawning/EnemySpawner.cs
_TDS/Code/Spawning/WaveManager.cs
_TDS/Code/UI/GameplayUI.cs
```

Đã thay thế bằng code thực tế từ Recovery (ưu tiên) + Recovery 2.

## 6. Nguồn dữ liệu

| Source | Vị trí gốc | Vị trí đích | Ưu tiên |
|---|---|---|---|
| Recovery | `_GameToolkit/` | `_GameToolkit/` | ✅ Cao nhất |
| Recovery | `_TDS/` | `_TDS/` | ✅ Cao nhất |
| Recovery | `_TDS assets/` | `_TDS Source/` | ✅ Cao nhất |
| Recovery 2 | `_KITSystem/` | `_GameToolkit/` (bổ sung) | ⬜ Thấp hơn |
| Recovery 2 | `_Game/` | `_TDS/` (bổ sung) | ⬜ Thấp hơn |
| Recovery 2 | `_Source/` | `_TDS Source/` (bổ sung) | ⬜ Thấp hơn |
| Recovery 2 | `_BattleSource/` | `_TDS Source/` (bổ sung) | ⬜ Thấp hơn |
| Recovery 2 | `TextMesh Pro/` | `_TDS Source/Fonts/` | ⬜ Thấp hơn |
