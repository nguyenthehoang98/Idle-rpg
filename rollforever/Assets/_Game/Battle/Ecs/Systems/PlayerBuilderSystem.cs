using _Game.Battle.Ecs.Data;
using _Game.Battle.Ecs.Events;
using _Game.Scripts.Model;
using _KIT.Event;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Ecs.Systems
{
    public class PlayerBuilderSystem : IEcsInitSystem, IEcsDestroySystem
    {
        public void Init(IEcsSystems systems)
        {
             var world = systems.GetWorld();
             var shapePool = world.GetPool<ShapeData>();
             var healthPool = world.GetPool<HealthData>();
             var statPool = world.GetPool<StatData>();
             var playerFlagPool = world.GetPool<PlayerFlag>();
             
             int maxHealth = 1000;
             
             int entity = world.NewEntity();
             playerFlagPool.Add(entity);
             healthPool.Add(entity) = new HealthData(maxHealth);
             shapePool.Add(entity) = ShapeData.Box(new float2(1.5f, 1.5f));
             statPool.Add(entity) = new StatData().Insert(StatType.MaxHealth, new Stat(maxHealth));
             
             EventBus.Instance.Publish(new DamagePlayerEvent(0, maxHealth, maxHealth));
        }

        public void Destroy(IEcsSystems systems)
        {
        }
    }
}