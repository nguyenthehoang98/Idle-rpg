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

        public override float Duration()
        {
            return delay;
        }

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

        public override float Duration()
        {
            return 0;
        }

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

        private Stopwatch sw;

        public override float Duration()
        {
            return 0;
        }

        public override MotionHandle Play()
        {
            sw = Stopwatch.StartNew();

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

        public override void OnStop()
        {
            sw.Stop();

            if (!string.IsNullOrEmpty(messageOnStop))
            {
                switch (logTypeOnStop)
                {
                    case LogType.Log:
                    case LogType.Assert:
                        Debug.Log($"[{Math.Round(Time.time, 2)}] {messageOnStop} {sw.ElapsedMilliseconds}ms");
                        break;
                    case LogType.Warning:
                        Debug.LogWarning($"[{Math.Round(Time.time, 2)}] {messageOnStop} {sw.ElapsedMilliseconds}ms");
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                        Debug.LogError($"[{Math.Round(Time.time, 2)}] {messageOnStop} {sw.ElapsedMilliseconds}ms");
                        break;
                }
            }

#if UNITY_EDITOR
            if (breakOnStop) Debug.Break();
#endif
        }
    }

    [Serializable]
    [LitMotionAnimationComponentMenu("Control/Play LitMotion Animation")]
    public sealed class PlayLitMotionAnimationComponent : LitMotionAnimationComponent
    {
        [SerializeField] LitMotionAnimation target;

        public override float Duration()
        {
            return target != null ? target.Duration() : 0;
        }

        public override MotionHandle Play()
        {
            target.Play();
            return LMotion.Create(0f, 1f, float.MaxValue)
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