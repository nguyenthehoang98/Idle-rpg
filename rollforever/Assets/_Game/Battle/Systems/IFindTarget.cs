using _Game.Battle.Data;
using Leopotam.EcsLite;
using RVO;
using Unity.Mathematics;

namespace _Game.Battle.Systems
{
    interface IFindTarget
    {
        bool Find(float2 startPos, float maxDistance, EcsFilter filter, out int target);
    }

    class NearestFindTarget : IFindTarget
    {
        private Simulator simulator;
        private EcsPool<UnitData> unitPool;

        public NearestFindTarget(Simulator simulator, EcsPool<UnitData> unitPool)
        {
            this.simulator = simulator;
            this.unitPool = unitPool;
        }
        
        public bool Find(float2 startPos, float maxDistance, EcsFilter filter, out int target)
        {
            target = -1;
            float maxDistanceSq = maxDistance * maxDistance;
            float distancesq = float.MaxValue;
            foreach (var e in filter)
            {
                var unitData = unitPool.Get(e);
                float2 pos = simulator.GetAgentPosition(unitData.agentId);
                float sq = math.distancesq(startPos, pos);
                if (sq < distancesq && sq <= maxDistanceSq)
                {
                    distancesq = sq;
                    target = e;
                }
            }

            return target != -1;
        }
    }

    class FarthestFindTarget : IFindTarget
    {
        private Simulator simulator;
        private EcsPool<UnitData> unitPool;

        public FarthestFindTarget(Simulator simulator, EcsPool<UnitData> unitPool)
        {
            this.simulator = simulator;
            this.unitPool = unitPool;
        }
        
        public bool Find(float2 startPos, float maxDistance, EcsFilter filter, out int target)
        {
            target = -1;
            float maxDistanceSq = maxDistance * maxDistance;
            float distancesq = 0;
            foreach (var e in filter)
            {
                var unitData = unitPool.Get(e);
                float2 pos = simulator.GetAgentPosition(unitData.agentId);
                float sq = math.distancesq(startPos, pos);
                if (sq < distancesq && sq <= maxDistanceSq)
                {
                    distancesq = sq;
                    target = e;
                }
            }

            return target != -1;
        }
    }
}