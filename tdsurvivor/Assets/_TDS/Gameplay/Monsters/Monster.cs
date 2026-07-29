using System;
using _GameToolkit.Resource;
using _TDS.GameConfig;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _TDS.Unit
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Monster : MonoBehaviour
    {
        private CircleCollider2D circle;
        
        private Vector3 targetPosition;
        private Vector3 previousPosition;
        float deltaTime;
        float elapsedTime;
        bool isInitialized;

        public float Radius => circle.radius;
        
        private void Awake()
        {
            circle = GetComponent<CircleCollider2D>();
        }

        public void Initialize(Vector3 position, MonsterConfigData configData, MonsterRuntimeData runtimeData)
        {
            transform.position = position;

            transform.localScale = Vector3.one * runtimeData.SizeScale;
            
            MonsterMoveUpdater.Instance.CreateAgent(this, configData, runtimeData);
            
            gameObject.SetActive(true);

            isInitialized = true;
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