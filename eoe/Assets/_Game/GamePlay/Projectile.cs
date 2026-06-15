using _KITSystem.Resource;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.GamePlay
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private UnityEvent onStart;
        [SerializeField] private UnityEvent onComplete;

        public void Initialize() => onStart.Invoke();
        
        public void Destroy() => onComplete?.Invoke();

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
