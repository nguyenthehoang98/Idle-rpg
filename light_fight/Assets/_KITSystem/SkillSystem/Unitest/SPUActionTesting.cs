using System;
using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace _KITSystem.SkillSystem.Unitest
{
    public class SPUActionTesting
    {
        [Test]
        public void Add_TriggerTimer()
        {
            var spu = CreateSPU();
            var skillConfig = ScriptableObject.CreateInstance<SkillConfig>();
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new EventConfig
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new CastProjectileConfig
                    {
                        projectileConfig = new MeleeProjectileConfig()
                    }
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
            var spu = CreateSPU();
            var skillConfig = ScriptableObject.CreateInstance<SkillConfig>();
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new EventConfig
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new CastProjectileConfig
                    {
                        projectileConfig = new MeleeProjectileConfig()
                    }
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
            var spu = CreateSPU();
            var skillConfig = ScriptableObject.CreateInstance<SkillConfig>();
            skillConfig.defaultSkillConfig = new DefaultSkillConfig
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new EventConfig[1]
            {
                new EventConfig
                {
                    triggerConfig = new TriggerConfig
                    {
                        timer = 0.5f, type = TriggerConfig.TriggerType.Timeline
                    },
                    actionConfig = new CastProjectileConfig
                    {
                        projectileConfig = new MeleeProjectileConfig()
                    }
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(1.99f);
            bool flag = spu.HasSkill(skillId, out var actions);
            Assert.True(flag);
        }

        SPU CreateSPU()
        {
            return new SPU();
        }
    }
}
