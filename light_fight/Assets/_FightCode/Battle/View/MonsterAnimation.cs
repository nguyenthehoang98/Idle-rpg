using System;
using System.Collections;
using _KITSystem.Utils;
using Animancer;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class MonsterAnimation : MonoBehaviour
    {
        // move at transform
        [SerializeField] private Transform root; // play animation scale ,rotate
        [SerializeField] private Transform flip; // flip
        [SerializeField] private AnimationClip moveAnimationClip;
        [SerializeField] private AnimationClip deathAnimationClip;

        private AnimancerComponent animancer;
        private AnimancerState state;

        private Vector3 localScale;
        private bool defaultFace = true; // false: left, true: right

        private void Awake()
        {
            animancer = GetComponent<AnimancerComponent>();
            localScale = flip.localScale;
        }

        /*private void OnValidate()
        {
            animancer = GetComponent<AnimancerComponent>();
            if (animancer.Animator == null)
                animancer.Animator = GetComponent<Animator>();
        }*/

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;

            bool right = pos.x < 0;
            if (right != defaultFace)
            {
                defaultFace = right;
                flip.localScale = new Vector3(localScale.x * (right ? 1 : -1), localScale.y, localScale.z);
            }
        }

        public void BeHit()
        {
            // spawn fx
        }

        public void Dead(Vector3 force, Action onDestroy)
        {
            if (state != null) state.Stop();
            state = animancer.Play(deathAnimationClip);
            
            this.WaitInvoke(state.Duration, () =>
            {
                gameObject.SetActive(false);
                onDestroy?.Invoke();
            });
            StartCoroutine(Knockback(force, state.Duration));
        }

        private IEnumerator Knockback(Vector3 force, float duration)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + force;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // EaseOutBack
                float ease = EaseOutBack(t);

                transform.position = Vector3.LerpUnclamped(
                    startPos,
                    endPos,
                    ease);

                yield return null;
            }

            transform.position = endPos;
        }
        
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3)
                      + c1 * Mathf.Pow(t - 1f, 2);
        }

        public void Idle()
        {
            // code
            if (state != null) state.Stop();
        }

        public void Move()
        {
            if (state != null) state.Stop();
            state = animancer.Play(moveAnimationClip);
            // code
        }
    }
}
