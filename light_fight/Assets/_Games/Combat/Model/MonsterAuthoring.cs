using System;
using _Games.Combat.Event;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Utils;
using Animancer;
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
        [SerializeField] private Vector3 offsetTextDamage = new Vector3(0.4f, 0.3f, 0);
        [Header("Collider")]
        [SerializeField] private float radius;
        [Header("Animations")] 
        [SerializeField] private float delayExecuteAttack;
        [SerializeField] private AnimationData[] clips;

        private AnimancerComponent animancerComponent;
        private Coroutine attackCoroutine;
        
        public float Radius => radius;
        public float DelayExecuteAttack => delayExecuteAttack;
        
        private static readonly int HitEffectBlend = Shader.PropertyToID("_HitEffectBlend");
        private const float BLEND_VALUE = 0.7f;
        private const float BLEND_DURATION = 0.15f;

        private MaterialPropertyBlock mpb;
        private Sequence sequence;

        private void Awake()
        {
            animancerComponent = GetComponent<AnimancerComponent>();
            mpb = new MaterialPropertyBlock();
        }

        public void Initialize(Entity entity)
        {
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
            float x = Mathf.Abs(offsetTextDamage.x);
            Vector3 finalPosition = position + new Vector3(RandomUtils.Range(-x, x), offsetTextDamage.y, 0);
            EventBus.Instance.Publish(new SpawnTextDamageEvent(TextDamageType.Normal, damage, finalPosition));
        }

        public void Behit()
        {
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
            sequence.Stop();
            KitPool.Destroy(gameObject);
        }

        public AnimancerState PlayAnimation(AnimationName animationName)
        {
            foreach (var data in clips)
            {
                if (data.name == animationName) return animancerComponent.Play(data.transition);
            }

            return null;
        }

        public virtual void PlayAttackAnimation()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
            AnimancerState state = PlayAnimation(AnimationName.Attack);
            attackCoroutine = this.WaitInvoke(state.Duration, () => PlayAnimation(AnimationName.Idle));
        }

        [Serializable]
        class AnimationData
        {
            public AnimationName name;
            public ClipTransition transition;
        }
    }
}