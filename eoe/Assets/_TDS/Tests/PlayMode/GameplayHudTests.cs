using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace _TDS.Tests.PlayMode
{
    public sealed class GameplayHudTests
    {
        [UnityTest]
        public IEnumerator HudBindsWidgetsFromTheCanvasTemplate()
        {
            GameObject root = new GameObject("HudTestRoot");
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(root.transform, false);
            canvasObject.AddComponent<Canvas>();
            GameObject safeArea = new GameObject("SafeArea", typeof(RectTransform));
            safeArea.transform.SetParent(canvasObject.transform, false);
            GameObject wave = new GameObject("Wave", typeof(RectTransform));
            wave.transform.SetParent(safeArea.transform, false);
            Text waveText = wave.AddComponent<Text>();

            System.Type hudType = System.Type.GetType("_TDS.Gameplay.GameplayHud, Assembly-CSharp");
            Assert.That(hudType, Is.Not.Null);
            Component hud = root.AddComponent(hudType);
            yield return null;

            Assert.That((bool)hudType.GetProperty("enabled").GetValue(hud), Is.True,
                "HUD must stay enabled when the Canvas exposes its widgets");
            hudType.GetMethod("SetWave").Invoke(hud, new object[] { 7 });
            Assert.That(waveText.text, Is.EqualTo("WAVE 7"));

            Object.Destroy(root);
            yield return null;
        }
    }
}
