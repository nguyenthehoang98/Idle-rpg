using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace _Games.Combat.Model
{
    public class Monster : MonoBehaviour, IAuthoring
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private UnityEvent OnDestroy;
        
        public void Initialize(Entity entity)
        {
        }
    }
}