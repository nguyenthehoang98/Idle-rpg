using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Projectile : MonoBehaviour
    {
        static readonly int Death = Animator.StringToHash("Death");
        static readonly int Initialize_ = Animator.StringToHash("Initialize");
        
        [SerializeField] private Animator animator;

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private bool isInitialized = false;
        
        public void Initialize()
        {
            animator.Play(Initialize_);

            elapsedTime = 0;
            targetPosition = previousPosition = transform.position;
            
            isInitialized = true;
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
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            transform.position = Vector3.Lerp(previousPosition, targetPosition, Mathf.Clamp01(elapsedTime / deltaTime));
        }

        public void Destroy()
        {
            if (!isInitialized) return;
            
            isInitialized = false;
            
            animator.Play(Death);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
