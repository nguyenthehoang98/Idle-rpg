using System;
using System.Collections.Generic;
using _KIT.Utils;
using Animancer;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Combat.Model
{
    [RequireComponent(typeof(AnimancerComponent))]
    public class Monster : MonoBehaviour, IAuthoring
    {
        [Header("Renderer")]
        [SerializeField] private SortingGroup sortingGroup;
        [Header("Collider")]
        [SerializeField] private float radius;
        [SerializeField] private Vector3 offset;
        [Header("Animations")] 
        [SerializeField] private AnimationData[] clips;

        AnimancerComponent animancerComponent;

        private void Awake()
        {
            animancerComponent = GetComponent<AnimancerComponent>();
        }

        public void Initialize(Entity entity)
        {
            PlayAnimation(AnimationName.Move);
            this.WhileInvoke(1, () =>
            {
                sortingGroup.sortingOrder = -(int)(transform.position.y * 10);
            });
        }
        
        public float Radius => radius;

        public Vector3 Offset => offset;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + offset, radius);
        }

        public void OnAttackStart()
        {
        }

        public void OnAttack()
        {
        }

        public void OnAttackEnd()
        {
            Debug.Log("trigger cooldown");
        }

        public AnimancerState PlayAnimation(AnimationName animationName)
        {
            foreach (var data in clips)
            {
                if (data.name == animationName)
                {
                    return animancerComponent.Play(data.transition);
                }
            }

            return null;
        }

        public void QueueAnimation(AnimationName animationName, AnimationName nextAnimationName)
        {
            var state = PlayAnimation(animationName);
            this.WaitInvoke(state.Duration, () => PlayAnimation(nextAnimationName));
        }

        [System.Serializable]
        class AnimationData
        {
            public AnimationName name;
            public ClipTransition transition;
        }
    }
}