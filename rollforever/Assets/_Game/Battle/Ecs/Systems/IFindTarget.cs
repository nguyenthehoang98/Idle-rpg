using System;
using Leopotam.EcsLite;
using Unity.Mathematics;

namespace _Game.Battle.Ecs.Systems
{
    interface IFindTarget
    {
        bool Find(float2 startPos, float maxDistance, EcsFilter filter, Func<int, float2> funcAgentPos, out int target);
    }

    class NearestFindTarget : IFindTarget
    {
        public bool Find(float2 startPos, float maxDistance, EcsFilter filter, Func<int, float2> funcAgentPos,
            out int target)
        {
            target = -1;
            float maxDistanceSq = maxDistance * maxDistance;
            float distancesq = float.MaxValue;
            foreach (var e in filter)
            {
                float2 pos = funcAgentPos(e);
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
        public bool Find(float2 startPos, float maxDistance, EcsFilter filter, Func<int, float2> funcAgentPos,
            out int target)
        {
            target = -1;
            float maxDistanceSq = maxDistance * maxDistance;
            float distancesq = 0;
            foreach (var e in filter)
            {
                float2 pos = funcAgentPos(e);
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