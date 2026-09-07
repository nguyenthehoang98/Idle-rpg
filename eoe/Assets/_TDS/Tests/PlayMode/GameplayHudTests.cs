using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace _TDS.Tests.PlayMode
{
    public sealed class GameplayHudTests
    {
        [UnityTest]
        public IEnumerator HudShowsWaveStatusGoldAndCircuitSlotDetails()
        {
            GameObject root = new GameObject("HudTestRoot");
            System.Type hudType = System.Type.GetType("_TDS.Gameplay.GameplayHud, Assembly-CSharp");
            Assert.That(hudType, Is.Not.Null);
            root.AddComponent(hudType);
            yield return null;

            Transform canvas = root.transform.GetChild(0);
            Assert.That(canvas.Find("SafeArea/TopBar/Wave"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/TopBar/Status"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/TopBar/Gold"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/Slots/Slot0/Index"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/Slots/Slot0/Stack"), Is.Not.Null);

            Object.Destroy(root);
            yield return null;
        }
    }
}
