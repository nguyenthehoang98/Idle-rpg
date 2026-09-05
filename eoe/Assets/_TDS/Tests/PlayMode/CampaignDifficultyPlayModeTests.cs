using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace _TDS.Tests.PlayMode
{
    public sealed class CampaignDifficultyPlayModeTests
    {
        [UnityTest]
        public IEnumerator LevelTwoClearsWithConfiguredUpgradeChoices()
        {
            yield return RunLevelWithUpgradeChoices(2);
        }

        [UnityTest]
        public IEnumerator LevelThreeClearsWithConfiguredUpgradeChoices()
        {
            yield return RunLevelWithUpgradeChoices(3);
        }

        private static IEnumerator RunLevelWithUpgradeChoices(int level)
        {
            SelectLevel(level);
            yield return SceneManager.LoadSceneAsync("BootScene");
            yield return WaitForScene("HomeScene", 20f);
            yield return SceneManager.LoadSceneAsync("GamePlayScene");

            float elapsed = 0f;
            string outcome = null;
            while (elapsed < 240f)
            {
                Button rollButton = GameObject.Find("UpgradeRoll")?.GetComponent<Button>();
                if (rollButton != null && rollButton.gameObject.activeInHierarchy)
                {
                    rollButton.onClick.Invoke();
                }

                Button cardButton = GameObject.Find("Card0")?.GetComponent<Button>();
                if (cardButton != null && cardButton.gameObject.activeInHierarchy && cardButton.interactable)
                {
                    cardButton.onClick.Invoke();
                }

                string status = GameObject.Find("Status")?.GetComponent<Text>()?.text;
                if (status != null && (status.StartsWith("VICTORY") || status.StartsWith("DEFEAT")))
                {
                    outcome = status.StartsWith("VICTORY") ? "VICTORY" : "DEFEAT";
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(outcome, Is.EqualTo("VICTORY"), $"Level {level} should clear with upgrade choices");
        }

        private static void SelectLevel(int level)
        {
            Type runSelection = Type.GetType("_TDS.Gameplay.RunSelection, Assembly-CSharp");
            Assert.That(runSelection, Is.Not.Null);
            runSelection.GetMethod("SelectLevel", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { level });
        }

        private static IEnumerator WaitForScene(string sceneName, float timeout)
        {
            float elapsed = 0f;
            while (SceneManager.GetActiveScene().name != sceneName && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
        }
    }
}
