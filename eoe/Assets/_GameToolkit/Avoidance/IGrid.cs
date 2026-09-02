using Unity.Mathematics;

namespace _KITSystem.Grid
{
    public interface IGrid
    {
        bool Insert(int unitId, float2 position);
        bool Remove(int unitId);
        int Query(float2 position, float2 size, out int[] results);
    }
}