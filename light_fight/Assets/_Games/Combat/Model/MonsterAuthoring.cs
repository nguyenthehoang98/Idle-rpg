using System;
using _Games.Combat.Event;
using _KIT.Event;
using _KIT.Pool;
using _KIT.Utils;
using Animancer;
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
        [SerializeField] private float offsetYTextDamage = 0.2f;
        [SerializeField] private float offsetXTextDamage = 0.2f;
        [Header("Collider")]
        [SerializeField] private float radius;
        [Header("Animations")] 
        [SerializeField] private float delayExecuteAttack;
        [SerializeField] private AnimationData[] clips;

        private AnimancerComponent animancerComponent;
        private Coroutine attackCoroutine;
        
        public float Radius => radius;
        public float DelayExecuteAttack => delayExecuteAttack;

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public void ShowTextDamage(int damage, Vector3 position)
        {
            float x = Mathf.Abs(offsetXTextDamage);
            Vector3 finalPosition = position + new Vector3(RandomUtils.Range(-x, x), offsetYTextDamage, 10);
            EventBus.Instance.Publish(new SpawnTextDamageEvent(TextDamageType.Normal, damage, finalPosition));
        }

        public void Destroy()
        {
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