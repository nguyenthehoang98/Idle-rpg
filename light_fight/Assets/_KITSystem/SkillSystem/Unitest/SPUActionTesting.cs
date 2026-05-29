using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.SkillSystem.Unitest
{
    public class SPUActionTesting
    {
        private SPU spu;

        [SetUp]
        public void Setup()
        {
            spu = new SPU();
        }

        [Test]
        public void Add_TriggerTimer()
        {
            var skillConfig = ScriptableObject.CreateInstance<SkillFrameConfig>();
            
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new()
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new TriggerEventIdConfig()
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(1);
            bool flag = spu.HasSkill(skillId, out var actions);
            Assert.IsTrue(flag);
        }

        [Test]
        public void Add_TriggerTimer_EndLifeTime()
        {
            var skillConfig = ScriptableObject.CreateInstance<SkillFrameConfig>();
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new()
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new TriggerEventIdConfig()
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(2.1f);
            bool flag = spu.HasSkill(skillId, out var actions);
            Assert.IsFalse(flag);
        }

        [Test]
        public void Add_TriggerTimer_None_EndLifeTime()
        {
            var skillConfig = ScriptableObject.CreateInstance<SkillFrameConfig>();
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new()
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new TriggerEventIdConfig()
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(1.99f);
            bool flag = spu.HasSkill(skillId, out var actions);
            Assert.True(flag);
        }
    }
}