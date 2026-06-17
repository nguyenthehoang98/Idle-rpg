using System;
using System.Collections;
using _Game.Configs;
using _Game.GamePlay.SkillSystem;
using _KITSystem.Resource;
using _KITSystem.Schedule;
using _KITSystem.SkillSystem.Core;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay
{
    public class WeaponPedestal : MonoBehaviour
    {
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
        [SerializeField] private Transform weaponParent;
        [SerializeField] private Color weaponInactive;
        [SerializeField] private Color weaponActive1;
        [SerializeField] private Color weaponActive2;
        [SerializeField] private Color weaponActive3;

        public float DeltaTime { get; set; }

        private Weapon weapon;

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        public async UniTask Initialize(WeaponData weaponData, float timeScale, float deltaTime)
        {
            DeltaTime = deltaTime;
            var go = await AssetBundleManager.GetAsset<GameObject>(weaponData.prefabName);
            weapon = Object.Instantiate(go, weaponParent).GetComponent<Weapon>();
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.rotation = Quaternion.identity;
            weapon.transform.localScale = Vector3.one;
            weapon.Initialize(weaponData, timeScale, deltaTime);
            await UniTask.CompletedTask;
        }

        public IEnumerator Setup(int level, float duration)
        {
            Vector3 position = pivot.position;
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
                outline.color = Color.Lerp(outlineColor, outlineColorTarget, t);
                background.color = Color.Lerp(backgroundColor, backgroundColorTarget, t);
                highlight.color = Color.Lerp(highlightColor, highlightColorTarget, t);

                yield return new WaitForSeconds(DeltaTime);
            }

            pivot.position = positionTarget;
            outline.color = outlineColorTarget;
            background.color = backgroundColorTarget;
            highlight.color = highlightColorTarget;

            if (weapon != null) weapon.IsActivated = level > 0;
        }
    }
}