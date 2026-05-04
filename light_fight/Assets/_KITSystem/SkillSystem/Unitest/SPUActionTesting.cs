using _KITSystem.SkillSystem.Config;
using _KITSystem.SkillSystem.Config.Action;
using _KITSystem.SkillSystem.Config.Model;
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
            skillConfig.defineSkill = new DefineSkill
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
                        projectile = new CastProjectileAction.MeleeProjectile()
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
            skillConfig.defineSkill = new DefineSkill
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
                        projectile = new CastProjectileAction.MeleeProjectile()
                    }
                }
            };

            int skillId = SkillFactory.Build(spu, skillConfig);
            spu.Tick(2);
            bool flag = spu.HasSkill(skillId, out var actions);
            if (flag) Debug.Log($"Actions:" + string.Join(',', actions));
            Assert.IsTrue(flag);
        }

        SPU CreateSPU()
        {
            return new SPU();
        }
    }
}
