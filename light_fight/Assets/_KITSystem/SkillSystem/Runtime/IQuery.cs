using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    public interface IQuery
    {
        bool FindRandomTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        bool FindFarthestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        bool FindNearestTargetPosition(float2 center, float radius, Func<int, bool> funcFilterEntity, out QueryResult result);

        List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float radius);
        List<int> GetEntities(float2 center, Func<int, bool> funcFilterEntity, float2 size);
    }

    public struct QueryResult
    {
        public int Entity;
        public float2 Position;
    }
}