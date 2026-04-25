using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class Projectile : ScriptableObject
    {
        private ProjectileAuthoring authoring;

        public void Init(ProjectileAuthoring authoring)
        {
            this.authoring = authoring;
        }

        public void Hit(int skillId, float3 position)
        {
            if (authoring != null) authoring.Hit(skillId, position);
        }

        public void Destroy()
        {
            if(authoring != null) authoring.Destroy();
            Destroy(this);
        }
    }
}