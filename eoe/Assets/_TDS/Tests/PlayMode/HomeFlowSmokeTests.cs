using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace _TDS.Tests.PlayMode
{
    public sealed class HomeFlowSmokeTests
    {
        [UnityTest]
        public IEnumerator HomeSceneHasThreeWorkingTabs()
        {
            yield return SceneManager.LoadSceneAsync("HomeScene");
            yield return null;
            yield return null;

            GameObject homeObject = GameObject.Find("HomeScene");
            Component homeScene = homeObject?.GetComponent("HomeScene");
            Assert.That(homeScene, Is.Not.Null);
            Assert.That((int)homeScene.GetType().GetProperty("TabCount").GetValue(homeScene), Is.EqualTo(3));

            GameObject canvasObject = GameObject.Find("HomeCanvas");
            Assert.That(canvasObject, Is.Not.Null);
            Assert.That(canvasObject.GetComponent<Canvas>(), Is.Not.Null);
            foreach (Component component in canvasObject.GetComponents<Component>())
            {
                Assert.That(component, Is.Not.Null, "HomeCanvas contains a missing script.");
            }

            Transform content = canvasObject.transform.Find("TabScrollRect/Viewport/Content");
            Assert.That(content, Is.Not.Null);
            Assert.That(content.childCount, Is.EqualTo(3));
            Assert.That(content.GetChild(0).name, Is.EqualTo("ShopTab"));
            Assert.That(content.GetChild(1).name, Is.EqualTo("Main"));
            Assert.That(content.GetChild(2).name, Is.EqualTo("UpgradeTab"));

            RectTransform background = content.Find("Main/Background")?.GetComponent<RectTransform>();
            Assert.That(background, Is.Not.Null);
            Assert.That(background.rect.width, Is.EqualTo(3240f).Within(1f));

            Button play = content.Find("Main/Button Play")?.GetComponent<Button>();
            Assert.That(play, Is.Not.Null);
            Assert.That(play.interactable, Is.True);

            Button home = canvasObject.transform.Find("Bottom Menu/Button Home")?.GetComponent<Button>();
            Button shop = canvasObject.transform.Find("Bottom Menu/Button Shop")?.GetComponent<Button>();
            Button upgrade = canvasObject.transform.Find("Bottom Menu/Button Upgrade")?.GetComponent<Button>();
            Assert.That(home, Is.Not.Null);
            Assert.That(shop, Is.Not.Null);
            Assert.That(upgrade, Is.Not.Null);

            Transform tabScrollRect = canvasObject.transform.Find("TabScrollRect");
            Assert.That(tabScrollRect, Is.Not.Null);
            ScrollRect scrollRect = tabScrollRect.GetComponent<ScrollRect>();
            Assert.That(scrollRect, Is.Not.Null);
            Assert.That(scrollRect.horizontal, Is.True);
            Assert.That(scrollRect.vertical, Is.False);
            Assert.That(scrollRect.horizontalScrollbar, Is.Not.Null);
            Assert.That(scrollRect.content, Is.EqualTo(content.GetComponent<RectTransform>()));
            Assert.That(((RectTransform)content.GetChild(0)).rect.width, Is.EqualTo(scrollRect.viewport.rect.width).Within(1f));
            Assert.That(((RectTransform)content.GetChild(1)).rect.width, Is.EqualTo(scrollRect.viewport.rect.width).Within(1f));
            Assert.That(((RectTransform)content.GetChild(2)).rect.width, Is.EqualTo(scrollRect.viewport.rect.width).Within(1f));

            Component scrollSnap = tabScrollRect.GetComponent("ScrollSnap");
            Assert.That(scrollSnap, Is.Not.Null);
            float elapsed = 0f;
            while (GetSelectedIndex(scrollSnap) != 1 && elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(1));
            Assert.That(homeScene.GetType().GetProperty("CurrentTabName").GetValue(homeScene), Is.EqualTo("HOME"));

            shop.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(0));

            upgrade.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(2));

            home.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(1));

            SimulateSwipe(scrollRect, new Vector2(Screen.width * 0.75f, Screen.height * 0.5f),
                new Vector2(Screen.width * 0.25f, Screen.height * 0.5f));
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(2));
            Assert.That(scrollRect.horizontalNormalizedPosition, Is.EqualTo(1f).Within(0.01f));

            SimulateSwipe(scrollRect, new Vector2(Screen.width * 0.25f, Screen.height * 0.5f),
                new Vector2(Screen.width * 0.75f, Screen.height * 0.5f));
            yield return new WaitForSecondsRealtime(0.7f);
            Assert.That(GetSelectedIndex(scrollSnap), Is.EqualTo(1));
            Assert.That(scrollRect.horizontalNormalizedPosition, Is.EqualTo(0.5f).Within(0.01f));
        }

        private static void SimulateSwipe(ScrollRect scrollRect, Vector2 start, Vector2 end)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = start,
                pressPosition = start
            };

            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            eventData.position = end;
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }

        private static int GetSelectedIndex(Component scrollSnap)
        {
            return (int)scrollSnap.GetType().GetProperty("SelectedItemIndex").GetValue(scrollSnap);
        }
    }
}
