using System;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace LitMotion.Animation.Components
{
    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Delay")]
    public sealed class DelayComponent : LitMotionAnimationComponent
    {
        [SerializeField] float delay;

        public override float Duration() => delay;

        public override MotionHandle Play()
        {
            return LMotion.Create(0f, 1f, delay)
                .RunWithoutBinding();
        }

        public override void OnStop() { }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Event")]
    public sealed class EventComponent : LitMotionAnimationComponent
    {
        [Space(5f)]
        [SerializeField] UnityEvent onPlay;
        [SerializeField] UnityEvent onStop;

        public override float Duration() => 0f;

        public override MotionHandle Play()
        {
            onPlay.Invoke();
            return LMotion.Create(0f, 1f, 0f).RunWithoutBinding();
        }

        public override void OnStop()
        {
            onStop.Invoke();
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Execute")]
    public sealed class ExecuteComponent : LitMotionAnimationComponent
    {
        [SerializeField, Min(0)] float waitTime;
        [SerializeField] UnityEvent onExecute;

        public ExecuteComponent()
        {
            type = "Execute";
        }

        public override float Duration() => waitTime == 0f ? Mathf.Epsilon : waitTime;

        public override MotionHandle Play()
        {
            if (waitTime <= 0f)
            {
                onExecute?.Invoke();
                return MotionHandle.None;
            }

            return LMotion.Create(0f, 1f, waitTime)
                .WithOnComplete(() => onExecute?.Invoke())
                .RunWithoutBinding();
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Debug")]
    public sealed class DebugComponent : LitMotionAnimationComponent
    {
        [SerializeField] string messageOnPlay;
        [SerializeField] LogType logTypeOnPlay = LogType.Log;
        [SerializeField] bool breakOnPlay;

        public override MotionHandle Play()
        {
            if (!string.IsNullOrEmpty(messageOnPlay))
            {
                switch (logTypeOnPlay)
                {
                    case LogType.Log:
                    case LogType.Assert:
                        Debug.Log($"[{Math.Round(Time.time, 2)}] {messageOnPlay}");
                        break;
                    case LogType.Warning:
                        Debug.LogWarning($"[{Math.Round(Time.time, 2)}] {messageOnPlay}");
                        break;
                    default:
                        Debug.LogError($"[{Math.Round(Time.time, 2)}] {messageOnPlay}");
                        break;
                }
            }

#if UNITY_EDITOR
            if (breakOnPlay) Debug.Break();
#endif
            return LMotion.Create(0f, 1f, 0f).RunWithoutBinding();
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Play LitMotion Animation")]
    public sealed class PlayLitMotionAnimationComponent : LitMotionAnimationComponent
    {
        [SerializeField] LitMotionAnimation target;

        public PlayLitMotionAnimationComponent()
        {
            type = "Play LitMotion";
        }

        public override float Duration() => target != null ? target.Duration() : 0f;

        public override MotionHandle Play()
        {
            target.Play();
            return LMotion.Create(0f, 1f, target.Duration())
                .Bind(this, (x, state) =>
                {
                    if (target == null) TrackedHandle.TryComplete();
                    if (!target.IsPlaying) TrackedHandle.TryComplete();
                });
        }

        public override void OnResume()
        {
            target.Play();
        }

        public override void OnPause()
        {
            target.Pause();
        }

        public override void OnStop()
        {
            target.Stop();
        }
    }
}