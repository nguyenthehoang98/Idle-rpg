using _Games.Combat.EntityComponentSystem.Model;
using _Games.Config;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class RangedMonster : Monster
    {
        private Vector3 muzzleOffset;
        
        public void Init(MonsterAuthoring authoring, Vector3 muzzleOffset,
            Entity entity, Entity player,
            int monsterId, int skillId, int skillLevel,
            MonsterConfig monsterConfig, SkillConfig skillConfig,
            float delayExecuteAttack)
        {
            this.muzzleOffset = muzzleOffset;
            Init(authoring, entity, player, monsterId, skillId, skillLevel, monsterConfig, skillConfig, delayExecuteAttack);
        }

        protected override void OnAttack()
        {
            base.OnAttack();
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            LocalTransform transform = manager.GetComponentData<LocalTransform>(Entity);
            bool flip = transform.Rotation.value.y != 0;
            int offset = flip ? -1 : 1;
            RangedMonsterCastSkillManager.Instance.Trigger(Entity, muzzleOffset * offset);
        }
    }
}