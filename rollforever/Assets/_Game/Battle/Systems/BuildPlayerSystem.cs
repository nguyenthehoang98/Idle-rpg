
using _Game.Battle.Data;
using GoodCat.EcsLite.Shared;
using Leopotam.EcsLite;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace _Game.Battle.Systems
{
    public class BuildPlayerSystem : IEcsInitSystem
    {
        [EcsInject] private readonly BattleStartupShareData shareData;
        
        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var unitPool = world.GetPool<UnitData>();
            var playerPool = world.GetPool<PlayerFlag>();
            var shapePool = world.GetPool<ShapeData>();
            var healthPool = world.GetPool<HealthData>();
            var attackCasterPool = world.GetPool<AttackCasterData>();
            var statPool = world.GetPool<StatData>();

            int entity = world.NewEntity();
            int agentId = shareData.Simulator.AddAgent(float2.zero);
            unitPool.Add(entity) = new UnitData(agentId);
#if UNITY_EDITOR
            unitPool.Get(entity).color = Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1);
#endif  
            playerPool.Add(entity);
            attackCasterPool.Add(entity) = new AttackCasterData {cooldown = 1};
            shapePool.Add(entity) = ShapeData.Circle(1);
            statPool.Add(entity) = new StatData()
                .Insert(StatType.Attack, new Stat(10))
                .Insert(StatType.Defense, new Stat(10))
                .Insert(StatType.MaxHealth, new Stat(1000))
                .Insert(StatType.MoveSpeed, new Stat(0))
                .Insert(StatType.SkillReduceCooldown, new Stat(0))
                .Insert(StatType.CriticalRate, new Stat(0))
                .Insert(StatType.CriticalDamage, new Stat(0));
            statPool.Get(entity).TryGetValue(StatType.MaxHealth, out var healthStat);
            healthPool.Add(entity) = new HealthData((int)healthStat.Value);

            float radius = 0.5f;
            shareData.Simulator.SetAgentRadius(agentId, radius);
            shareData.Simulator.PauseAgent(agentId, true);
        }
    }
}
