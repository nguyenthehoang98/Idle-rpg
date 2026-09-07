using System;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.UI
{
    public enum UiTextRole
    {
        Display,
        Heading,
        Body,
        Label,
        Caption,
    }

    [RequireComponent(typeof(Text))]
    public sealed class UiText : MonoBehaviour
    {
        [SerializeField] private UiTheme theme;
        [SerializeField] private UiTextRole role = UiTextRole.Body;

        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
            if (theme != null) Apply(theme, role);
        }

        public void Apply(UiTheme nextTheme, UiTextRole nextRole)
        {
            if (nextTheme == null) throw new ArgumentNullException(nameof(nextTheme));

            theme = nextTheme;
            role = nextRole;
            text ??= GetComponent<Text>();
            text.fontSize = SizeFor(nextTheme, nextRole);
            text.color = ColorFor(nextTheme, nextRole);
        }

        private static int SizeFor(UiTheme theme, UiTextRole role)
        {
            return role switch
            {
                UiTextRole.Display => theme.DisplaySize,
                UiTextRole.Heading => theme.HeadingSize,
                UiTextRole.Body => theme.BodySize,
                UiTextRole.Label => theme.LabelSize,
                UiTextRole.Caption => theme.CaptionSize,
                _ => theme.BodySize,
            };
        }

        private static Color ColorFor(UiTheme theme, UiTextRole role)
        {
            return role == UiTextRole.Caption ? theme.TextSecondary : theme.TextPrimary;
        }
    }
}
