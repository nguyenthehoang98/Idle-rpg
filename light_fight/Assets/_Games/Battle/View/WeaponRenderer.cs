using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Battle.View
{
    public class WeaponRenderer : MonoBehaviour
    {
        [TitleGroup("Animation")]
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private NamedAnimancerComponent animancer;
        [SerializeField] private AnimationClip attackClip;
        
        AnimancerState animancerState;

        public void PlayAttack(float timeScale)
        {
            animancerState = animancer.Play(attackClip);
            animancerState.Time = 0;
            animancerState.Speed = timeScale;
        }

        public void Stop()
        {
            if (animancerState != null) animancerState.Stop();
        }

        public void Activate()
        {
            sortingGroup.sortingOrder = 1;
        }

        public void Deactivate()
        {
            sortingGroup.sortingOrder = 0;
        }
    }
}