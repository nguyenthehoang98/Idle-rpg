using _Toolkit.Shared;
using UnityEngine;

namespace _Toolkit.Collider
{
    [RequireComponent(typeof(BoxCollider2D))]
    sealed class BoxCollision2DDetector : BaseCollisionDetector
    {
        private BoxCollider2D collider2D;

        protected override void Awake()
        {
            base.Awake();
            collider2D = GetComponent<BoxCollider2D>();
        }

        public override void Tick(float deltaTime)
        {
            Vector2 size = Vector2.Scale(collider2D.size, transform.lossyScale);

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