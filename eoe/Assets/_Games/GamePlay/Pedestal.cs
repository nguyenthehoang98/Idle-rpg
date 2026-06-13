using System;
using System.Collections;
using UnityEngine;

namespace _Games.GamePlay
{
    public class Pedestal : MonoBehaviour
    {
        public SpriteRenderer slotRenderer;

        [SerializeField, ColorUsage(true, true)]
        private Color[] inactiveColors = new Color[7];

        [SerializeField, ColorUsage(true, true)]
        private Color[] activeColors = new Color[7];

        private MaterialPropertyBlock propertyBlock;
        private Coroutine[] coroutines;
        
        private readonly Color[] currentColors = new Color[7];
        private readonly bool[] triggered = new bool[7];

        private static readonly int[] ColorPropIds =
        {
            Shader.PropertyToID("_Color_1"),
            Shader.PropertyToID("_Color_2"),
            Shader.PropertyToID("_Color_3"),
            Shader.PropertyToID("_Color_4"),
            Shader.PropertyToID("_Color_5"),
            Shader.PropertyToID("_Color_6"),
            Shader.PropertyToID("_Color_7"),
        };

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
          
            coroutines = new Coroutine[7];
            
            for (int i = 0; i < 7; i++)
            {
                currentColors[i] = inactiveColors[i];
            }
        }
        
        private void LateUpdate()
        {
            bool shouldApplyColor = false;
            
            foreach (var boolean in triggered)
            {
                if (boolean)
                {
                    shouldApplyColor = true;
                    break;
                }
            }

            if (!shouldApplyColor) return;
            
            slotRenderer.GetPropertyBlock(propertyBlock);

            for (int i = 0; i < 7; i++)
            {
                propertyBlock.SetColor(ColorPropIds[i], currentColors[i]);
            }

            slotRenderer.SetPropertyBlock(propertyBlock);
        }

        public void Activate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;
            
            Coroutine coroutine = coroutines[slotIndex];

            if (coroutine != null) StopCoroutine(coroutine);
            
            triggered[slotIndex] = true;

            coroutines[slotIndex] = StartCoroutine(
                LerpColor(slotIndex, activeColors[slotIndex], duration, () =>
                {
                    triggered[slotIndex] = false;
                })
            );
        }

        public void Deactivate(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex > 6) return;

            Coroutine coroutine = coroutines[slotIndex];

            if (coroutine != null) StopCoroutine(coroutine);
            
            triggered[slotIndex] = true;

            coroutines[slotIndex] = StartCoroutine(
                LerpColor(slotIndex, inactiveColors[slotIndex], duration, () =>
                {
                    triggered[slotIndex] = false;
                })
            );
        }

        IEnumerator LerpColor(int slotIndex, Color targetColor, float duration, Action onComplete)
        {
            float elapsed = 0f;

            Color startColor = currentColors[slotIndex];

            if (duration <= 0)
            {
                currentColors[slotIndex] = targetColor;
            }
            else
            {
                while (elapsed <= duration)
                {
                    elapsed += Time.deltaTime;

                    float t = Mathf.Clamp01(elapsed / duration);

                    currentColors[slotIndex] = Color.Lerp(startColor, targetColor, t);
                    
                    yield return null;
                }
            }
            
            onComplete?.Invoke();
        }
    }
}