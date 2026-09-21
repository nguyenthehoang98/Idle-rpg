using NUnit.Framework;
using UnityEngine;
using _TDS.Utils;

namespace _TDS.Tests.Editor
{
    /// <summary>
    /// MaterialPropertySetter ghi property qua MaterialPropertyBlock (không tạo material instance).
    /// </summary>
    public class MaterialPropertySetterTests
    {
        private GameObject gameObject;
        private SpriteRenderer renderer;
        private MaterialPropertySetter setter;
        private MaterialPropertyBlock block;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("MaterialPropertySetterTests");
            renderer = gameObject.AddComponent<SpriteRenderer>();
            setter = gameObject.AddComponent<MaterialPropertySetter>();
            block = new MaterialPropertyBlock();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        private float Read(string name)
        {
            renderer.GetPropertyBlock(block);
            return block.GetFloat(name);
        }

        [Test]
        public void SetFloatWritesToPropertyBlock()
        {
            setter.SetFloat("_HitEffectBlend", 0.75f);

            Assert.That(Read("_HitEffectBlend"), Is.EqualTo(0.75f));
            Assert.That(setter.GetFloat("_HitEffectBlend"), Is.EqualTo(0.75f));
        }

        [Test]
        public void SetColorWritesToPropertyBlock()
        {
            setter.SetColor("_HitEffectColor", Color.red);

            renderer.GetPropertyBlock(block);
            Assert.That(block.GetColor("_HitEffectColor"), Is.EqualTo(Color.red));
        }

        [Test]
        public void ClearRestoresPlainMaterial()
        {
            setter.SetFloat("_HitEffectBlend", 0.75f);

            setter.Clear();

            Assert.That(Read("_HitEffectBlend"), Is.EqualTo(0f), "Sau Clear phải quay về material gốc");
        }

        [Test]
        public void MultiplePropertiesKeepTheirValues()
        {
            setter.SetFloat("_HitEffectBlend", 0.25f);
            setter.SetColor("_HitEffectColor", Color.green);

            renderer.GetPropertyBlock(block);
            Assert.That(block.GetFloat("_HitEffectBlend"), Is.EqualTo(0.25f));
            Assert.That(block.GetColor("_HitEffectColor"), Is.EqualTo(Color.green));
        }
    }
}
