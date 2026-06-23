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

        private readonly Dictionary<int, TotalWeaponUpgradeData> upgradeDatas = new Dictionary<int, TotalWeaponUpgradeData>();
        private readonly Dictionary<int, int> elementStackNumber = new Dictionary<int, int>();
        private readonly Dictionary<int, int> damageReport = new Dictionary<int, int>();
        private List<Weapon> allWeapons;
        private SpawnerTickable spawner;
        private SkillTickable skillTickable;
        private PlayerConfig playerConfig;
        private WeaponConfig weaponConfig;
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

            weaponConfig = ConfigManager.Get<WeaponConfig>();
            playerConfig = ConfigManager.Get<PlayerConfig>();
            MonsterConfig monsterConfig = ConfigManager.Get<MonsterConfig>();
            LevelConfig levelConfig = ConfigManager.Get<LevelConfig>();
            levelConfig.TryGetLevelData(1, out LevelData levelData);
            spawner.SetLevel(levelData, monsterConfig);

            pedestal.SetWeaponDeltaTime(owner.TickInterval);
            owner.OnScaleTimeChanged += pedestal.SetWeaponDeltaTime;

            foreach (var weaponId in weaponsId)
            {
                if (weaponConfig.TryGetUpgradeWeapon(weaponId, 1, UpgradeType.LevelUp, out var list))
                {
                    upgradeDatas.Add(weaponId, new TotalWeaponUpgradeData { level = 1, levelups = list});
                }
            }

            await owner.Initialize();
            allWeapons = await pedestal.Initialize(weaponConfig, weaponsId, owner.Loop, owner.TickInterval);
            uiManager.Initialize();
            owner.IsPaused = false;
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

        private void StartReset()
        {
            elementStackNumber.Clear();
        }

        private void ElementChanged(int slot)
        {
            if (!elementStackNumber.TryAdd(slot, 1))
            {
                elementStackNumber[slot]++;
            }

            int level = elementStackNumber[slot];
            pedestal.SetWeaponLevel(slot, level, 0.3f);
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

            int id = 1003;
            foreach (var weapon in allWeapons)
            {
                if (weapon.WeaponId == id)
                {
                    if (upgradeDatas.TryGetValue(id, out var data))
                    {
                        if (data.levelups.Count > 0)
                        {
                            weapon.Increase(data.levelups[0]);
                            data.levelups.RemoveAt(0);
                        }
                        else
                        {
                            if(weaponConfig.TryGetUpgradeWeapon(id, data.level+1, UpgradeType.LevelUp, out var list))
                            {
                                data.levelups = list;
                                data.level++;
                                
                                weapon.Increase(data.levelups[0]);
                                data.levelups.RemoveAt(0);
                            }
                        }

                        upgradeDatas[id] = data;
                    }

                    break;
                }
            }
        }

        private void WaveSpawnCompleted(int waveIndex)
        {
            Debug.Log($"Complete wave {waveIndex} - {spawner.IsCompleted}");
        }

        struct TotalWeaponUpgradeData
        {
            public int level;
            public List<WeaponUpgradeData> levelups;
        }
    }
}