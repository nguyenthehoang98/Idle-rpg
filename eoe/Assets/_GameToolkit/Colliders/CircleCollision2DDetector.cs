using _GameToolkit.Share;
using UnityEngine;

namespace _GameToolkit.Colliders
{
    [RequireComponent(typeof(CircleCollider2D))]
    sealed class CircleCollision2DDetector : CollisionDetector
    {
        private CircleCollider2D circleCollider2D;

        protected override void Awake()
        {
            base.Awake();
            circleCollider2D = GetComponent<CircleCollider2D>();
        }

        public override void Tick(float deltaTime)
        {
            int count = Physics2D.OverlapCircle(
                transform.position, circleCollider2D.radius, ContactFilter, Results
            );
            
            for (int i = 0; i < count; i++)
            {
                Unique unique = Results[i].GetComponent<Unique>();

                if (unique != null)
                {
                    Overlapped(unique);
                }
            }
        }
    }
}