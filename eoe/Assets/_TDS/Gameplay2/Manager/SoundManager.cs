using System.Collections.Generic;
using UnityEngine;

namespace _TDS.Gameplay.Manager
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private float defaultPlayInterval = 0.1f;

        private readonly Dictionary<AudioClip, float> lastPlayTimes = new();

        private AudioSource audioSource;

        private void Awake()
        {
            Instance = this;
            
            audioSource = GetComponent<AudioSource>();
            
            DontDestroyOnLoad(gameObject);
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f, float interval = -1f)
        {
            if (clip == null)
            {
                Debug.LogWarning($"{nameof(SoundManager)}::PlayOneShot - Clip is null.");
                return;
            }

            interval = interval < 0f ? defaultPlayInterval : interval;

            if (lastPlayTimes.TryGetValue(clip, out float lastPlayTime))
            {
                if (Time.time - lastPlayTime < interval)
                    return;
            }

#if UNITY_EDITOR
            Debug.Log($"[Sound] PlayOneShot {clip.name}");
#endif
            
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));

            lastPlayTimes[clip] = Time.time;
        }

        public bool CanPlay(AudioClip clip, float interval)
        {
            if (clip == null)
                return false;

            return !lastPlayTimes.TryGetValue(clip, out float lastPlayTime)
                   || Time.time - lastPlayTime >= interval;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}