using System;
using System.Collections.Generic;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Level;
using _Games.Config;
using _KIT.Config;
using Unity.Entities;
using Unity.Transforms;

namespace _Games.Combat.Equipment
{
    public class EquipmentManager
    {
        LevelDesign levelDesign;
        Dictionary<int, Data> container = new Dictionary<int, Data>();
        Queue<Action> queueBuffer = new Queue<Action>();
        SkillConfig skillConfig;
        EntityQuery query;
        
        public EquipmentManager(LevelDesign levelDesign)
        {
            skillConfig = KitConfigManager.Get<SkillConfig>();
            this.levelDesign = levelDesign;
            var world = World.DefaultGameObjectInjectionWorld;
            var manager = world.EntityManager;
            query = manager.CreateEntityQuery(typeof(MonsterTag), typeof(LocalTransform), typeof(HealthData));
        }
        
        
        struct Data
        {
        }
    }
}