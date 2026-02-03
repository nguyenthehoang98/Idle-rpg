using UnityEngine;

namespace _KIT.Foundation
{
    public sealed class KitBoolean
    {
        private bool value;
        private readonly string key;

        public KitBoolean(string key, bool defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(bool boolValue)
        {
            value = boolValue;
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool Get()
        {
            return PlayerPrefs.GetInt(key, value ? 1 : 0) == 1;
        }
    }
}