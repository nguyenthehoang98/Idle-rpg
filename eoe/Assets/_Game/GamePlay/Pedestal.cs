using UnityEngine;

namespace _Game.GamePlay
{
    public class Pedestal : MonoBehaviour
    {
        [SerializeField] private WeaponPedestal[] weapons = new WeaponPedestal[0];

        private Coroutine[] coroutines;

        private void Awake()
        {
            coroutines = new Coroutine[weapons.Length];
        }

        private void Start()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                StartCoroutine(weapons[i].Setup(0, 0));
            }
        }

        public void SetWeaponLevel(int slot, int level, float duration)
        {
            if (slot < 0 || slot > 4) return;
            if (level < 0 || level > 3) return;
            
            StartCoroutine(weapons[slot].Setup(level, duration));
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