using LitMotion.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Home
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonHomeMenu : MonoBehaviour
    {
        [SerializeField] private LitMotionAnimation focusAnimation;
        [SerializeField] private LitMotionAnimation unfocusAnimation;

        private Button button;

        public Button Button => button != null ? button : button = GetComponent<Button>();
        public bool IsFocused { get; private set; }

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Focus()
        {
            IsFocused = true;
            focusAnimation?.Restart();
        }

        public void Unfocus()
        {
            IsFocused = false;
            unfocusAnimation?.Restart();
        }

        public void SetFocused(bool focused)
        {
            if (focused)
            {
                Focus();
                return;
            }

            Unfocus();
        }

    }
}
