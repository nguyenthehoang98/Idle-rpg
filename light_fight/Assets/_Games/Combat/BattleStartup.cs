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
using _KIT.Checker;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
using ProjectDawn.Custom;
using Unity.Entities;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace _Games.Combat
{
    public class BattleStartup : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        
        private ShareData shareData;
        private SpawnLogic spawnLogic;
        private LevelDesign levelDesign;
        private EquipmentManager equipmentManager;
        private float battleScaleTime;
        private bool isRunning = false;
        
        private async void Start()
        {
            var group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.Iterations = KitEntryScene.Instance.GamePlayIterationsUpdate;
            group.TimeStep = 1f / KitEntryScene.Instance.GameplayFrameRate;
            battleScaleTime = KitEntryScene.Instance.GameplayScaleTime;
            group.TimeScale = 0;

            // todo: validate data
            int levelId = 1;
            LevelConfig levelConfig = KitConfigManager.Get<LevelConfig>();
            bool foundLevelData = levelConfig.FindData(levelId, out var levelData);
            if(!foundLevelData) Debug.LogError("Not found level data with levelId: " + levelId);
            bool foundSpawnData = levelConfig.FindSpawn(levelId, out var dictionary);
            if(!foundSpawnData) Debug.LogError("Not found spawn data with levelId: " + levelId);
            
            EventBus.Instance.Publish(new OpenWeaponSelectPopupEvent());
            
            // todo: init level spawn
            GameObject go = await KitLoaded.LoadAsync<GameObject>(levelData.LevelDesign);
            levelDesign = Instantiate(go).GetComponent<LevelDesign>();
            Entity player = ECSFactory.BuildPlayer(levelDesign);

            // todo: init object
            PlayerHealthUI.Instantiate(canvas.transform, player);
            TextDamageSpawner.Instantiate(transform);
            RangedMonsterCastSkillManager.Instantiate(transform);
            GameTimeUI.Instantiate(canvas.transform);
            WeaponSelectPopup.Instantiate(canvas.transform);
            
            // todo: register object
            shareData = new ShareData(dictionary, levelDesign);
            spawnLogic = new SpawnLogic(shareData, player);
            equipmentManager = new EquipmentManager(levelDesign);
            levelDesign.OnTriggerWeapon += equipmentManager.Trigger;
            
            // todo: close loading scene
            KitEntryScene.Instance.CloseLoadingScene();
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
            Debug.Log(@"Phần tường raào mà monster stop & tấn công được nên có 1 cái fx như shield của BagMaster");
        }
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<WaveContinueEvent>(OnWaveContinue);
            EventBus.Instance.Subscribe<WaveCompleteEvent>(OnWaveComplete);
        } 

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveContinueEvent>(OnWaveContinue);
            EventBus.Instance.Unsubscribe<WaveCompleteEvent>(OnWaveComplete);
        }

        private void OnWaveComplete(WaveCompleteEvent e)
        {
            if (e.CurrentWave > e.TotalWave)
            {
                Debug.LogError("end game");
            }
            else
            {
                this.WaitInvoke(1, () =>
                {
                    Pause(); // Sau thêm biến source pause để check với trường hợp khi người chơi bấm pause
                    EventBus.Instance.Publish(new OpenWeaponSelectPopupEvent());
                });                
            }
        }

        private void Resume()
        {
            isRunning = true;
            var group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.TimeScale = battleScaleTime;
        }

        private void Pause()
        { 
            isRunning = false;
            var group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.TimeScale = 0;
        }

        private async void OnWaveContinue(WaveContinueEvent e)
        {
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            for (int i = 0; i < levelDesign.Slots.Length; i++)
            {
                SlotView slot = levelDesign.Slots[i];
                if (slot.IsEquipped)
                {
                    if (skillConfig.Find(slot.ItemView.WeaponData.SkillId, out SkillData skillData))
                    {
                        Skill skill = await SkillFactory.CreateSkill(skillData);
                        if (skill.projectile.hitEffectPrefab != null)
                            KitPool.RegisterPool(skill.projectile.hitEffectPrefab, true);
                    }
                    equipmentManager.Equip(i, slot.ItemView.WeaponData, slot.ItemView.WeaponLevel);
                }
            }

            spawnLogic.NextWaveSpawn();
            Resume();
        }
        
        private void Update()
        {
            if (!isRunning)
                return;
            
            float deltaTime = Time.deltaTime * battleScaleTime;
            spawnLogic.Update(deltaTime);
            equipmentManager.Update();
        }
    }
}
