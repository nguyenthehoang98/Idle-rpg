using UnityEngine;
namespace _Games.Config
{
    [CreateAssetMenu(menuName = "Weapon SO")]
    public class WeaponSO : ScriptableObject
    {
        [SerializeField] private Sprite iconLevel1, iconLevel2, iconLevel3, iconLevel4, iconLevel5;

        public Sprite GetIcon(int level)
        {
            switch (level)
            {
                case 1: return iconLevel1;
                case 2: return iconLevel2;
                case 3: return iconLevel3;
                case 4: return iconLevel4;
                case 5: return iconLevel5;
            }

            return null;
        }
    }
}