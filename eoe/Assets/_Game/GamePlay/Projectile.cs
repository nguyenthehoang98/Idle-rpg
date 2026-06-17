using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Projectile : MonoBehaviour
    {
        static readonly int Death = Animator.StringToHash("Death");
        static readonly int Initialize_ = Animator.StringToHash("Initialize");
        
        [SerializeField] private Animator animator;

        public void Initialize()
        {
            animator.Play(Initialize_);
        }

        public void Destroy()
        {
            animator.Play(Death);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}
