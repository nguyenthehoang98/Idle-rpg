using System;
using System.Collections;
using _KITSystem.Utils;
using UnityEngine;

namespace _FightCode.Battle
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

        public float Speed { private get; set; }

        private Coroutine coroutine;
        private Vector3 originalPosition;

        private void Awake()
        {
            originalPosition = transform.localPosition;;
        }

        public float Roll(int value, Action onComplete)
        {
            if (value < 1 || value > 6)
            {
                Debug.LogError("Dice value must be 1-6");
                return 0;
            }

            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(SequenceRoll(value, onComplete));
            return duration;
        }

        private IEnumerator SequenceRoll(int value, Action onComplete)
        {
            Vector3 startPos = originalPosition;
            Vector3 randomRotation = new Vector3(
                360 * randomSpinCountStart + RandomUtils.Range(0, 360),
                360 * randomSpinCountStart + RandomUtils.Range(0, 360),
                360 * randomSpinCountStart + RandomUtils.Range(0, 360)
            );
            Vector3 targetRotation = GetDiceRotation(value);

            float d = 0.08f;

            float elapsedTime;
            Vector3 originalScale;

            // visual.DOScale(squashScale, d);
            // yield return new WaitForSeconds(d);
            originalScale = visual.localScale;
            elapsedTime = 0;
            while (elapsedTime < d)
            {
                elapsedTime += Time.deltaTime * Speed;
                visual.transform.localScale = Vector3.Lerp(originalScale, squashScale, Mathf.Clamp01(elapsedTime / d));
                yield return null;
            }

            //visual.DOScale(stretchScale, d);
            //yield return new WaitForSeconds(d);
            originalScale = visual.localScale;
            elapsedTime = 0;
            while (elapsedTime < d)
            {
                elapsedTime += Time.deltaTime * Speed;
                visual.transform.localScale = Vector3.Lerp(originalScale, stretchScale, Mathf.Clamp01(elapsedTime / d));
                yield return null;
            }

            float d1 = duration * 0.5f - d * 2;
            Vector3 localPosition;
            Vector3 targetPosition;
            Vector3 localEulerAngles;
            
            // visual.DOLocalMoveY(startPos.y + jumpHeight, d1).SetEase(Ease.OutQuart);
            // visual.DOLocalRotate(randomRotation, d1, RotateMode.FastBeyond360).SetEase(Ease.Linear);
            // yield return new WaitForSeconds(d1);
            localEulerAngles = visual.eulerAngles;
            localPosition = visual.localPosition;
            targetPosition = localPosition;
            targetPosition.y = startPos.y + jumpHeight;
            elapsedTime = 0;
            while (elapsedTime < d1)
            {
                elapsedTime += Time.deltaTime * Speed;
                float p = Mathf.Clamp01(elapsedTime / d1);
                float f = EaseOutQuart(p);
                visual.localPosition = Vector3.Lerp(localPosition, targetPosition, f);
                visual.localEulerAngles = Vector3.LerpUnclamped(localEulerAngles, randomRotation, p);

                yield return null;
            }
            

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

            // visual.DOLocalMoveY(startPos.y, d1).SetEase(Ease.InQuart);
            // visual.DOLocalRotate(finalRotation, d1, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
            // yield return new WaitForSeconds(d1);
            localEulerAngles = visual.eulerAngles;
            localPosition = visual.localPosition;
            targetPosition = localPosition;
            targetPosition.y = startPos.y;
            elapsedTime = 0;
            while (elapsedTime < d1)
            {
                elapsedTime += Time.deltaTime * Speed;
                float p = Mathf.Clamp01(elapsedTime / d1);
                visual.localPosition = Vector3.Lerp(localPosition, targetPosition, EaseInQuart(p));
                visual.localEulerAngles = Vector3.LerpUnclamped(localEulerAngles, finalRotation, EaseOutQuad(p));

                yield return null;
            }
            
            // Rơi xuống
            
            onComplete?.Invoke();

            // Bounce nhẹ lúc chạm đất
            // visual.DOScale(new Vector3(1.1f, 0.85f, 1.1f), d);
            // yield return new WaitForSeconds(d);
            originalScale = visual.localScale;
            elapsedTime = 0;
            while (elapsedTime < d)
            {
                elapsedTime += Time.deltaTime * Speed;
                visual.transform.localScale = Vector3.Lerp(originalScale, new Vector3(1.1f, 0.85f, 1.1f), Mathf.Clamp01(elapsedTime / d));
                yield return null;
            }

            // visual.DOScale(Vector3.one, d);
            // yield return new WaitForSeconds(d);
            originalScale = visual.localScale;
            elapsedTime = 0;
            while (elapsedTime < d)
            {
                elapsedTime += Time.deltaTime * Speed;
                visual.transform.localScale = Vector3.Lerp(originalScale, Vector3.one, Mathf.Clamp01(elapsedTime / d));
                yield return null;
            }
            
            visual.transform.localScale = Vector3.one;
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

        private float EaseLinear(float t) => t;
        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
        private float EaseInQuart(float t) => t * t * t * t;
    }
}