using UnityEngine;
using EntityId = _GameToolkit.Entity.EntityId;

namespace _GameToolkit.Collision
{
    [RequireComponent(typeof(CapsuleCollider2D))]
    public sealed class CapsuleCollision2D : BaseCollision
    {
        [SerializeField] private int maximumResult = 10;

        private CapsuleCollider2D capsual;
        
        private Collider2D[] results;
        private ContactFilter2D filter;
        private CapsuleDirection2D direction;

        private void Awake()
        {
            capsual = GetComponent<CapsuleCollider2D>();

            direction = capsual.direction;
            
            results = new Collider2D[maximumResult];

            filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            filter.useLayerMask = true;
        }

        protected override void OnTick()
        {
            Vector2 size = Vector2.Scale(capsual.size, transform.lossyScale);
            
            int count = Physics2D.OverlapCapsule(transform.position, size, direction, transform.eulerAngles.z, filter, results);

            for (int i = 0; i < count; i++)
            {
                EntityId entityId = results[i].GetComponent<EntityId>();

                if (entityId != null)
                {
                    Overlap(entityId);
                }
            }
        }
    }
}