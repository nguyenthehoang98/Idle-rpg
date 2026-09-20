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
            Assert.That(canvas.Find("SafeArea/CircuitPanel/Energy"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/CircuitControls/ModeButton"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/CircuitControls/RollButton"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/RollResult"), Is.Not.Null);
            Assert.That(canvas.Find("SafeArea/CircuitPanel/Slots/Slot0/Index"), Is.Not.Null);

            Component hud = root.GetComponent(hudType);
            System.Type contentType = System.Type.GetType("_TDS.Battle.CircuitSlotContent, Assembly-CSharp");
            System.Type rollType = System.Type.GetType("_TDS.Battle.CircuitRollEvent, Assembly-CSharp");
            object emptyContent = System.Activator.CreateInstance(contentType);
            object roll = System.Activator.CreateInstance(rollType, 0, 3, 3, emptyContent);
            hudType.GetMethod("ShowRoll").Invoke(hud, new[] { roll });
            Assert.That(canvas.Find("SafeArea/CircuitPanel/RollResult").GetComponent<Text>().text,
                Is.EqualTo("ROLL +3  →  04"));

            Object.Destroy(root);
            yield return null;
        }
    }
}
