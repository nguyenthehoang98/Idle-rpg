using System.Collections;
using _Games.GamePlay.SkillSystem;
using _KITSystem.Utils;
using UnityEngine;

namespace _Games.GamePlay
{
    public class WeaponPedestal : MonoBehaviour
    {
        public SkillAsset skillAsset;
        [SerializeField] private Transform pivot;     
        [SerializeField] private Vector2 offsetPosition;
        [SerializeField] private SpriteRenderer highlight;
        [Header("Background")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private Color backgroundInactive = Color.gray;
        [SerializeField] private Color backgroundActive = Color.white;
        [Header("Outline")]
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private Color outlineInactive;
        [SerializeField] private Color outlineActive1;
        [SerializeField] private Color outlineActive2;
        [SerializeField] private Color outlineActive3;
        [Header("Weapon")]
        [SerializeField] private Weapon weapon;
        [SerializeField] private Color weaponInactive;
        [SerializeField] private Color weaponActive1;
        [SerializeField] private Color weaponActive2;
        [SerializeField] private Color weaponActive3;
        [SerializeField] private float weaponRotationDuration = 0.15f;
        [SerializeField] private AnimationCurve weaponRotationCurve;

        private int currentLevel;

        public float DeltaTime { get; set; }

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        private void Start()
        {
            StartCoroutine(AutoCast());
        }
        
        private IEnumerator AutoCast()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                
                if (currentLevel != 0)
                {
                    Vector3 position = pivot.position;
                    Vector3 destination = new Vector3(RandomUtils.Range(-1f, 1f), RandomUtils.Range(-1f, 1f)).normalized * 5;
                    Vector3 direction = (destination - position).normalized;
                    
                    float angleCurrent = weapon.EulerAngleZ;
                    float angleTarget = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    float elapsedTime = 0;
                    while (elapsedTime < weaponRotationDuration)
                    {
                        elapsedTime += DeltaTime;
                        float t = Mathf.Clamp01(elapsedTime / weaponRotationDuration);
                        float s = weaponRotationCurve.Evaluate(t);
                        float angle = Mathf.LerpAngle(angleCurrent, angleTarget, s);
                        weapon.EulerAngleZ = angle;

                        yield return new WaitForSeconds(DeltaTime);
                    }

                    weapon.transform.rotation = Quaternion.Euler(0, 0, angleTarget);

                    yield return null;
                    
                    weapon.Attack(() =>
                    {
                        SkillTickable.CastSkill(skillAsset.SkillData, weapon.MuzzlePosition, destination); 
                    });
                }
            }
        }

        public IEnumerator Setup(int level, float duration)
        {
            currentLevel = level;
            
            Vector3 position = pivot.position;
            Color weaponColor = weapon.Color;
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
                elapsedTime += DeltaTime;
                        
                float t = Mathf.Clamp01(elapsedTime / duration);
                
                pivot.position = Vector3.Lerp(position, positionTarget, t);
                weapon.Color = Color.Lerp(weaponColor, weaponColorTarget, t);
                outline.color = Color.Lerp(outlineColor, outlineColorTarget, t);
                background.color = Color.Lerp(backgroundColor, backgroundColorTarget, t);
                highlight.color = Color.Lerp(highlightColor, highlightColorTarget, t);
                
                yield return new WaitForSeconds(DeltaTime);
            }
            
            pivot.position = positionTarget;
            weapon.Color = weaponColorTarget;
            outline.color = outlineColorTarget;
            background.color = backgroundColorTarget;
            highlight.color = highlightColorTarget;
        }
    }
}