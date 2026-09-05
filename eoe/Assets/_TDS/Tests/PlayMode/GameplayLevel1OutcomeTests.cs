using System.Collections;
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
            while (elapsed < 90f)
            {
                Text statusText = GameObject.Find("Status")?.GetComponent<Text>();
                status = statusText?.text;
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
            Assert.That(outcome == "VICTORY" || outcome == "DEFEAT", Is.True);
        }
    }
}
