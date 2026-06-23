using _KITSystem.Resource;
using UnityEngine;

namespace _Game.GamePlay
{
    public class Aura : MonoBehaviour
    {
        static readonly int Playing = Animator.StringToHash("Playing");
        
        [SerializeField] private Transform root;
        [SerializeField] private Animator animator;

        public void Scale(float scale)
        {
            root.localScale = Vector3.one * scale;
            animator.Play(Playing);
        }
        
        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}