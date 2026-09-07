using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiPanelTests
    {
        [Test]
        public void PanelAppliesThemeToneToItsImage()
        {
            GameObject gameObject = new GameObject("Panel");
            UiPanel panel = gameObject.AddComponent<UiPanel>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            panel.Apply(theme, UiPanelTone.Elevated);

            Assert.That(gameObject.GetComponent<Image>().color, Is.EqualTo(theme.SurfaceElevated));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void PanelSupportsSemanticTones()
        {
            GameObject gameObject = new GameObject("Panel");
            UiPanel panel = gameObject.AddComponent<UiPanel>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();
            Image image = gameObject.GetComponent<Image>();

            panel.Apply(theme, UiPanelTone.Primary);
            Assert.That(image.color, Is.EqualTo(theme.Primary));

            panel.Apply(theme, UiPanelTone.Danger);
            Assert.That(image.color, Is.EqualTo(theme.Danger));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }
    }
}
