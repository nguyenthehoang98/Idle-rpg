using _Game.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Pedestal : MonoBehaviour
    {
        [SerializeField] private WeaponPedestal[] weapons = new WeaponPedestal[0];

        private Coroutine[] coroutines = new Coroutine[4];

        public async UniTask Initialize(WeaponData[] weaponsData, float timeScale, float deltaTime)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                var pedestal = weapons[i];
                if (i < weaponsData.Length)
                    await pedestal.Initialize(weaponsData[i], timeScale, deltaTime);

                if (coroutines[i] != null) StopCoroutine(coroutines[i]);
                coroutines[i] = StartCoroutine(pedestal.Setup(0, 0));
            }
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