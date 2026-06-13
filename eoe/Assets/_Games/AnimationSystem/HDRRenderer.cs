using System.Collections;
using UnityEngine;

namespace _Games.AnimationSystem
{
    public class HDRRenderer : MonoBehaviour
    {
        private static readonly int GlowTex = Shader.PropertyToID("_GlowTex");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material hdrMaterial;
        [SerializeField, ColorUsage(true, true)] private Color hdrColor;
        
        private MaterialPropertyBlock propertyBlock;

        private Texture normalTexture;
        private Texture hdrTexture;
        private bool isDefault = true;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        public void Activate(float duration)
        {
            SetTexture(normalTexture, hdrTexture, isDefault);
            StartCoroutine(SaturationEnumerator(1f, duration));
        }

        public void Deactivate(float duration)
        {
            bool previous = isDefault;
            SetTexture(normalTexture, hdrTexture, true);
            StartCoroutine(SaturationEnumerator(0f, duration));
            isDefault = previous;
        }

        private IEnumerator SaturationEnumerator(float target, float duration)
        {
            float elapsedTime = 0;

            spriteRenderer.GetPropertyBlock(propertyBlock);

            float value = propertyBlock.GetFloat("_Saturation");

            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                
                float f = Mathf.Lerp(value, target, Mathf.Clamp01(elapsedTime / duration));
                
                propertyBlock.SetFloat("_Saturation", f);
                
                spriteRenderer.SetPropertyBlock(propertyBlock);
                
                yield return null;
            }
        }

        public void SetTexture(Texture normalTexture, Texture hdrTexture, bool isDefault = false)
        {
            this.normalTexture = normalTexture;
            this.hdrTexture = hdrTexture;
            this.isDefault = isDefault;
            
            spriteRenderer.sharedMaterial = isDefault ? normalMaterial : hdrMaterial;

            if(hdrTexture != null && !isDefault)
            {
                spriteRenderer.GetPropertyBlock(propertyBlock);
                
                propertyBlock.SetTexture(MainTex, normalTexture);
                propertyBlock.SetTexture(GlowTex, hdrTexture);
                propertyBlock.SetColor(GlowColor, hdrColor);
                
                spriteRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}