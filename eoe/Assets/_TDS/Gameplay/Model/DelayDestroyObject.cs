

using _GameToolkit.ResourceManagement;
using _GameToolkit.Shared;
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