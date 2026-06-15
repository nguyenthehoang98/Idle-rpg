using System;
using System.Collections;
using _Game.Configs;
using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Monster : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform scaler;

        public static event Action<Monster> OnMonsterEnable;
        public static event Action<Monster> OnMonsterDisable;
  
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

        public void Initialize(MonsterData monsterData)
        {
            AgentTickable.Add(this, monsterData);

            spriteRenderer.color = monsterData.color;
            scaler.transform.localScale = Vector3.one * monsterData.scale; 
            
            OnMonsterEnable?.Invoke(this);
        }

        public void Destroy()
        {
            AgentTickable.Remove(this);
            
            OnMonsterDisable?.Invoke(this);
            
            Pool.Destroy(gameObject);
        }
    }
}
