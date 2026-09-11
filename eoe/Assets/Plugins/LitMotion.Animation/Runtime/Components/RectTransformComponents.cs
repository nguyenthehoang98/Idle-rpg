using System;
using LitMotion.Animation;
using UnityEngine;

[Serializable]
[LitMotionAnimationComponentMenu("UI/Rect Transform/Size Delta")]
public sealed class RectTransformSizeDeltaAnimation : Vector2PropertyAnimationComponent<RectTransform>
{
    public RectTransformSizeDeltaAnimation() : base() {
        type = "SizeDelta";
    }
    
    protected override Vector2 GetValue(RectTransform target) => target.sizeDelta;
    protected override void SetValue(RectTransform target, in Vector2 value) => target.sizeDelta = value;
}

[Serializable]
[LitMotionAnimationComponentMenu("UI/Rect Transform/Pivot")]
public sealed class RectTransformPivotAnimation : Vector2PropertyAnimationComponent<RectTransform>
{
    public RectTransformPivotAnimation() : base(){
        type = "Pivot";
    }
    
    protected override Vector2 GetValue(RectTransform target) => target.pivot;
    protected override void SetValue(RectTransform target, in Vector2 value) => target.pivot = value;
}

[Serializable]
[LitMotionAnimationComponentMenu("UI/Rect Transform/Anchored Position")]
public sealed class RectTransformAnchoredPositionAnimation : Vector2PropertyAnimationComponent<RectTransform>
{
    public RectTransformAnchoredPositionAnimation() : base()
    {
        type = "Anchored Position";
    }
    
    protected override Vector2 GetValue(RectTransform target) => target.anchoredPosition;
    protected override void SetValue(RectTransform target, in Vector2 value) => target.anchoredPosition = value;
}

[Serializable]
[LitMotionAnimationComponentMenu("UI/Rect Transform/Anchored Position 3D")]
public sealed class RectTransformPosition3DAnimation : Vector3PropertyAnimationComponent<RectTransform>
{
    public RectTransformPosition3DAnimation() : base()
    {
        type = "Anchored Position 3D";
    }
    
    protected override Vector3 GetValue(RectTransform target) => target.anchoredPosition3D;
    protected override void SetValue(RectTransform target, in Vector3 value) => target.anchoredPosition3D = value;
}