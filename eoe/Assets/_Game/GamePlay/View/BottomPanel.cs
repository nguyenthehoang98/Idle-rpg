using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class BottomPanel : MonoBehaviour
    {
        [SerializeField] private Sprite spHammerActive;
        [SerializeField] private Sprite spHammerInactive;
        [SerializeField] private UIAnimation hammerAnimation;
        [SerializeField] private Button btnHammer;
        [SerializeField] private EquipmentQueue equipmentQueue;

        private void Awake()
        {
            equipmentQueue.OnFill += Fill;
            equipmentQueue.OnQueueFull += QueueFull;
            hammerAnimation.OnCompleted += HammerComplete;
            btnHammer.onClick.AddListener(ClickHammer); 
        }

        private void Start() => DefaultButton();

        private void OnDestroy()
        {
            equipmentQueue.OnFill -= Fill;
            equipmentQueue.OnQueueFull -= QueueFull;
            hammerAnimation.OnCompleted -= HammerComplete;
            btnHammer.onClick.RemoveListener(ClickHammer);
        }

        private void QueueFull() => DefaultButton();

        private void DefaultButton()
        {
            btnHammer.interactable = false;
            SetHammerInactive();
        }

        private void ClickHammer()
        {
            btnHammer.interactable = false;
            float f = hammerAnimation.Play();
            equipmentQueue.Decrease(f);
            RefreshUI();
        }

        private void HammerComplete()
        {
            btnHammer.interactable = equipmentQueue.GetAllEquipment().Count != 0;
            RefreshUI();
        }

        private void Fill()
        {
            if (!btnHammer.interactable) btnHammer.interactable = true;
            RefreshUI();
        }

        private void RefreshUI()
        {
            bool f1 = btnHammer.interactable;
            bool f2 = equipmentQueue.GetAllEquipment().Count > 0;
            if (f1 && f2)
            {
                SetHammerActive();
            }
            else
            {
                SetHammerInactive();
            }
        }

        private void SetHammerActive() => btnHammer.GetComponent<Image>().sprite = spHammerActive;
        private void SetHammerInactive() => btnHammer.GetComponent<Image>().sprite = spHammerInactive;
    }
}