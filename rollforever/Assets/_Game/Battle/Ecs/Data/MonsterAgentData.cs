using UnityEngine;

namespace _Game.Battle.Ecs.Data
{
    public struct MonsterAgentData
    {
        public readonly int agentId;
#if UNITY_EDITOR
        public Color color;
#endif
        public MonsterAgentData(int agentId)
        {
            this.agentId = agentId;
#if UNITY_EDITOR
            this.color = Color.black;
#endif
        }
    }
}