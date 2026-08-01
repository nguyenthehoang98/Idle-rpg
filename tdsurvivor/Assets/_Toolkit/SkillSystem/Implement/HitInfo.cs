using _Toolkit.Shared;

namespace _Toolkit.SkillSystem.Implement
{
    public sealed class HitInfo
    {
        public HitInfo(Unique unique, bool isLastHit)
        {
            Unique = unique;
            IsLastHit = isLastHit;
        }

        public Unique Unique { get; }
        
        public bool IsLastHit { get; }
    }
}