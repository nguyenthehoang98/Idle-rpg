using UnityEngine;

namespace _Game.Scripts.Weapon
{
    [CreateAssetMenu(menuName = "Weapon SO")]
    public class WeaponSO : ScriptableObject
    {
        [SerializeField] private Sprite weaponIcon;

        public Sprite WeaponIcon => weaponIcon;
    }
}
