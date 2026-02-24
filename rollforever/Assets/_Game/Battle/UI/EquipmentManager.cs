using _Game.Battle.Ecs.Events;
using _KIT.Event;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.Battle.UI
{
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RectTransform container;
        [SerializeField] private float yStartPosition;

        private void Awake()
        {
            Vector3 anchoredPosition = container.anchoredPosition3D;
            anchoredPosition.y = yStartPosition;
            container.anchoredPosition3D = anchoredPosition;
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<OpenEquipmentSelectionEvent>(OnOpenEquipmentSelection);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<OpenEquipmentSelectionEvent>(OnOpenEquipmentSelection);
        }

        void OnOpenEquipmentSelection(OpenEquipmentSelectionEvent e)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.x = e.CameraOffsetPosition.x;
            cameraPosition.y = e.CameraOffsetPosition.y;
            if (math.abs(e.Duration) > 0)
            {
                container.gameObject.SetActive(true);
                mainCamera.transform.DOMove(cameraPosition, e.Duration).SetEase(Ease.OutSine);
                mainCamera.DOOrthoSize(e.OrthoSize, e.Duration).SetEase(Ease.OutSine);
                container.DOAnchorPosY(0, e.Duration).SetEase(Ease.OutSine);
            }
            else
            {
                mainCamera.orthographicSize = e.OrthoSize;
                mainCamera.transform.position = cameraPosition;
                container.anchoredPosition3D = Vector3.zero;
                container.gameObject.SetActive(true);
            }
        }
    }
}