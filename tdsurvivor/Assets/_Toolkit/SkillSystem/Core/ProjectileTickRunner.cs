using _Toolkit.Updater;

namespace _Toolkit.SkillSystem.Core
{
    public sealed class ProjectileTickRunner : BaseTickRunner<Projectile>
    {
        public static ProjectileTickRunner Instance { get; private set; }

        public ProjectileTickRunner()
        {
            if(Instance == null) Instance = this; 
        }

        public void Dispose()
        {
            if (Instance != null && Instance == this) Instance = null;
        }
    }
}