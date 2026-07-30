// System
using System;

// Unity
using UnityEngine.Events;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Detector;

namespace GUPS.AntiCheat.Detector
{
    /// <summary>
    /// Serializable <see cref="UnityEvent{T}"/> carrying an <see cref="IDetectorStatus"/>, used to wire up detector
    /// callbacks from the inspector.
    /// </summary>
    /// <typeparam name="T">The concrete detector status type raised by the detector.</typeparam>
    [Serializable]
    public class CheatingDetectionEvent<T> : UnityEvent<T>
        where T : IDetectorStatus
    {
    }
}
