namespace _Games.Combat.Model
{
    public class Projectile : UnityEngine.Object
    {
        private ProjectileAuthoring authoring;

        public Projectile(ProjectileAuthoring authoring)
        {
            this.authoring = authoring;
        }

        public void Destroy()
        {
            if(authoring != null) authoring.Destroy();
        }
    }
}