using _KIT.Event;
using Unity.Mathematics;

namespace _Game.Battle.Events
{
    public readonly struct CastSkillEvent : IEvent
    {
        public readonly int SkillId;
        public readonly int Source;
        public readonly float2 StartPosition;
        public readonly int Target;
        
        public CastSkillEvent(int source, int skillId, float2 startPosition, int target)
        {
            SkillId = skillId;
            StartPosition = startPosition;
            Target = target;
            Source = source;
        }
    }
}