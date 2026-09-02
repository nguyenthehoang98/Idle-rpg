using _GameToolkit.Resource;
using _GameToolkit.Utils;
using UnityEngine;

namespace _TDS.Gameplay.Model
{
    public class DelayDestroyObject : MonoBehaviour
    {
        public float duration;
        
        private void OnEnable()
        {
            Timing.CallDelayed(duration, () => { Pool.Destroy(gameObject); });
        }
    }
}