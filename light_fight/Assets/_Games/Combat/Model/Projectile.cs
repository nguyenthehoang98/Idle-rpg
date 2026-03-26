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

        public void Destroy()
        {
            if(authoring != null) authoring.Destroy();
            Destroy(this);
        }
    }
}