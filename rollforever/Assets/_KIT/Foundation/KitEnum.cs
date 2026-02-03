using System;
using UnityEngine;

namespace _KIT.Foundation
{
    public sealed class KitEnum<T> where T : Enum
    {
        private T value;
        private readonly string key;

        public KitEnum(string key, T defaultValue)
        {
            this.key = key;
            value = defaultValue;
        }

        public void Set(T enumValue)
        {
            value = enumValue;
            PlayerPrefs.SetString(key, value.ToString());
            PlayerPrefs.Save();
        }

        public T Get()
        {
            string data = PlayerPrefs.GetString(key, value.ToString());
            return (T)Enum.Parse(typeof(T), data);
        }
    }
}