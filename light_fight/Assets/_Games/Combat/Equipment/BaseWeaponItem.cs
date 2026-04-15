using System;
using _Games.Config;
using _Games.Utils;
using _KIT.Resource;
using _KIT.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Games.Combat.Equipment
{
    public abstract class BaseWeaponItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer item;
        
        public WeaponData WeaponData { get; private set; }
        public int WeaponLevel { get; private set; }
        
        public static async UniTask<BaseWeaponItem> Build(Transform parent, WeaponData weaponData, int weaponLevel)
        {
            WeaponSO so = await KitLoaded.LoadAsync<WeaponSO>(GlobalsPath.GetWeaponSOPath(weaponData.WeaponId));
            GameObject go = await KitLoaded.LoadAsync<GameObject>(GlobalsPath.GetWeaponItemPath(weaponData.WeaponId), true);
            BaseWeaponItem wi = Object.Instantiate(go).GetComponent<BaseWeaponItem>();
            wi.transform.SetParent(parent);
            wi.transform.localPosition = Vector3.zero;
            wi.transform.localScale = Vector3.one;
            wi.item.sprite = so.GetIcon(weaponLevel);
            wi.WeaponLevel = weaponLevel;
            wi.WeaponData = weaponData;
            Debug.Log("Mỗi level là 1 rarity, rarity sẽ thay đổi màu border");
            return wi;
        }

        public void Execute(Action onComplete)
        {
            this.WaitInvoke(OnExecuteDelay(BattleTime.ScaleTime), onComplete);
        }

        protected abstract float OnExecuteDelay(float timeScale);
    }
}