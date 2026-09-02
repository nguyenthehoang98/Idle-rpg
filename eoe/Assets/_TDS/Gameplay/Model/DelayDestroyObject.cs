using _GameToolkit.Resource;
using _KITSystem.Utils;
using UnityEngine;

namespace _TDS.Gameplay.Model
{
    public class DelayDestroyObject : MonoBehaviour
    {
        public float duration;
        
        private void OnEnable()
        {
            this.WaitInvoke(duration, () => { Pool.Destroy(gameObject); });
        }
    }
}