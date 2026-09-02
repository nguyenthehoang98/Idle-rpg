using System;
using _GameToolkit.ResourceManagement;
using _TDS.GameConfig;
using _TDS.Gameplay.Manager;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace _TDS.Gameplay.Model
{
    [RequireComponent(typeof(MonsterSortingLayer))]
    public class Monster : MonoBehaviour
    {
        [SerializeField] private Transform scaleTransform;
        [SerializeField] private UnityEvent OnBeHit;
        [SerializeField] private UnityEvent OnDeath;
        [SerializeField] private float radius;
         
        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

        private GameObject vfxPrefab;
        private AudioClip deathAudioClip;
        
        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private bool isInitialized;
        
        public float Radius => radius;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            
            Vector3 center = transform.position;
            
            Color color = Color.green;
            
            Handles.color = color;
            
            Handles.DrawWireDisc(center, Vector3.forward, radius, 2f);
            
            Handles.color = new Color(color.r, color.g, color.b, 0.1f);
            
            Handles.DrawSolidDisc(center, Vector3.forward, radius);
#endif
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
        }

        public async void Initialize(MonsterConfigData monsterConfigData, SpawnScaleDefinition scaleDefinition)
        {
            if (deathAudioClip == null && !string.IsNullOrEmpty(monsterConfigData.deathAudioClipName))
            {
                deathAudioClip = await AssetLoader.GetAssetCached<AudioClip>(monsterConfigData.deathAudioClipName);
            }

            if (vfxPrefab == null)
            {
                vfxPrefab = await AssetLoader.GetAssetCached<GameObject>(monsterConfigData.deathVfxName);
            }
            
            scaleTransform.localScale = scaleDefinition.sizeMultiplier * Vector3.one;
            
            OnMonsterEnable?.Invoke(this);
            
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

        public void BeHit() => OnBeHit?.Invoke();

        public async void Destroy()
        {
            if (!isInitialized) return;
            
            SoundManager.Instance.PlayOneShot(deathAudioClip);

            if (vfxPrefab != null)
            {
                Pool.Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            }
            
            OnMonsterDisable?.Invoke(this);
            
            isInitialized = false;
            
            OnDeath?.Invoke();
            
            gameObject.SetActive(false);

            await UniTask.NextFrame(PlayerLoopTiming.Update);
            
            Pool.Destroy(gameObject);
        }
    }
}
