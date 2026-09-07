using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiBadgeDividerTests
    {
        [Test]
        public void BadgeAppliesWarningTone()
        {
            GameObject gameObject = new GameObject("Badge");
            gameObject.AddComponent<Image>();
            UiBadge badge = gameObject.AddComponent<UiBadge>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            badge.Apply(theme, UiBadgeTone.Warning);

            Assert.That(gameObject.GetComponent<Image>().color, Is.EqualTo(theme.Warning));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void DividerAppliesAccentTone()
        {
            GameObject gameObject = new GameObject("Divider");
            gameObject.AddComponent<Image>();
            UiDivider divider = gameObject.AddComponent<UiDivider>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            divider.Apply(theme, UiDividerTone.Accent);

            Assert.That(gameObject.GetComponent<Image>().color, Is.EqualTo(theme.Accent));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }
    }
}
