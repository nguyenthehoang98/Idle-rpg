using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Battle.View
{
    public class ObjectWeaponView : MonoBehaviour, IWeaponView
    {
        [TitleGroup("Feedback")] 
        [SerializeField] private MMF_Player playFeedback;
        [SerializeField] private MMF_Player activateFeedback;
        [SerializeField] private MMF_Player deactivateFeedback;
        [TitleGroup("Element")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform zPivot;
        [SerializeField] private Transform xPivot;
        [SerializeField] private SortingGroup sortingGroup; // animator/animation

        private Vector3 zEulerAngles;
        private Vector3 zLocalPosition;
        
        public IWeaponView Instantiate(int xPivotAngle, int zPivotAngle, ISlotView slotView)
        {
            ObjectWeaponView view = Instantiate(this, slotView.WeaponRoot);
            view.xPivot.localEulerAngles = new Vector3(xPivotAngle, 0, 0);
            view.zPivot.localEulerAngles = new Vector3(0, 0, zPivotAngle);
            return view;
        }

        public void Play(float timeScale)
        {
            zEulerAngles = zPivot.eulerAngles;
            zLocalPosition = zPivot.localPosition;
            playFeedback.TimescaleMultiplier = timeScale;
            playFeedback.PlayFeedbacks();
        }

        public float Activate(float delayActivate, float timeScale)
        {
            this.WaitInvoke(delayActivate / timeScale, () =>
            {
                sortingGroup.sortingOrder = 1;
                activateFeedback.TimescaleMultiplier = timeScale;
                activateFeedback.PlayFeedbacks();
            });
            return (delayActivate + activateFeedback.TotalDuration) / timeScale;
        }

        public float Deactivate(float delayActivate,float timeScale)
        {
            this.WaitInvoke(delayActivate / timeScale, () =>
            {
                sortingGroup.sortingOrder = 0;
                deactivateFeedback.TimescaleMultiplier = timeScale;
                deactivateFeedback.PlayFeedbacks();
            });
            return (delayActivate + deactivateFeedback.TotalDuration) / timeScale;
        }
    }
}