using System.Collections;
using _Game.Configs;
using _KITSystem.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Game.GamePlay
{
    public class WeaponPedestal : MonoBehaviour
    {
        [SerializeField] private int groupColorId;
        [SerializeField] private Transform pivot;     
        [SerializeField] private Vector2 offsetPosition;
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private PedestalPath path;
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private Transform weaponParent;
        
        private Color backgroundInactive = Color.gray;
        private Color backgroundActive = Color.white;
        private Color outlineInactive;
        private Color outlineActive1;
        private Color outlineActive2;
        private Color outlineActive3;

        public float DeltaTime { get; set; }

        private Weapon weapon;

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
            path.gameObject.SetActive(false);
        }

        private void Start()
        {
            ColorSetting.Instance.TryGetColor(groupColorId, out var colorData);
            outlineInactive = colorData.inactiveColor;
            outlineActive1 = colorData.activeColor1;
            outlineActive2 = colorData.activeColor2;
            outlineActive3 = colorData.activeColor2;
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
            if (weapon == null)
            {
                outline.color = outlineInactive;
                background.color = backgroundInactive;
                yield break;
            }
            
            Vector3 position = pivot.position;
            Color outlineColor = outline.color;
            Color backgroundColor = background.color;

            Vector3 positionTarget = level == 0 ? Vector3.zero : offsetPosition;
            Color backgroundColorTarget = level == 0 ? backgroundInactive : backgroundActive;
            Color outlineColorTarget = outlineInactive;
            if (level == 1) outlineColorTarget = outlineActive1;
            else if (level == 2) outlineColorTarget = outlineActive2;
            else if (level == 3) outlineColorTarget = outlineActive3;

            path.gameObject.SetActive(level == 3);

            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += DeltaTime;

                float t = Mathf.Clamp01(elapsedTime / duration);

                pivot.position = Vector3.Lerp(position, positionTarget, t);
                outline.color = Color.Lerp(outlineColor, outlineColorTarget, t);
                background.color = Color.Lerp(backgroundColor, backgroundColorTarget, t);

                yield return new WaitForSeconds(DeltaTime);
            }

            pivot.position = positionTarget;
            outline.color = outlineColorTarget;
            background.color = backgroundColorTarget;

            if (weapon != null) weapon.IsActivated = level > 0;
        }
    }
}