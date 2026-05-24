using _Games.Battle.Logic;
using UnityEngine;

namespace _Games.Battle.Model
{
    [System.Serializable]
    public class BattleShare
    {
        public float timeScale = 1f;
        public BattleOwner owner;
        public Transform coneParent;
    }
}