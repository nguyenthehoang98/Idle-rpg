using _Toolkit.ResourceManagement;
using TMPro;
using UnityEngine;

namespace _TDS.GameplayScene.View
{
    public class TextDamage : MonoBehaviour
    {
        private static readonly int Play = Animator.StringToHash("Play");

        [SerializeField] private TextMeshProUGUI txtDamage;
        [SerializeField] private Animator animator;

        public void Execute(int damage)
        {
            txtDamage.text = damage.ToString();
            animator.Play(Play, 0, 0);
        }

        public void Release()
        {
            Pool.Destroy(gameObject);
        }
    }
}