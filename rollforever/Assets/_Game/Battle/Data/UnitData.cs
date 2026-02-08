using UnityEngine;

namespace _Game.Battle.Data
{
    public struct UnitData
    {
        public readonly int agentId;
#if UNITY_EDITOR
        public Color color;
#endif
        public UnitData(int agentId)
        {
            this.agentId = agentId;
#if UNITY_EDITOR
            this.color = Color.black;
#endif
        }
    }
}