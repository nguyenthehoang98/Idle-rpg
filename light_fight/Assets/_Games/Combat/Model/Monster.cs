using System;
using System.Threading;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Config;
using _Games.Utils;
using _KIT.Event;
using Cysharp.Threading.Tasks;
using Unity.Entities;

namespace _Games.Combat.Model
{
    public class Monster : UnityEngine.Object, IDisposable
    {
        private Entity player;
        protected readonly Entity Entity;
        private MonsterAuthoring authoring;
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        private int monsterId;
        private int skillId;
        private int skillLevel;
        private float delayExecuteAttack;
        private CancellationTokenSource cts;

        public Monster(MonsterAuthoring authoring, Entity entity, Entity player, 
            int monsterId, int skillId, int skillLevel,
            MonsterConfig monsterConfig, SkillConfig skillConfig,
            float delayExecuteAttack)
        {
            this.authoring = authoring;
            this.Entity = entity;
            this.delayExecuteAttack = delayExecuteAttack;
            this.monsterConfig = monsterConfig;
            this.skillConfig = skillConfig;
            this.player = player;
            this.monsterId = monsterId;
            this.skillId = skillId;
            this.skillLevel = skillLevel;
        }

        public async void Attack()
        {
            cts = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(delayExecuteAttack), cancellationToken: cts.Token);
            OnAttack();
        }

        protected virtual void OnAttack()
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            HealthData playerHealthData = manager.GetComponentData<HealthData>(player);
            monsterConfig.Find(monsterId, out var monsterData);
            skillConfig.Find(skillId, out var skillData);
            playerHealthData.Health -= FormulaUtils.Output(monsterData.Attack, skillData, skillLevel, 0, 0, 0);
            manager.SetComponentData(player, playerHealthData);     
            EventBus.Instance.Publish(new PlayerOnDamageEvent());
        }

        public void Death()
        {
        }

        public void PlayAnimation(AnimationName animationName)
        {
            if (authoring != null) authoring.PlayAnimation(animationName);
        }

        public void PlayAttackAnimation()
        {
            if (authoring != null) authoring.PlayAttackAnimation();
        }

        public virtual void Dispose()
        {
            cts?.Cancel();
        }
    }
}