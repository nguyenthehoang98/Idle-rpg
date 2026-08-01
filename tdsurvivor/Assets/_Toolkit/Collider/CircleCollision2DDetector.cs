using _Toolkit.Shared;
using UnityEngine;

namespace _Toolkit.Collider
{
    [RequireComponent(typeof(CircleCollider2D))]
    sealed class CircleCollision2DDetector : BaseCollisionDetector
    {
        CircleCollider2D collider2D;

        protected override void Awake()
        {
            base.Awake();
            collider2D = GetComponent<CircleCollider2D>();
        }

        public override void Tick(float deltaTime)
        {
            int count = Physics2D.OverlapCircle(
                transform.position, collider2D.radius, ContactFilter, Results
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