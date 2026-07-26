using UnityEngine;
using EntityId = _GameToolkit.Entity.EntityId;

namespace _GameToolkit.Collision
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class BoxCollision2D : BaseCollision
    {
        [SerializeField] private int maximumResult = 10;
     
        private BoxCollider2D box;
        
        private Collider2D[] results;
        private ContactFilter2D filter;

        private void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            
            results = new Collider2D[maximumResult];

            filter = new ContactFilter2D();
            filter.SetLayerMask(layerMask);
            filter.useLayerMask = true;
        }

        protected override void OnTick()
        {
            Vector2 size = Vector2.Scale(box.size, transform.lossyScale);
            
            int count = Physics2D.OverlapBox(transform.position, size, transform.eulerAngles.z, filter, results);

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