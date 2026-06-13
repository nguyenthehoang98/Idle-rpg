using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace _Echo.Scripts.Battle
{
    public class Projectile : MonoBehaviour
    {
        public float duration = 0.5f;
        public float radius = 3;
        public UnityEvent onStart;
        public UnityEvent onComplete;

        public void SetDestination(Vector3 destination)
        {
            StartCoroutine(Translate(destination));
        }

        IEnumerator Translate(Vector3 destination)
        {
            onStart?.Invoke();

            Vector3 startPosition = transform.position;
            
            Vector3 direction = (destination - startPosition).normalized;
        
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.eulerAngles = new Vector3(0, 0, angle - 90);
            
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                
                elapsed += dt;

                transform.position += direction * (dt * radius);
            
                yield return null;
            }
            
            onComplete?.Invoke();
        }
    }
}
