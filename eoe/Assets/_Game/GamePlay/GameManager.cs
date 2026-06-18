using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _Game.GamePlay.SpawnerSystem;
using _Game.GamePlay.View;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TickSystemOwner owner;
        [SerializeField] private Pedestal pedestal;
        [SerializeField] private BattleUIManager uiManager;
        [SerializeField] private int[] weaponsId = new int[4];

        private Dictionary<int, int> damageReport = new Dictionary<int, int>();
        private Pedestal pedestalInstance;
        private SpawnerTickable spawner;
        private int totalMonsterAlive = 0;

        private void Awake()
        {
            owner.TryGetTickable(out SkillTickable skillTickable);
            skillTickable.OnPostDamage += (data, damage) =>
            {
                if (!damageReport.TryAdd(data.skillId, damage))
                {
                    damageReport[data.skillId] += damage;
                }
            };
            owner.TryGetTickable(out spawner);
            spawner.OnWaveSpawnCompleted += waveIndex =>
            {
                Debug.Log($"Complete wave {waveIndex} - {spawner.IsCompleted}");
            };

            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;
        }

        private async void Start()
        {
            AssetBundleManager.SetLocationBundle(true);

            await ConfigManager.Load(new string[] { "MonsterConfig", "LevelConfig", "WeaponConfig" });
            ColorSetting setting = ColorSetting.Instance;
            
            MonsterConfig monsterConfig = ConfigManager.Get<MonsterConfig>();
            LevelConfig levelConfig = ConfigManager.Get<LevelConfig>();
            levelConfig.TryGetLevelData(1, out LevelData levelData);
            spawner.SetLevel(levelData, monsterConfig);
            
            pedestalInstance = Object.Instantiate(pedestal, transform);
            pedestalInstance.SetWeaponDeltaTime(owner.TickInterval);
            owner.OnScaleTimeChanged += pedestalInstance.SetWeaponDeltaTime;

            WeaponConfig weaponConfig = ConfigManager.Get<WeaponConfig>();
            List<WeaponData> datas = new List<WeaponData>();
            foreach (var weaponId in weaponsId)
            {
                if (weaponConfig.TryGetWeaponData(weaponId, out WeaponData weaponData))
                    datas.Add(weaponData);
            }
            
            await owner.Initialize();
            await pedestalInstance.Initialize(datas.ToArray(), owner.Loop, owner.TickInterval);
            uiManager.Initialize();
            owner.IsPaused = false;
        }

        private void Update()
        {
            float duration = 0.1f;

            if (Input.GetKeyDown(KeyCode.F1)) SetWeapon(0, 1, duration);
            if (Input.GetKeyDown(KeyCode.F2)) SetWeapon(0, 2, duration);
            if (Input.GetKeyDown(KeyCode.F3)) SetWeapon(0, 3, duration);

            if (Input.GetKeyDown(KeyCode.F4)) SetWeapon(1, 1, duration);
            if (Input.GetKeyDown(KeyCode.F5)) SetWeapon(1, 2, duration);
            if (Input.GetKeyDown(KeyCode.F6)) SetWeapon(1, 3, duration);

            if (Input.GetKeyDown(KeyCode.F7)) SetWeapon(2, 1, duration);
            if (Input.GetKeyDown(KeyCode.F8)) SetWeapon(2, 2, duration);
            if (Input.GetKeyDown(KeyCode.F9)) SetWeapon(2, 3, duration);

            if (Input.GetKeyDown(KeyCode.F10)) SetWeapon(3, 1, duration);
            if (Input.GetKeyDown(KeyCode.F11)) SetWeapon(3, 2, duration);
            if (Input.GetKeyDown(KeyCode.F12)) SetWeapon(3, 3, duration);

            if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeapon(0, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeapon(1, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeapon(2, 0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetWeapon(3, 0, duration);
        }

        private void OnDestroy()
        {
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;
        }

        private void MonsterDisable(Monster monster)
        {
            totalMonsterAlive--;

            if (totalMonsterAlive == 0 && spawner.IsPaused)
            {
                spawner.IsPaused = false;
            }
        }

        private void MonsterEnable(Monster monster)
        {
            totalMonsterAlive++;
        }

        private void SetWeapon(int slot, int level, float duration)
        {
            pedestalInstance.SetWeaponLevel(slot, level, duration);
        }
    }
}