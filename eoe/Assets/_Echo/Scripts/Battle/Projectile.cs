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

        public void SetDirection(Vector3 dir)
        {
            StartCoroutine(Lerp(transform.position, dir.normalized * radius));
        }

        IEnumerator Lerp(Vector3 start, Vector3 end)
        {
            onStart?.Invoke();

            transform.position = start;
     
            Vector2 direction = end - start;
        
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.eulerAngles = new Vector3(0, 0, angle - 90);
        
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
            
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
            
                yield return null;
            }
        
            transform.position = end;
            
            onComplete?.Invoke();
        }
    }
}
