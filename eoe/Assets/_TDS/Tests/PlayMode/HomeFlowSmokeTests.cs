using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace _TDS.Tests.PlayMode
{
    public sealed class HomeFlowSmokeTests
    {
        [UnityTest]
        public IEnumerator HomeSceneHasFiveWorkingTabs()
        {
            yield return SceneManager.LoadSceneAsync("HomeScene");
            yield return null;
            yield return null;

            GameObject homeObject = GameObject.Find("HomeScene");
            Component homeScene = homeObject?.GetComponent("HomeScene");
            Assert.That(homeScene, Is.Not.Null);
            Assert.That((int)homeScene.GetType().GetProperty("TabCount").GetValue(homeScene), Is.EqualTo(5));

            GameObject canvasObject = GameObject.Find("HomeCanvas");
            Assert.That(canvasObject, Is.Not.Null);
            Assert.That(canvasObject.GetComponent<Canvas>(), Is.Not.Null);
            foreach (Component component in canvasObject.GetComponents<Component>())
            {
                Assert.That(component, Is.Not.Null, "HomeCanvas contains a missing script.");
            }

            Transform content = canvasObject.transform.Find("TabContainer/Viewport/Content");
            Assert.That(content, Is.Not.Null);
            Assert.That(content.childCount, Is.EqualTo(5));
            Assert.That(content.GetChild(0).name, Is.EqualTo("ShopTab"));
            Assert.That(content.GetChild(1).name, Is.EqualTo("EquipmentTab"));
            Assert.That(content.GetChild(2).name, Is.EqualTo("Main"));
            Assert.That(content.GetChild(3).name, Is.EqualTo("UpgradeTab"));
            Assert.That(content.GetChild(4).name, Is.EqualTo("PetTab"));
            foreach (Transform tab in content)
            {
                Image placeholder = tab.GetComponent<Image>();
                Assert.That(placeholder, Is.Not.Null);
                Assert.That(placeholder.sprite, Is.Null);
            }

            Button shop = canvasObject.transform.Find("Bottom Menu/Button Shop")?.GetComponent<Button>();
            Button equipment = canvasObject.transform.Find("Bottom Menu/Button Equipment")?.GetComponent<Button>();
            Button battle = canvasObject.transform.Find("Bottom Menu/Button Battle")?.GetComponent<Button>();
            Button talent = canvasObject.transform.Find("Bottom Menu/Button Talent")?.GetComponent<Button>();
            Button pet = canvasObject.transform.Find("Bottom Menu/Button Pet")?.GetComponent<Button>();
            Assert.That(shop, Is.Not.Null);
            Assert.That(equipment, Is.Not.Null);
            Assert.That(battle, Is.Not.Null);
            Assert.That(talent, Is.Not.Null);
            Assert.That(pet, Is.Not.Null);

            Component equipmentTab = content.GetChild(1).GetComponent("BaseTab");
            Assert.That(equipmentTab, Is.Not.Null);
            UnityEvent onOpened = (UnityEvent)equipmentTab.GetType().GetProperty("OnOpened").GetValue(equipmentTab);
            int openedCount = 0;
            onOpened.AddListener(() => openedCount++);

            AssertOnlyTabActive(content, 2);
            Assert.That(homeScene.GetType().GetProperty("CurrentTabName").GetValue(homeScene), Is.EqualTo("BATTLE"));

            shop.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 0);

            equipment.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 1);
            Assert.That(openedCount, Is.EqualTo(1));

            battle.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 2);

            talent.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 3);

            pet.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 4);

            battle.onClick.Invoke();
            yield return null;
            AssertOnlyTabActive(content, 2);
        }

        private static void AssertOnlyTabActive(Transform content, int selectedIndex)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Assert.That(content.GetChild(i).gameObject.activeSelf, Is.EqualTo(i == selectedIndex));
            }
        }
    }
}
