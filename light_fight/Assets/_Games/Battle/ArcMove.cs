using System;
using DG.Tweening;
using UnityEngine;

public class ArcMove : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private TrailRenderer trail;

    public void MoveTo(Vector3 start, Vector3 target,
        float duration, float radius, float offsetY, float smooth, Action onComplete)
    {
        visual.transform.position = start;
        visual.gameObject.SetActive(true);
        trail.enabled = false;
        
        Vector3 begin = start + Vector3.up * offsetY;
        Vector3 end = target;
        Vector3 dir = (end - begin).normalized;
        Vector3 normal = Vector3.Cross(dir, Vector3.forward);
        DOTween.Sequence()
            .Append(visual.DOMoveY(begin.y, 0.2f).SetEase(Ease.OutQuad))
            .AppendCallback(() => trail.enabled = true)
            .Append(DOVirtual.Float(0, 1, duration - 0.2f, t =>
            {
                Vector3 linear = Vector3.Lerp(begin, end, t);
                float height = Mathf.Sin(t * Mathf.PI);
                Vector3 offset = normal * (height * radius);
                visual.position = Vector3.Lerp(linear, linear + offset, smooth);
            })).SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                visual.transform.position = end;
                visual.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }
}