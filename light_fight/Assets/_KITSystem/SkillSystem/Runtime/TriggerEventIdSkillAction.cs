using _KITSystem.SkillSystem.Config;

namespace _KITSystem.SkillSystem.Runtime
{
    public class TriggerEventIdSkillAction : BaseSkillAction
    {
        private int eventId;
        private int skillId;
        
        public TriggerEventIdSkillAction(int skillId, int eventId, SPU spu, TriggerConfig triggerConfig, float lifeTime) : base(spu, triggerConfig, lifeTime)
        {
            this.skillId = skillId;
            this.eventId = eventId;
        }

        protected override void Execute()
        {
            base.Execute();
            spu.TriggerEventId(skillId, eventId);
        }
    }
}