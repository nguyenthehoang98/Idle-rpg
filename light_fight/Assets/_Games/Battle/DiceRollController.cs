using System;
using System.Collections;
using _KITSystem.Utils;
using DG.Tweening;
using UnityEngine;

namespace _Games.Battle
{
    public class DiceRollController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visual;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 3f;
        [SerializeField] private float duration = 1.2f;

        [Header("Rotation")]
        [SerializeField] private int randomSpinCountStart = 4;
        [SerializeField] private int randomSpinCountEnd = 4;

        [Header("Scale FX")]
        [SerializeField] private Vector3 squashScale = new Vector3(1.1f, 0.9f, 1.1f);
        [SerializeField] private Vector3 stretchScale = new Vector3(0.9f, 1.1f, 0.9f);

        [ContextMenu("Roll")]
        public void Roll()
        {
            Roll(RandomUtils.Range(1, 6), null);
        }

        public void Roll(int value, Action onComplete)
        {
            if (value < 1 || value > 6)
            {
                Debug.LogError("Dice value must be 1-6");
                return;
            }

            StartCoroutine(SequenceRoll(value, onComplete));
        }

        private IEnumerator SequenceRoll(int value, Action onComplete)
        {
            Vector3 startPos = transform.localPosition;
            Vector3 randomRotation = new Vector3(
                360 * randomSpinCountStart + RandomUtils.Range(0, 360),
                360 * randomSpinCountStart + RandomUtils.Range(0, 360),
                360 * randomSpinCountStart + RandomUtils.Range(0, 360)
            );
            Vector3 targetRotation = GetDiceRotation(value);
        
            float d = 0.08f;
            visual.DOScale(squashScale, d);
            yield return new WaitForSeconds(d);
            visual.DOScale(stretchScale, d);
            yield return new WaitForSeconds(d);

            float d1 = duration * 0.5f - d * 2;
            visual.DOLocalMoveY(startPos.y + jumpHeight, d1).SetEase(Ease.OutQuart);
            visual.DOLocalRotate(randomRotation, d1, RotateMode.FastBeyond360).SetEase(Ease.Linear);
            yield return new WaitForSeconds(d1);

            Vector3 currentEuler = visual.localEulerAngles;
            Vector3 extraSpin = new Vector3(
                360 * randomSpinCountEnd + RandomUtils.Range(0, 360),
                360 * randomSpinCountEnd + RandomUtils.Range(0, 360),
                360 * randomSpinCountEnd + RandomUtils.Range(0, 360)
            );

            Vector3 finalRotation = currentEuler + extraSpin;
            finalRotation.x += Mathf.DeltaAngle(finalRotation.x, targetRotation.x);
            finalRotation.y += Mathf.DeltaAngle(finalRotation.y, targetRotation.y);
            finalRotation.z += Mathf.DeltaAngle(finalRotation.z, targetRotation.z);

            // Rơi xuống
            visual.DOLocalMoveY(startPos.y, d1).SetEase(Ease.InQuart);
            visual.DOLocalRotate(finalRotation, d1, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(d1);
        
            onComplete?.Invoke();

            // Bounce nhẹ lúc chạm đất
            visual.DOScale(new Vector3(1.1f, 0.85f, 1.1f), d);
            yield return new WaitForSeconds(d);

            visual.DOScale(Vector3.one, d);
            yield return new WaitForSeconds(d);
        }

        private Vector3 GetDiceRotation(int value)
        {
            switch (value)
            {
                case 1: return new Vector3(0, -90, 90);
                case 2: return new Vector3(0, 0, 0);
                case 3: return new Vector3(0, -180, 0);
                case 4: return new Vector3(90, 0, 0);
                case 5: return new Vector3(0, -90, 0);
                case 6: return new Vector3(0, 90, 0);
                default: return Vector3.zero;
            }
        }
    }
}