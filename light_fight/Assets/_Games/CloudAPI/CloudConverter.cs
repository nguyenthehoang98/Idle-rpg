using _Games.CloudAPI.Model;
using UnityEngine;

namespace _Games.CloudAPI
{
    public static class CloudConverter
    {
        public static bool TryGetObject<T>(IObjectData[] objects, out T result) where T : struct, IObjectData
        {
            result = default(T);
            foreach (var obj in objects)
            {
                if (obj.Name() == result.Name())
                {
                    result = JsonUtility.FromJson<T>(obj.Value().ToString());
                    return true;
                }
            }

            return false;
        }
    }
}