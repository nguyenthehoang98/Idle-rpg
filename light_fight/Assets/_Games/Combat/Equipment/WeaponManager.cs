using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Level;
using _Games.Combat.SkillSystem;
using _Games.Combat.SkillSystem.Model;
using _Games.Config;
using _KIT.Config;
using _KIT.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Games.Combat.Equipment
{
    public class WeaponManager
    {
        private LevelDesign levelDesign;
        private Dictionary<int, Data> container = new Dictionary<int, Data>();
        private SkillConfig skillConfig;
        private EntityManager manager;
        private EntityQuery query;
        
        public WeaponManager(LevelDesign levelDesign)
        {
            skillConfig = KitConfigManager.Get<SkillConfig>();
            this.levelDesign = levelDesign;
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(LocalTransform), typeof(HealthData));
        }

        public void Equip(int slotIndex, WeaponData weaponData, int level)
        {
            container[slotIndex] = new Data
            {
                WeaponData = weaponData,
                Level = level
            };
            Debug.Log(@"Thêm cơ chế lock mục tiêu tới khi chết, tránh đảo nhiều quá ko hay. Weapon lock target chứ ko phải player lock");
        }

        public void Trigger(int slotIndex)
        {
            Vector3 position = levelDesign.Slots[slotIndex].transform.position;
            if (!container.ContainsKey(slotIndex))
                return; // ko có vũ khí
            Data data = container.GetValueOrDefault(slotIndex);
            ECSFactory.PrepareProjectileBuild(query, data.WeaponData.SkillId, position, tuple =>
            {
                float delay = 0.2f / BattleTime.ScaleTime;
                Vector3 direction = (Vector3)tuple.targetPosition - position;
                float rad = Mathf.Atan2(direction.y, direction.x);
                levelDesign.Slots[slotIndex].Rotation(rad, delay, () =>
                {
                    ECSFactory.BuildProjectile(manager, Entity.Null, tuple.target,
                        position, tuple.targetPosition, tuple.skill, tuple.skillData, data.Level
                    );
                });
            });
        }

        struct Data
        {
            public int Level;
            public WeaponData WeaponData;
        }
    }
}