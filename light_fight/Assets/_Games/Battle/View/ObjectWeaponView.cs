using _KITSystem.Utils;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Games.Battle.View
{
    public class ObjectWeaponView : MonoBehaviour, IWeaponView
    {
        [TitleGroup("Setting")]
        [SerializeField] private Vector3 localPositionOffsetMin = new Vector3(0,0.2f,0);
        [SerializeField] private Vector3 localPositionOffsetMax = new Vector3(0,0.5f,0);
        [TitleGroup("Feedback")] 
        [SerializeField] private AnimationCurve rotateCurve;
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
        private Coroutine rotateCoroutine;
        
        public IWeaponView Instantiate(int xPivotAngle, int zPivotAngle, ISlotView slotView)
        {
            ObjectWeaponView view = Instantiate(this, slotView.WeaponRoot);
            view.xPivot.localEulerAngles = new Vector3(xPivotAngle, 0, 0);
            view.zPivot.localEulerAngles = new Vector3(0, 0, zPivotAngle);
            view.transform.localPosition = new Vector3(0, 0, -0.1f);
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
            float waitTimePlayFeedback = delayActivate / timeScale;
            float rollbackDuration = Mathf.Min(waitTimePlayFeedback, 0.2f / timeScale);

            this.WaitInvoke(Mathf.Max(0, waitTimePlayFeedback - rollbackDuration), () =>
            {
                Rollback(rollbackDuration);
            });
            
            this.WaitInvoke(waitTimePlayFeedback, () =>
            {
                sortingGroup.sortingOrder = 0;
                deactivateFeedback.TimescaleMultiplier = timeScale;
                deactivateFeedback.PlayFeedbacks();
            });
            return (delayActivate + deactivateFeedback.TotalDuration) / timeScale;
        }

        public void Rotate(Vector3 goal, float duration, bool needUpdatePosition)
        {
            Vector3 position = zPivot.position;
            Vector3 direction = goal - position;
            direction.z = 0;
            if (direction.sqrMagnitude < 0.0001f) return;
            
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = zPivot.eulerAngles.z;
            float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
            float endAngle = currentAngle + deltaAngle;
            bool currentFlipRot = Mathf.Abs(Mathf.DeltaAngle(0f, currentAngle)) > 90f;
            
#if UNITY_EDITOR
            Vector3 v1 = position + (muzzle.position - position).normalized;
            Vector3 v2 = position + direction.normalized;
            Debug.DrawLine(position, v1, Color.yellow, 0.5f);
            Debug.DrawLine(position, v2, Color.yellow, 0.5f);
            Debug.DrawLine(v1, v2, Color.yellow, 0.5f);
#endif
            Vector3 localPositionOffset = Vector3.Lerp(
                localPositionOffsetMin, localPositionOffsetMax,
                Mathf.Clamp01(Mathf.Abs(deltaAngle) / 180f)
            );
            Vector3 directionOffset = needUpdatePosition ? localPositionOffset : Vector3.zero;
            Vector3 originLocalPosition = zPivot.localPosition;
            Vector3 targetLocalPosition = originLocalPosition + directionOffset;
            
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = this.CurveNormalize(0, 1, rotateCurve, duration, f =>
            {
                float angle = Mathf.LerpAngle(currentAngle, endAngle, f);
                zPivot.eulerAngles = new Vector3(0, 0, angle);
#if UNITY_EDITOR
                float rad = angle * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(
                    Mathf.Cos(rad),
                    Mathf.Sin(rad),
                    0);
                Debug.DrawLine(position, position + dir * 1.5f, Color.red, 0.05f);
#endif
                float signed = Mathf.DeltaAngle(0f, angle);
                bool flipRot = Mathf.Abs(signed) > 90f;
                if (flipRot != currentFlipRot)
                {
                    currentFlipRot = flipRot;
                    xPivot.localRotation = Quaternion.Euler(flipRot ? 180: 0, 0, 0);
                }

                if (needUpdatePosition)
                {
                    zPivot.localPosition = Vector3.Lerp(originLocalPosition, targetLocalPosition, f);
                }
            });
        }

        private void Rollback(float duration)
        {
            float currentAngle = zPivot.eulerAngles.z;
            float targetAngle = zEulerAngles.z;
            float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
            float endAngle = currentAngle + deltaAngle;
            bool currentFlipRot = Mathf.Abs(Mathf.DeltaAngle(0f, currentAngle)) > 90f;
            
            Vector3 originLocalPosition = zPivot.localPosition;
            bool needUpdatePosition = originLocalPosition != zLocalPosition;
            
            if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
            rotateCoroutine = this.CurveNormalize(0, 1, rotateCurve, duration, f =>
            {
                float angle = Mathf.LerpAngle(currentAngle, endAngle, f);
                zPivot.eulerAngles = new Vector3(0, 0, angle);
                float signed = Mathf.DeltaAngle(0f, angle);
                bool flipRot = Mathf.Abs(signed) > 90f;
                if (flipRot != currentFlipRot)
                {
                    currentFlipRot = flipRot;
                    xPivot.localRotation = Quaternion.Euler(flipRot ? 180: 0, 0, 0);
                }

                if (needUpdatePosition)
                {
                    zPivot.localPosition = Vector3.Lerp(originLocalPosition, zLocalPosition, f);
                }
            });
        }
    }
}