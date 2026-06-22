using System;
using System.Collections;
using _Game.Configs;
using _Game.GamePlay.SoundSystem;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
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

        private MonsterData MonsterData { get; set; }
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

        public async UniTask Initialize(MonsterData monsterData, MonsterScaleStatData scaleStat)
        {
            AgentTickable.Add(this, scaleStat, monsterData);

            this.MonsterData = monsterData;
            spriteRenderer.color = monsterData.color;
            scaler.transform.localScale = Vector3.one * monsterData.scale; 
            animator.Play(Initialize_);
            
            if (!string.IsNullOrEmpty(this.MonsterData.deathAudioClip))
            { 
                await AssetBundleManager.GetAssetCached<AudioClip>(this.MonsterData.deathAudioClip);
            }
            
            OnMonsterEnable?.Invoke(this);
            
            isInitialized = true;
        }

        public void BeHit()
        {
            animator.Play(BeHit_, 0, 0);
        }

        public async void Destroy()
        {
            if (!isInitialized) return;
            
            isInitialized = false;
            
            AgentTickable.Remove(this);
            
            OnMonsterDisable?.Invoke(this);
            
            AudioClip clip = null;
            if (!string.IsNullOrEmpty(MonsterData.deathAudioClip))
            {
                clip = await AssetBundleManager.GetAssetCached<AudioClip>(MonsterData.deathAudioClip);
            }

            SoundManager.Instance.PlayOneShot(clip, MonsterData.deathVolume);

            animator.Play(Death);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
