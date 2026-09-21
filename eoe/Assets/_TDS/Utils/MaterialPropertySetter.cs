using UnityEngine;

namespace _TDS.Utils
{
    /// <summary>
    /// API công khai để chỉnh material property theo tên, kèm chỉnh sẵn cho SpriteRenderer.
    /// Dùng Shader.PropertyToID và MaterialPropertyBlock nên không tạo material instance.
    /// </summary>
    public sealed class MaterialPropertySetter : MonoBehaviour
    {
        [SerializeField] private Renderer target;
        [SerializeField] private string propertyName = "_HitEffectBlend";

        private MaterialPropertyBlock block;
        private int propertyId;

        private Renderer Target => target != null ? target : GetComponent<Renderer>();
        private MaterialPropertyBlock Block => block ??= new MaterialPropertyBlock();
        private int PropertyId => propertyId != 0 ? propertyId : (propertyId = Shader.PropertyToID(propertyName));

        public void SetFloat(string name, float value) => SetFloat(Shader.PropertyToID(name), value);
        public void SetColor(string name, Color value) => SetColor(Shader.PropertyToID(name), value);
        public void SetVector(string name, Vector4 value) => SetVector(Shader.PropertyToID(name), value);
        public void SetInt(string name, int value) => SetInteger(Shader.PropertyToID(name), value);

        public void SetFloat(int id, float value)
        {
            if (!TryRead()) return;
            Block.SetFloat(id, value);
            Apply();
        }

        public void SetColor(int id, Color value)
        {
            if (!TryRead()) return;
            Block.SetColor(id, value);
            Apply();
        }

        public void SetVector(int id, Vector4 value)
        {
            if (!TryRead()) return;
            Block.SetVector(id, value);
            Apply();
        }

        public void SetInteger(int id, int value)
        {
            if (!TryRead()) return;
            Block.SetInteger(id, value);
            Apply();
        }

        /// <summary>Ghi property đã cấu hình trong Inspector.</summary>
        public void SetFloat(float value) => SetFloat(PropertyId, value);

        public float GetFloat(string name) => GetFloat(Shader.PropertyToID(name));
        public Color GetColor(string name) => GetColor(Shader.PropertyToID(name));

        public float GetFloat(int id) => TryRead() ? Block.GetFloat(id) : 0f;

        public Color GetColor(int id) => TryRead() ? Block.GetColor(id) : default;

        /// <summary>Xoá mọi property đã set, quay về material gốc.</summary>
        public void Clear()
        {
            Renderer renderer = Target;
            if (renderer == null) return;

            block = null;
            renderer.SetPropertyBlock(null);
        }

        private bool TryRead()
        {
            Renderer renderer = Target;
            if (renderer == null) return false;

            renderer.GetPropertyBlock(Block);
            return true;
        }

        private void Apply()
        {
            Target?.SetPropertyBlock(Block);
        }
    }
}
