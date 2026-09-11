using System;
using LitMotion.Adapters;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LitMotion.Animation.Components
{
    public abstract class ValueAnimationComponent<TValue, TOptions, TAdapter> : LitMotionAnimationComponent
        where TValue : unmanaged
        where TOptions : unmanaged, IMotionOptions
        where TAdapter : unmanaged, IMotionAdapter<TValue, TOptions>
    {
        [SerializeField] SerializableMotionSettings<TValue, TOptions> settings;
        [SerializeField] UnityEvent<TValue> onValueChanged;

        public override MotionHandle Play()
        {
            return LMotion.Create<TValue, TOptions, TAdapter>(settings)
                .Bind(this, (x, state) =>
                {
                    state.onValueChanged.Invoke(x);
                });
        }

        public override void OnStop() { }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Float")]
    public sealed class FloatValueAnimation : ValueAnimationComponent<float, NoOptions, FloatMotionAdapter>
    {
        public FloatValueAnimation()
        {
            type = "Float";
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Double")]
    public sealed class DoubleValueAnimation : ValueAnimationComponent<double, NoOptions, DoubleMotionAdapter>
    {
        public DoubleValueAnimation()
        {
            type = "Double";
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Int")]
    public sealed class IntValueAnimation : ValueAnimationComponent<int, IntegerOptions, IntMotionAdapter>
    {
        public IntValueAnimation()
        {
            type = "Int";
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Long")]
    public sealed class LongValueAnimation : ValueAnimationComponent<long, IntegerOptions, LongMotionAdapter> { }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Vector2")]
    public sealed class Vector2ValueAnimation : ValueAnimationComponent<Vector2, NoOptions, Vector2MotionAdapter>
    {
        public Vector2ValueAnimation()
        {
            type = "Vector2";
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Vector3")]
    public sealed class Vector3ValueAnimation : ValueAnimationComponent<Vector3, NoOptions, Vector3MotionAdapter>
    {
        public Vector3ValueAnimation()
        {
            type = "Vector3";
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Vector4")]
    public sealed class Vector4ValueAnimation : ValueAnimationComponent<Vector4, NoOptions, Vector4MotionAdapter> { }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/Color")]
    public sealed class ColorValueAnimation : ValueAnimationComponent<Color, NoOptions, ColorMotionAdapter> { }

    [Serializable]
    [LitMotionAnimationComponentMenu("Value/String")]
    public sealed class StringValueAnimation : LitMotionAnimationComponent
    {
        [SerializeField] SerializableMotionSettings<FixedString512Bytes, StringOptions> settings;
        [SerializeField] UnityEvent<string> onValueChanged;

        public override MotionHandle Play()
        {
            return LMotion.Create<FixedString512Bytes, StringOptions, FixedString512BytesMotionAdapter>(settings)
                .Bind(this, static (x, state) =>
                {
                    // TODO: avoid allocation
                    state.onValueChanged.Invoke(x.ConvertToString());
                });
        }

        public override void OnStop() { }
    }
}