## Project

idle_rpg → engine → unity 6000.0.60f1
idle_rpg → language → csharp
idle_rpg → root → light_fight/
idle_rpg → test_framework → Unity Test Framework 1.6.0

## Directories

_Games → path → Assets/_Games/
_Games → purpose → Game-specific code (Battle/)
_Games → asmdef → Assembly-CSharp (no asmdef)

_Games_Config → path → Assets/_Games/Config/
_Games_Utils → path → Assets/_Games/Utils/

_KITSystem → path → Assets/_KITSystem/
_KITSystem → purpose → Reusable framework (Grid, Movement, Schedule, SkillSystem, EventBus, Resource, Utils)

## Architecture Direction (2026-05-23)

target_architecture → logic_view_separation
target_architecture → logic → pure C# POCO, no UnityEngine dependency
target_architecture → view → MonoBehaviour, DOTween, MMF only
target_architecture → testability → unit test without Unity runtime

logic_layer → interfaces → IWeaponLogic, IDiceLogic, IConeLogic, ISpawnerLogic
logic_layer → packages → _Games.Battle.Logic
logic_layer → location → Assets/_Games/Battle/Logic/

view_layer → location → Assets/_Games/Battle/View/
view_layer → mono → WeaponMono, ConeMono, DiceRollController, ArcMove, BattleLevel

## Battle

battle → entry → BattleManager.Start()
battle → flow → BattleLevel.Initialize() → UIBattleControlArena.Initialize() → Play()
battle → async → UniTask (not coroutines)

dice → logic → cooldown → delayTrigger → random value → trigger event
weapon → logic → cooldown → query target → fire skill

## Knowledge Files

knowledge → facts → docs/knowledge/KG/facts.md
knowledge → palace → docs/knowledge/Palace/config-reference.md
knowledge → architecture → docs/architecture/
knowledge → plans → docs/plans/
