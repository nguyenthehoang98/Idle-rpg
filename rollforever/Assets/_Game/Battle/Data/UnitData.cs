using UnityEngine;

namespace _Game.Battle.Data
{
    public struct UnitData
    {
        public int agentId;
        public int cellId;
        public int shapeId;
#if UNITY_EDITOR
        public Color color;
#endif
    }
}