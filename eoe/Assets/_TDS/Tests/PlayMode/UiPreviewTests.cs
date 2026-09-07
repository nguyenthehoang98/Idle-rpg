using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace _TDS.Tests.PlayMode
{
    public sealed class UiPreviewTests
    {
        [UnityTest]
        public IEnumerator PreviewBuildsHomeShellFromBaseComponents()
        {
            GameObject root = new GameObject("UiPreviewTestRoot");
            System.Type screenType = System.Type.GetType("_TDS.UI.UiPreviewScreen, Assembly-CSharp");
            Assert.That(screenType, Is.Not.Null);
            root.AddComponent(screenType);
            yield return null;

            Transform canvas = root.transform.Find("UiCanvas");
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/Background"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/HeroCard"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/Continue"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/BottomNav"), Is.Not.Null);

            Object.Destroy(root);
            yield return null;
        }
    }
}
