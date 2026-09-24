using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using _TDS.Gameplay;

namespace _TDS.Tests.Editor
{
    public class RoundTimelineTests
    {
        [Test]
        public void AppendingSlidesLeftAndRecyclesTheOldestCircle()
        {
            GameObject root = new GameObject("bg", typeof(RectTransform));
            Sprite[] sprites = new Sprite[5];
            Texture2D[] textures = new Texture2D[sprites.Length];

            try
            {
                RectTransform[] circles = new RectTransform[4];
                UnityEngine.UI.Image[] icons = new UnityEngine.UI.Image[4];
                for (int i = 0; i < circles.Length; i++)
                {
                    GameObject circle = new GameObject($"circle{i}", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    circle.transform.SetParent(root.transform, false);
                    circles[i] = (RectTransform)circle.transform;
                    circles[i].anchoredPosition = new Vector2(-50 + i * 100, 0);
                    circle.SetActive(true);

                    GameObject icon = new GameObject("icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    icon.transform.SetParent(circle.transform, false);
                    icons[i] = icon.GetComponent<UnityEngine.UI.Image>();
                }

                UnityEngine.UI.Image[] fills = new UnityEngine.UI.Image[3];
                float[] linePositions = { -87.5f, 0f, 100f };
                for (int i = 0; i < fills.Length; i++)
                {
                    GameObject line = new GameObject($"line{i}", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    line.transform.SetParent(root.transform, false);
                    ((RectTransform)line.transform).anchoredPosition = new Vector2(linePositions[i], 0);
                    GameObject fill = new GameObject("fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                    fill.transform.SetParent(line.transform, false);
                    fills[i] = fill.GetComponent<UnityEngine.UI.Image>();
                    fills[i].type = UnityEngine.UI.Image.Type.Filled;
                    fills[i].fillAmount = 0f;
                }

                for (int i = 0; i < sprites.Length; i++)
                {
                    textures[i] = new Texture2D(1, 1);
                    sprites[i] = Sprite.Create(textures[i], new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                }

                RoundTimeline timeline = root.AddComponent<RoundTimeline>();
                SerializedObject serialized = new SerializedObject(timeline);
                serialized.FindProperty("slideDuration").floatValue = 0f;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                timeline.SetInitialItems(new[] { sprites[0], sprites[1], sprites[2] });
                Assert.That(circles[0].gameObject.activeSelf, Is.True);
                Assert.That(circles[1].gameObject.activeSelf, Is.True);
                Assert.That(circles[2].gameObject.activeSelf, Is.True);
                Assert.That(circles[3].gameObject.activeSelf, Is.False);
                timeline.SetProgress(0.75f);
                Assert.That(fills[0].fillAmount, Is.EqualTo(0.75f));

                Assert.That(timeline.AppendItem(sprites[3]), Is.True);
                Assert.That(circles[0].gameObject.activeSelf, Is.False);
                Assert.That(circles[0].anchoredPosition.x, Is.EqualTo(250f));
                Assert.That(icons[1].sprite, Is.SameAs(sprites[1]));
                Assert.That(icons[2].sprite, Is.SameAs(sprites[2]));
                Assert.That(icons[3].sprite, Is.SameAs(sprites[3]));
                Assert.That(circles[1].anchoredPosition.x, Is.EqualTo(-50f));
                Assert.That(fills[1].fillAmount, Is.EqualTo(0f));

                Assert.That(timeline.AppendItem(sprites[4]), Is.True);
                Assert.That(circles[1].gameObject.activeSelf, Is.False);
                Assert.That(icons[2].sprite, Is.SameAs(sprites[2]));
                Assert.That(icons[3].sprite, Is.SameAs(sprites[3]));
                Assert.That(icons[0].sprite, Is.SameAs(sprites[4]));
                Assert.That(circles[2].anchoredPosition.x, Is.EqualTo(-50f));
                Assert.That(circles[3].anchoredPosition.x, Is.EqualTo(50f));
                Assert.That(circles[0].anchoredPosition.x, Is.EqualTo(150f));
            }
            finally
            {
                Object.DestroyImmediate(root);
                foreach (Sprite sprite in sprites)
                    if (sprite != null) Object.DestroyImmediate(sprite);
                foreach (Texture2D texture in textures)
                    if (texture != null) Object.DestroyImmediate(texture);
            }
        }
    }
}
