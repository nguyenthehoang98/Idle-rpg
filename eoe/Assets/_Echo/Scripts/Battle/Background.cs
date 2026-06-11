using System;
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

        private void Start()
        {
            propertyBlocks = new MaterialPropertyBlock[7]
            {
                new MaterialPropertyBlock(), new MaterialPropertyBlock(), new MaterialPropertyBlock(),
                new MaterialPropertyBlock(), new MaterialPropertyBlock(), new MaterialPropertyBlock(),
                new MaterialPropertyBlock()
            };
            coroutines = new Coroutine[7];
            
            Deactivate(0, 0);
            Deactivate(1, 0);
            Deactivate(2, 0);
            Deactivate(3, 0);
            Deactivate(4, 0);
            Deactivate(5, 0);
            Deactivate(6, 0);
        }

        private void Update()
        {
            float duration = 0.5f;

            if (Input.GetKeyDown(KeyCode.Alpha1)) Activate(0, duration);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Activate(1, duration);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Activate(2, duration);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Activate(3, duration);
            if (Input.GetKeyDown(KeyCode.Alpha5)) Activate(4, duration);
            if (Input.GetKeyDown(KeyCode.Alpha6)) Activate(5, duration);
            if (Input.GetKeyDown(KeyCode.Alpha7)) Activate(6, duration);

            if (Input.GetKeyDown(KeyCode.F1)) Deactivate(0, duration);
            if (Input.GetKeyDown(KeyCode.F2)) Deactivate(1, duration);
            if (Input.GetKeyDown(KeyCode.F3)) Deactivate(2, duration);
            if (Input.GetKeyDown(KeyCode.F4)) Deactivate(3, duration);
            if (Input.GetKeyDown(KeyCode.F5)) Deactivate(4, duration);
            if (Input.GetKeyDown(KeyCode.F6)) Deactivate(5, duration);
            if (Input.GetKeyDown(KeyCode.F7)) Deactivate(6, duration);
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