using NUnit.Framework;
using UnityEngine;
using _TDS.UI;

namespace _TDS.Tests.Editor
{
    public sealed class UiThemeTests
    {
        [Test]
        public void DefaultThemeUsesReferenceCanvasAndSemanticTokens()
        {
            UiTheme theme = ScriptableObject.CreateInstance<UiTheme>();

            Assert.That(theme.ReferenceResolution, Is.EqualTo(new Vector2(1080f, 2400f)));
            Assert.That(theme.SpacingUnit, Is.EqualTo(4f));
            Assert.That(theme.Background.a, Is.EqualTo(1f));
            Assert.That(theme.Primary.a, Is.EqualTo(1f));

            Object.DestroyImmediate(theme);
        }
    }
}
