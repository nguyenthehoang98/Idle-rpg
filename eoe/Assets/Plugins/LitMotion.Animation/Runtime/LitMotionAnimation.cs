using System;
using System.Collections.Generic;
using System.Text;
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

        [SerializeField] private bool debug;
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

        private HashSet<int> handlesParallel = new HashSet<int>();

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
                        MotionManager.GetManagedDataRef(handle).OnCompleteAction += () =>
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
            if (components == null) return 0;
             
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

                    handlesParallel.Clear();
                    
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

                            MotionManager.GetManagedDataRef(handle).OnCompleteAction += () =>
                            {
                                handlesParallel.Remove(handle.GetHashCode());

                                if (handlesParallel.Count == 0) CheckStop();
                            };

                            playingComponents.Add(component);

                            handlesParallel.Add(handle.GetHashCode());
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Error at " + GetPath(transform) + "\n" + ex.Message);
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
            if (animationMode == AnimationMode.Sequential && IsPlaying) return;

            if (animationMode == AnimationMode.Parallel && handlesParallel.Count > 0) return;
            
            if (Application.isPlaying && isActiveAndEnabled) Stop();
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
        
        private static string GetPath(Transform target)
        {
            string path = target.name;

            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }

            return path;
        }
    }
}