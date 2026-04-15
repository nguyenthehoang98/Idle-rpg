using Animancer;
using UnityEngine;

namespace _Games.Combat.Equipment
{
    public class AnimationWeaponItem : BaseWeaponItem
    {
        public AnimationClip clip;
        public AnimancerComponent animancerComponent;
        public float delayCastProjectileTime;
        public ParticleSystem particleSystem;
        
        protected override float OnExecuteDelay(float timeScale)
        {
            if (animancerComponent == null) 
                animancerComponent = GetComponent<AnimancerComponent>();
            
            AnimancerState state = animancerComponent.Play(clip);
            state.Time = 0;
            state.Speed = timeScale;
            
            ParticleSystem.MainModule main = particleSystem.main;
            main.simulationSpeed = timeScale;
            particleSystem.Play();
           
            return delayCastProjectileTime / timeScale;
        }
    }
}