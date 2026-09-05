# Gameplay TODO

- [x] Fix editor-only runtime assembly references.
- [x] Add one-shot monster EXP/gold reward events.
- [x] Accumulate run rewards and expose the final result.
- [x] Add result feedback after the approved SVG UI is converted to runtime UI.
- [x] Verify projectile prefab coverage for configured skills.
- [x] Add modifier application feedback in the gameplay HUD.
- [x] Add modifier visual tint feedback without new assets.
- [ ] Add modifier VFX particle assets.
- [x] Add Normal/Elite/Boss ranks and monster skill rotation.
- [x] Add resistance and stacking rules for monster statuses.
- [x] Add post-wave shop/upgrade roll UI with config-backed hero and skill-specific upgrades.
- [x] Add save game and capped offline progress.
- [x] Add persistent campaign XP/level tracking and 1x/2x/4x battle speed controls.
- [x] Expand campaign spawn/config coverage to 20 levels and add next/retry/home result loop.
- [ ] Add HP bars, damage numbers and status duration icons.

## Quality gates after each feature

- [ ] Add/maintain logic tests for every new behavior.
- [x] Add config validation tests for 20 campaign levels and hero/monster/skill prefab coverage.
- [ ] Add common integration tests for Boot, config loading, Addressables and required scene components.
- [x] Expand TDS PlayMode tests for stuck waves, pause/resume, shop/roll, result loop, Level 1/2 clearability, Level 3 upgrade-gated clear, and Victory/Defeat timeout.
- [ ] Fail PlayMode on unexpected runtime errors, missing scripts or missing Addressables.
- [x] Run EditMode, TDS PlayMode and Windows Player build before merging this campaign expansion.
