using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Projectile : MonoBehaviour
    {
        static readonly int Death = Animator.StringToHash("Death");
        static readonly int _Initialize = Animator.StringToHash("Initialize");
        
        [SerializeField] private Animator animator;

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private bool isRunning = false;
        private bool shouldDestroy = false;
        
        public void Initialize()
        {
            animator.Play(_Initialize);

            elapsedTime = 0;
            targetPosition = previousPosition = transform.position;
            
            shouldDestroy = false;
            isRunning = true;
        }

        public void SetPosition(Vector3 position, float deltaTime)
        {
            this.previousPosition = transform.position;
            this.targetPosition = position;
            this.deltaTime = deltaTime;
            this.elapsedTime = 0;
            
            Vector3 direction = position - previousPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation =  Quaternion.Euler(0, 0, angle + 90f);
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);

            if (t >= 1.0f && shouldDestroy) isRunning = false;
        }

        public void Destroy()
        {
            if (shouldDestroy) return;
            
            shouldDestroy = true;
            
            animator.Play(Death);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
