using UnityEngine;
using UnityEngine.Events;

namespace _FightCode.Battle.View
{
    public class MonsterAnimationCallback : MonoBehaviour
    {
        [SerializeField] private UnityEvent onAttackEvent;
        [SerializeField] private UnityEvent onDeathEvent;
        
        public void OnAttack()
        {
            onAttackEvent?.Invoke();
        }

        public void OnDeath()
        {
            onDeathEvent?.Invoke();
        }
    }
}