using Animancer;
using UnityEngine;

namespace _Games.Combat.Equipment
{
    public class AnimationWeaponItem : BaseWeaponItem
    {
        public AnimationClip clip;
        public AnimancerComponent animancerComponent;
        
        protected override float OnExecuteDelay(float timeScale)
        {
            if (animancerComponent == null) 
                animancerComponent = GetComponent<AnimancerComponent>();
            AnimancerState state = animancerComponent.Play(clip);
            state.Time = 0;
            state.Speed = timeScale;
            return state.Duration / timeScale;
        }
    }
}