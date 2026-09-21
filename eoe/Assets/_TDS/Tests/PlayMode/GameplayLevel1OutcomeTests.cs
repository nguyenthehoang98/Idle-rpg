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
            System.Type.GetType("_TDS.Utils.GameRng, Assembly-CSharp")
                ?.GetMethod("Seed", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { 1 });
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

            yield return SceneManager.LoadSceneAsync("GameplayScene");

            elapsed = 0f;
            string outcome = null;
            string status = null;
            bool selectedUpgrade = false;
            bool firstChoiceWasStat = false;
            bool firstChoiceObserved = false;
            bool coreShopShown = false;
            while (elapsed < 180f)
            {
                Text title = GameObject.Find("Title")?.GetComponent<Text>();
                if (title != null && title.gameObject.activeInHierarchy)
                {
                    if (!firstChoiceObserved && (title.text == "UPGRADE ROLL" || title.text == "CORE SHOP"))
                    {
                        firstChoiceObserved = true;
                        firstChoiceWasStat = title.text == "UPGRADE ROLL";
                    }

                    coreShopShown |= title.text == "CORE SHOP";
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
            Assert.That(firstChoiceWasStat, Is.True, "The first post-wave choice was not a stat upgrade");
            Assert.That(coreShopShown, Is.True, "The alternating core shop choice was not shown");

            yield return null;
            Button continueButton = GameObject.Find("Continue")?.GetComponent<Button>();
            Assert.That(continueButton, Is.Not.Null, "The result loop button was not shown");
            continueButton.onClick.Invoke();
            elapsed = 0f;
            while (SceneManager.GetActiveScene().name != "GameplayScene" && elapsed < 20f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameplayScene"));
            elapsed = 0f;
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
