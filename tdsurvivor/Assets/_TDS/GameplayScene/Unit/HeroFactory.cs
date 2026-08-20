using _TDS.Config;
using _TDS.GameplayScene.SkillSystem;
using _Toolkit.ResourceManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    public static class HeroFactory
    {
        public static async UniTask<HeroController> SpawnHero(string prefabAsset, Vector3 position, Transform parent = null)
        {
            GameObject prefab = await AssetLoader.GetAsset<GameObject>(prefabAsset);

            if (prefab == null)
            {
                Debug.LogError($"[HeroFactory] Not found prefab '{prefabAsset}'");
                return null;
            }

            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity, parent);

            HeroController hero = go.GetComponent<HeroController>();

            if (hero == null)
            {
                Debug.LogError($"[HeroFactory] Prefab '{go.name}' missing HeroController component");
                return null;
            }

            hero.Initialize();

            return hero;
        }
    }
}
