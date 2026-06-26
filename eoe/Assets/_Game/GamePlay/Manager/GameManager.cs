using System.Collections.Generic;
using _Game.Configs;
using _Game.GamePlay.Data;
using _Game.GamePlay.Model;
using _Game.GamePlay.Utils;
using _Game.GamePlay.View;
using _KITSystem.Config;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Game.GamePlay.Manager
{
    [RequireComponent(typeof(TickSystemOwner))]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private Button btnPushEquipment;
        [SerializeField] private UpgradeCardUIPicker cardUIPicker;
        [SerializeField] private Energy energy;
        [SerializeField] private EquipmentQueue equipmentQueue;
        
        [SerializeField] private int[] equipments = new int[4];
        [SerializeField] private Transform[] slots = new Transform[4];

        private Dictionary<int, Weapon> weaponContainer = new Dictionary<int, Weapon>();
        private Dictionary<int, int> damageMemory = new Dictionary<int, int>();
        private TickSystemOwner owner;
        private SpawnManager spawnManager;
        private SkillManager skillManager;
        private WeaponConfig weaponConfig;
        private PlayerConfig playerConfig;
        private PlayerRuntimeData player;

        private int totalMonsterAlive;
        private int currentWeaponSlot;

        private void Awake()
        {
            energy.enabled = false;
            
            owner = GetComponent<TickSystemOwner>();
            owner.TryGetTickable(out skillManager);
            owner.TryGetTickable(out spawnManager);
            
            owner.OnChangeScaleTime += OnChangeScaleTime;
            skillManager.OnPostDamage += PostDamage;
            skillManager.OnPostEarnExp += EarnExp;
            
            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;

            cardUIPicker.OnPickCard += PickCard;
            energy.OnFill += FillEnergy;

            btnPushEquipment.onClick.AddListener(() => { equipmentQueue.Decrease(); });
        }

        private async void Start()
        {
            playerConfig = ConfigManager.Get<PlayerConfig>();
            weaponConfig = ConfigManager.Get<WeaponConfig>();
            player = new PlayerRuntimeData();

            // load 
            GameObject go = null;
            go = await AssetBundleManager.GetAssetCached<GameObject>(Const.TEXT_DAMAGE_NORMAL);
            Pool.RegisterPool(go, true);
            Pool.Destroy(Pool.Instantiate(go));
            
            go = await AssetBundleManager.GetAssetCached<GameObject>(Const.TEXT_DAMAGE_CRITICAL);
            Pool.RegisterPool(go, true);
            Pool.Destroy(Pool.Instantiate(go));
            
            spawnManager.SetLevel(1);

            await BuildHero(10);

            await equipmentQueue.Init(weaponConfig, equipments);

            await owner.Initialize();

            for (int i = 0; i < equipments.Length; i++)
            {
                await Equip(equipments[i]);
            }
            
            owner.IsPaused = false;
            owner.Loop = 1;

            foreach (var pair in weaponContainer)
            {
                pair.Value.WeaponLevel = 2;
            }
            
            energy.enabled = true;
            
            KitEntryScene.Instance.HideLoadingScene();
        }

        private void OnDestroy()
        {
            owner.OnChangeScaleTime -= OnChangeScaleTime;
            skillManager.OnPostDamage -= PostDamage;
            skillManager.OnPostEarnExp -= EarnExp;
            
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;
            
            cardUIPicker.OnPickCard -= PickCard;
            energy.OnFill -= FillEnergy;
        }

        private async UniTask Equip(int weaponId)
        {
            if (!weaponConfig.TryGetWeaponData(weaponId, out WeaponData weaponData))
            {
                Debug.LogWarning($"Not found weapon with id '{weaponId}'");
                return;
            }
            
            if (!string.IsNullOrEmpty(weaponData.prefabName))
            {
                if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX2, out var list1))
                    Debug.LogError($"Not found upgrade weapon x2 with '{weaponId}'");

                if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX3, out var list2))
                    Debug.LogError($"Not found upgrade weapon x3 with '{weaponId}'");

                GameObject go = await AssetBundleManager.GetAsset<GameObject>(weaponData.prefabName);
                go = Object.Instantiate(go, slots[currentWeaponSlot]);
                go.transform.localPosition = Vector3.zero;

                float flip = currentWeaponSlot % 2 == 0 ? 1 : -1;
                
                currentWeaponSlot++;

                Weapon weapon = go.GetComponent<Weapon>();
                if(weapon == null) Debug.LogError($"Gameobject '{go}' not attach Weapon component");
                
                WeaponUpgradeData upgradeDataX2 = list1[0];
                WeaponUpgradeData upgradeDataX3 = list2[0];
                await weapon.Initialize(weaponData, upgradeDataX2, upgradeDataX3, flip);

                weaponContainer[weaponId] = weapon;
            }
            else Debug.LogError($"Not found weapon_prefab '{weaponData.prefabName}'");
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

        private void FillEnergy() => equipmentQueue.Increase();
        
        private void OnChangeScaleTime(float deltaTime)
        {
            foreach (var pair in weaponContainer)
            {
                pair.Value.TimeScale = owner.Loop;
                pair.Value.DeltaTime = owner.TickInterval / owner.Loop;
            }
        }

        private void PickCard(WeaponUpgradeData @params)
        {
            if (weaponContainer.TryGetValue(@params.id, out Weapon weapon))
            {
                weapon.IncreaseUpgradeData(@params);
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