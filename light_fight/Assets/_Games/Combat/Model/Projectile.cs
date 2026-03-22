using _Games.Combat.Model;
using _KIT.Pool;
using Unity.Entities;
using UnityEngine;

public class Projectile : MonoBehaviour, IAuthoring
{
    private bool shouldDestroy;

    public void Initialize(Entity entity)
    {
        shouldDestroy = false;
    }

    private void Update()
    {
        if (shouldDestroy)
        {
            KitPool.Destroy(gameObject);
        }
    }

    public void DestroyProjectile()
    {
        shouldDestroy = true;
    }
}
