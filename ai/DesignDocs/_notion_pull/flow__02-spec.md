# flow/02-spec

<!-- Pulled from Notion. Review before overwriting source design docs. -->

> Synced from Unity project: ai/DesignDocs/flow/02-spec.md

# 02 - Game Spec Draft

Status: draft - updated after interview answers

Dựa trên skill spec-driven-development.

## 1. Objective

Xây dựng prototype Unity 2D cho game Hero Defense / Tower Defense theo hướng PVE.

Người chơi chọn 4 hero vào trận để bảo vệ Sacred Core ở trung tâm trước các đợt quái plant spawn từ xung quanh.

## 2. Target Player

Đã nghiêng về nhóm người chơi thích Hero Defense / Survivor Defense hơn tower building truyền thống.

## 3. Core Fantasy

```plain text
Một đội animal heroes cố thủ ở trung tâm, chống lại làn sóng quái plant ngày càng đông và mạnh để bảo vệ Sacred Core.
```

## 4. Core Loop

Updated draft:

```plain text
Start level
-> Load level config and character data
-> Roll 4 heroes into 4 active slots (3 hero choices shown each roll)
-> Enter battle
-> Monsters spawn from spawner config via spawner updater
-> Heroes auto-target and attack
-> Monsters die or reach Sacred Core
-> Player receives combat rewards/resources
-> During gameplay player can roll temporary items at fixed intervals or via low-probability monster drops
-> Between waves player chooses a buff/stat roll or special item
-> Next wave starts harder
-> Final wave or boss clears
-> Victory / Defeat reward resolution
```

## 5. MVP 0.1 Scope

In scope:

```plain text
- 1 gameplay scene
- 1 Sacred Core/Base
- 4 active hero slots
- 3-hero choice UI per initial hero roll
- Hero auto attack flow
- Monster spawn driven by level spawner config
- LevelConfig-driven starting data
- Between-wave roll choice for buff/stat
- In-combat energy-based random item roll with duration
- In-combat hero buff items / Super state
- Health/damage/death flow
- Base/Core damage and Game Over
- Simple waves
- Basic UI: Base HP, Wave, Game State
- Coin as in-match currency
- Gold as out-of-match currency
```

Out of scope MVP 0.1:

```plain text
- Camera animation
- Multiple movement-controlled heroes
- Advanced skills framework polish
- Shop/meta progression beyond config-driven rewards
- Save/load
- Addressables
- Excel import
- Advanced avoidance
- Real art/audio
```

## 6. Gameplay Rules

### Sacred Core / Base

```plain text
- Sacred Core sits at the center.
- Sacred Core has HP.
- If Sacred Core HP <= 0, game state becomes GameOver.
- Reward configuration is driven by LevelConfig.
```

### Heroes

```plain text
- Player starts with 4 hero rolls into 4 active slots.
- Each roll shows 3 hero choices, and the chosen hero is removed from the shown pool for that slot selection.
- Heroes do not move in MVP 0.1.
- Heroes auto-target valid enemies based on hero-specific behavior.
- Each hero has a normal attack and a skill.
- Skill usage depends on energy availability.
- Heroes can enter Super state when buffed by in-combat hero item rolls.
- Heroes can be replaced by newly rolled heroes during gameplay merge/roll upgrade flows.
- Heroes have rarity progression: Common -> UnCommon -> Rare -> Epic -> Legendary -> Artifact.
- Hero equipment is hero-bound and not handled as a separate general gameplay resource.
- Hero data includes: id, name, level, max rarity, evolution, upgrade.
- Main classes: Archer, Mage, Control.
  - Archer: high single-target damage, good for bosses.
  - Mage: strong AoE, good for small enemies.
  - Control: CC, buff, debuff, and battlefield control.
- Player can force all heroes to attack in a chosen direction by interacting with the screen; heroes still respect their attack range.
- Hero skills can be active or passive.
```

### Monster

```plain text
- Monster behavior is driven by monster type.
- Mob: common, weak, numerous, slow, no CC.
- Elite: stronger threat, can attack from range, summon, CC, buff, or debuff.
- Boss: appears mainly in the final wave and can decide level clear.
- Monsters spawn from level spawner config and are read by spawner updater.
- Monsters die when HP <= 0.
- Monster damage / death / drop flow is handled by the configured combat pipeline.
```

### Projectile / Skill Toolkit

```plain text
- GameToolkit is reusable across projects.
- Shared systems include projectile, trajectory, collision, and skill modifier logic.
- Skill APIs can expose params/callback outputs such as hit target, projectile destroy, and damage.
```

### Progression in Battle

```plain text
- After each wave, player selects one buff/stat roll.
- In combat, player can use an energy bar to roll a temporary random item.
- In-combat item rolls can also grant hero buff items; if the buff is on a hero, that hero enters Super state.
- Some waves can sell items directly with longer-lasting properties than in-combat rolls.
- Player continues to the next wave after choosing.
```

### Combat Feedback

```plain text
- Monster hit feedback briefly flashes white.
- Monster death grants EXP.
- Monster death triggers monster explode FX.
- Some monsters can drop items/resources such as coin, equipment, or materials with low probability.
- Coin is the in-match currency and does not carry over after the match ends.
- Gold is the out-of-match currency.
```

## 7. Data Defaults MVP 0.1

```plain text
LevelConfig:
- Starting character data
- Starting goal count
- Reward config
- Wave reward config

Battle defaults:
- 4 hero slots
- Level-config driven monster spawn
- Simple wave progression
```

