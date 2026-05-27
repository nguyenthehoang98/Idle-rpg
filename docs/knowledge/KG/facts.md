## Project

idle_rpg → engine → unity 6000.0.60f1
idle_rpg → language → csharp
idle_rpg → root → light_fight/
idle_rpg → test_framework → Unity Test Framework 1.6.0

## Directories

_Games → path → Assets/_Games/
_Games → purpose → Game-specific code (Battle/)
_Games → asmdef → Games.Battle, Game.Config, Games.Utils

_Games_Config → path → Assets/_Games/Config/
_Games_Utils → path → Assets/_Games/Utils/

_KITSystem → path → Assets/_KITSystem/
_KITSystem → purpose → Reusable framework (Grid, Movement, Schedule, SkillSystem, EventBus, Resource, Utils)

## Architecture Direction (2026-05-23)

target_architecture → logic_view_separation
target_architecture → logic → pure C# POCO, no UnityEngine dependency
target_architecture → view → MonoBehaviour, DOTween, MMF only
target_architecture → testability → unit test without Unity runtime

logic_layer → planned_interfaces → IWeaponLogic, IDiceLogic, ISlotLogic, ISpawnerLogic
logic_layer → packages → _Games.Battle.Logic
logic_layer → location → Assets/_Games/Battle/Logic/

view_layer → location → Assets/_Games/Battle/View/
view_layer → components → ObjectWeaponView, PureWeaponView, ObjectSlotView, PureSlotView, DiceRollController, DiceControlView, StarView

## Battle

battle → entry → BattleOwner.Start()
battle → scene → Assets/_Games/Battle/game scene.unity
battle → flow → load configs → initialize tickables → initialize battle → initialize dice control → StartGame()
battle → async → UniTask for config/prefab loading; coroutine still used for BattleTickable view sequencing

dice → logic → cooldown → delayTrigger → random value → trigger event
weapon → logic → cooldown → query target → fire skill
slot → logic → activate/deactivate weapon and slot view

## Current Manager Risks

manager_fix → BattleOwner → entity cleanup before wave-clear check; OnDestroy unsubscribe/dispose
manager_fix → SpawnerTickable → currentWave completion guard; empty wave guard
manager_fix → SPU → independent skill ids; swapped action mapping by action id

## Knowledge Files

knowledge → facts → docs/knowledge/KG/facts.md
knowledge → palace → docs/knowledge/Palace/config-reference.md
knowledge → architecture → docs/architecture/
knowledge → plans → docs/plans/
