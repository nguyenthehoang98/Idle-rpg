using System;
using System.Collections;
using _Game.Configs;
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

        public void Initialize(MonsterData monsterData)
        {
            AgentTickable.Add(this, monsterData);

            spriteRenderer.color = monsterData.color;
            scaler.transform.localScale = Vector3.one * monsterData.scale; 
            animator.Play(Initialize_);
            
            OnMonsterEnable?.Invoke(this);
            
            isInitialized = true;
        }

        public void BeBit()
        {
            animator.Play(BeHit_, 0, 0);
        }

        public void Destroy()
        {
            isInitialized = false;
            
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
