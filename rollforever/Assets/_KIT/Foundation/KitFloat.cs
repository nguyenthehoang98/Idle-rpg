using UnityEngine;

namespace _KIT.Foundation
{
    public sealed class KitFloat
    {
        private float value;
        private readonly string key;

        public KitFloat(string key, float defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(float floatValue)
        {
            value = floatValue;
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public float Get()
        {
            return PlayerPrefs.GetFloat(key, value);
        }
    }
}