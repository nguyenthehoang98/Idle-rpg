using System;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Equipment;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem;
using _Games.Config;
using _Games.Misc.Model;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Resource;
using _KIT.Utils;
using ProjectDawn.Navigation;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace _Games.Combat
{
    public class DevelopmentBattleStartup : KitEntryScene
    {
        public TMP_InputField inputFieldPickWeapon;
        public Button buttonPickWeapon;
        public TMP_InputField inputFieldSpawnMonster;
        public Button buttonSpawnMonster;
        public Button buttonMonsterPause;
        public Button buttonWeaponPause;
        public Button buttonResetSkill;

        private Entity player;
        private LevelDesign levelDesign;
        private WeaponManager _weaponManager;
        private bool isMonsterPaused = true;
        private bool isWeaponPaused = true;

        protected override async void OnStart()
        {
            buttonPickWeapon.onClick.AddListener(PickWeapon);
            buttonSpawnMonster.onClick.AddListener(SpawnMonster);
            buttonMonsterPause.onClick.AddListener(MonsterPause);
            buttonWeaponPause.onClick.AddListener(WeaponPause);
            buttonResetSkill.onClick.AddListener(ResetSkill);

            MonsterPause();
            WeaponPause();
            
            // todo: load default config
            await KitConfigManager.Load(new[]
            {
                "MonsterConfig",
                "SkillConfig",
                "LevelConfig",
                "WeaponConfig",
            });

            await KitLoaded.LoadAsync<RaritySO>(GlobalsPath.RARITY_SO, true);            
            
            // todo: build world
            if(World.DefaultGameObjectInjectionWorld != null) World.DefaultGameObjectInjectionWorld.Dispose();
            DefaultWorldInitialization.Initialize("GameWorld", false);
            
            // todo: init level spawn
            int levelId = 1;
            GameObject go = await KitLoaded.LoadAsync<GameObject>(GlobalsPath.GetLevelDesignPath(levelId));
            levelDesign = Instantiate(go).GetComponent<LevelDesign>();
            levelDesign.Initialize(1);
            player = ECSFactory.BuildPlayer(levelDesign);
            
            // todo: register object
            //shareData = new ShareData(dictionary, levelDesign);
            //spawnLogic = new SpawnLogic(shareData, player);
            _weaponManager = new WeaponManager(levelDesign);
            levelDesign.OnTriggerWeapon += _weaponManager.Trigger;
            
            EventBus.Instance.Publish(new WaveContinueEvent());
        }

        void MonsterPause()
        {
            isMonsterPaused = !isMonsterPaused;
            buttonMonsterPause.GetComponentInChildren<TextMeshProUGUI>().text
                = isMonsterPaused ? "Monster Resume" : "Monster Pause";
            buttonMonsterPause.image.color = isMonsterPaused ? Color.red : Color.green;

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(AgentBody));
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
            NativeArray<AgentBody> agentBody = query.ToComponentDataArray<AgentBody>(Allocator.TempJob);

            JobHandle handle = new PauseUnpauseMonsterJob
            {
                AgentBody = agentBody,
                IsRunning = !isMonsterPaused
            }.Schedule();
            handle.Complete();
            
            for (int i = 0; i < agentBody.Length; i++)
            {
                manager.SetComponentData(entities[i], agentBody[i]);
            }

            agentBody.Dispose();
        }

        void ResetSkill() => SkillFactory.UnloadAll();

        void WeaponPause()
        {
            isWeaponPaused = !isWeaponPaused;
            buttonWeaponPause.GetComponentInChildren<TextMeshProUGUI>().text
                = isWeaponPaused ? "Weapon Resume" : "Weapon Pause";
            buttonWeaponPause.image.color = isWeaponPaused ? Color.red : Color.green;
            
            if (isWeaponPaused)
                EventBus.Instance.Publish(new WavePauseEvent());
            else 
                EventBus.Instance.Publish(new WaveContinueEvent());
        }

        async void SpawnMonster()
        {
            if (int.TryParse(inputFieldSpawnMonster.text, out int value))
            {
                MonsterConfig monsterConfig = KitConfigManager.Get<MonsterConfig>();
                if (monsterConfig.Find(value, out MonsterData monsterData))
                {
                    if (isMonsterPaused) MonsterPause();
                    
                    EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    EntityQuery query = manager.CreateEntityQuery(typeof(MonsterTag));
                    NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
                    for (int i = 0; i < entities.Length; i++)
                    {
                        Entity entity = entities[i];
                        Monster monster = null;
                        if (manager.HasComponent<MonsterRangedTag>(entity))
                        {
                            monster = manager.GetComponentObject<RangedMonster>(entity);
                        }
                        else if (manager.HasComponent<MonsterMeleeTag>(entity))
                        {
                            monster = manager.GetComponentObject<Monster>(entity);
                        }

                        if (monster != null) monster.Death();
                    }

                    manager.DestroyEntity(query);
                    
                    float3 position = new float3(
                        RandomUtils.Value > 0.5f ? RandomUtils.Range(-12f, -9f) : RandomUtils.Range(9f, 12f),
                        RandomUtils.Value > 0.5f ? RandomUtils.Range(-12f, -9f) : RandomUtils.Range(9f, 12f),
                        0);
                    MonsterData clone = monsterData.Clone(10000000);
                    await ECSFactory.BuildMonster(player, clone, levelDesign.Radius, position);
                }
                else
                {
                    Debug.LogError($"Not found monster_id '{value}'");
                }
            }
        }

        void PickWeapon()
        {
            int slot = 0;
            Rarity rarity = Rarity.Common;
            if (int.TryParse(inputFieldPickWeapon.text, out int value))
            {
                WeaponConfig weaponConfig = KitConfigManager.Get<WeaponConfig>();
                if (weaponConfig.Find(value, out WeaponData weaponData))
                {
                    SlotItem slotItem = levelDesign.Slots[slot];
                    slotItem.Equip(weaponData, rarity);
                    _weaponManager.Equip(slot, weaponData, RarityMethod.ParseLevel(rarity));
                }
                else
                {
                    Debug.LogError($"Not found weapon_id '{value}'");
                    inputFieldPickWeapon.text = String.Empty;
                }
            }
        }
    }

    [BurstCompile]
    struct PauseUnpauseMonsterJob : IJob
    {
        public NativeArray<AgentBody> AgentBody;
        public bool IsRunning;
        
        public void Execute()
        {
            for (int i = 0; i < AgentBody.Length; i++)
            {
                AgentBody agent = AgentBody[i];
                if (IsRunning)
                    agent.SetDestination(float3.zero);
                else
                    agent.Stop();
                AgentBody[i] = agent;
            }
        }
    }
}