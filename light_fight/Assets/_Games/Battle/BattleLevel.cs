using System.Collections.Generic;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [SerializeField] private Transform coneParent;
        [SerializeField] private Cone prefab;

        private readonly List<Cone> cones = new List<Cone>();

        public async UniTask Initialize(int maxStar, float timeScale)
        {
            for (int i = 0; i < 6; i++)
            {
                Cone cone = Instantiate(prefab, coneParent);
#if UNITY_EDITOR
                cone.name = "Cone " + (i + 1);
#endif
                await cone.Initialize(i, maxStar, timeScale);
                cones.Add(cone);
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

        public void ResetStack(int index) => cones[index].ResetStack();
        
        public void TriggerStack(int index, int stack) => cones[index].DoStack(stack);

        public void Trigger(int[] numbers)
        {
            for (int i = 0; i < cones.Count; i++)
            {
                Cone cone = cones[i];
                int stack = numbers[i];
                if (stack > 0 && !cone.IsPlaying)
                {
                    this.WaitInvoke(0.2f, cone.Active);
                }
                else if(stack == 0 && cone.IsPlaying)
                {
                    cone.Inactive();
                }
            }
        }

        public Vector3 GetConeWorldPosition(int index, int stack) => cones[index].GetStarPosition(stack);

        public Vector3 GetConeWorldRotation(int index, int stack) => cones[index].GetStarRotation(stack);
        
        public Cone GetCone(int index) => cones[index];
    }
}
