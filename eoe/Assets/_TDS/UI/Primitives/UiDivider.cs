using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiDividerTone
    {
        Subtle,
        Accent,
    }

    [RequireComponent(typeof(Image))]
    public sealed class UiDivider : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private UiDividerTone tone = UiDividerTone.Subtle;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (theme != null) Apply(theme, tone);
        }

        public void Apply(UiTheme nextTheme, UiDividerTone nextTone)
        {
            if (nextTheme == null) throw new ArgumentNullException(nameof(nextTheme));

            theme = nextTheme;
            tone = nextTone;
            image ??= GetComponent<Image>();
            image.color = nextTone == UiDividerTone.Accent
                ? nextTheme.Accent
                : new Color(nextTheme.TextSecondary.r, nextTheme.TextSecondary.g, nextTheme.TextSecondary.b, 0.45f);
        }
    }
}
