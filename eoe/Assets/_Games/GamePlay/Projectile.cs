using _KITSystem.Resource;
using UnityEngine;
using UnityEngine.Events;

namespace _Games.GamePlay
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float radius = 3;
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
