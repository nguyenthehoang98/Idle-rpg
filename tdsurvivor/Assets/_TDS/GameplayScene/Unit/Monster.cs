using System;
using _TDS.Config;
using _Toolkit.Entities;
using _Toolkit.ResourceManagement;
using _Toolkit.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.GameplayScene.Unit
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Monster : Unique
    {
        public event Action<float> OnTakeDamage;
        public event Action OnDeath;
        
        // [Required]
        public float Radius { get { return circle.radius; } }

#if UNITY_EDITOR
        //[ProgressBar("Health", 300, EColor.Red)]
        //public int health = 250;
        // health-data debug gui
#endif

        CircleCollider2D circle;
        Vector3 targetPosition;
        Vector3 previousPosition;
        float deltaTime;
        float elapsedTime;
        bool isInitialized;
        int agent;
        
        private void Awake()
        {
            circle = GetComponent<CircleCollider2D>();
        }

        public void Initialize(Vector3 position, MonsterConfigData configData, MonsterContext context)
        {
            transform.position = position;
            transform.localScale = context.SizeScale * Vector3.one;

            agent = MonsterTickRunner.Instance.Create(
                this, configData, context
            );

            ComponentManager<HealthComponent>.Add(agent, new HealthComponent(100));
            
            gameObject.SetActive(true);
            
            isInitialized = true;
        }

        public void SetPosition(Vector3 position, float dt)
        {
            previousPosition = transform.position;
            targetPosition = position;
            deltaTime = dt; 
            elapsedTime = 0;
        }

        public void TakeDamagePost(float damage)
        {
            OnTakeDamage?.Invoke(damage);
        }
        
        private void FixedUpdate()
        {
            if (isInitialized)
            {
                elapsedTime += Time.fixedDeltaTime;
                float f = Mathf.Clamp01(elapsedTime / deltaTime);
                transform.position = Vector3.Lerp(previousPosition, targetPosition, f);
            }
        }

        public async void Destroy()
        {
            if (isInitialized)
            {
                OnDeath?.Invoke();
                
                ComponentManager<HealthComponent>.Remove(agent);
                
                isInitialized = false;
                
                gameObject.SetActive(false);

                await UniTask.NextFrame(PlayerLoopTiming.Update);
                
                Pool.Destroy(gameObject);
            }
        }

        public override int Id() => agent;
    }
}
