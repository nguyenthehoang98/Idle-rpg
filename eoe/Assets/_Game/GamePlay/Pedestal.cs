using System.Collections.Generic;
using _Game.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Pedestal : MonoBehaviour
    {
        [SerializeField] private WeaponPedestal[] weapons = new WeaponPedestal[0];

        private readonly Coroutine[] coroutines = new Coroutine[4];

        public async UniTask<List<Weapon>> Initialize(WeaponConfig weaponConfig, int[] weaponsId, float timeScale, float deltaTime)
        {
            List<Weapon> list = new List<Weapon>();
            for (int i = 0; i < weapons.Length; i++)
            {
                if (i < weaponsId.Length)
                {
                    int weaponId = weaponsId[i];

                    if (!weaponConfig.TryGetWeaponData(weaponId, out var weaponsData))
                        continue;

                    if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX2, out var list1)) 
                        Debug.LogError($"Not found upgrade weapon x2 with '{weaponId}'");
                    
                    if (!weaponConfig.TryGetUpgradeWeapon(weaponId, 0, UpgradeType.PowerX3, out var list2)) 
                        Debug.LogError($"Not found upgrade weapon x3 with '{weaponId}'");
                        
                    list.Add(await weapons[i].Initialize(weaponsData, list1[0], list2[0], timeScale, deltaTime));
                }
                else list.Add(null);

                if (coroutines[i] != null) StopCoroutine(coroutines[i]);
                coroutines[i] = StartCoroutine(weapons[i].Setup(0, 0));
            }

            return list;
        }

        public void SetWeaponLevel(int slot, int level, float duration)
        {
            if (slot < 0 || slot > 4) return;
            if (level < 0 || level > 3) return;

            if (coroutines[slot] != null) StopCoroutine(coroutines[slot]);
            coroutines[slot] = StartCoroutine(weapons[slot].Setup(level, duration));
        }

        public void SetWeaponDeltaTime(float deltaTime)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].DeltaTime = deltaTime;
            }
        }
    }
}