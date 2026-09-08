using System;
using System.Diagnostics;
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
        [SerializeField] private UnityEvent onExecute;
        
        public override float Duration()
        {
            if (delay == 0) return Mathf.Epsilon;
            return delay;
        }

        public override MotionHandle Play()
        {
            if (delay > 0)
            {
                return LMotion.Create(0f, 1f, delay)
                    .WithOnComplete(() => { onExecute?.Invoke(); })
                    .RunWithoutBinding();                
            }
            else
            {
                onExecute?.Invoke();
                return MotionHandle.None;
            }
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Event")]
    public sealed class EventComponent : LitMotionAnimationComponent
    {
        [Space(5f)]
        [SerializeField] UnityEvent onPlay;
        [SerializeField] UnityEvent onStop;

        public override float Duration()
        {
            return 0;
        }

        public override MotionHandle Play()
        {
            onPlay.Invoke();
            return LMotion.Create(0f, 1f, 0f).RunWithoutBinding();
        }

        /*public override void OnStop()
        {
            onStop.Invoke();
        }*/
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Debug")]
    public sealed class DebugComponent : LitMotionAnimationComponent
    {
        [Space(5f)]
        [SerializeField] string messageOnPlay;
        [SerializeField] LogType logTypeOnPlay = LogType.Log;
        [SerializeField] bool breakOnPlay = false;
        [SerializeField] string messageOnStop;
        [SerializeField] LogType logTypeOnStop = LogType.Log;
        [SerializeField] bool breakOnStop = false;

        public override float Duration()
        {
            return 0;
        }

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
                    case LogType.Error:
                    case LogType.Exception:
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

        public PlayLitMotionAnimationComponent() : base()
        {
            type = "Play LitMotion";
        }
        
        public override float Duration()
        {
            return target != null ? target.Duration() : 0;
        }

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

        /*public override void OnStop()
        {
            target.Stop();
        }*/
    }
}