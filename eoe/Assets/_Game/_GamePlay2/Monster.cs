using System;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Game._GamePlay2
{
    [RequireComponent(typeof(MonsterSkin))]
    [RequireComponent(typeof(MonsterSortingLayer))]
    public class Monster : MonoBehaviour
    {
        [SerializeField] private Transform scaleTransform;
        
        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

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

            transform.position = Vector3.Lerp(previousPosition, targetPosition, Mathf.Clamp01(elapsedTime / deltaTime));
        }
        
        public async UniTask Initialize(MonsterData monsterData)
        {
            await GetComponent<MonsterSkin>().UpdateSkin(monsterData.skin);
            
            if (!string.IsNullOrEmpty(monsterData.deathAudioClip) && deathAudioClip == null)
            {
                deathAudioClip = await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
            }
            
            deathVolume = monsterData.deathVolume;

            int x = transform.position.x < 0 ? 1 : -1;

            scaleTransform.localScale = monsterData.scale * new Vector3(x, 1, 1);

            OnMonsterEnable?.Invoke(this);
            
            isInitialized = true;
        }
        
        public void SetPosition(Vector3 position, float dt)
        {
            previousPosition = transform.position;
            
            targetPosition = position;
            
            deltaTime = dt;
            
            elapsedTime = 0;
        }

        public void BeHit()
        {
        }

        public void Destroy()
        {
            if (!isInitialized) return;
            
            if(deathAudioClip != null) SoundManager.Instance.PlayOneShot(deathAudioClip, deathVolume);
            
            OnMonsterDisable?.Invoke(this);
            
            isInitialized = false;
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
