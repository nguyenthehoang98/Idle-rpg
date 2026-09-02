using _GameToolkit.Share;
using UnityEngine;

namespace _GameToolkit.Colliders
{
    [RequireComponent(typeof(CapsuleCollider2D))]
    internal sealed class CapsuleCollision2DDetector : CollisionDetector
    {
        private CapsuleCollider2D capsuleCollider2D;
        private CapsuleDirection2D direction;

        protected override void Awake()
        {
            base.Awake();
            capsuleCollider2D = GetComponent<CapsuleCollider2D>();
            direction = capsuleCollider2D.direction;
        }

        public override void Tick(float deltaTime)
        {
            Vector2 size = Vector2.Scale(capsuleCollider2D.size, transform.lossyScale);

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