## 8. Project Structure

```plain text
Assets/_TDSurvivor/Code
Assets/_TDSurvivor/Content
ai/DesignDocs
```

Detailed structure: see 02-proposed-folder-structure.md.

## 9. Commands / Verification

TBD after Unity version/package setup confirmed.

Manual verification for now:

```plain text
1. Open Unity project.
2. Open Gameplay scene.
3. Check Console no red errors.
4. Press Play.
5. Confirm monsters spawn from config.
6. Confirm 4 heroes are rolled into active slots.
7. Confirm hero auto-attacks.
8. Confirm rewards / wave progression / GameOver flow.
```

## 10. Boundaries

Always:

```plain text
- Update spec before changing design.
- Keep MVP small.
- Use namespace TDSurvivor.
- Keep code and content separated.
```

Ask first:

```plain text
- Copy files from Recovery.
- Add plugins/packages.
- Introduce Addressables/UniTask/ExcelExtension.
- Change Unity project settings.
```

Never:

```plain text
- Copy `.meta`, `.prefab`, `.unity` blindly from Recovery.
- Implement unapproved scope.
- Hide assumptions.
```

## 11. Success Criteria MVP 0.1

```plain text
SC01 - Gameplay scene runs without red Console errors.
SC02 - Monster spawns from level config.
SC03 - Heroes are rolled into 4 active slots at level start.
SC04 - Heroes auto-attack monsters.
SC05 - Skills consume energy and trigger correctly.
SC06 - Monster dies at 0 HP and gives feedback.
SC07 - Monster damages Sacred Core if reaching it.
SC08 - Base/Core HP UI updates.
SC09 - GameOver when Sacred Core HP <= 0.
SC10 - Victory when all configured waves complete.
```

## 12. Open Questions

```plain text
OQ01 - Define the official Sacred Core name and data model.
OQ02 - Define the exact hero reroll protection / anti-bad-roll rule.
OQ03 - Define the exact in-combat shop timing, refresh, and lock rules.
OQ04 - Define exact energy gain pacing and whether each hero has an independent energy bar only.
OQ05 - Define exact monster attribute formulas for elite/boss modifiers.
OQ06 - Define exact resource rules for goal / EXP / equipment / materials.
OQ07 - Define exact upgrade/evolution system.
OQ08 - Confirm target platform later.
```

## 13. Latest Design Decisions

### World Direction

```plain text
- Game world has no humans.
- World consists of animals and plants.
- Mutation event changed the ecosystem.
- Some animals evolved into Animal Heroes.
- Mutated plants became monsters.
- Animal Heroes protect the Sacred Core.
```

### Battle Direction

```plain text
- Remove war/building placement focus.
- Game is Hero Defense, not traditional tower building.
- Player focuses on selecting, upgrading and using animal heroes.
- Heroes stay fixed in the center area.
- Heroes do not move.
- Enemies move toward the defense point.
- Active team contains 4 hero slots.
```

### Hero Direction

```plain text
- Planned total heroes: ~30.
- MVP scope: 5 heroes.

Main classes:
1. Archer
   - Ranged physical damage.
   - High single-target damage.

2. Mage
   - Ranged magic damage.
   - Area damage focus.

3. Support Mage / Control
   - CC.
   - Buff.
   - Debuff.
   - Team support.
```

## 14. Combat Rules / Skill Framework

### SkillConfig-driven Combat

```plain text
- Attack-related stats such as AR, SP, SC, trajectory, target finding, and damage tick behavior are defined in SkillConfig.
- SkillConfig also defines modifiers such as burn, slow speed, lock, and CC.
- Damage tick types are only instant and damage-over-time.
- There is only one damage type in the game; the combat system does not distinguish physical, magical, or true damage.
- Secondary stats such as lifesteal and armor are defined in StatConfig.
```

### Damage Formula

```plain text
- Buff stacking behavior is driven by config.
- If buff stacking is enabled, buffs use additive stacking and the total buff output becomes a stat output.
- Damage reduction is applied at the final step.
- output_dmg = final_dmg * reduction
```

### Hero Skill Framework

```plain text
- A hero has 3 skill types in the framework: normal attack, active skill, passive skill.
- Each hero still uses only 1 normal attack + 1 hero skill in gameplay MVP.
```

### Target Priority

```plain text
- When casting a skill, target selection behavior is defined by SkillConfig.
```

### Buff / Debuff System

```plain text
- Buffs are also skills and are defined in SkillConfig.
```

### Super State

```plain text
- When Super State is activated, it buffs skill stats such as cooldown, damage, and size.
- Visual changes can be applied if the hero rarity is higher than the threshold defined in HeroConfig.
```

### Combat Numbers

```plain text
- Normal attack damage text: white.
- Crit damage text: yellow.
- Heal text: green.
- Burn damage text: red and small.
- Poison damage text: dark green.
```

## 15. Updated Open Questions

```plain text
OQ01 - Define the official Sacred Core name and data model.
OQ02 - Define the exact hero reroll protection / anti-bad-roll rule.
OQ03 - Define the exact in-combat shop timing, refresh, and lock rules.
OQ04 - Define exact energy gain pacing and whether each hero has an independent energy bar only.
OQ05 - Define exact monster attribute formulas for elite/boss modifiers.
OQ06 - Define exact resource rules for goal / EXP / equipment / materials.
OQ07 - Define exact upgrade/evolution system.
OQ08 - Confirm target platform later.
OQ09 - Define the concrete SkillConfig and StatConfig fields for the combat pipeline.
```
