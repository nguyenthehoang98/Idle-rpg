using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiBadgeTone
    {
        Neutral,
        Accent,
        Warning,
        Danger,
    }

    [RequireComponent(typeof(Image))]
    public sealed class UiBadge : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private UiBadgeTone tone = UiBadgeTone.Neutral;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (theme != null) Apply(theme, tone);
        }

        public void Apply(UiTheme nextTheme, UiBadgeTone nextTone)
        {
            if (nextTheme == null) throw new ArgumentNullException(nameof(nextTheme));

            theme = nextTheme;
            tone = nextTone;
            image ??= GetComponent<Image>();
            image.sprite = SpriteFor(nextTheme, nextTone);
            image.type = image.sprite == null ? Image.Type.Simple : Image.Type.Sliced;
            image.color = ColorFor(nextTheme, nextTone);
        }

        private static Sprite SpriteFor(UiTheme theme, UiBadgeTone tone)
        {
            return tone switch
            {
                UiBadgeTone.Accent => theme.BadgeAccentSprite,
                UiBadgeTone.Warning => theme.BadgeWarningSprite,
                _ => null,
            };
        }

        private static Color ColorFor(UiTheme nextTheme, UiBadgeTone nextTone)
        {
            return nextTone switch
            {
                UiBadgeTone.Accent => nextTheme.Accent,
                UiBadgeTone.Warning => nextTheme.Warning,
                UiBadgeTone.Danger => nextTheme.Danger,
                _ => nextTheme.SurfaceElevated,
            };
        }
    }
}
