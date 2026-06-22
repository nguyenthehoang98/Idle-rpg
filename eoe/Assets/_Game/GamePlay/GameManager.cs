using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _Game.GamePlay.SpawnerSystem;
using _Game.GamePlay.View;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using UnityEngine;

namespace _Game.GamePlay
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TickSystemOwner owner;
        [SerializeField] private Pedestal pedestal;
        [SerializeField] private BattleUIManager uiManager;
        [SerializeField] private int[] weaponsId = new int[4];

        private const int MAX = 4;

        private readonly Dictionary<int, int> elementStackNumber = new Dictionary<int, int>();
        private readonly Dictionary<int, int> damageReport = new Dictionary<int, int>();
        private SpawnerTickable spawner;
        private SkillTickable skillTickable;
        private PlayerConfig playerConfig;
        private PlayerRuntimeData player;
        private int totalMonsterAlive = 0;

        private void Awake()
        {
            owner.TryGetTickable(out skillTickable);
            owner.TryGetTickable(out spawner);

            player = new PlayerRuntimeData();
            
            skillTickable.OnPostDamage += PostDamage;
            skillTickable.OnPostEarnExp += EarnExp;
            spawner.OnWaveSpawnCompleted += WaveSpawnCompleted;

            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;

            uiManager.OnElementStartReset += StartReset;
            uiManager.OnElementChanged += ElementChanged;
            uiManager.OnElementStopReset += StopReset;
        }

        private async void Start()
        {
            AssetBundleManager.SetLocationBundle(true);

            await ConfigManager.Load(new string[] { "MonsterConfig", "LevelConfig", "WeaponConfig", "PlayerConfig" });
          
            ColorSetting setting = ColorSetting.Instance;
            playerConfig = ConfigManager.Get<PlayerConfig>();
            MonsterConfig monsterConfig = ConfigManager.Get<MonsterConfig>();
            LevelConfig levelConfig = ConfigManager.Get<LevelConfig>();
            levelConfig.TryGetLevelData(1, out LevelData levelData);
            spawner.SetLevel(levelData, monsterConfig);
            
            pedestal.SetWeaponDeltaTime(owner.TickInterval);
            owner.OnScaleTimeChanged += pedestal.SetWeaponDeltaTime;

            WeaponConfig weaponConfig = ConfigManager.Get<WeaponConfig>();
            List<WeaponData> datas = new List<WeaponData>();
            foreach (var weaponId in weaponsId)
            {
                if (weaponConfig.TryGetWeaponData(weaponId, out WeaponData weaponData))
                    datas.Add(weaponData);
            }
            
            await owner.Initialize();
            await pedestal.Initialize(datas.ToArray(), owner.Loop, owner.TickInterval);
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
            skillTickable.OnPostDamage -= PostDamage;
            skillTickable.OnPostEarnExp -= EarnExp;
            spawner.OnWaveSpawnCompleted -= WaveSpawnCompleted;
            
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;

            uiManager.OnElementStartReset -= StartReset;
            uiManager.OnElementChanged -= ElementChanged;
            uiManager.OnElementStopReset -= StopReset;
        }

        private void SetWeapon(int slot, int level, float duration)
        {
            pedestal.SetWeaponLevel(slot, level, duration);
        }

        // Callback
        
        private void StartReset()
        {
            elementStackNumber.Clear();
        }

        private void ElementChanged(int id)
        {
            if (!elementStackNumber.TryAdd(id, 1))
            {
                elementStackNumber[id]++;
            }
            
            pedestal.SetWeaponLevel(id, elementStackNumber[id], 0.3f);
        }

        private void StopReset()
        {
            for (int i = 0; i < MAX; i++)
            {
                if (elementStackNumber.ContainsKey(i)) continue;
                
                pedestal.SetWeaponLevel(i, 0, 0.3f);
            }
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

        private void PostDamage(SkillData source, int damage)
        {
            if (!damageReport.TryAdd(source.skillId, damage))
            {
                damageReport[source.skillId] += damage;
            }
        }

        private void EarnExp(int exp)
        {
            player.CurrentExp += exp;

            if (playerConfig.TryGetExp(player.CurrentLevel + 1, out var data))
            {
                if (player.CurrentExp >= data.exp)
                {
                    player.CurrentLevel += 1;
                    player.CurrentExp -= data.exp;
                    LevelUp();
                }
            }
        }

        private void LevelUp()
        {
            Debug.Log("Level " + player.CurrentLevel);
        }

        private void WaveSpawnCompleted(int waveIndex)
        {
            Debug.Log($"Complete wave {waveIndex} - {spawner.IsCompleted}");
        }
    }
}