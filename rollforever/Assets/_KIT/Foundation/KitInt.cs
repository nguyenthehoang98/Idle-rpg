using UnityEngine;

namespace _KIT.Foundation
{
    public sealed class KitInt
    {
        private int value;
        private readonly string key;

        public KitInt(string key, int defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(int intValue)
        {
            value = intValue;
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public int Get()
        {
            return PlayerPrefs.GetInt(key, value);
        }
    }
}