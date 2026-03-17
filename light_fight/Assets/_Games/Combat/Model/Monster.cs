using System;
using System.Collections.Generic;
using _KIT.Utils;
using Unity.Entities;
using UnityEngine;

namespace _Games.Combat.Model
{
    public class Monster : MonoBehaviour, IAuthoring
    {
        [Header("Animations")] 
        [SerializeField] private Animation unityAnimation;
        [SerializeField] private AnimationData[] clips;
        [Header("Collider")]
        [SerializeField] private float radius;
        [SerializeField] private Vector3 offset;

        private void OnValidate()
        {
            if (unityAnimation.GetClipCount() == clips.Length) return;
            
            List<string> clipsToRemove = new List<string>();
            foreach (AnimationState state in unityAnimation)
            {
                clipsToRemove.Add(state.name);
            }

            foreach (var clipName in clipsToRemove)
            {
                unityAnimation.RemoveClip(clipName);
            }
            
            foreach (var data in clips)
            {
                unityAnimation.AddClip(data.clip, data.name.ToString());
            }

            if (clips.Length > 0) unityAnimation.clip = clips[0].clip;
        }

        public void Initialize(Entity entity)
        {
            PlayAnimation(AnimationName.Move);
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
        }

        public void PlayAnimation(AnimationName animationName)
        {
            unityAnimation.Stop();
            unityAnimation.Play(animationName.ToString());
        }

        public void QueueAnimation(AnimationName animationName, AnimationName nextAnimationName)
        {
            PlayAnimation(animationName);
            this.WaitInvoke(unityAnimation[animationName.ToString()].length, () => PlayAnimation(nextAnimationName));
        }

        [System.Serializable]
        class AnimationData
        {
            public AnimationName name;
            public AnimationClip clip;
        }
    }
}