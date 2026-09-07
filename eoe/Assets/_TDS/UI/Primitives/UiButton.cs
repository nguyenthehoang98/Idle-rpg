using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiButtonTone
    {
        Primary,
        Secondary,
        Ghost,
        Danger,
    }

    [RequireComponent(typeof(Image), typeof(Button))]
    public sealed class UiButton : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private UiButtonTone tone = UiButtonTone.Primary;

        private Image image;
        private Button button;

        private void Awake()
        {
            image = GetComponent<Image>();
            button = GetComponent<Button>();
            if (theme != null) Apply(theme, tone);
        }

        public Button Button => button == null ? GetComponent<Button>() : button;

        public void Apply(UiTheme nextTheme, UiButtonTone nextTone)
        {
            if (nextTheme == null) throw new ArgumentNullException(nameof(nextTheme));

            theme = nextTheme;
            tone = nextTone;
            image ??= GetComponent<Image>();
            button ??= GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            Color normal = NormalColor(nextTheme, nextTone);
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = HighlightColor(nextTheme, nextTone);
            colors.pressedColor = PressedColor(nextTheme, nextTone);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = DisabledColor(nextTheme);
            button.colors = colors;
            image.color = normal;
        }

        private static Color NormalColor(UiTheme theme, UiButtonTone tone)
        {
            return tone switch
            {
                UiButtonTone.Primary => theme.Primary,
                UiButtonTone.Secondary => theme.SurfaceElevated,
                UiButtonTone.Ghost => new Color(theme.SurfaceElevated.r, theme.SurfaceElevated.g, theme.SurfaceElevated.b, 0.35f),
                UiButtonTone.Danger => theme.Danger,
                _ => theme.Primary,
            };
        }

        private static Color HighlightColor(UiTheme theme, UiButtonTone tone)
        {
            return tone == UiButtonTone.Danger ? theme.Warning : theme.Accent;
        }

        private static Color PressedColor(UiTheme theme, UiButtonTone tone)
        {
            return tone == UiButtonTone.Danger ? theme.Danger : theme.Primary;
        }

        private static Color DisabledColor(UiTheme theme)
        {
            return new Color(theme.SurfaceElevated.r, theme.SurfaceElevated.g, theme.SurfaceElevated.b, 0.45f);
        }
    }
}
