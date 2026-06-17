using System;
using System.Collections;
using _Game.Configs;
using _Game.GamePlay.SoundSystem;
using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Monster : MonoBehaviour
    {
        static readonly int Death = Animator.StringToHash("Death");
        static readonly int Initialize_ = Animator.StringToHash("Initialize");
        static readonly int BeHit_ = Animator.StringToHash("Behit");
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform scaler;
        [SerializeField] private Animator animator;

        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;

        private MonsterData monsterData;
        private Vector3 targetPosition;
        private Vector3 previousPosition;
        private float elapsedTime;
        private float deltaTime;
        private bool isInitialized;
  
        void Start()
        {
            StartCoroutine(AutoSort());
        }
        
        IEnumerator AutoSort()
        {
            while (true)
            {
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
 
                yield return new WaitForSeconds(1f);
            }
        }

        public void SetPosition(Vector3 position, float deltaTime)
        {
            this.previousPosition = transform.position;
            this.targetPosition = position;
            this.deltaTime = deltaTime;
            this.elapsedTime = 0;
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            elapsedTime += Time.fixedDeltaTime;

            transform.position = Vector3.Lerp(previousPosition, targetPosition, Mathf.Clamp01(elapsedTime / deltaTime));
        }

        public void Initialize(MonsterData data)
        {
            AgentTickable.Add(this, data);

            monsterData = data;
            spriteRenderer.color = data.color;
            scaler.transform.localScale = Vector3.one * data.scale; 
            animator.Play(Initialize_);
            
            OnMonsterEnable?.Invoke(this);
            
            isInitialized = true;
        }

        public async void BeHit()
        {
            animator.Play(BeHit_, 0, 0);

            AudioClip clip = null;
            if (!string.IsNullOrEmpty(monsterData.beHitAudioClip))
            {
                clip = await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.beHitAudioClip);
            }

            SoundManager.Instance.PlayOneShot(clip, monsterData.beHitVolume);
        }

        public async void Destroy()
        {
            isInitialized = false;

            AudioClip clip = null;
            if (!string.IsNullOrEmpty(monsterData.deathAudioClip))
            {
                clip = await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
            }

            SoundManager.Instance.PlayOneShot(clip, monsterData.deathVolume);
            
            AgentTickable.Remove(this);
            
            OnMonsterDisable?.Invoke(this);
            
            animator.Play(Death);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
