using System;
using System.Collections;
using _Game.Configs;
using _Game.GamePlay.Manager;
using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Monster : MonoBehaviour
    {
        static readonly int _Death = Animator.StringToHash("Death");
        static readonly int _Initialize = Animator.StringToHash("Initialize");
        static readonly int _BeHit = Animator.StringToHash("Behit");

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

        private void OnEnable()
        {
            StartCoroutine(AutoSort());            
        }

        private IEnumerator AutoSort()
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

        public void Initialize(MonsterData monsterData)
        {
            this.monsterData = monsterData;

            spriteRenderer.color = monsterData.color;
            scaler.transform.localScale = Vector3.one * monsterData.scale;

            animator.Play(_Initialize);

            OnMonsterEnable?.Invoke(this);

            isInitialized = true;
        }

        public void BeHit()
        {
            animator.Play(_BeHit, 0, 0);
        }

        public async void Destroy()
        {
            if (!isInitialized) return;

            isInitialized = false;

            if (!string.IsNullOrEmpty(monsterData.deathAudioClip))
            {
                AudioClip clip = await AssetBundleManager.GetAssetCached<AudioClip>(monsterData.deathAudioClip);
                SoundManager.Instance.PlayOneShot(clip, monsterData.deathVolume);
            }

            animator.Play(_Death);

            OnMonsterDisable?.Invoke(this);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}