using _GameToolkit.Shared;
using UnityEngine;

namespace _GameToolkit.Collider
{
    [RequireComponent(typeof(CapsuleCollider2D))]
    sealed class CapsuleCollision2DDetector : BaseCollisionDetector
    {
        CapsuleCollider2D collider2D;
        private CapsuleDirection2D direction;

        protected override void Awake()
        {
            base.Awake();
            collider2D = GetComponent<CapsuleCollider2D>();
            direction = collider2D.direction;
        }

        public override void Tick(float deltaTime)
        {
            Vector2 size = Vector2.Scale(collider2D.size, transform.lossyScale);

            int count = Physics2D.OverlapCapsule(
                transform.position, size, direction, transform.eulerAngles.z, ContactFilter, Results
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