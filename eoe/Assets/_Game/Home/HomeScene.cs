using System;
using System.Collections;
using System.Collections.Generic;
using _GameToolkit.Startup;
using _TDS.Gameplay;
using LightScrollSnap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Game.Home
{
    /// <summary>
    /// Binds Home tab buttons to the horizontal ScrollSnap page controller.
    /// Add a Tab entry and a matching child under ScrollSnap/Viewport/Content to extend the Home UI.
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
            public GameObject focus;
        }

        [Header("Home UI References")]
        [SerializeField] private Canvas homeCanvas;
        [SerializeField] private Button playButton;
        [SerializeField] private ScrollRect tabScrollRect;
        [SerializeField] private ScrollSnap tabScrollSnap;

        [Header("Tabs")]
        [Tooltip("Keep the same order as the direct children of ScrollSnap/Viewport/Content.")]
        [SerializeField] private List<TabView> tabs = new List<TabView>();
        [SerializeField, Min(0)] private int initialTabIndex = 1;
        [SerializeField] private float activeScale = 1.05f;
        [SerializeField] private float tabTransitionDuration = 0.3f;

        private int currentPageIndex;
        private bool initialTabPositionPending;

        public int TabCount => tabs?.Count ?? 0;
        public string CurrentTabName => GetTabName(currentPageIndex);
        public bool IsTransitioning => tabScrollSnap != null &&
            tabScrollSnap.SelectedItemIndex != currentPageIndex;

        private void Awake()
        {
            if (tabs == null)
            {
                tabs = new List<TabView>();
            }

            if (!Application.isPlaying || !HasRequiredReferences())
            {
                if (Application.isPlaying && tabScrollSnap != null)
                {
                    tabScrollSnap.enabled = false;
                }

                return;
            }

            homeCanvas.transform.localScale = Vector3.one;
            EnsureEventSystem();
            BindPlayButton();
            for (int i = 0; i < tabs.Count; i++)
            {
                BindTab(tabs[i]);
            }

            currentPageIndex = GetPageIndex(tabs[Mathf.Clamp(initialTabIndex, 0, tabs.Count - 1)]);
            SetAllContentActive();
            SetTabVisuals(currentPageIndex);
        }

        private void Start()
        {
            if (!Application.isPlaying || tabScrollSnap == null || !tabScrollSnap.enabled ||
                tabs == null || tabs.Count == 0)
            {
                return;
            }

            StartCoroutine(InitializeTabs());
        }

        private IEnumerator InitializeTabs()
        {
            yield return null;

            Canvas.ForceUpdateCanvases();
            ConfigurePageWidths();
            Canvas.ForceUpdateCanvases();
            tabScrollSnap.OnItemSelected.RemoveListener(HandleTabSelected);
            tabScrollSnap.OnItemSelected.AddListener(HandleTabSelected);
            currentPageIndex = GetPageIndex(tabs[Mathf.Clamp(initialTabIndex, 0, tabs.Count - 1)]);
            initialTabPositionPending = currentPageIndex >= 0;
        }

        private void LateUpdate()
        {
            if (!initialTabPositionPending || tabScrollSnap == null ||
                !TryGetScrollRect(out ScrollRect scrollRect))
            {
                return;
            }

            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;
            if (content == null || viewport == null || viewport.rect.width <= 0f)
            {
                return;
            }

            float initialPosition = tabScrollSnap.GetScrollPositionOfItem(currentPageIndex);
            float maxOffset = Mathf.Max(0f, content.rect.width - viewport.rect.width);

            tabScrollSnap.ScrollToItem(currentPageIndex);
            content.anchoredPosition = new Vector2(-maxOffset * initialPosition, content.anchoredPosition.y);
            scrollRect.horizontalNormalizedPosition = initialPosition;
            initialTabPositionPending = false;
        }

        private void ConfigurePageWidths()
        {
            if (!TryGetScrollRect(out ScrollRect scrollRect))
            {
                return;
            }

            RectTransform viewport = scrollRect.viewport;
            RectTransform content = scrollRect.content;
            if (viewport == null || content == null || viewport.rect.width <= 0f)
            {
                return;
            }

            float pageWidth = viewport.rect.width;
            for (int i = 0; i < content.childCount; i++)
            {
                RectTransform page = content.GetChild(i) as RectTransform;
                page?.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pageWidth);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private bool HasRequiredReferences()
        {
            bool valid = homeCanvas != null && playButton != null && tabScrollSnap != null && tabs.Count > 0;
            if (!valid)
            {
                Debug.LogError(
                    $"{nameof(HomeScene)}: assign Canvas, Play button, ScrollSnap, and at least one tab.",
                    this);
                return false;
            }

            if (!TryGetScrollRect(out ScrollRect scrollRect) || scrollRect.content == null ||
                scrollRect.viewport == null || scrollRect.horizontalScrollbar == null)
            {
                Debug.LogError(
                    $"{nameof(HomeScene)}: ScrollSnap needs a ScrollRect with Content, Viewport, and " +
                    "Horizontal Scrollbar.", this);
                return false;
            }

            if (scrollRect.content.childCount != tabs.Count)
            {
                Debug.LogError(
                    $"{nameof(HomeScene)}: configured {tabs.Count} tabs but Content has " +
                    $"{scrollRect.content.childCount} pages.", this);
                return false;
            }

            for (int i = 0; i < tabs.Count; i++)
            {
                TabView tab = tabs[i];
                string tabName = GetTabName(tab, i);
                if (!IsValidTab(tab, tabName) ||
                    tab.content.transform.parent != scrollRect.content ||
                    tab.content.transform.GetSiblingIndex() != i)
                {
                    Debug.LogError(
                        $"{nameof(HomeScene)}: tab '{tabName}' must reference Content child {i}.",
                        this);
                    valid = false;
                }
            }

            return valid;
        }

        private bool TryGetScrollRect(out ScrollRect scrollRect)
        {
            scrollRect = tabScrollRect;
            return scrollRect != null;
        }

        private static bool IsValidTab(TabView tab, string name)
        {
            if (tab == null || tab.button == null || tab.content == null)
            {
                Debug.LogWarning($"HomeScene: {name} tab needs Button and Content references.");
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
            if (tabScrollSnap == null || pageIndex < 0)
            {
                return;
            }

            tabScrollSnap.SmoothScrollToItem(pageIndex, Mathf.Max(0.01f, tabTransitionDuration));
        }

        private void HandleTabSelected(RectTransform _, int index)
        {
            if (index < 0 || index >= tabs.Count)
            {
                return;
            }

            currentPageIndex = index;
            SetAllContentActive();
            SetTabVisuals(currentPageIndex);
        }

        private void SetAllContentActive()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i]?.content != null)
                {
                    tabs[i].content.SetActive(true);
                }
            }
        }

        private void SetTabVisuals(int activePageIndex)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                SetTabVisuals(tabs[i], i == activePageIndex);
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
            if (tab?.content == null || !TryGetScrollRect(out ScrollRect scrollRect) ||
                scrollRect.content == null || tab.content.transform.parent != scrollRect.content)
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
