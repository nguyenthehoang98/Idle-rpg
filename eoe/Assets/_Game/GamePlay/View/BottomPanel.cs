using _KITSystem.Schedule;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.GamePlay.View
{
    public class BottomPanel : MonoBehaviour
    {
        [SerializeField] private Sprite spHammerActive;
        [SerializeField] private Sprite spHammerInactive;
        [SerializeField] private UIAnimation hammerAnimation;
        [SerializeField] private UIButton btnHammer;
        [SerializeField] private EquipmentQueue equipmentQueue;

        private void Awake()
        {
            equipmentQueue.OnFill += Fill;
            equipmentQueue.OnQueueFull += QueueFull;
            hammerAnimation.OnCompleted += HammerComplete;
            btnHammer.OnClicked += ClickHammer;
        }

        private void Start()
        {
            btnHammer.IsBlockInput = true;
            SetHammerInactive();
        }

        private void OnDestroy()
        {
            equipmentQueue.OnFill -= Fill;
            equipmentQueue.OnQueueFull -= QueueFull;
            hammerAnimation.OnCompleted -= HammerComplete;
            btnHammer.OnClicked -= ClickHammer;
        }

        private void QueueFull()
        {
            btnHammer.IsBlockInput = true;
            SetHammerInactive();
        }

        private void ClickHammer()
        {
            btnHammer.IsBlockInput = true;
            float f = hammerAnimation.Play();
            equipmentQueue.Decrease(f);
            RefreshUI();
        }

        private void HammerComplete()
        {
            btnHammer.IsBlockInput = equipmentQueue.GetAllEquipment().Count == 0;
            RefreshUI();
        }

        private void Fill()
        {
            if (!btnHammer.IsPressing && btnHammer.IsBlockInput) btnHammer.IsBlockInput = false;
            RefreshUI();
        }

        private void RefreshUI()
        {
            bool f1 = btnHammer.IsBlockInput;
            bool f2 = equipmentQueue.GetAllEquipment().Count > 0;
            if (!f1 && f2)
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