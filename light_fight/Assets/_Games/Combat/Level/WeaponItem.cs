using _Games.Config;
using _KIT.Resource;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.Combat.Level
{
    public class WeaponItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer item;
        
        public WeaponData WeaponData { get; private set; }
        public int WeaponLevel { get; private set; }
        
        public static async UniTask<WeaponItem> Build(Transform parent, WeaponData weaponData, int weaponLevel)
        {
            WeaponSO so = await KitLoaded.LoadAsync<WeaponSO>(weaponData.WeaponId.ToString());
            GameObject go = await KitLoaded.LoadAsync<GameObject>("WeaponItem", true);
            WeaponItem wi = Object.Instantiate(go).GetComponent<WeaponItem>();
            wi.transform.SetParent(parent);
            wi.transform.localPosition = Vector3.zero;
            wi.transform.localScale = Vector3.one;
            wi.item.sprite = so.GetIcon(weaponLevel);
            wi.WeaponLevel = weaponLevel;
            wi.WeaponData = weaponData;
            Debug.Log("Mỗi level là 1 rarity, rarity sẽ thay đổi màu border");
            return wi;
        }
    }
}