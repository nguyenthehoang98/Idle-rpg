using UnityEngine;
using System;
namespace _Games.Config
{
    [CreateAssetMenu(menuName = "Weapon SO")]
    public class WeaponSO : ScriptableObject
    {
        [SerializeField] private Data[] datas;

        public Sprite GetIcon(int level)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                Data data = datas[i];
                if (data.levelRange.x <= level && data.levelRange.y > level)
                    return data.icon;
            }

            return null;
        }

        [Serializable]
        struct Data
        {
            public Sprite icon;
            public Vector2Int levelRange;
        }
    }
}