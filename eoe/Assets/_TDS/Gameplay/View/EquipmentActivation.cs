using System.Collections;
using System.Collections.Generic;
using _GameToolkit.Resource;
using _TDS.GameConfig;
using _TDS.Gameplay.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace _TDS.Gameplay.View
{
    public class EquipmentActivation : MonoBehaviour
    {
        [SerializeField] private EquipmentSlot[] slots;
        [SerializeField] private Image[] backgrounds;

        public void SetBackgroundColor(List<int> list, int weaponId, int level)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == weaponId)
                {
                    Image img = backgrounds[i];
                    Color color = Color.white;
                    if (level == 2) color = Const.WEAPON_OUTLINE_X2;
                    else if (level == 3) color = Const.WEAPON_OUTLINE_X3;
                    StartCoroutine(ChangeBackgroundColor(img, color, 0.2f));
                }
            }
        }

        public void IdleAnimation(int idx, float speed)
        {
            EquipmentSlot sl = slots[idx];
            sl.Speed = speed;
            sl.PlayIdle();
        }

        public void ReleaseAnimation(int idx, float speed)
        {
            EquipmentSlot sl = slots[idx];
            sl.Speed = speed;
            sl.PlayRelease();
        }

        public async void SetWeapon(int idx, WeaponData data)
        {
            EquipmentSlot sl = slots[idx];
           
            if (!string.IsNullOrEmpty(data.iconName))
            {
                sl.SetIcon(await AssetManager.GetAssetCached<Sprite>(data.iconName));                
            }
        }

        private IEnumerator ChangeBackgroundColor(Image img, Color color, float duration)
        {
            Color defaultColor = img.color;
            float elapsedTime = 0;
            while (elapsedTime <= duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                img.color = Color.Lerp(defaultColor, color, t);
                yield return null;
            }

            img.color = color;
        }
    }
}