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
    public sealed class CampaignPlayModeTests
    {
        [UnityTest]
        public IEnumerator LevelTwentyLoadsAndStartsCombat()
        {
            Type runSelection = Type.GetType("_TDS.Gameplay.RunSelection, Assembly-CSharp");
            Assert.That(runSelection, Is.Not.Null);
            runSelection.GetMethod("SelectLevel", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { 20 });
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
            string status = null;
            while (elapsed < 60f)
            {
                status = GameObject.Find("Status")?.GetComponent<Text>()?.text;
                if (status == "FIGHTING" || status == "CHOOSE UPGRADE") break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(status == "FIGHTING" || status == "CHOOSE UPGRADE", Is.True,
                $"Level 20 did not start combat. Last status: {status}");
        }
    }
}
