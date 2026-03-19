using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    private static bool _isQuit = false;

    public static T Instance {
        get {
            if (_isQuit)
                return null;
            if (instance == null) {
                instance = FindObjectOfType(typeof(T)) as T;

                if (instance == null) {
                    instance = new GameObject().AddComponent<T>();
                    instance.gameObject.name = instance.GetType().Name;
                }
            }
            return instance;
        }
    }

    public void Reset() {
        instance = null;
    }

    public static bool Exists() {
        return (instance != null);
    }

    public void Destroy() {
        _isQuit = true;
    }
}
