using Unity.Collections;
using UnityEngine;

namespace _Games.GamePlay
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
                StartCoroutine(weapons[i].Setup(0, 0, 1));
            }
        }

        public void SetWeaponLevel(int slot, int level, float duration, float deltaTime)
        {
            if (slot < 0 || slot > 4) return;
            if (level < 0 || level > 3) return;
            
            Coroutine coroutine = coroutines[slot];
            
            if (coroutine != null) 
                StopCoroutine(coroutine);
            
            coroutines[slot] = StartCoroutine(weapons[slot].Setup(level, duration, deltaTime));
        }
    }
}