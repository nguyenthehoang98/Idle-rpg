using System.Collections.Generic;
using _KIT.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Combat.Model
{
    public class Monster : MonoBehaviour, IAuthoring
    {
        [Header("Renderer")]
        [SerializeField] private SortingGroup sortingGroup;
        [Header("Animations")] 
        [SerializeField] private Animation unityAnimation;
        [SerializeField] private AnimationData[] clips;
        [Header("Collider")]
        [SerializeField] private float radius;
        [SerializeField] private Vector3 offset;

        private void OnValidate()
        {
            if (unityAnimation == null || clips == null) return;

            // --- Build set name mới ---
            var newNames = new HashSet<string>();
            foreach (var data in clips)
            {
                if (data.clip == null) continue;
                newNames.Add(data.name.ToString());
            }

            // --- Remove clip không còn dùng ---
            var toRemove = new List<string>();
            foreach (AnimationState state in unityAnimation)
            {
                if (!newNames.Contains(state.name))
                    toRemove.Add(state.name);
            }

            foreach (var clipName in toRemove)
            {
                unityAnimation.RemoveClip(clipName);
            }

            // --- Add / Replace clip ---
            foreach (var data in clips)
            {
                if (data.clip == null) continue;

                string clipName = data.name.ToString();

                // đảm bảo legacy
                if (!data.clip.legacy)
                    data.clip.legacy = true;

                // nếu đã tồn tại → remove trước để tránh bug overwrite ngầm
                if (unityAnimation.GetClip(clipName) != null)
                {
                    unityAnimation.RemoveClip(clipName);
                }

                unityAnimation.AddClip(data.clip, clipName);
            }

            // --- Set default clip ---
            if (clips.Length > 0 && clips[0].clip != null)
            {
                unityAnimation.clip = clips[0].clip;
            }
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