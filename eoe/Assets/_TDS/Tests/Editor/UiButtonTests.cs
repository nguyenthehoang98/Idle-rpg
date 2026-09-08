using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiButtonTests
    {
        [Test]
        public void ButtonAppliesPrimaryThemeColors()
        {
            GameObject gameObject = new GameObject("Button");
            gameObject.AddComponent<Image>();
            UiButton button = gameObject.AddComponent<UiButton>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            button.Apply(theme, UiButtonTone.Primary);

            Assert.That(gameObject.GetComponent<Image>().color, Is.EqualTo(theme.Primary));
            Assert.That(gameObject.GetComponent<Button>().targetGraphic, Is.EqualTo(gameObject.GetComponent<Image>()));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ButtonUsesDisabledProductionSprite()
        {
            GameObject gameObject = new GameObject("Button");
            gameObject.AddComponent<Image>();
            UiButton button = gameObject.AddComponent<UiButton>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            button.Apply(theme, UiButtonTone.Primary);
            button.SetInteractable(false);

            Assert.That(gameObject.GetComponent<Image>().sprite, Is.EqualTo(theme.ButtonDisabledSprite));
            Assert.That(gameObject.GetComponent<Button>().interactable, Is.False);

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ButtonSupportsDangerTone()
        {
            GameObject gameObject = new GameObject("Button");
            gameObject.AddComponent<Image>();
            UiButton button = gameObject.AddComponent<UiButton>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            button.Apply(theme, UiButtonTone.Danger);

            Assert.That(gameObject.GetComponent<Image>().color, Is.EqualTo(theme.Danger));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }
    }
}
