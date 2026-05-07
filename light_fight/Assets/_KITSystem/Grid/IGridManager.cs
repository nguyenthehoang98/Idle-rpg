using System.Collections.Generic;
using UnityEngine;

namespace _KITSystem.Grid
{
    public interface IGridManager
    {
        // Chưa có thì thêm mới, có rồi thì ghi đè
        bool Insert(int unitId, Vector3 position, out int id);
        bool Remove(int unitId);
        bool Query(Vector3 position, float radius, List<int> results);
    }
}