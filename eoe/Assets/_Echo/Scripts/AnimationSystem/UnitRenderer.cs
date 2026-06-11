using System.Collections;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    public class UnitRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material hdrMaterial;
        [SerializeField, ColorUsage(true, true)] private Color hdrColor;
        
        private MaterialPropertyBlock mainTexturePropertyBlock;
        private MaterialPropertyBlock hdrTexturePropertyBlock;
        private MaterialPropertyBlock hdrColorPropertyBlock;
        private MaterialPropertyBlock saturationPropertyBlock;

        private Texture defaultTexture;
        private Texture hdrTexture;
        private bool isDefault = true;

        private void Awake()
        {
            saturationPropertyBlock = new MaterialPropertyBlock();
            mainTexturePropertyBlock = new MaterialPropertyBlock();
            hdrTexturePropertyBlock = new MaterialPropertyBlock();
            hdrColorPropertyBlock = new MaterialPropertyBlock();
            
            Debug.Log(@"Khi tấn công thì mới bật HDR cho đẹp. active thì chỉnh Saturation thôi");
        }

        public void Activate(float duration)
        {
            SetTexture(defaultTexture, hdrTexture, isDefault);
            StartCoroutine(SaturationEnumerator(1f, duration));
        }

        public void Deactivate(float duration)
        {
            bool previous = isDefault;
            SetTexture(defaultTexture, hdrTexture, true);
            StartCoroutine(SaturationEnumerator(0f, duration));
            isDefault = previous;
        }

        private IEnumerator SaturationEnumerator(float target, float duration)
        {
            float elapsedTime = 0;

            spriteRenderer.GetPropertyBlock(saturationPropertyBlock);

            float value = saturationPropertyBlock.GetFloat("_Saturation");

            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                
                float f = Mathf.Lerp(value, target, Mathf.Clamp01(elapsedTime / duration));
                
                saturationPropertyBlock.SetFloat("_Saturation", f);
                
                spriteRenderer.SetPropertyBlock(saturationPropertyBlock);
                
                yield return null;
            }
        }

        public void SetTexture(Texture defaultTexture, Texture hdrTexture, bool isDefault = false)
        {
            this.defaultTexture = defaultTexture;
            this.hdrTexture = hdrTexture;
            this.isDefault = isDefault;
            
            spriteRenderer.sharedMaterial = isDefault ? defaultMaterial : hdrMaterial;
            
            if(hdrTexture != null && !isDefault)
            {
                spriteRenderer.GetPropertyBlock(mainTexturePropertyBlock);
                mainTexturePropertyBlock.SetTexture("_MainTex", defaultTexture);
                spriteRenderer.SetPropertyBlock(mainTexturePropertyBlock);

                spriteRenderer.GetPropertyBlock(hdrTexturePropertyBlock);
                hdrTexturePropertyBlock.SetTexture("_GlowTex", hdrTexture);
                spriteRenderer.SetPropertyBlock(hdrTexturePropertyBlock);

                spriteRenderer.GetPropertyBlock(hdrColorPropertyBlock);
                hdrColorPropertyBlock.SetColor("_GlowColor", hdrColor);
                spriteRenderer.SetPropertyBlock(hdrColorPropertyBlock);
            }
        }
    }
}