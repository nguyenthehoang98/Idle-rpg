using System.Collections.Generic;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Games.Battle
{
    public class BattleLevel : MonoBehaviour
    {
        [TitleGroup("Settings")]
        [SerializeField] private float scale = 0.6f;
        [TitleGroup("Elements")]
        [SerializeField] private Transform coneParent;
        [SerializeField] private ConeMono prefab;

        private readonly List<ConeMono> cones = new List<ConeMono>();

        public async UniTask Initialize(int maxStar, float timeScale)
        {
            coneParent.transform.localScale = new Vector3(scale, scale, scale);
            for (int i = 0; i < 6; i++)
            {
                ConeMono coneMono = Instantiate(prefab, coneParent);
#if UNITY_EDITOR
                coneMono.name = "Cone " + (i + 1);
#endif
                await coneMono.Initialize(i, maxStar, timeScale);
                cones.Add(coneMono);
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
                ConeMono coneMono = cones[i];
                int stack = numbers[i];
                if (stack > 0 && !coneMono.IsPlaying)
                {
                    this.WaitInvoke(0.2f, coneMono.Active);
                }
                else if(stack == 0 && coneMono.IsPlaying)
                {
                    coneMono.Inactive();
                }
            }
        }

        public Vector3 GetConeWorldPosition(int index, int stack) => cones[index].GetStarPosition(stack);

        public Vector3 GetConeWorldRotation(int index, int stack) => cones[index].GetStarRotation(stack);
        
        public ConeMono GetCone(int index) => cones[index];
    }
}
