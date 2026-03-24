using _Games.Combat.Model;
using _KIT.Pool;
using Unity.Entities;
using UnityEngine;

public class Projectile : MonoBehaviour, IAuthoring
{
    private void Start()
    {
        Debug.Log(@"[Projectile]Tạo 1 monster chỉ chứa logic. Monster này sẽ chuyển thành monster view để có thể xử lý strest test.
Phần callback OnAttack sẽ chuyển sang invoke theo thời gian, config với mỗi monster");
    }

    public void Initialize(Entity entity)
    {
    }

    public void DestroyProjectile()
    {
        KitPool.Destroy(gameObject);
    }
}
