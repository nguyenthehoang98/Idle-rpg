using _Games.Combat.EntityComponentSystem;
using _Games.Combat.Event;
using _Games.Combat.Level;
using _Games.Combat.Model;
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
        private TriggerWeaponLogic weaponLogic;
        private bool isRunning = false;
        
        private async void Start()
        {
            LevelSpawnSO levelSpawn = await LevelSpawnSO.LoadSpawn(1);
            levelDesign = Instantiate(levelSpawn.design);
            Entity player = ECSFactory.BuildPlayer(levelDesign);
            healthUI.Initialize(player);
            shareData = new ShareData(levelSpawn);
            spawnLogic = new SpawnLogic(shareData);
            weaponLogic = new TriggerWeaponLogic(levelDesign);
            levelDesign.OnTriggerWeapon += weaponLogic.Trigger;
            
            // todo: close loading scene
            KitEntryScene.Instance.CloseLoadingScene();
#if DEVELOP_MODE
            gameObject.AddComponent<CpuFrame>();
#endif
            isRunning = true;
            Debug.Log(@"Tạo level config -> spawn level theo wave/batch...");
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
            weaponLogic.Update();
        }
    }
}
