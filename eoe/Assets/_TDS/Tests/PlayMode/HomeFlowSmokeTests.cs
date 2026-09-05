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
        public IEnumerator HomeSceneBuildsFiveLevelButtons()
        {
            yield return SceneManager.LoadSceneAsync("HomeScene");
            yield return null;

            GameObject canvasObject = GameObject.Find("HomeCanvas");
            Assert.That(canvasObject, Is.Not.Null);
            Assert.That(canvasObject.GetComponent<Canvas>(), Is.Not.Null);

            Button[] buttons = canvasObject.GetComponentsInChildren<Button>(true);
            Assert.That(buttons, Has.Length.EqualTo(5));
            for (int level = 1; level <= 5; level++)
            {
                Assert.That(canvasObject.transform.Find($"Panel/LevelRow/Level{level}"), Is.Not.Null);
            }
        }
    }
}
