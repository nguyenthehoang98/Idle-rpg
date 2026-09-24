using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _TDS.Gameplay
{
    public sealed class RoundTimeline : MonoBehaviour
    {
        private const int VisibleItemCount = 3;
        private const int PoolItemCount = VisibleItemCount + 1;

        [SerializeField, Min(0f)] private float slideDuration = 0.25f;

        private readonly List<Marker> markers = new List<Marker>();
        private readonly List<Segment> segments = new List<Segment>();
        private Vector2 poolPosition;
        private Vector2 poolSegmentPosition;
        private float spacing;
        private Vector2[] markerStarts;
        private Vector2[] segmentStarts;
        private Vector2[] markerTargets;
        private Vector2[] segmentTargets;
        private Coroutine slideRoutine;
        private bool cached;

        private sealed class Marker
        {
            public RectTransform Rect;
            public UnityEngine.UI.Image Icon;
        }

        private sealed class Segment
        {
            public RectTransform Rect;
            public UnityEngine.UI.Image Fill;
        }

        private void Awake()
        {
            CacheElements();
        }

        /// <summary>Sets the three visible icons and resets their progress.</summary>
        public void SetInitialItems(IReadOnlyList<Sprite> icons)
        {
            if (icons == null) throw new ArgumentNullException(nameof(icons));
            if (icons.Count != VisibleItemCount)
                throw new ArgumentException($"Exactly {VisibleItemCount} initial icons are required.", nameof(icons));
            if (!CacheElements() || markers.Count < VisibleItemCount)
                throw new InvalidOperationException("RoundTimeline needs three circle objects with child Images named 'icon'.");

            for (int i = 0; i < VisibleItemCount; i++)
                if (icons[i] == null) throw new ArgumentException("Initial icons cannot be null.", nameof(icons));

            for (int i = 0; i < VisibleItemCount; i++)
            {
                markers[i].Icon.sprite = icons[i];
                markers[i].Rect.gameObject.SetActive(true);
            }

            for (int i = VisibleItemCount; i < markers.Count; i++)
                markers[i].Rect.gameObject.SetActive(false);
            foreach (Segment segment in segments) segment.Fill.fillAmount = 0f;
        }

        /// <summary>Completes the current segment and slides a new icon in from the right.</summary>
        /// <returns>False when the fourth, pooled circle or a line fill is missing.</returns>
        public bool AppendItem(Sprite icon)
        {
            if (icon == null) throw new ArgumentNullException(nameof(icon));
            if (!CacheElements() || markers.Count < PoolItemCount || segments.Count < VisibleItemCount)
            {
                Debug.LogError("RoundTimeline needs four circles (the fourth hidden) and three line fills before appending.", this);
                return false;
            }

            if (slideRoutine != null)
            {
                StopCoroutine(slideRoutine);
                slideRoutine = null;
                CompleteSlide();
            }

            Marker incoming = markers[VisibleItemCount];
            incoming.Rect.anchoredPosition = poolPosition;
            incoming.Icon.sprite = icon;
            incoming.Rect.gameObject.SetActive(true);
            segments[0].Fill.fillAmount = 1f;
            BeginSlide();

            if (slideDuration <= 0f)
            {
                ApplySlide(1f);
                CompleteSlide();
            }
            else
            {
                slideRoutine = StartCoroutine(AnimateSlide());
            }

            return true;
        }

        /// <summary>Sets progress for the leftmost visible item, clamped to 0..1.</summary>
        public void SetProgress(float progress)
        {
            if (!CacheElements() || segments.Count < VisibleItemCount) return;
            segments[0].Fill.fillAmount = Mathf.Clamp01(progress);
        }

        private bool CacheElements()
        {
            if (cached) return true;

            markers.Clear();
            segments.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("circle", StringComparison.OrdinalIgnoreCase))
                {
                    Transform icon = child.Find("icon");
                    UnityEngine.UI.Image iconImage = icon != null ? icon.GetComponent<UnityEngine.UI.Image>() : null;
                    UnityEngine.UI.Image circleImage = child.GetComponent<UnityEngine.UI.Image>();
                    RectTransform rect = child as RectTransform;
                    if (rect != null && circleImage != null && iconImage != null)
                    {
                        circleImage.raycastTarget = false;
                        iconImage.raycastTarget = false;
                        markers.Add(new Marker { Rect = rect, Icon = iconImage });
                    }
                }
                else if (child.name.StartsWith("line", StringComparison.OrdinalIgnoreCase))
                {
                    Transform fill = child.Find("fill");
                    UnityEngine.UI.Image fillImage = fill != null ? fill.GetComponent<UnityEngine.UI.Image>() : null;
                    RectTransform rect = child as RectTransform;
                    if (rect != null && fillImage != null)
                    {
                        UnityEngine.UI.Image lineImage = child.GetComponent<UnityEngine.UI.Image>();
                        if (lineImage != null) lineImage.raycastTarget = false;
                        fillImage.raycastTarget = false;
                        segments.Add(new Segment { Rect = rect, Fill = fillImage });
                    }
                }
            }

            markers.Sort((a, b) =>
            {
                int activeOrder = b.Rect.gameObject.activeSelf.CompareTo(a.Rect.gameObject.activeSelf);
                return activeOrder != 0
                    ? activeOrder
                    : a.Rect.anchoredPosition.x.CompareTo(b.Rect.anchoredPosition.x);
            });
            segments.Sort((a, b) => a.Rect.anchoredPosition.x.CompareTo(b.Rect.anchoredPosition.x));

            if (markers.Count > PoolItemCount)
            {
                for (int i = PoolItemCount; i < markers.Count; i++) markers[i].Rect.gameObject.SetActive(false);
                markers.RemoveRange(PoolItemCount, markers.Count - PoolItemCount);
            }
            if (segments.Count > VisibleItemCount)
            {
                for (int i = VisibleItemCount; i < segments.Count; i++) segments[i].Rect.gameObject.SetActive(false);
                segments.RemoveRange(VisibleItemCount, segments.Count - VisibleItemCount);
            }

            if (markers.Count < VisibleItemCount || segments.Count < VisibleItemCount)
            {
                Debug.LogError("RoundTimeline requires three circle objects and three line objects with child Images named 'fill'.", this);
                return false;
            }

            spacing = markers[1].Rect.anchoredPosition.x - markers[0].Rect.anchoredPosition.x;
            if (spacing <= 0f)
            {
                Debug.LogError("RoundTimeline circles must be arranged left to right with non-zero spacing.", this);
                return false;
            }

            if (markers.Count >= PoolItemCount)
            {
                poolPosition = markers[VisibleItemCount - 1].Rect.anchoredPosition + Vector2.right * spacing;
                markers[PoolItemCount - 1].Rect.anchoredPosition = poolPosition;
                markers[PoolItemCount - 1].Rect.gameObject.SetActive(false);
            }
            poolSegmentPosition = segments[VisibleItemCount - 1].Rect.anchoredPosition;
            foreach (Segment segment in segments) segment.Fill.fillAmount = 0f;
            cached = true;
            return true;
        }

        private void BeginSlide()
        {
            markerStarts = new Vector2[markers.Count];
            markerTargets = new Vector2[markers.Count];
            for (int i = 0; i < markers.Count; i++)
            {
                markerStarts[i] = markers[i].Rect.anchoredPosition;
                markerTargets[i] = markerStarts[i] + Vector2.left * spacing;
            }

            segmentStarts = new Vector2[segments.Count];
            segmentTargets = new Vector2[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                segmentStarts[i] = segments[i].Rect.anchoredPosition;
                segmentTargets[i] = segmentStarts[i] + Vector2.left * spacing;
            }
        }

        private IEnumerator AnimateSlide()
        {
            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideDuration);
                ApplySlide(t * t * (3f - 2f * t));
                yield return null;
            }

            CompleteSlide();
            slideRoutine = null;
        }

        private void ApplySlide(float progress)
        {
            for (int i = 0; i < markers.Count; i++)
                markers[i].Rect.anchoredPosition = Vector2.Lerp(markerStarts[i], markerTargets[i], progress);
            for (int i = 0; i < segments.Count; i++)
                segments[i].Rect.anchoredPosition = Vector2.Lerp(segmentStarts[i], segmentTargets[i], progress);
        }

        private void CompleteSlide()
        {
            ApplySlide(1f);

            Marker recycledMarker = markers[0];
            markers.RemoveAt(0);
            markers.Add(recycledMarker);
            recycledMarker.Rect.anchoredPosition = poolPosition;
            recycledMarker.Rect.gameObject.SetActive(false);

            Segment recycledSegment = segments[0];
            segments.RemoveAt(0);
            segments.Add(recycledSegment);
            recycledSegment.Rect.anchoredPosition = poolSegmentPosition;
            recycledSegment.Fill.fillAmount = 0f;
        }
    }
}
