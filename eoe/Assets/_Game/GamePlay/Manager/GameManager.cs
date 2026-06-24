using System.Collections.Generic;
using _Game._GamePlay2;
using _Game.Configs;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.GamePlay.Manager
{
    [RequireComponent(typeof(TickSystemOwner))]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private UpgradeCardUIPicker cardUIPicker;

        private readonly Coroutine[] coroutines = new Coroutine[4];
        private Dictionary<int, Weapon> weaponContainer = new Dictionary<int, Weapon>();
        private Dictionary<int, int> damageMemory = new Dictionary<int, int>();
        private TickSystemOwner owner;
        private SpawnManager spawnManager;
        private SkillManager skillManager;
        private WeaponConfig weaponConfig;
        private PlayerConfig playerConfig;
        private PlayerRuntimeData player;

        private int totalMonsterAlive;

        private void Awake()
        {
            owner = GetComponent<TickSystemOwner>();
            owner.TryGetTickable(out skillManager);
            owner.TryGetTickable(out spawnManager);
            
            player = new PlayerRuntimeData();

            skillManager.OnPostDamage += PostDamage;
            skillManager.OnPostEarnExp += EarnExp;
            
            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;

            cardUIPicker.OnPickCard += PickCard;
        }

        private async void Start()
        {
            playerConfig = ConfigManager.Get<PlayerConfig>();
            weaponConfig = ConfigManager.Get<WeaponConfig>();
            
            spawnManager.SetLevel(1);

            await BuildHero(10);

            await owner.Initialize();
            
            owner.IsPaused = false;
            
            KitEntryScene.Instance.HideLoadingScene();
        }

        private void OnDestroy()
        {
            skillManager.OnPostDamage -= PostDamage;
            skillManager.OnPostEarnExp -= EarnExp;
            
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;
            
            cardUIPicker.OnPickCard -= PickCard;
        }

        public async UniTask Equip(int weaponId)
        {
            if (weaponConfig.TryGetWeaponData(weaponId, out WeaponData weaponData))
            {
                int count = weaponContainer.Count;
                
                if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX2, out var list1)) 
                    Debug.LogError($"Not found upgrade weapon x2 with '{weaponId}'");
                    
                if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX3, out var list2)) 
                    Debug.LogError($"Not found upgrade weapon x3 with '{weaponId}'");

                /*Weapon weapon = await pedestals[count].Initialize(weaponData, list1[0], list2[0], owner.Loop, owner.TickInterval);

                await pedestals[count].Setup(0, 0);*/

                //weaponContainer[weaponId] = weapon;
            }
            else Debug.LogError($"Not found weapon '{weaponId}'");
        }

        public void SetWeaponLevel(int slot, int level, float duration)
        {
            if (slot >= 0 && slot <= 3 && level >= 0 && level <= 2)
            {
                StopCoroutine(coroutines[slot]);
               
                //coroutines[slot] = StartCoroutine(pedestals[slot].Setup(level, duration));
            }
        }

        private async UniTask BuildHero(int heroId)
        {
            if (playerConfig.TryGetHero(heroId, out HeroData heroData))
            {
                GameObject go = null;
                go = await AssetBundleManager.GetAsset<GameObject>(heroData.prefabName);
                Object.Instantiate(go, Vector3.zero, Quaternion.identity);

                if (playerConfig.TryGetWing(heroData.wingId, out WingData wingData) && !string.IsNullOrEmpty(wingData.wingName))
                {
                    go = await AssetBundleManager.GetAsset<GameObject>(wingData.wingName);
                    Object.Instantiate(go, Vector3.zero, Quaternion.identity);
                }
            }
        }
        
        // callback

        private void OnChangeScaleTime(float deltaTime)
        {
            /*foreach (var pedestal in pedestals)
            {
                pedestal.DeltaTime = deltaTime;
            }*/
        }

        private void PickCard(WeaponUpgradeData @params)
        {
            if (weaponContainer.TryGetValue(@params.id, out Weapon weapon))
            {
                weapon.Increase(@params);
            }
        }

        private void MonsterDisable(Monster m)
        {
            totalMonsterAlive--;
            
            if (totalMonsterAlive == 0 && spawnManager.IsPaused)
            {
                spawnManager.IsPaused = false;
            }
        }

        private void MonsterEnable(Monster m)
        {
            totalMonsterAlive++;
        }

        private void EarnExp(PostEarnExpParams @params)
        {
            player.CurrentExp += @params.Exp;
            
            if (playerConfig.TryGetExp(player.CurrentLevel + 1, out PlayerExpData data) && player.CurrentExp >= data.exp)
            {
                player.CurrentLevel += 1;
                player.CurrentExp -= data.exp;
                
                cardUIPicker.Show();
                Debug.Log("pause");
            }
        }

        private void PostDamage(PostDamageParams @params)
        {
            if (!damageMemory.TryAdd(@params.Source.skillId, @params.Damage))
            {
                damageMemory[@params.Source.skillId] += @params.Damage;
            }
        }
    }
}