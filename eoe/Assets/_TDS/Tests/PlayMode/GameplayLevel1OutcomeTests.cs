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
    public sealed class GameplayLevel1OutcomeTests
    {
        [UnityTest]
        public IEnumerator Level1ReachesWinOrLose()
        {
            UnityEngine.Random.InitState(1);
            Type runSelection = Type.GetType("_TDS.Gameplay.RunSelection, Assembly-CSharp");
            Assert.That(runSelection, Is.Not.Null);
            runSelection.GetMethod("SelectLevel", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { 1 });

            yield return SceneManager.LoadSceneAsync("BootScene");

            float elapsed = 0f;
            while (SceneManager.GetActiveScene().name != "HomeScene" && elapsed < 20f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("HomeScene"));

            yield return SceneManager.LoadSceneAsync("GamePlayScene");

            elapsed = 0f;
            string outcome = null;
            string status = null;
            bool selectedUpgrade = false;
            bool shopTested = false;
            while (elapsed < 180f)
            {
                Button shopButton = GameObject.Find("GoldShop")?.GetComponent<Button>();
                if (!shopTested && shopButton != null && shopButton.gameObject.activeInHierarchy)
                {
                    shopButton.onClick.Invoke();
                    shopTested = true;
                }

                Button cardButton = GameObject.Find("Card0")?.GetComponent<Button>();
                if (cardButton != null && cardButton.gameObject.activeInHierarchy)
                {
                    if (cardButton.interactable)
                    {
                        cardButton.onClick.Invoke();
                    }
                    else
                    {
                        GameObject.Find("Back")?.GetComponent<Button>()?.onClick.Invoke();
                    }
                }

                Button rollButton = GameObject.Find("UpgradeRoll")?.GetComponent<Button>();
                if (rollButton != null && rollButton.gameObject.activeInHierarchy)
                {
                    rollButton.onClick.Invoke();
                }

                Text statusText = GameObject.Find("Status")?.GetComponent<Text>();
                status = statusText?.text;
                selectedUpgrade |= !string.IsNullOrEmpty(status) && status.StartsWith("UPGRADE:");
                if (!string.IsNullOrEmpty(status) &&
                    (status.StartsWith("VICTORY") || status.StartsWith("DEFEAT")))
                {
                    outcome = status.StartsWith("VICTORY") ? "VICTORY" : "DEFEAT";
                    break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Debug.Log($"[GameplayLevel1Outcome] outcome={outcome ?? "TIMEOUT"}, status={status ?? "<none>"}, elapsed={elapsed:0.0}s");
            Assert.That(outcome, Is.EqualTo("VICTORY"), "Level 1 must be clearable without a loss");
            Assert.That(selectedUpgrade, Is.True, "The post-wave upgrade choice was not applied");
            Assert.That(shopTested, Is.True, "The Gold Shop path was not shown");

            yield return null;
            Button continueButton = GameObject.Find("Continue")?.GetComponent<Button>();
            Assert.That(continueButton, Is.Not.Null, "The result loop button was not shown");
            continueButton.onClick.Invoke();
            elapsed = 0f;
            while (SceneManager.GetActiveScene().name != "GamePlayScene" && elapsed < 20f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GamePlayScene"));
            while (elapsed < 40f)
            {
                Text restartedStatus = GameObject.Find("Status")?.GetComponent<Text>();
                if (!string.IsNullOrEmpty(restartedStatus?.text) && restartedStatus.text != "LOADING BATTLE") break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
