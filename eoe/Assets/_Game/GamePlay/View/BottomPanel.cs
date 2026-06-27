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
            hammerAnimation.OnCompleted += HammerComplete;
            btnHammer.OnClicked += ClickHammer;
        }

        private void OnDestroy()
        {
            equipmentQueue.OnFill -= Fill;
            hammerAnimation.OnCompleted -= HammerComplete;
            btnHammer.OnClicked -= ClickHammer;
        }

        private void ClickHammer()
        {
            btnHammer.IsBlockInput = true;
            equipmentQueue.Decrease();
            hammerAnimation.Play();
            RefreshUI();
        }

        private void HammerComplete()
        {
            btnHammer.IsBlockInput = false;
            RefreshUI();
        }

        private void Fill()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (!btnHammer.IsBlockInput && equipmentQueue.GetAllEquipment().Count > 0)
            {
                SetHammerActive();
            }
            else
            {
                SetHammerActive();
            }
        }

        private void Start()
        {
            SetHammerInactive();
        }

        private void SetHammerActive() => btnHammer.GetComponent<Image>().sprite = spHammerActive;
        private void SetHammerInactive() => btnHammer.GetComponent<Image>().sprite = spHammerInactive;
    }
}