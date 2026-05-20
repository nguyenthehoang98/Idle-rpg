using UnityEngine;

namespace _KITSystem.ExcelConfig
{
    [System.Serializable]
    public abstract class KitBaseConfig : ScriptableObject
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