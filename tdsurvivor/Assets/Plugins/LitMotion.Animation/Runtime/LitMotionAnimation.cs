using System;
using System.Collections.Generic;
using LitMotion.Collections;
using UnityEngine;

namespace LitMotion.Animation
{
    [AddComponentMenu("LitMotion Animation")]
    public sealed class LitMotionAnimation : MonoBehaviour, ISerializationCallbackReceiver
    {
        enum AutoPlayMode
        {
            None,
            OnStart,
            OnEnable
        }

        enum AutoStopMode
        {
            None,
            OnDisable,
        }

        enum AnimationMode
        {
            Parallel,
            Sequential
        }

        [SerializeField] private bool isReverseWhenStop = false;
        [SerializeField] AutoStopMode autoStopMode = AutoStopMode.OnDisable;
        [SerializeField] AutoPlayMode autoPlayMode = AutoPlayMode.OnStart;
        [SerializeField] AnimationMode animationMode;

        [SerializeReference] LitMotionAnimationComponent[] components;

        readonly Queue<LitMotionAnimationComponent> queue = new();
        FastListCore<LitMotionAnimationComponent> playingComponents;

        [HideInInspector, SerializeField] bool playOnAwake = true;
        [HideInInspector, SerializeField] int version;

        public IReadOnlyList<LitMotionAnimationComponent> Components => components;

        void OnEnable()
        {
            if (autoPlayMode == AutoPlayMode.OnEnable)
                Play();
        }

        void Start()
        {
            if (autoPlayMode == AutoPlayMode.OnStart)
                Play();
        }

        void MoveNextMotion()
        {
            if (queue.TryDequeue(out LitMotionAnimationComponent queuedComponent))
            {
                try
                {
                    MotionHandle handle = queuedComponent.Play();
                    bool isActive = handle.IsActive();

                    if (isActive)
                    {
                        handle.Preserve();
                        MotionManager.GetManagedDataRef(handle, false).OnCompleteAction += () =>
                        {
                            MoveNextMotion();
                            CheckStop();
                        };
                    }

                    queuedComponent.TrackedHandle = handle;
                    playingComponents.Add(queuedComponent);

                    if (!isActive)
                    {
                        MoveNextMotion();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public float Duration()
        {
            float duration = 0;
            switch (animationMode)
            {
                case AnimationMode.Sequential:
                    foreach (LitMotionAnimationComponent component in components)
                    {
                        if (component == null) continue;
                        if (!component.Enabled) continue;

                        duration += component.Duration();
                    }

                    MoveNextMotion();
                    break;
                case AnimationMode.Parallel:
                    foreach (LitMotionAnimationComponent component in components)
                    {
                        if (component == null) continue;
                        if (!component.Enabled) continue;

                        duration = Mathf.Max(duration, component.Duration());
                    }

                    break;
            }

            return duration;
        }

        public void Play()
        {
            bool isPlaying = false;

            foreach (LitMotionAnimationComponent component in playingComponents.AsSpan())
            {
                MotionHandle handle = component.TrackedHandle;
                if (handle.IsActive())
                {
                    handle.PlaybackSpeed = 1f;

                    isPlaying = true;

                    component.OnResume();
                }
            }

            if (isPlaying) return;

            playingComponents.Clear();

            switch (animationMode)
            {
                case AnimationMode.Sequential:
                    foreach (LitMotionAnimationComponent component in components)
                    {
                        if (component == null) continue;

                        if (!component.Enabled) continue;

                        queue.Enqueue(component);
                    }

                    MoveNextMotion();
                    break;
                case AnimationMode.Parallel:
                    foreach (LitMotionAnimationComponent component in components)
                    {
                        if (component == null) continue;
                        if (!component.Enabled) continue;

                        try
                        {
                            MotionHandle handle = component.Play();
                            component.TrackedHandle = handle;

                            if (handle.IsActive())
                            {
                                handle.Preserve();
                            }

                            MotionManager.GetManagedDataRef(handle, false).OnCompleteAction += CheckStop;

                            playingComponents.Add(component);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }

                    break;
            }
        }

        public void Pause()
        {
            foreach (LitMotionAnimationComponent component in playingComponents.AsSpan())
            {
                MotionHandle handle = component.TrackedHandle;
                if (handle.IsActive())
                {
                    handle.PlaybackSpeed = 0f;
                    component.OnPause();
                }
            }
        }

        private void CheckStop()
        {
            double time = 0;
            double duration = 0;
            foreach (LitMotionAnimationComponent comp in playingComponents.AsSpan())
            {
                time = Math.Max(time, comp.TrackedHandle.Time);
                if (animationMode == AnimationMode.Sequential)
                    duration += comp.TrackedHandle.TotalDuration;
                else
                    duration = Math.Max(duration, comp.TrackedHandle.TotalDuration);
            }

            if (time >= duration)
            {
                if (Application.isPlaying && isActiveAndEnabled)
                {
                    Stop();
                }
            }
        }

        public void Stop()
        {
            Span<LitMotionAnimationComponent> span = playingComponents.AsSpan();
            span.Reverse();
            foreach (LitMotionAnimationComponent component in span)
            {
                MotionHandle handle = component.TrackedHandle;
                handle.TryCancel();
                if (isReverseWhenStop) component.OnStop();
                component.TrackedHandle = handle;
            }

            playingComponents.Clear();
            queue.Clear();
        }

        public void Restart()
        {
            Stop();
            Play();
        }

        public bool IsActive
        {
            get
            {
                if (queue.Count > 0) return true;

                foreach (LitMotionAnimationComponent component in playingComponents.AsSpan())
                {
                    MotionHandle handle = component.TrackedHandle;
                    if (handle.IsActive()) return true;
                }

                return false;
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (queue.Count > 0) return true;

                foreach (LitMotionAnimationComponent component in playingComponents.AsSpan())
                {
                    MotionHandle handle = component.TrackedHandle;
                    if (handle.IsPlaying()) return true;
                }

                return false;
            }
        }

        void OnDisable()
        {
            if (autoStopMode == AutoStopMode.OnDisable)
                Stop();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (version < 1)
            {
                autoPlayMode = playOnAwake ? AutoPlayMode.OnStart : AutoPlayMode.None;
                version = 1;
            }
        }
    }
}