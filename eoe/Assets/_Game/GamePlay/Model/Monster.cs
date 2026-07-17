using System;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _Game.GamePlay.Model
{
    [RequireComponent(typeof(MonsterSkin))]
    [RequireComponent(typeof(MonsterSortingLayer))]
    public class Monster : MonoBehaviour
    {
        [SerializeField] private Transform scaleTransform;
        [SerializeField] private Transform rendererTransform;
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

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / deltaTime);

            transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
        }

        public async void Initialize(MonsterData monsterData)
        {
            await GetComponent<MonsterSkin>().UpdateSkin(monsterData.skin);
            
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

            scaleTransform.localScale = monsterData.scale * Vector3.one;
            rendererTransform.localScale = new Vector3(x, 1, 1);

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
            
            SoundManager.Instance.PlayOneShot(deathAudioClip, deathVolume);

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
