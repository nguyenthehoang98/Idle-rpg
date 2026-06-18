using UnityEngine;

namespace _Game.GamePlay.View
{
    [System.Serializable]
    public struct ElementData
    {
        public Color coreColor;
        public Color outlineColor;
        public Color inactiveColor;
    }
    
    public class ElementManager : MonoBehaviour
    {
        [SerializeField] private Element[] elements = new Element[4];
        [SerializeField] private ElementData[] datas = new ElementData[4];
    }
}
