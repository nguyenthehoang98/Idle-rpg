using System;
using _Games.Combat.Event;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Utils;
using Animancer;
using MoreMountains.Feedbacks;
using PrimeTween;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Combat.Model
{
    [RequireComponent(typeof(AnimancerComponent))]
    [RequireComponent(typeof(Animator))]
    public class MonsterAuthoring : MonoBehaviour, IAuthoring
    {
        [Header("Renderer")]
        [SerializeField] private SortingGroup sortingGroup;
        [SerializeField] private SpriteRenderer[] parts;
        [Header("Combat")]
        [SerializeField] private float radius;
        [SerializeField, Tooltip("Thời gian chờ tính từ khi play animation Attack")]
        private float delayExecuteAttack;
        [Header("Animations")] 
        [SerializeField] private AnimationData[] clips;
        [Header("Feedback & Effects")] 
        [SerializeField] private bool shouldPlayDeathAnimation;
        [SerializeField] private BaseDeathEffect deathEffect;
        [SerializeField] private MMF_Player takeDamageFeedback;

        private AnimancerComponent animancerComponent;
        private Coroutine attackCoroutine;
        
        public float Radius => radius;
        public float DelayExecuteAttack => delayExecuteAttack;
        
        private static readonly int HitEffectBlend = Shader.PropertyToID("_HitEffectBlend");
        private const float BLEND_VALUE = 0.7f;
        private const float BLEND_DURATION = 0.15f;

        private MaterialPropertyBlock mpb;
        private Sequence sequence;
        private bool died;

        private void Awake()
        {
            animancerComponent = GetComponent<AnimancerComponent>();
            mpb = new MaterialPropertyBlock();
        }

        public void Initialize(Entity entity)
        {
            died = true;
            PlayAnimation(AnimationName.Move);
            this.WhileInvoke(1, () =>
            {
                sortingGroup.sortingOrder = -(int)(transform.position.y * 10);
            });
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public void ShowTextDamage(int damage, Vector3 position)
        {
            EventBus.Instance.Publish(new SpawnTextDamageEvent(TextDamageType.Normal, damage, position));
        }

        public void Behit()
        {
            if (takeDamageFeedback.IsPlaying) takeDamageFeedback.StopFeedbacks();
            takeDamageFeedback.PlayFeedbacks();
            if (sequence.isAlive) 
                return;
            sequence = Sequence.Create();
            sequence.Group(Tween.Custom(0, BLEND_VALUE, BLEND_DURATION, Apply));
            sequence.Chain(Tween.Custom(BLEND_VALUE, 0, BLEND_DURATION, Apply));
        }
        
        private void Apply(float blend)
        {
            foreach (var part in parts)
            {
                part.GetPropertyBlock(mpb);
            }

            mpb.SetFloat(HitEffectBlend, blend);
            
            foreach (var part in parts)
            {
                part.SetPropertyBlock(mpb);
            }
        }

        public void Destroy()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            sequence.Complete();

            Action playEffectAction = () =>
            {
                deathEffect.Play(() => { KitPool.Destroy(gameObject); });
            };

            if (shouldPlayDeathAnimation)
            {
                AnimancerState state = PlayAnimation(AnimationName.Death);
                float duration = state.Duration - deathEffect.EarlyPlayTime;
                this.WaitInvoke(duration / BattleTime.ScaleTime, playEffectAction);
            }
            else
            {
                playEffectAction();
            }
        }

        public AnimancerState PlayAnimation(AnimationName animationName)
        {
            foreach (var data in clips)
            {
                if (data.name == animationName)
                {
                    AnimancerState state = animancerComponent.Play(data.transition);
                    state.Speed = BattleTime.ScaleTime * data.transition.Speed;
                    return state;
                }
            }

            return null;
        }

        public virtual void PlayAttackAnimation()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            AnimancerState state = PlayAnimation(AnimationName.Attack);
            float duration = state.Duration;
            attackCoroutine = this.WaitInvoke(duration / BattleTime.ScaleTime, () => PlayAnimation(AnimationName.Idle));
        }

        [Serializable]
        class AnimationData
        {
            public AnimationName name;
            public ClipTransition transition;
        }
    }
}