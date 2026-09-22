using UnityEngine;

namespace _TDS.Shares
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TextRenderOnTop : MonoBehaviour
    {
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int sortingOrder = 100;

        private MeshRenderer meshRenderer;

        private void Awake()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer == null)
                return;

            meshRenderer.sortingLayerName = sortingLayerName;
            meshRenderer.sortingOrder = sortingOrder;
        }
    }
}