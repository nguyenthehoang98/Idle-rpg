using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Model;
using _Games.Combat.Equipment;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Combat.View;
using _Games.Config;
using _Games.Misc.Model;
using _Games.Utils;
using _KIT.Checker;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Popup;
using _KIT.Resource;
using _KIT.Utils;
using MoreMountains.Feedbacks;
using ProjectDawn.Custom;
using Unity.Core;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat
{
    public class BattleStartup : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [Header("Feedbacks")]
        [SerializeField] private MMF_Player zoomInCameraFeedback;
        [SerializeField] private MMF_Player zoomOutCameraFeedback;
        
        private ShareData shareData;
        private SpawnLogic spawnLogic;
        private LevelDesign levelDesign;
        private EquipmentManager equipmentManager;
        
        private async void Start()
        {
            // todo: init world
            DefaultWorldInitialization.Initialize("GameWorld", false);
            CustomSimulationGroup group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.Iterations = KitEntryScene.Instance.GamePlayIterationsUpdate;
            BattleTime.DeltaTime = group.TimeStep = 1f / KitEntryScene.Instance.GameplayFrameRate;
            BattleTime.ScaleTime = group.TimeScale = KitEntryScene.Instance.GameplayScaleTime;
            BattleTime.Time = 0;
            World.DefaultGameObjectInjectionWorld.Time = new TimeData(0, 0);
            Pause();

            // todo: validate data
            int levelId = 1;
            LevelConfig levelConfig = KitConfigManager.Get<LevelConfig>();
            bool foundSpawnData = levelConfig.FindSpawn(levelId, out var dictionary);
            if(!foundSpawnData) Debug.LogError("Not found spawn data with levelId: " + levelId);
           
            var popup = await PopupManager.Instance.PushAsync<WeaponSelectPopup>();
            popup.ClosedCallback += () => zoomOutCameraFeedback.PlayFeedbacks();
            zoomInCameraFeedback.PlayFeedbacks();
            
            // todo: init level spawn
            GameObject go = await KitLoaded.LoadAsync<GameObject>(GlobalsPath.GetLevelDesignPath(levelId));
            levelDesign = Instantiate(go).GetComponent<LevelDesign>();
            Entity player = ECSFactory.BuildPlayer(levelDesign);

            // todo: init object
            PlayerHealthUI.Instantiate(canvas.transform, player);
            FloatingTextDamageSpawner.Instantiate(transform);
            RangedMonsterCastSkillManager.Instantiate(transform);
            GameTimeUI.Instantiate(canvas.transform);
            GameObject rmcsm = new GameObject("RangedMonsterCastSkillManager");
            rmcsm.AddComponent<RangedMonsterCastSkillManager>();
            rmcsm.transform.SetParent(transform);
            
            // todo: register object
            shareData = new ShareData(dictionary, levelDesign);
            spawnLogic = new SpawnLogic(shareData, player);
            equipmentManager = new EquipmentManager(levelDesign);
            levelDesign.OnTriggerWeapon += equipmentManager.Trigger;
            
            // todo: close loading scene
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
            Debug.Log(@"Phần tường raào mà monster stop & tấn công được nên có 1 cái fx như shield của BagMaster");
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<WaveLoseEvent>(OnWaveLose);
            EventBus.Instance.Subscribe<WaveCompleteEvent>(OnWaveComplete);
            EventBus.Instance.Subscribe<WaveContinueEvent>(OnWaveContinue);
            EventBus.Instance.Subscribe<WavePauseEvent>(OnWavePause);
        } 

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveLoseEvent>(OnWaveLose);
            EventBus.Instance.Unsubscribe<WaveCompleteEvent>(OnWaveComplete);
            EventBus.Instance.Unsubscribe<WaveContinueEvent>(OnWaveContinue);
            EventBus.Instance.Unsubscribe<WavePauseEvent>(OnWavePause);
        }

        private void OnWaveLose(WaveLoseEvent e)
        {
            if(BattleTime.IsRunning)
            {
                PopupManager.Instance.Push<LosePopup>();
                Pause();
                EventBus.Instance.Publish(new WavePauseEvent());
                
                MonsterAuthoring[] monsters = GameObject.FindObjectsByType<MonsterAuthoring>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var monster in monsters)
                {
                    monster.PlayAnimation(AnimationName.Idle);
                }
            }
        }

        private void OnWaveComplete(WaveCompleteEvent e)
        {
            if (e.CurrentWave > e.TotalWave)
            {
                Debug.LogError("win game");
                EventBus.Instance.Publish(new WavePauseEvent());
            }
            else
            {
                async void Action()
                {
                    EventBus.Instance.Publish(new WavePauseEvent());
                    var popup = await PopupManager.Instance.PushAsync<WeaponSelectPopup>();
                    popup.ClosedCallback += () => zoomOutCameraFeedback.PlayFeedbacks();
                    zoomInCameraFeedback.PlayFeedbacks();
                }

                this.WaitInvoke(1, Action);
            }
        }

        private void OnWavePause(WavePauseEvent e)
        {
            Pause(); // Sau thêm biến source pause để check với trường hợp khi người chơi bấm pause
        }

        private void Resume()
        {
            BattleTime.IsRunning = true;
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>().TimeScale = BattleTime.ScaleTime;
        }

        private void Pause()
        { 
            BattleTime.IsRunning = false;
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>().TimeScale = 0;
        }

        private async void OnWaveContinue(WaveContinueEvent e)
        {
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            for (int i = 0; i < levelDesign.Slots.Length; i++)
            {
                SlotItem slot = levelDesign.Slots[i];
                if (slot.IsEquipped)
                {
                    if (skillConfig.Find(slot.WeaponData.SkillId, out SkillData skillData))
                    {
                        Skill skill = await SkillFactory.CreateSkill(skillData);
                        if (skill.projectile.hitEffectPrefab != null)
                            KitPool.RegisterPool(skill.projectile.hitEffectPrefab, true);
                    }
                    equipmentManager.Equip(i, slot.WeaponData, RarityMethod.ParseLevel(slot.WeaponRarity));
                }
            }

            spawnLogic.NextWaveSpawn();
            Resume();
        }
        
        private void Update()
        {
            if (!BattleTime.IsRunning)
                return;
            
            float deltaTime = Time.deltaTime * BattleTime.ScaleTime;
            BattleTime.Time += deltaTime;
            spawnLogic.Update(deltaTime);
        }

        private void OnDestroy()
        {
            if(World.DefaultGameObjectInjectionWorld != null) World.DefaultGameObjectInjectionWorld.Dispose();
        }
    }
}
