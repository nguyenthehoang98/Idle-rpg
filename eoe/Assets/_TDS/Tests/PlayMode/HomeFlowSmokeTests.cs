using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace _TDS.Tests.PlayMode
{
    public sealed class HomeFlowSmokeTests
    {
        [UnityTest]
        public IEnumerator HomeSceneBuildsTwentyLevelButtons()
        {
            yield return SceneManager.LoadSceneAsync("HomeScene");
            yield return null;

            GameObject canvasObject = GameObject.Find("HomeCanvas");
            Assert.That(canvasObject, Is.Not.Null);
            Assert.That(canvasObject.GetComponent<Canvas>(), Is.Not.Null);

            Text title = canvasObject.transform.Find("Panel/Title")?.GetComponent<Text>();
            Assert.That(title, Is.Not.Null);
            Assert.That(title.fontSize, Is.EqualTo(41).Within(1));

            RectTransform level1 = canvasObject.transform.Find("Panel/LevelRow/Level1")?.GetComponent<RectTransform>();
            Assert.That(level1, Is.Not.Null);
            Assert.That(level1.rect.width, Is.EqualTo(148f).Within(1));

            Image importedImage = canvasObject.transform.Find("santa-claus 1")?.GetComponent<Image>();
            Assert.That(importedImage, Is.Not.Null);
            Assert.That(importedImage.sprite, Is.Not.Null);

            Button[] buttons = canvasObject.GetComponentsInChildren<Button>(true);
            Assert.That(buttons, Has.Length.EqualTo(20));
            for (int level = 1; level <= 20; level++)
            {
                Assert.That(canvasObject.transform.Find($"Panel/LevelRow/Level{level}"), Is.Not.Null);
            }
        }
    }
}
