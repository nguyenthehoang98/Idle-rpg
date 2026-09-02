using _GameToolkit.Share;
using UnityEngine;

namespace _GameToolkit.Colliders
{
    [RequireComponent(typeof(BoxCollider2D))]
    sealed class BoxCollision2DDetector : CollisionDetector
    {
        private BoxCollider2D boxCollider2D;

        protected override void Awake()
        {
            base.Awake();
            boxCollider2D = GetComponent<BoxCollider2D>();
        }

        public override void Tick(float deltaTime)
        {
            Vector2 size = Vector2.Scale(boxCollider2D.size, transform.lossyScale);

            int count = Physics2D.OverlapBox(
                transform.position, size, transform.eulerAngles.z, ContactFilter, Results
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