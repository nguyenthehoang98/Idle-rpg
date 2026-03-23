using System;
using _Games.Combat.EntityComponentSystem.Data;
using _Games.Combat.Event;
using _Games.Utils;
using _KIT.Config;
using _KIT.Event;
using _KIT.Utils;
using Animancer;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Combat.Model
{
    [RequireComponent(typeof(AnimancerComponent))]
    [RequireComponent(typeof(Animator))]
    public class Monster : MonoBehaviour, IAuthoring
    {
        [Header("Renderer")]
        [SerializeField] private SortingGroup sortingGroup;
        [Header("Collider")]
        [SerializeField] private float radius;
        [Header("Animations")] 
        [SerializeField] private AnimationData[] clips;

        private AnimancerComponent animancerComponent;
        private MeleeAttackRequest meleeAttackRequest;
        private Coroutine attackCoroutine;
        private MonsterConfig monsterConfig;
        private SkillConfig skillConfig;
        protected Entity Entity { get; private set; }
        
        public float Radius => radius;

        private void Awake()
        {
            monsterConfig = KitConfigManager.Get<MonsterConfig>();
            skillConfig = KitConfigManager.Get<SkillConfig>();
            animancerComponent = GetComponent<AnimancerComponent>();
        }

        private void Start()
        {
            Debug.Log(@"Tạo 1 monster chỉ chứa logic. Monster này sẽ chuyển thành monster view để có thể xử lý strest test.
Phần callback OnAttack sẽ chuyển sang invoke theo thời gian, config với mỗi monster");
        }

        public void Initialize(Entity entity)
        {
            Entity = entity;
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
        
        public virtual void OnAttack()
        {
            if (meleeAttackRequest.IsValid)
            {
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                HealthData player = manager.GetComponentData<HealthData>(meleeAttackRequest.Target);
                monsterConfig.Find(meleeAttackRequest.MonsterId, out var monsterData);
                skillConfig.Find(meleeAttackRequest.SkillId, meleeAttackRequest.SkillLevel, out var skillData);
                player.Health -= FormulaUtils.Output(monsterData.Attack, skillData, 0, 0, 0);
                manager.SetComponentData(meleeAttackRequest.Target, player);     
                EventBus.Instance.Publish(new PlayerOnDamageEvent());
                
                meleeAttackRequest = MeleeAttackRequest.None();
            }
        }

        public virtual void OnDeath()
        {
        }

        public void InjectMeleeAttack(MeleeAttackRequest request)
        {
            meleeAttackRequest = request;
        }

        public AnimancerState PlayAnimation(AnimationName animationName)
        {
            foreach (var data in clips)
            {
                if (data.name == animationName)
                    return animancerComponent.Play(data.transition);
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