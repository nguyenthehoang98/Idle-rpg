using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.GamePlay.SoundSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        Dictionary<AudioClip, float> triggerPlay = new Dictionary<AudioClip, float>();
        private AudioSource audioSource;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f, float interval = 0.1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("SoundManager::PlayOneShot: clip is null");
                return;
            }
            
            if (triggerPlay.TryGetValue(clip, out float time))
            {
                if (Time.time - time < interval) return;
            }

            audioSource.PlayOneShot(clip, volume);
         
            triggerPlay[clip] = Time.time;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
