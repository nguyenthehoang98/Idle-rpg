using System;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using _KITSystem.Utils;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.GamePlay.Model
{
    [RequireComponent(typeof(MonsterSortingLayer))]
    public class Monster : MonoBehaviour
    {
        [SerializeField] private Transform scaleTransform;
        [SerializeField] private Transform flipTransform;
        [SerializeField] private UnityEvent OnBeHit;
        [SerializeField] private UnityEvent OnDeath;
        
        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

        private GameObject vfxPrefab;
        private AudioClip deathAudioClip;
        private float deathVolume;
        
        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private bool isInitialized;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) 
                return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
#endif
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
        }

        public async void Initialize(MonsterData monsterData)
        {
            if (deathAudioClip == null && !string.IsNullOrEmpty(monsterData.deathAudioClip))
            {
                deathAudioClip = await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
            }

            if (vfxPrefab == null)
            {
                vfxPrefab = await AssetBundleManager.GetAssetCached<GameObject>(monsterData.deathVfx);
            }
            
            deathVolume = monsterData.deathVolume;

            int x = transform.position.x < 0 ? 1 : -1;

            scaleTransform.localScale = Vector3.zero;
            flipTransform.localScale = new Vector3(x, 1, 1);

            Vector3 scale = monsterData.scale * Vector3.one;

            OnMonsterEnable?.Invoke(this);
            
            gameObject.SetActive(true);

            float duration = 0.15f;
            
            LMotion.Create(Vector3.zero, scale, duration)
                .BindToLocalScale(scaleTransform);
            this.WaitInvoke(duration, () => { isInitialized = true; });
        }
        
        public void SetPosition(Vector3 position, float dt)
        {
            previousPosition = transform.position;
            
            targetPosition = position;
            
            deltaTime = dt;
            
            elapsedTime = 0;
        }

        public void BeHit() => OnBeHit?.Invoke();

        public void Destroy()
        {
            if (!isInitialized) return;
            
            SoundManager.Instance.PlayOneShot(deathAudioClip, deathVolume);

            if (vfxPrefab != null)
            {
                Pool.Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            }
            
            OnMonsterDisable?.Invoke(this);
            
            isInitialized = false;
            
            OnDeath?.Invoke();
            
            gameObject.SetActive(false);
            
            Pool.Destroy(gameObject);
        }
    }
}
