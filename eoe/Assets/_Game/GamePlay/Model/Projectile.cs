using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay.Model
{
    public class Projectile : MonoBehaviour
    {
        public Vector3 TargetPosition { get; private set; }
        public Vector3 PreviousPosition { get; private set; }
        private float elapsedTime;
        private float deltaTime;
        private bool isRunning = false;
        private bool shouldDestroy = false;
        
        public void Initialize()
        {
            elapsedTime = 0;
            TargetPosition = PreviousPosition = transform.position;
            
            shouldDestroy = false;
            isRunning = true;
        }

        public void SetPosition(Vector3 position, float deltaTime)
        {
            this.PreviousPosition = transform.position;
            this.TargetPosition = position;
            this.deltaTime = deltaTime;
            this.elapsedTime = 0;
            
            Vector3 direction = position - PreviousPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation =  Quaternion.Euler(0, 0, angle + 90f);
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(PreviousPosition, TargetPosition, t);

            if (t >= 1.0f && shouldDestroy)
            {
                Pool.Destroy(gameObject);
                
                isRunning = false;
            }
        }

        public void Destroy()
        {
            if (shouldDestroy) return;
            
            shouldDestroy = true;
        }
    }
}
