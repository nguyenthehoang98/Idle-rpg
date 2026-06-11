using System.Collections;
using UnityEngine;

namespace _Echo.Scripts.Battle
{
    public class Background : MonoBehaviour
    {
        public SpriteRenderer slotRenderer;
        
        [SerializeField, ColorUsage(true, true)] 
        private Color[] inactiveColors;
        [SerializeField, ColorUsage(true, true)]
        private Color[] activeColors;
        
        private MaterialPropertyBlock[] propertyBlocks;
        private Coroutine[] coroutines;

        private void Awake()
        {
            propertyBlocks = new MaterialPropertyBlock[7]
            {
                new MaterialPropertyBlock(), new MaterialPropertyBlock(), new MaterialPropertyBlock(),
                new MaterialPropertyBlock(), new MaterialPropertyBlock(), new MaterialPropertyBlock(),
                new MaterialPropertyBlock()
            };
            coroutines = new Coroutine[7];
        }

        public void Activate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;
            
            Coroutine coroutine = coroutines[slotIndex];

            if (coroutine != null) StopCoroutine(coroutine);

            coroutines[slotIndex] = StartCoroutine(
                LerpColor(slotRenderer, propertyBlocks, slotIndex, activeColors[slotIndex], duration)
            );
        }

        public void Deactivate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;
            
            Coroutine coroutine = coroutines[slotIndex];
            
            if (coroutine != null) StopCoroutine(coroutine);

            coroutines[slotIndex] = StartCoroutine(
                LerpColor(slotRenderer, propertyBlocks, slotIndex, inactiveColors[slotIndex], duration)
            );
        }

        static IEnumerator LerpColor(SpriteRenderer sp, MaterialPropertyBlock[] propertyBlocks, int slotIndex, Color color, float duration)
        {
            float elapsed = 0f;

            string prop = "_Color_" + (slotIndex + 1);

            MaterialPropertyBlock property = propertyBlocks[slotIndex];
            
            sp.GetPropertyBlock(property);
            
            Color a = property.GetColor(prop);

            while (elapsed <= duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                Color lerp = Color.Lerp(a, color, t);

                property.SetColor(prop, lerp);

                sp.SetPropertyBlock(property);

                yield return null;
            }
        }
    }
}