using System.Collections;
using UnityEngine;

namespace _Games.GamePlay
{
    public class WeaponPedestal : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private SpriteRenderer weapon;
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private SpriteRenderer highlight;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Vector2 offsetPosition;
        [SerializeField] private Color backgroundInactive = Color.gray;
        [SerializeField] private Color backgroundActive = Color.white;
        [SerializeField] private Color outlineInactive;
        [SerializeField] private Color outlineActive1;
        [SerializeField] private Color outlineActive2;
        [SerializeField] private Color outlineActive3;
        [SerializeField] private Color weaponInactive;
        [SerializeField] private Color weaponActive1;
        [SerializeField] private Color weaponActive2;
        [SerializeField] private Color weaponActive3;

        public IEnumerator Setup(int level, float duration, float deltaTime)
        {
            Vector3 position = pivot.position;
            Color weaponColor = weapon.color;
            Color outlineColor = outline.color;
            Color highlightColor = highlight.color;
            Color backgroundColor = background.color;

            Vector3 positionTarget = level == 0 ? Vector3.zero : offsetPosition;
            Color highlightColorTarget = highlightColor;
            highlightColorTarget.a = level == 3 ? 1 : 0;
            Color backgroundColorTarget = level == 0 ? backgroundInactive : backgroundActive;
            Color weaponColorTarget = weaponInactive;
            if (level == 1) weaponColorTarget = weaponActive1;
            else if (level == 2) weaponColorTarget = weaponActive2;
            else if (level == 3) weaponColorTarget = weaponActive3;
            Color outlineColorTarget = outlineInactive;
            if (level == 1) outlineColorTarget = outlineActive1;
            else if (level == 2) outlineColorTarget = outlineActive2;
            else if (level == 3) outlineColorTarget = outlineActive3;
            
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += deltaTime;
                        
                float t = Mathf.Clamp01(elapsedTime / duration);
                
                pivot.position = Vector3.Lerp(position, positionTarget, t);
                weapon.color = Color.Lerp(weaponColor, weaponColorTarget, t);
                outline.color = Color.Lerp(outlineColor, outlineColorTarget, t);
                background.color = Color.Lerp(backgroundColor, backgroundColorTarget, t);
                highlight.color = Color.Lerp(highlightColor, highlightColorTarget, t);
                
                yield return new WaitForSeconds(deltaTime);
            }
            
            pivot.position = positionTarget;
            weapon.color = weaponColorTarget;
            outline.color = outlineColorTarget;
            background.color = backgroundColorTarget;
            highlight.color = highlightColorTarget;
        }
    }
}