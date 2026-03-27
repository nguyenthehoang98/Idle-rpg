using System;
using System.Threading;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Config;
using _Games.Utils;
using _KIT.Event;
using Cysharp.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class Monster : ScriptableObject
    {
        private Entity player;
        protected Entity Entity;
        private MonsterAuthoring authoring;
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        private int monsterId;
        private int skillId;
        private int skillLevel;
        private float delayExecuteAttack;
        private CancellationTokenSource cts;

        public void Init(MonsterAuthoring authoring, Entity entity, Entity player,
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
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delayExecuteAttack), cancellationToken: cts.Token);
                OnAttack();
            }
            catch (OperationCanceledException)
            {
                // bị cancel là bình thường → ignore
            }
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
            if(authoring != null) authoring.Destroy();
            cts?.Cancel();
            Destroy(this);
        }

        public void TakeDamage(int damage, Vector3 position)
        {
            if(authoring != null)
            {
                authoring.Behit();
                authoring.ShowTextDamage(damage, position);
            }
        }

        public void PlayAnimation(AnimationName animationName)
        {
            if (authoring != null) authoring.PlayAnimation(animationName);
        }

        public void PlayAttackAnimation()
        {
            if (authoring != null) authoring.PlayAttackAnimation();
        }
    }
}