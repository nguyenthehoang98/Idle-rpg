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
        [SerializeField] private PedestalPath[] paths;
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private Transform weaponParent;
        
        private Color backgroundInactive = new Color(0.7f, 0.7f, 0.7f);
        private Color backgroundActive = Color.white;
        private Color outlineInactive;
        private Color outlineActive;

        public float DeltaTime { get; set; }

        private Weapon weapon;

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            ColorSetting.Instance.TryGetColor(groupColorId, out var colorData);
            outlineInactive = colorData.inactiveColor;
            outlineActive = colorData.activeColor;
        }

        public async UniTask Initialize(WeaponData weaponData, float timeScale, float deltaTime)
        {
            DeltaTime = deltaTime;
            var go = await AssetBundleManager.GetAsset<GameObject>(weaponData.prefabName);
            weapon = Object.Instantiate(go, weaponParent).GetComponent<Weapon>();
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.rotation = Quaternion.identity;
            weapon.transform.localScale = Vector3.one;
            await weapon.Initialize(weaponData, timeScale, deltaTime);
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
            Color outlineColorTarget = level == 0 ? outlineInactive : outlineActive;

            for (int i = 0; i < paths.Length; i++)
            {
                paths[i].gameObject.SetActive(false);
                if(i < level - 1) paths[i].gameObject.SetActive(true);
            }
            
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