using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiTextTests
    {
        [Test]
        public void TextAppliesRoleTypographyAndColor()
        {
            GameObject gameObject = new GameObject("Text");
            UiText text = gameObject.AddComponent<UiText>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            text.Apply(theme, UiTextRole.Heading);

            Text nativeText = gameObject.GetComponent<Text>();
            Assert.That(nativeText.fontSize, Is.EqualTo(theme.HeadingSize));
            Assert.That(nativeText.color, Is.EqualTo(theme.TextPrimary));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void CaptionUsesSecondaryColor()
        {
            GameObject gameObject = new GameObject("Text");
            UiText text = gameObject.AddComponent<UiText>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            text.Apply(theme, UiTextRole.Caption);

            Assert.That(gameObject.GetComponent<Text>().fontSize, Is.EqualTo(theme.CaptionSize));
            Assert.That(gameObject.GetComponent<Text>().color, Is.EqualTo(theme.TextSecondary));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }
    }
}
