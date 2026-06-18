using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Configs
{
    [Serializable]
    public struct ColorData
    {
        public int id;
        public Color inactiveColor;
        public Color activeColor1;
        public Color activeColor2;
    }

    [CreateAssetMenu]
    public class ColorSetting : ScriptableObject
    {
        [SerializeField] private ColorData[] datas = new ColorData[4];

        private Dictionary<int, ColorData> cached;
        
        private static ColorSetting instance;

        public static ColorSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<ColorSetting>("ColorSetting");
                    instance.OnMapValue();
                }
                return instance;
            }
        }

        private void OnMapValue()
        {
            cached = new Dictionary<int, ColorData>();
            foreach (var colorData in datas)
            {
                cached.Add(colorData.id, colorData);
            }
        }

        public bool TryGetColor(int id, out ColorData color)
        {
            return cached.TryGetValue(id, out color);
        }
    }
}
