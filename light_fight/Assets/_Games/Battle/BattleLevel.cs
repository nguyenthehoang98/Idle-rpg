using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private Cone prefab;

        private readonly List<Cone> cones = new List<Cone>();
        private int[] dicesStackNumber = new int[6];

        public async UniTask Initialize(float timeScale)
        {
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

        // trigger: (0, 1),(1, 5),(2, 4),(3, 3)
        public void Trigger(List<int> triggers)
        {
            // todo: stop all dice active
            for (int i = 0; i < dicesStackNumber.Length; i++)
            {
                dicesStackNumber[i] = 0;
            }
            
            for (int i = 0; i < triggers.Count; i++)
            {
                int diceNumber = triggers[i];
                int index = diceNumber - 1; // index of stack
                dicesStackNumber[index]++;
            }
            
            // * dicesStackNumber: [0,1,1,0,1,1]
            for (int i = 0; i < cones.Count; i++)
            {
                var cone = cones[i];
                int value = dicesStackNumber[i];
                if (value > 0)
                {
                    cone.StackColor(value);
                    if (!cone.IsPlaying) cone.Active();
                }
                else if (value == 0 && cone.IsPlaying)
                {
                    cone.Inactive();
                }
            }
        }
    }
}
