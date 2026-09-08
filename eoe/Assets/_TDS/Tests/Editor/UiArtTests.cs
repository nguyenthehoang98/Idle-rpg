using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiArtTests
    {
        [Test]
        public void ProductionArtPackLoadsAsSprites()
        {
            foreach (string assetName in new[]
            {
                "ui-panel",
                "ui-panel-selected",
                "ui-button-primary",
                "ui-button-secondary",
                "ui-button-disabled",
                "ui-button-danger",
                "ui-badge-accent",
                "ui-badge-warning",
                "ui-slot-empty",
                "ui-slot-active",
                "ui-divider-accent",
            })
            {
                Assert.That(Resources.Load<Sprite>($"UI/Art/{assetName}"), Is.Not.Null, assetName);
            }
        }

        [Test]
        public void PanelUsesNineSliceProductionSprite()
        {
            GameObject gameObject = new GameObject("Panel");
            gameObject.AddComponent<Image>();
            UiPanel panel = gameObject.AddComponent<UiPanel>();
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            panel.Apply(theme, UiPanelTone.Surface);

            Image image = gameObject.GetComponent<Image>();
            Assert.That(image.sprite, Is.Not.Null);
            Assert.That(image.sprite.border.x, Is.GreaterThan(0));
            Assert.That(image.type, Is.EqualTo(Image.Type.Sliced));

            Object.DestroyImmediate(theme);
            Object.DestroyImmediate(gameObject);
        }
    }
}
