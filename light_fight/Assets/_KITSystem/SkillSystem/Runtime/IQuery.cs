using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.SkillSystem.Runtime
{
    public interface IQuery
    {
        List<int> GetUnits(float2 center, float radius);
        List<int> GetUnits(float2 center, float2 size);
    }
}