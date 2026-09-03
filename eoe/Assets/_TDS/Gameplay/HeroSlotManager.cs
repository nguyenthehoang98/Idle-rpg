using System;
using System.Collections.Generic;
using _GameToolkit.GameConfig;
using _GameToolkit.ResourceManagement;
using _TDS.Battle;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Gameplay
{
    /// <summary>
    /// Quản lý các slot đặt hero. Mỗi slot = 1 Transform.
    /// Nhận danh sách heroId từ GameplayScene.heroIds, id thoả config mới tạo.
    /// </summary>
    public class HeroSlotManager : MonoBehaviour
    {
        [SerializeField] private Transform[] slots;

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
            {
                Debug.LogError($"[{name}] No hero slots assigned", this);
            }
        }

        public async UniTask BuildHeroes(int[] heroIds)
        {
            HeroConfig heroConfig = ConfigManager.Get<HeroConfig>();
            SkillConfig skillConfig = ConfigManager.Get<SkillConfig>();

            int count = Mathf.Min(heroIds.Length, slots.Length);

            for (int i = 0; i < count; i++)
            {
                int heroId = heroIds[i];
                if (heroId <= 0) continue; // ô trống

                if (!heroConfig.TryGetHero(heroId, out HeroConfigData heroData))
                {
                    Debug.LogWarning($"[{name}] Hero id '{heroId}' not found in HeroConfig");
                    continue;
                }

                GameObject prefab = await AssetLoader.GetAssetCached<GameObject>(heroData.prefabName);
                if (prefab == null)
                {
                    Debug.LogWarning($"[{name}] Hero prefab '{heroData.prefabName}' not found");
                    continue;
                }

                Hero hero = Instantiate(prefab, slots[i]).GetComponent<Hero>();
                hero.transform.localPosition = Vector3.zero;

                // skill cấu hình theo attackId
                if (!skillConfig.TryGetSkill(heroData.attackId, out SkillConfigData skillData))
                {
                    Debug.LogWarning($"[{name}] Hero '{heroId}' attack skill '{heroData.attackId}' not found in SkillConfig");
                    Pool.Destroy(hero.gameObject);
                    continue;
                }

                Debug.Log($"Hero added {heroData.id}");

                hero.Initialize(heroData, skillData);
            }
        }
    }
}
