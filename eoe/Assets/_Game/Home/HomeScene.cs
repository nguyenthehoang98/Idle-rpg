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
            public ButtonHomeMenu button;
            public BaseTab tab;
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
        [SerializeField, Min(0f)] private float tabClickCooldown = 0.25f;

        private int currentPageIndex = -1;
        private float nextTabSelectionTime;

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
                    tab.tab.transform.parent != tabContainer ||
                    tab.tab.transform.GetSiblingIndex() != i)
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
            if (tab == null || tab.button == null || tab.tab == null)
            {
                Debug.LogWarning($"HomeScene: {name} tab needs ButtonHomeMenu and BaseTab references.");
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
            tab.button.Button.onClick.RemoveAllListeners();
            TabView capturedTab = tab;
            tab.button.Button.onClick.AddListener(() => SelectTab(capturedTab));
        }

        private void SelectTab(TabView tab)
        {
            int pageIndex = GetPageIndex(tab);
            if (pageIndex < 0 || pageIndex == currentPageIndex || Time.unscaledTime < nextTabSelectionTime)
            {
                return;
            }

            OpenTab(pageIndex);
            nextTabSelectionTime = Time.unscaledTime + tabClickCooldown;
        }

        private void OpenTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
            {
                return;
            }

            int previousPageIndex = currentPageIndex;
            currentPageIndex = index;
            for (int i = 0; i < tabs.Count; i++)
            {
                bool active = i == currentPageIndex;
                tabs[i].tab.gameObject.SetActive(active);
                SetTabVisuals(tabs[i], active);
                if (active)
                {
                    tabs[i].button.Focus();
                    tabs[i].tab.Open();
                }
                else if (i == previousPageIndex)
                {
                    tabs[i].button.Unfocus();
                }
            }
        }

        private void SetTabVisuals(TabView tab, bool active)
        {
            if (tab == null || tab.button == null)
            {
                return;
            }

            SetButtonColor(tab.button.Button, active);
            tab.button.Button.transform.localScale = active ? Vector3.one * activeScale : Vector3.one;
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
            if (tab?.tab == null || tab.tab.transform.parent != tabContainer)
            {
                return -1;
            }

            return tab.tab.transform.GetSiblingIndex();
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
            if (tab?.button == null)
            {
                return $"Tab {index}";
            }

            const string buttonPrefix = "Button ";
            string buttonName = tab.button.name;
            return buttonName.StartsWith(buttonPrefix)
                ? buttonName.Substring(buttonPrefix.Length)
                : buttonName;
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
