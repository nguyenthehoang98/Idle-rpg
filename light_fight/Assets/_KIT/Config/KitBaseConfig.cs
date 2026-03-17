using UnityEngine;

namespace _KIT.Config
{
    [System.Serializable]
    public abstract class KitBaseConfig : ScriptableObject, IConfig
    {
        [HideInInspector]
        public string fileUrl;

        [HideInInspector] 
        public bool enable = true;

        public virtual void OnPostImported()
        {
        }

        public virtual void OnGuiEnable() { }
		
        public abstract void OnMapValue();
    }
}