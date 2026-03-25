using _KIT.Pool;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class ProjectileAuthoring : MonoBehaviour, IAuthoring
    {
        public void Initialize(Entity entity)
        {
        }

        public void Destroy()
        {
            KitPool.Destroy(gameObject);
        }
    }
}
