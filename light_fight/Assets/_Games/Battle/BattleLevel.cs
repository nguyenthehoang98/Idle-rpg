using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private Cone prefab;

        private readonly List<Cone> cones = new List<Cone>();
        private int[] dices;

        public async UniTask Initialize(int totalDice, float timeScale)
        {
            dices = new int[totalDice];

            for (int i = 0; i < 6; i++)
            {
                cones.Add(Instantiate(prefab, prefab.transform.parent));
            }

            for (int i = 0; i < cones.Count; i++)
            {
                await cones[i].Initialize(i + 1, timeScale);
            }
        }

        public float Play()
        {
            float duration = 0;
            for (int i = 0; i < cones.Count; i++)
            {
                duration = Mathf.Max(duration, cones[i].Play());
            }

            return duration;
        }

        public void Trigger(Vector2Int trigger)
        {
            StartCoroutine(TriggerIE(trigger));   
        }

        private IEnumerator TriggerIE(Vector2Int trigger)
        {
            int dice = trigger.x;
            int value = trigger.y;
            int prev = dices[dice];

            if (prev == value) yield break;

            int stack;
            if (prev != value)
            {
                stack = cones[prev].Stack;
                cones[prev].RemoveId(dice);
                if (cones[prev].Stack == 0 && stack > 0)
                    cones[prev].Inactive();
                else if (cones[prev].Stack != 0)
                    cones[prev].SetMultiplierColor();
            }

            yield return null;

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
