using _Games.Combat.Event;
using _KIT.Event;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace _Games.Combat.View
{
    public class CameraWeaponSelectListener : MonoBehaviour
    {
        [Header("Tween")] 
        [SerializeField] private MMF_Player openFeedback;
        [SerializeField] private MMF_Player closeFeedback;
        
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<OpenWeaponSelectPopupEvent>(OpenPopup);
            EventBus.Instance.Subscribe<CloseWeaponSelectPopupEvent>(ClosePopup);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<OpenWeaponSelectPopupEvent>(OpenPopup);
            EventBus.Instance.Unsubscribe<CloseWeaponSelectPopupEvent>(ClosePopup);
        }
        
        private void OpenPopup(OpenWeaponSelectPopupEvent e)
        {
            openFeedback.PlayFeedbacks();
        }

        private void ClosePopup(CloseWeaponSelectPopupEvent e)
        {
            closeFeedback.PlayFeedbacks();
        }
    }
}