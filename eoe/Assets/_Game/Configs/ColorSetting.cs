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
        public Color activeColor;
    }

    [CreateAssetMenu]
    public class ColorSetting : ScriptableObject
    {
        [SerializeField] private ColorData[] datas = new ColorData[4];

        private Dictionary<int, ColorData> cached;

        public static ColorSetting Instance { get; private set; }

        public static void Load()
        {
            if (Instance == null)
            {
                Instance = Resources.Load<ColorSetting>("ColorSetting");
                Instance.OnMapValue();
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
