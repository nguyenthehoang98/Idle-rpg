using System;
using _Game.Battle.Events;
using _KIT.Event;
using UnityEngine;

namespace _Game.Battle.UI
{
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<OpenEquipmentSelectionEvent>(OnOpenEquipmentSelection);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<OpenEquipmentSelectionEvent>(OnOpenEquipmentSelection);
        }

        private void OnOpenEquipmentSelection(OpenEquipmentSelectionEvent e)
        {
        }
    }
}