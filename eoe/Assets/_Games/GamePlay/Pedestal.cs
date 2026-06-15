using System;
using System.Collections;
using UnityEngine;

namespace _Games.GamePlay
{
    public class Pedestal : MonoBehaviour
    {
        [Serializable]
        private class Renderer
        {
            public Transform pivot;
            public SpriteRenderer weapon;
            public SpriteRenderer outline;
            public SpriteRenderer highlight;
            public Vector2 offset;
            public Color inactive;
            public Color active1;
            public Color active2;

            public IEnumerator Inactive(float duration, float deltaTime)
            {
                float elapsedTime = 0;

                Vector3 position = pivot.position;
                Color weaponColor = weapon.color;
                Color outlineColor = outline.color;
                Color highlightColor = highlight.color;
                    
                while (elapsedTime < duration)
                {
                    elapsedTime += deltaTime;
                        
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    pivot.position = Vector3.Lerp(position, Vector3.zero, t);
                    weapon.color = Color.Lerp(weaponColor, inactive, t);
                    outline.color = Color.Lerp(outlineColor, inactive, t);
                    highlightColor.a = 1 - t;
                        
                    yield return null;
                }

                highlightColor.a = 0;
                highlight.color = highlightColor;
                pivot.position = Vector3.zero;
                weapon.color = inactive;
                outline.color = inactive;
            }

            public IEnumerator Level1(float duration, float deltaTime)
            {
                Vector3 position = pivot.position;
                Color weaponColor = weapon.color;
                Color outlineColor = outline.color;
                Color highlightColor = highlight.color;

                float elapsedTime = 0;
                    
                while (elapsedTime < duration)
                {
                    elapsedTime += deltaTime;
                        
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    pivot.position = Vector3.Lerp(position, offset, t);
                    weapon.color = Color.Lerp(weaponColor, inactive, t);
                    outline.color = Color.Lerp(outlineColor, inactive, t);
                    highlightColor.a = Mathf.Lerp(highlightColor.a, 0, t);

                    yield return null;
                }
                
                highlightColor.a = 0;
                highlight.color = highlightColor;
                pivot.position = offset;
                weapon.color = active1;
                outline.color = active1;
            }

            public IEnumerator Level2(float duration, float deltaTime)
            {
                Vector3 position = pivot.position;
                Color weaponColor = weapon.color;
                Color outlineColor = outline.color;
                Color highlightColor = highlight.color;
                    
                float elapsedTime = 0;
                    
                while (elapsedTime < duration)
                {
                    elapsedTime += deltaTime;
                        
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    pivot.position = Vector3.Lerp(position, offset, t);
                    weapon.color = Color.Lerp(weaponColor, active2, t);
                    outline.color = Color.Lerp(outlineColor, active2, t);
                    highlightColor.a = Mathf.Lerp(highlightColor.a, 0, t);

                    yield return null;
                }
                
                highlightColor.a = 0;
                highlight.color = highlightColor;
                pivot.position = offset;
                weapon.color = active2;
                outline.color = active2;
            }

            public IEnumerator Level3(float duration, float deltaTime)
            {
                Vector3 position = pivot.position;
                Color weaponColor = weapon.color;
                Color outlineColor = outline.color;
                Color highlightColor = highlight.color;
                    
                float elapsedTime = 0;
                    
                while (elapsedTime < duration)
                {
                    elapsedTime += deltaTime;
                        
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    pivot.position = Vector3.Lerp(position, offset, t);
                    weapon.color = Color.Lerp(weaponColor, active2, t);
                    outline.color = Color.Lerp(outlineColor, active1, t);
                    highlightColor.a = t;

                    yield return null;
                }
                    
                highlightColor.a = 1;
                highlight.color = highlightColor;
                pivot.position = offset;
                weapon.color = active2;
                outline.color = active1;
            }
        }

        [SerializeField] private Renderer[] renderers = new Renderer[0];

        private Coroutine[] coroutines;

        private void Awake()
        {
            coroutines = new Coroutine[renderers.Length];
        }

        private void Start()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                StartCoroutine(renderers[i].Inactive(0, 1));
            }
        }

        public void SetWeapon(int slot, int level, float duration, float deltaTime)
        {
            Coroutine coroutine = coroutines[slot];
            if (coroutine != null) StopCoroutine(coroutine);

            Renderer r = renderers[slot];

            if(level == 0)  coroutines[slot] = StartCoroutine(r.Inactive(duration, deltaTime));
            else if (level == 1) coroutines[slot] = StartCoroutine(r.Level1(duration, deltaTime));
            else if (level == 2) coroutines[slot] = StartCoroutine(r.Level2(duration, deltaTime));
            else if (level == 3) coroutines[slot] = StartCoroutine(r.Level3(duration, deltaTime));
        }
    }
}