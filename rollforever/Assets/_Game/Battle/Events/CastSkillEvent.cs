using _Game.Battle.Model;
using _KIT.Event;
using Unity.Mathematics;

namespace _Game.Battle.Events
{
    public readonly struct CastSkillEvent : IEvent
    {
        public readonly int SkillId;
        public readonly int Source;
        public readonly float2 StartPosition;
        public readonly Team Team;
        
        public CastSkillEvent(int source, int skillId, float2 startPosition, Team team)
        {
            SkillId = skillId;
            StartPosition = startPosition;
            Team = team;
            Source = source;
        }
    }
}