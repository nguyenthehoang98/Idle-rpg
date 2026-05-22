using System;
using _KITSystem.Utils;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace _Games.Battle
{
    public class ArcMove : MonoBehaviour
    {
        [SerializeField] private Vector3 localScale = new Vector3(0.35f, 0.35f, 0.35f);
        [SerializeField] private Vector3 to = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private Transform visual;
        [SerializeField] private TrailRenderer trail;

        private void Awake()
        {
            trail.enabled = false;
            visual.gameObject.SetActive(false);
        }

        public void MoveTo(Vector3 start, Vector3 target, Vector3 rot, float delay,
            float duration, float radius, float offsetY, float smooth, Action onComplete)
        {
            visual.transform.localScale = localScale;
            visual.transform.rotation = Quaternion.Euler(0, 0, 0);
            visual.transform.position = start;
            visual.gameObject.SetActive(true);
            trail.enabled = false;
            bool completed = false;
            ActiveTrail();
            float d = duration * 0.6f;
            Vector3 begin = start + Vector3.up * offsetY;
            Vector3 end = target;
            Vector3 dir = (end - begin).normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.forward);
            DOTween.Sequence()
                .Append(DOVirtual.Float(0, 1f, d, t =>
                {
                    visual.position = Vector3.Lerp(start, begin, t);
                }).SetEase(Ease.InOutSine))
                .AppendInterval(d)
                .AppendInterval(delay)
                .Append(DOVirtual.Float(0, 1, duration - d, t =>
                {
                    Vector3 linear = Vector3.Lerp(begin, end, t);
                    float height = Mathf.Sin(t * Mathf.PI);
                    Vector3 offset = normal * (height * radius);
                    visual.position = Vector3.Lerp(linear, linear + offset, smooth);
                    visual.rotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, rot, t));
                    visual.localScale = Vector3.Lerp(localScale, to, t);
                
                    float s = math.lengthsq(visual.position - end);
                    if (s <= 0.01f && !completed)
                    {
                        completed = true;
                        this.WaitInvoke(0.15f, () =>
                        {
                            onComplete?.Invoke();
                            this.WaitNextFrame(() =>
                            {
                                visual.transform.position = end;
                                visual.gameObject.SetActive(false);
                            });
                        });
                    }
                
                })).SetEase(Ease.OutQuart);
        }

        void ActiveTrail()
        {
            this.WaitNextFrame(() => trail.enabled = true);
        }
    }
}