using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
using _Games.Config;
using _KIT.Checker;
using _KIT.Config;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Resource;
using _KIT.Utils;
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
        private bool isRunning = false;
        
        private async void Start()
        {
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
            spawnLogic = new SpawnLogic(shareData);
            
            // todo: close loading scene
            KitEntryScene.Instance.CloseLoadingScene();
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
            isRunning = true;
            Debug.Log(@"Tạo level config -> spawn level theo wave/batch...");
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
        
        private void OnBattleResume(BattleResumeEvent e) => isRunning = true;

        private void OnBattlePause(BattlePauseEvent e) => isRunning = false;

        private async void OnWaveResume(WaveResumeEvent e)
        {
            SkillConfig skillConfig = KitConfigManager.Get<SkillConfig>();
            for (int i = 0; i < levelDesign.Slots.Length; i++)
            {
                SlotView slot = levelDesign.Slots[i];
                if (slot.IsEquipped)
                {
                    /*WeaponData weaponData = slot.View.WeaponData;
                    int weaponLevel = slot.View.WeaponLevel;
                    if (skillConfig.Find(weaponData.SkillId, weaponLevel, out var skillData))
                    {
                        AbilitySO ability = await KitLoaded.LoadAsync<AbilitySO>(skillData.SkillPath, true);
                        if (ability.core.hitEffectPrefab != null)
                        {
                            KitPool.RegisterPool(ability.core.hitEffectPrefab, true);
                        }
                    }
                    weaponLogic.Equip(i, weaponData, weaponLevel);*/
                }
            }
            EventBus.Instance.Publish(new BattleResumeEvent());
        }
        
        private void Update()
        {
            if (!isRunning) return;
            float deltaTime = Time.deltaTime;
            spawnLogic.Update(deltaTime);
        }
    }
}
