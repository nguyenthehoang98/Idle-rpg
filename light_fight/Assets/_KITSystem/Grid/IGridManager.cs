using System.Collections.Generic;
using Unity.Mathematics;

namespace _KITSystem.Grid
{
    public interface IGridManager
    {
        bool Insert(int unitId, float2 position);
        bool Remove(int unitId);
        int Query(float2 position, float radius, out List<int> results);
    }
}