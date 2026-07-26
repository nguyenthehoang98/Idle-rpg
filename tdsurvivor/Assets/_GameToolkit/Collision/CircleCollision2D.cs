using UnityEngine;
using EntityId = _GameToolkit.Entity.EntityId;

namespace _GameToolkit.Collision
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class CircleCollision2D : BaseCollision
    {
        [SerializeField] private int maximumResult = 10;
     
        private CircleCollider2D circle;
        
        private Collider2D[] results;
        private ContactFilter2D filter;

        private void Awake()
        {
            circle = GetComponent<CircleCollider2D>();
            
            results = new Collider2D[maximumResult];

            filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            filter.useLayerMask = true;
        }

        protected override void OnTick()
        {
            int count = Physics2D.OverlapCircle(transform.position, circle.radius, filter, results);
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