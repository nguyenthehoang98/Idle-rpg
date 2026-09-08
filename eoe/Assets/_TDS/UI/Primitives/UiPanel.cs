using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiPanelTone
    {
        Background,
        Surface,
        Elevated,
        Primary,
        Accent,
        Warning,
        Danger,
    }

    [RequireComponent(typeof(Image))]
    public sealed class UiPanel : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private UiPanelTone tone = UiPanelTone.Surface;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (theme != null) Apply(theme, tone);
        }

        public void Apply(UiTheme nextTheme, UiPanelTone nextTone)
        {
            if (nextTheme == null) throw new ArgumentNullException(nameof(nextTheme));

            theme = nextTheme;
            tone = nextTone;
            image ??= GetComponent<Image>();
            image.sprite = SpriteFor(nextTheme, nextTone);
            image.type = image.sprite == null ? Image.Type.Simple : Image.Type.Sliced;
            image.color = ColorFor(nextTheme, nextTone);
        }

        private static Sprite SpriteFor(UiTheme theme, UiPanelTone tone)
        {
            return tone switch
            {
                UiPanelTone.Surface => theme.PanelSprite,
                UiPanelTone.Elevated => theme.PanelSprite,
                UiPanelTone.Accent => theme.PanelSelectedSprite,
                _ => null,
            };
        }

        private static Color ColorFor(UiTheme theme, UiPanelTone tone)
        {
            return tone switch
            {
                UiPanelTone.Background => theme.Background,
                UiPanelTone.Surface => theme.Surface,
                UiPanelTone.Elevated => theme.SurfaceElevated,
                UiPanelTone.Primary => theme.Primary,
                UiPanelTone.Accent => theme.Accent,
                UiPanelTone.Warning => theme.Warning,
                UiPanelTone.Danger => theme.Danger,
                _ => theme.Surface,
            };
        }
    }
}
