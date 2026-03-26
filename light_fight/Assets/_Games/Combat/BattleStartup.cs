using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Model;
using _Games.Combat.Equipment;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
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

namespace _Games.Combat
{
    public class BattleStartup : MonoBehaviour
    {
        [SerializeField] private PlayerHealthUI healthUI;
        
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
            EventBus.Instance.Publish(new OpenWeaponSelectPopupEvent());
            int levelId = 1;
            LevelConfig levelConfig = KitConfigManager.Get<LevelConfig>();
            bool foundLevelData = levelConfig.FindData(levelId, out var levelData);
            if(!foundLevelData) Debug.LogError("Not found level data with levelId: " + levelId);
            bool foundSpawnData = levelConfig.FindSpawn(levelId, out var dictionary);
            if(!foundSpawnData) Debug.LogError("Not found spawn data with levelId: " + levelId);
            GameObject go = await KitLoaded.LoadAsync<GameObject>(levelData.LevelDesign);
            levelDesign = Instantiate(go).GetComponent<LevelDesign>();
            Entity player = ECSFactory.BuildPlayer(levelDesign);
            healthUI.Initialize(player);
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
            EventBus.Instance.Subscribe<WaveResumeEvent>(OnWaveResume);
            EventBus.Instance.Subscribe<BattlePauseEvent>(OnBattlePause);
            EventBus.Instance.Subscribe<BattleResumeEvent>(OnBattleResume);
        } 

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<WaveResumeEvent>(OnWaveResume);
            EventBus.Instance.Unsubscribe<BattlePauseEvent>(OnBattlePause);
            EventBus.Instance.Unsubscribe<BattleResumeEvent>(OnBattleResume);
        }

        private void OnBattleResume(BattleResumeEvent e)
        {
            isRunning = true;
            var group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.TimeScale = battleScaleTime;
        }

        private void OnBattlePause(BattlePauseEvent e)
        { 
            isRunning = false;
            var group = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<CustomSimulationGroup>();
            group.TimeScale = 0;
        }

        private async void OnWaveResume(WaveResumeEvent e)
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
            EventBus.Instance.Publish(new BattleResumeEvent());
        }
        
        private void Update()
        {
            if (!isRunning) return;
            float deltaTime = Time.deltaTime * battleScaleTime;
            spawnLogic.Update(deltaTime);
            equipmentManager.Update();
        }
    }
}
