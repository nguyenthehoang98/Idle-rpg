using System;
using _GameToolkit.Resource;
using _TDS.Combat;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Unit
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Monster : MonoBehaviour, IDamageable
    {
        [SerializeField] private Health health;

        private CircleCollider2D circle;

        private Vector3 targetPosition;
        private Vector3 previousPosition;
        float deltaTime;
        float elapsedTime;
        bool isInitialized;

        public float Radius => circle.radius;

        public int EntityId { get; private set; }
        public bool IsAlive => isInitialized && (health == null || health.IsAlive);

        private void Awake()
        {
            circle = GetComponent<CircleCollider2D>();
            
            if (health == null) health = GetComponentInChildren<Health>();
        }

        public void Initialize(Vector3 position, MonsterConfigData configData, MonsterRuntimeData runtimeData)
        {
            transform.position = position;

            transform.localScale = Vector3.one * runtimeData.SizeScale;

            if (health != null)
            {
                health.Initialize(Mathf.Max(1, Mathf.RoundToInt(configData.health * runtimeData.HealthScale)));
            }

            int agent = MonsterMoveUpdater.Instance.CreateAgent(this, configData, runtimeData);
            EntityId = agent;

            gameObject.SetActive(true);

            isInitialized = true;
        }

        public void TakeDamage(int damage)
        {
            if (!isInitialized) return;

            health?.TakeDamage(damage);

            if (health != null && !health.IsAlive)
            {
                Destroy();
            }
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
        }

        public void SetPosition(Vector3 position, float dt)
        {
            previousPosition = transform.position;

            targetPosition = position;

            deltaTime = dt;

            elapsedTime = 0;
        }

        public async void Destroy()
        {
            if (!isInitialized) return;

            isInitialized = false;

            gameObject.SetActive(false);

            await UniTask.NextFrame(PlayerLoopTiming.Update);

            Pool.Destroy(gameObject);
        }
    }
}
