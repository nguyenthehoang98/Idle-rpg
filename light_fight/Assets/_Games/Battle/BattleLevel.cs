using System.Collections.Generic;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private Cone prefab;

        private readonly List<Cone> cones = new List<Cone>();
        private int[] dices = new int[2];
        
        private void Start()
        {
            cones.Add(prefab);
            for (int i = 0; i < 5; i++)
            {
                cones.Add(Instantiate(prefab, prefab.transform.parent));
            }

            for (int i = 0; i < cones.Count; i++)
            {
                cones[i].Init(i + 1);
            }
        }

        public void Trigger(Vector2Int trigger)
        {
            int dice = trigger.x;
            int value = trigger.y;

            int prev = dices[dice];

            if (prev == value) return;

            int stack;
            if (prev != value)
            {
                stack = cones[prev].Stack;
                cones[prev].RemoveId(dice);
                if (cones[prev].Stack == 0 && stack > 0) cones[prev].Inactive();
            }

            stack = cones[value].Stack;
            cones[value].InsertId(dice);
            if (stack == 0)
            {
                cones[value].Active();
            }
            else
            {
                cones[value].SetMultiplierColor();
            }
            
            dices[dice] = value;
        }
    }
}
