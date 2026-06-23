using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class ElementTrigger : MonoBehaviour
    {
        [SerializeField] private Image img;

        private void Awake()
        {
            SetData(Color.grey);
        }

        public void SetData(Color color) => img.color = color;
    }
}