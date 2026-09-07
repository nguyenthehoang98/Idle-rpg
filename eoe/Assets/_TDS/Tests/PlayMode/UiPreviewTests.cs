using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
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

        [UnityTest]
        public IEnumerator PreviewBuildsEveryPage()
        {
            GameObject root = new GameObject("UiPreviewPagesTestRoot");
            System.Type screenType = System.Type.GetType("_TDS.UI.UiPreviewScreen, Assembly-CSharp");
            System.Type pageType = System.Type.GetType("_TDS.UI.UiPreviewPage, Assembly-CSharp");
            Assert.That(screenType, Is.Not.Null);
            Assert.That(pageType, Is.Not.Null);
            Component screen = root.AddComponent(screenType);
            System.Reflection.MethodInfo showPage = screenType.GetMethod("ShowPage");
            Assert.That(showPage, Is.Not.Null);
            yield return null;

            foreach (string pageName in new[] { "Home", "Gameplay", "Reward", "Shop", "Augment", "Result" })
            {
                object page = System.Enum.Parse(pageType, pageName);
                showPage.Invoke(screen, new[] { page });
                yield return null;
                Assert.That(root.transform.Find("UiCanvas/SafeArea"), Is.Not.Null, pageName);
            }

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PreviewConnectsAugmentToResultAndHome()
        {
            GameObject root = new GameObject("UiPreviewFlowTestRoot");
            System.Type screenType = System.Type.GetType("_TDS.UI.UiPreviewScreen, Assembly-CSharp");
            System.Type pageType = System.Type.GetType("_TDS.UI.UiPreviewPage, Assembly-CSharp");
            Component screen = root.AddComponent(screenType);
            System.Reflection.MethodInfo showPage = screenType.GetMethod("ShowPage");
            showPage.Invoke(screen, new[] { System.Enum.Parse(pageType, "Augment") });
            yield return null;

            root.transform.Find("UiCanvas/SafeArea/AcceptAugment").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(root.transform.Find("UiCanvas/SafeArea/ResultCard"), Is.Not.Null);

            root.transform.Find("UiCanvas/SafeArea/ResultCard/NextLevel").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(root.transform.Find("UiCanvas/SafeArea/HeroCard"), Is.Not.Null);

            Object.Destroy(root);
            yield return null;
        }
    }
}
