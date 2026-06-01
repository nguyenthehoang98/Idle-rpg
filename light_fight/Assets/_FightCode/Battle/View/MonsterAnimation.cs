using System;
using System.Collections;
using _KITSystem.Resource;
using _KITSystem.Utils;
using Animancer;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _FightCode.Battle.View
{
    public class MonsterAnimation : MonoBehaviour
    {
        private static readonly int HitEffectBlend = Shader.PropertyToID("_HitEffectBlend");
        
        public static int Order = 1;

        // move at transform
        [SerializeField] private Transform flip; // flip
        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private AnimationClip moveAnimationClip;
        [SerializeField] private AnimationClip attackAnimationClip;
        [SerializeField] private AnimationClip beHitAnimationClip;
        [SerializeField] private AnimationClip deathAnimationClip;

        private AnimancerState state;
        private Action onDeathCallback;

        private Vector3 localScale;
        private bool defaultFace = true; // false: left, true: right

        private MaterialPropertyBlock hitEffectProperty;
        private Coroutine hitCoroutine;
        private MMF_Player currentBeHit;

        private bool isDead = false;

        private void Awake()
        {
            hitEffectProperty = new MaterialPropertyBlock();
            localScale = flip.localScale;
        }

        private void OnEnable()
        {
            isDead = false;
            renderer.sortingOrder = Order++;
        }

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
        }

        public void BeHit()
        {
            if (isDead) return;
            
            if (hitCoroutine != null) StopCoroutine(hitCoroutine);

            renderer.GetPropertyBlock(hitEffectProperty);

            hitCoroutine = this.LerpNormalize(0, 1, 0.1f, f =>
            {
                hitEffectProperty.SetFloat(HitEffectBlend, f);

                renderer.SetPropertyBlock(hitEffectProperty);
            }, () =>
            {
                hitEffectProperty.SetFloat(HitEffectBlend, 0);

                renderer.SetPropertyBlock(hitEffectProperty);
            });
            
            currentBeHit = BattleEffect.Instance.SpawnHitEffect(transform.position);
        }

        public void Dead(Vector3 force, Action onDestroy)
        {
            isDead = true;
            
            onDeathCallback = onDestroy;
            
            if (state != null) state.Stop();
            
            state = animancer.Play(deathAnimationClip);
            
            StartCoroutine(Knockback(force, state.Duration));

            if (currentBeHit != null && currentBeHit.IsPlaying)
            {
                currentBeHit.StopFeedbacks();
                
                KitPool.Destroy(currentBeHit.gameObject);
            }
        }

        public void OnDeathEvent()
        {
            gameObject.SetActive(false);
            
            onDeathCallback?.Invoke();
        }
    }
}
