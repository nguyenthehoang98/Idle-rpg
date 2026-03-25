using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Config;
using _KIT.Config;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.Model
{
    public class RangedMonsterCastSkillManager : MonoBehaviour
    {
        private Dictionary<int, RangedCastSkillData> container = new Dictionary<int, RangedCastSkillData>();
        private Queue<RangedCastSkillData> queue = new Queue<RangedCastSkillData>();

        public static RangedMonsterCastSkillManager Instance {get; private set;}

        private EntityManager manager;
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            monsterConfig = KitConfigManager.Get<MonsterConfig>();
            skillConfig = KitConfigManager.Get<SkillConfig>();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private async void Update()
        {
            while (queue.Count > 0)
            {
                RangedCastSkillData item = queue.Dequeue();
                if (manager.HasComponent<MonsterTag>(item.Entity) && manager.HasComponent<MonsterRangedTag>(item.Entity))
                {
                    MonsterSkillData data = manager.GetComponentData<MonsterSkillData>(item.Entity);
                    monsterConfig.Find(data.MonsterId, out MonsterData monsterData);
                    skillConfig.Find(monsterData.SkillId, out SkillData skillData);
                    Skill skill = await SkillFactory.CreateSkill(skillData);
                    ECSFactory.BuildProjectile(manager, item.Entity,
                        item.StartPosition, item.EndPosition,
                        skill, skillData, monsterData.SkillLevel
                    );
                }
            }
        }

        public void QueueSkill(RangedCastSkillData skillData)
        {
            int id = skillData.Entity.Index;
            container[id] = skillData;
        }

        public void Trigger(Entity entity, Vector3 offsetMuzzle)
        {
            int id = entity.Index;
            if (container.Remove(id, out var skillData))
            {
                skillData.StartPosition += new float3(offsetMuzzle.x, offsetMuzzle.y, offsetMuzzle.z); 
                queue.Enqueue(skillData);
            }
        }
    }
}