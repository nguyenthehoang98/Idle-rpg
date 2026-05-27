using System.Collections.Generic;
using _FightCode.Battle.Logic;
using _FightCode.Battle.View;
using _KITSystem.Grid;
using UnityEngine;

namespace _FightCode.Battle.Model
{
    [System.Serializable]
    public class BattleShare
    {
        public float timeScale = 1f;
        public BattleOwner owner;
        public Transform coneParent;
        public Transform attractorParent;
        public AgentGrid agentGrid;
        public List<IDiceView> dices;
        public List<ISlotView> slots;
    }
}