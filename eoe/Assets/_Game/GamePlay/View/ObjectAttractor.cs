using System;
using _KITSystem.Utils;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace _Game.GamePlay.View
{
    public class ObjectAttractor : MonoBehaviour
    {
        [SerializeField] private Vector3 localScale = new Vector3(0.35f, 0.35f, 0.35f);
        [SerializeField] private Vector3 to = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Transform visual;
        //[SerializeField] private TrailRenderer trail;

        private Sequence sequence;
        private Vector3 originalPosition;

        protected virtual void Awake()
        {
            Debug.Log(@"Thiếu trail");
            visual.gameObject.SetActive(false);
            //trail.enabled = false;
            originalPosition = visual.position;
            Debug.Log(originalPosition);
        }

        public void ResetPosition()
        {
            visual.position = originalPosition;
        }

        public Vector3 OriginalPosition
        {
            get { return originalPosition; }
        }

        public void MoveTo(Vector3 start, Vector3 target, Vector3 rot, float flyToTargetDelay,
            float duration, float radius, float offsetY,
            float smooth, Action onComplete)
        {
            if (sequence != null && sequence.IsPlaying()) sequence.Kill();

            visual.localScale = localScale;
            visual.rotation = Quaternion.Euler(0, 0, 0);
            visual.position = start;
            Active();

            bool completed = false;
            float d = duration * 0.6f;
            Vector3 begin = start + Vector3.up * offsetY;
            Vector3 end = target;
            Vector3 dir = (end - begin).normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward);

            sequence = DOTween.Sequence()
                .Append(DOVirtual.Float(0, 1f, d, t => { visual.position = Vector3.Lerp(start, begin, t); }).SetEase(Ease.InOutSine))
                .AppendInterval(d)
                .AppendInterval(flyToTargetDelay)
                .Append(DOVirtual.Float(0, 1, duration - d, t =>
                {
                    if (!completed)
                    {
                        Vector3 linear = Vector3.Lerp(begin, end, t);
                        float height = Mathf.Sin(t * Mathf.PI);
                        Vector3 offset = normal * (height * radius);
                        visual.position = Vector3.Lerp(linear, linear + offset, smooth);
                        visual.rotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, rot, t));
                        visual.localScale = Vector3.Lerp(localScale, to, t);
                    }

                    if (math.lengthsq(visual.position - end) <= 0.01f && !completed)
                    {
                        completed = true;
                        
                        visual.gameObject.SetActive(false);
                        
                        onComplete?.Invoke();
                    }
                })).SetEase(Ease.OutQuart);
        }

        void Active()
        {
            this.WaitNextFrame(() =>
            {
                //trail.enabled = true;
                
                visual.gameObject.SetActive(true);
            });
        }
    }
}