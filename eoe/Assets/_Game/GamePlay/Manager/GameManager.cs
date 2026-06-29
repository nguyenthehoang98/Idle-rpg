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

namespace _Game.GamePlay.Manager
{
    [RequireComponent(typeof(TickSystemOwner))]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private BottomPanel bottomPanel;
        [SerializeField] private UpgradeCardUIPicker cardUIPicker;
        [SerializeField] private Energy energy;
        [SerializeField] private EquipmentQueue equipmentQueue;
        [SerializeField] private EquipmentActivation equipmentActivation;
        
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

            owner.OnChangePause += ChangePause; 
            owner.OnChangeScaleTime += ChangeScaleTime;
            skillManager.OnPostDamage += PostDamage;
            skillManager.OnPostEarnExp += EarnExp;
            
            Monster.OnMonsterEnable += MonsterEnable;
            Monster.OnMonsterDisable += MonsterDisable;

            cardUIPicker.OnPickCard += PickCard;
            energy.OnFill += FillEnergy;
            equipmentQueue.OnQueueFull += QueueFull;
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

            await owner.Initialize();

            for (int i = 0; i < equipments.Length; i++)
            {
                await Equip(equipments[i]);
            }

            await equipmentQueue.Init(weaponConfig, equipments);

            await UniTask.WaitForSeconds(1);
            
            owner.IsPaused = false;
            owner.Loop = 1;
            
            energy.enabled = true;
            
            KitEntryScene.Instance.HideLoadingScene();
        }

        private void OnDestroy()
        {
            owner.OnChangePause -= ChangePause;
            owner.OnChangeScaleTime -= ChangeScaleTime;
            skillManager.OnPostDamage -= PostDamage;
            skillManager.OnPostEarnExp -= EarnExp;
            
            Monster.OnMonsterEnable -= MonsterEnable;
            Monster.OnMonsterDisable -= MonsterDisable;
            
            cardUIPicker.OnPickCard -= PickCard;
            energy.OnFill -= FillEnergy;
            equipmentQueue.OnQueueFull -= QueueFull;
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
                if (!weaponConfig.TryGetUpgradePowerWeapon(weaponId, UpgradeType.PowerX2, out var dataX2))
                    Debug.LogError($"Not found upgrade weapon x2 with '{weaponId}'");

                if (!weaponConfig.TryGetUpgradePowerWeapon(weaponId, UpgradeType.PowerX3, out var dataX3))
                    Debug.LogError($"Not found upgrade weapon x3 with '{weaponId}'");

                Dictionary<int, List<WeaponUpgradeData>> dict = weaponConfig.GetUpgradesLevelWeapon(weaponId);

                foreach (var pair in dict)
                {
                    foreach (var upgradeData in pair.Value) 
                        await AssetBundleManager.GetAssetCached<Sprite>(upgradeData.iconName);
                }

                GameObject go = await AssetBundleManager.GetAsset<GameObject>(weaponData.prefabName);
                go = Object.Instantiate(go, slots[currentWeaponSlot]);
                go.transform.localPosition = Vector3.zero;

                float flip = currentWeaponSlot % 2 == 0 ? 1 : -1;
                
                currentWeaponSlot++;

                Weapon weapon = go.GetComponent<Weapon>();
                if(weapon == null) Debug.LogError($"Gameobject '{go}' not attach Weapon component");
                
                await weapon.Initialize(weaponData, dict, dataX2, dataX3, flip);

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

        private void QueueFull()
        {
            float d = 0;
            float speed = 1;//owner.Loop;
            for (int i = 0; i < Const.MAX_WEAPON_SLOT; i++)
            {
                int index = i;
                float f1 = 0.1f * i;
                f1 /= speed;
                float f2 = 0.1f + 0.1f * i;
                f2 /= speed;
                float f3 = 0.5f + 0.1f * i;
                f3 /= speed;
                this.WaitInvoke(f1, () => { equipmentActivation.ReleaseAnimation(index, speed); });
                this.WaitInvoke(f2, () => { equipmentQueue.PushAnimation(index, speed); });
                this.WaitInvoke(f3, () =>
                {
                    if (equipmentQueue.TryGetWeaponData(index, out WeaponData weaponData))
                        equipmentActivation.SetWeapon(index, weaponData);
                    equipmentActivation.IdleAnimation(index, speed);
                });
                d = Mathf.Max(d, f1, f2, f3);
            }

            this.WaitInvoke(d, () =>
            {
                Dictionary<int, int> dict = new Dictionary<int, int>();

                List<int> list = equipmentQueue.GetAllEquipment();
                foreach (var id in list)
                {
                    if (!dict.TryAdd(id, 1)) dict[id]++;
                }

                foreach (var pair in dict)
                {
                    weaponContainer[pair.Key].WeaponLevel = pair.Value;
                }

                foreach (var pair in weaponContainer)
                {
                    equipmentActivation.SetBackgroundColor(list, pair.Key, pair.Value.WeaponLevel);
                }
                
                equipmentQueue.Clear();
            });
        }

        private void FillEnergy() => equipmentQueue.Increase();
        
        private void ChangePause(bool paused)
        {
            foreach (var pair in weaponContainer)
            {
                pair.Value.SetPause(paused);
            }
            energy.SetPause(paused);
        }
        
        private void ChangeScaleTime(float deltaTime)
        {
            foreach (var pair in weaponContainer)
            {
                pair.Value.TimeScale = owner.Loop;
                pair.Value.DeltaTime = owner.TickInterval / owner.Loop;
            }
        }

        private async void PickCard(WeaponUpgradeData @params)
        {
            if (weaponContainer.TryGetValue(@params.id, out Weapon weapon))
            {
                weapon.IncreaseUpgradeData(@params);
                cardUIPicker.Hide();
                bottomPanel.Show();

                await UniTask.WaitForSeconds(0.2f);
                owner.IsPaused = false;
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

                PickCardItemData();
                
                owner.IsPaused = true;
            }
        }

        private async void PickCardItemData()
        {
            List<CardItemData> temp = new List<CardItemData>();
            foreach (var pair in weaponContainer)
            {
                temp.AddRange(pair.Value.GetUpgradeDataAvailable());
            }
                
            List<CardItemData> collects = new List<CardItemData>();
            if (temp.Count < 3)
            {
                int count = temp.Count;
                collects.AddRange(temp);
                for (int i = count; i <= 3; i++) collects.Add(temp[i % count]);
            }
            else
            {
                CollectionUtils.Shuffle(ref temp);
                for (int i = 0; i < 3; i++) collects.Add(temp[i]);
            }

            await cardUIPicker.Show(collects);
                
            bottomPanel.Hide();
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