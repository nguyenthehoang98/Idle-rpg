using System;
using CloudAPI.Model;
using UnityEngine;

namespace CloudAPI.Utils
{
    public static class CloudConverter
    {
        public static CloudObjectData CreateObjectData(string title, params IConvertObjectData[] objects)
        {
            ObjectData[] array = new ObjectData[objects.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new ObjectData
                {
                    Name = objects[i].ObjectTitle(),
                    Object = objects[i]
                };
            }

            return new CloudObjectData { Title = title, Objects = array };
        }

        public static bool TryGetObject<T>(CloudObjectData objectData, out T result) where T : class, IConvertObjectData, new()
        {
            result = new T();
            foreach (var obj in objectData.Objects)
            {
                if (obj.Name == result.ObjectTitle())
                {
                    bool success = true;
                    try
                    {
                        result = JsonUtility.FromJson<T>(obj.Object.ToString());
                    }
                    catch (Exception e)
                    {
                        success = false;
                        Debug.LogError("Error parse json: " + obj.Name + "\n" + e.Message);
                    }
                    
                    return success;
                }
            }
            
            return false;
        }
    }
}