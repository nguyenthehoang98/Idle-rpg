using System;
using System.Collections.Generic;
using _GameToolkit.Startup;
using _TDS.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Game.Home
{
    /// <summary>
    /// Binds Home tab buttons to stacked tab pages.
    /// Each page receives a BaseTab open event when selected.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class HomeScene : MonoBehaviour
    {
        private const string GameplaySceneName = "GamePlayScene";

        private static readonly Color ActiveTabColor = ParseColor("5EEAD4");
        private static readonly Color InactiveTabColor = ParseColor("FFFFFF");

        [Serializable]
        private sealed class TabView
        {
            [Tooltip("Optional name used by CurrentTabName. The page GameObject name is used when empty.")]
            public string id;
            public Button button;
            public GameObject content;
            public BaseTab tab;
            public GameObject focus;
        }

        [Header("Home UI References")]
        [SerializeField] private Canvas homeCanvas;
        [SerializeField] private Button playButton;
        [SerializeField] private RectTransform tabContainer;

        [Header("Tabs")]
        [Tooltip("Keep the same order as the direct children of the tab container.")]
        [SerializeField] private List<TabView> tabs = new List<TabView>();
        [SerializeField, Min(0)] private int initialTabIndex = 1;
        [SerializeField] private float activeScale = 1.05f;

        private int currentPageIndex = -1;

        public int TabCount => tabs?.Count ?? 0;
        public string CurrentTabName => GetTabName(currentPageIndex);

        private void Awake()
        {
            if (tabs == null)
            {
                tabs = new List<TabView>();
            }

            if (!Application.isPlaying || !HasRequiredReferences())
            {
                return;
            }

            homeCanvas.transform.localScale = Vector3.one;
            EnsureEventSystem();
            BindPlayButton();
            for (int i = 0; i < tabs.Count; i++)
            {
                BindTab(tabs[i]);
            }

            OpenTab(Mathf.Clamp(initialTabIndex, 0, tabs.Count - 1));
        }

        private bool HasRequiredReferences()
        {
            bool valid = homeCanvas != null && playButton != null && tabContainer != null && tabs.Count > 0;
            if (!valid)
            {
                Debug.LogError(
                    $"{nameof(HomeScene)}: assign Canvas, Play Button, tab container, and at least one tab.",
                    this);
                return false;
            }

            if (tabContainer.childCount != tabs.Count)
            {
                Debug.LogError(
                    $"{nameof(HomeScene)}: configured {tabs.Count} tabs but the container has " +
                    $"{tabContainer.childCount} pages.", this);
                return false;
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                TabView tab = tabs[i];
                string tabName = GetTabName(tab, i);
                if (!IsValidTab(tab, tabName) ||
                    tab.content.transform.parent != tabContainer ||
                    tab.content.transform.GetSiblingIndex() != i)
                {
                    Debug.LogError(
                        $"{nameof(HomeScene)}: tab '{tabName}' must reference tab container child {i}.",
                        this);
                    valid = false;
                }
            }

            return valid;
        }

        private static bool IsValidTab(TabView tab, string name)
        {
            if (tab == null || tab.button == null || tab.content == null || tab.tab == null)
            {
                Debug.LogWarning($"HomeScene: {name} tab needs Button, Content, and BaseTab references.");
                return false;
            }

            return true;
        }

        private void BindPlayButton()
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGameplay);
        }

        private void BindTab(TabView tab)
        {
            tab.button.onClick.RemoveAllListeners();
            TabView capturedTab = tab;
            tab.button.onClick.AddListener(() => SelectTab(capturedTab));
        }

        private void SelectTab(TabView tab)
        {
            int pageIndex = GetPageIndex(tab);
            if (pageIndex >= 0 && pageIndex != currentPageIndex)
            {
                OpenTab(pageIndex);
            }
        }

        private void OpenTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
            {
                return;
            }

            currentPageIndex = index;
            for (int i = 0; i < tabs.Count; i++)
            {
                bool active = i == currentPageIndex;
                tabs[i].content.SetActive(active);
                SetTabVisuals(tabs[i], active);
                if (active)
                {
                    tabs[i].tab.Open();
                }
            }
        }

        private void SetTabVisuals(TabView tab, bool active)
        {
            if (tab == null || tab.button == null)
            {
                return;
            }

            SetFocus(tab, active);
            SetButtonColor(tab.button, active);
            tab.button.transform.localScale = active ? Vector3.one * activeScale : Vector3.one;
        }

        private static void SetFocus(TabView tab, bool active)
        {
            if (tab.focus != null)
            {
                tab.focus.SetActive(active);
            }
        }

        private static void SetButtonColor(Button button, bool active)
        {
            Image image = button.targetGraphic as Image;
            if (image != null)
            {
                image.color = active ? ActiveTabColor : InactiveTabColor;
            }
        }

        private int GetPageIndex(TabView tab)
        {
            if (tab?.content == null || tab.content.transform.parent != tabContainer)
            {
                return -1;
            }

            return tab.content.transform.GetSiblingIndex();
        }

        private string GetTabName(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < tabs.Count)
            {
                return GetTabName(tabs[pageIndex], pageIndex);
            }

            return string.Empty;
        }

        private static string GetTabName(TabView tab, int index)
        {
            if (tab == null)
            {
                return $"Tab {index}";
            }

            if (string.IsNullOrWhiteSpace(tab.id))
            {
                return tab.content != null ? tab.content.name : $"Tab {index}";
            }

            return tab.id;
        }

        private void StartGameplay()
        {
            RunSelection.SelectLevel(RunSelection.SelectedLevel);

            if (BootScene.Instance == null)
            {
                SceneManager.LoadScene(GameplaySceneName);
                return;
            }

            BootScene.Instance.LoadSceneAsync(GameplaySceneName);
            BootScene.Instance.CloseLoadingScene();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static Color ParseColor(string html)
        {
            return ColorUtility.TryParseHtmlString($"#{html}", out Color color) ? color : Color.white;
        }
    }
}
