using System.Collections.Generic;
using _Games.Battle.Logic;
using _Games.Battle.View;
using UnityEngine;

namespace _Games.Battle.Model
{
    [System.Serializable]
    public class BattleShare
    {
        public float timeScale = 1f;
        public BattleOwner owner;
        public Transform coneParent;
        public Transform attractorParent;
        public List<IDiceView> dices = new List<IDiceView>();
        public List<ISlotView> slots = new List<ISlotView>();
    }
}