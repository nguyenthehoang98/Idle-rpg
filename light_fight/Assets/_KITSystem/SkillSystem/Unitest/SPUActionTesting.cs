using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Runtime;
using NUnit.Framework;
using UnityEngine;
using CastProjectileAction = _KITSystem.SkillSystem.Config.CastProjectileAction;

namespace _KITSystem.SkillSystem.Unitest
{
    public class SPUActionTesting
    {
        [Test]
        public void Add_TriggerTimer()
        {
            var spu = CreateSPU();
            var skillConfig = ScriptableObject.CreateInstance<SkillConfig>();
            skillConfig.defaultSkill = new DefaultSkill
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new BaseEvent[1]
            {
                new BaseEvent
                {
                    trigger = new Trigger
                    {
                        timer = 0.5f, type = Trigger.TriggerType.Timeline
                    },
                    action = new CastProjectileAction
                    {
                        projectile = new MeleeProjectile()
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
            skillConfig.defaultSkill = new DefaultSkill
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new BaseEvent[1]
            {
                new BaseEvent
                {
                    trigger = new Trigger
                    {
                        timer = 0.5f, type = Trigger.TriggerType.Timeline
                    },
                    action = new CastProjectileAction
                    {
                        projectile = new MeleeProjectile()
                    }
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(2f);
            bool flag = spu.HasSkill(skillId, out var actions);
            Assert.IsFalse(flag);
        }
        
        [Test]
        public void Add_TriggerTimer_None_EndLifeTime()
        {
            var spu = CreateSPU();
            var skillConfig = ScriptableObject.CreateInstance<SkillConfig>();
            skillConfig.defaultSkill = new DefaultSkill
            {
                lifeTimeInSeconds = 2,
            };
            skillConfig.events = new BaseEvent[1]
            {
                new BaseEvent
                {
                    trigger = new Trigger
                    {
                        timer = 0.5f, type = Trigger.TriggerType.Timeline
                    },
                    action = new CastProjectileAction
                    {
                        projectile = new MeleeProjectile()
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
