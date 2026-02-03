using UnityEngine;

namespace _KIT.Foundation
{
    public class KitString
    {
        private string value;
        private readonly string key;

        public KitString(string key, string defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(string value)
        {
            this.value = value;
            PlayerPrefs.SetString(key, this.value);
            PlayerPrefs.Save();
        }

        public string Get()
        {
            return PlayerPrefs.GetString(key, value);
        }
    }
}