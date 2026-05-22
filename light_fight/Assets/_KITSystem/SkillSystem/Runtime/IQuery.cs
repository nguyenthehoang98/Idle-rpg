using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    public interface IQuery
    {
        bool FindNearestTargetPosition(float2 center, float radius, out float2 targetPosition);
        List<int> GetEntities(float2 center, float radius);
        List<int> GetEntities(float2 center, float2 size);
    }
}