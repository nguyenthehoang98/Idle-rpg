using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Model;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Combat.EntityComponentSystem.View
{
    public class EntityCastSkillManager : MonoBehaviour
    {
        private Dictionary<int, RangedCastSkillData> container = new Dictionary<int, RangedCastSkillData>();
        private Queue<RangedCastSkillData> queue = new Queue<RangedCastSkillData>();

        public static EntityCastSkillManager Instance {get; private set;}

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            while (queue.Count > 0)
            {
                RangedCastSkillData data = queue.Dequeue();
                Debug.Log("cast skill data: " + data.Entity);
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