using UnityEngine;

namespace _Games.GamePlay.AnimationSystem
{
    [CreateAssetMenu]
    public class AnimationAsset : ScriptableObject
    {
        public int frameEvent = -1;
        public AnimationClipData[] Clips = new AnimationClipData[0];

        private void OnValidate()
        {
            for (int i = 0; i < Clips.Length; i++)
            {
                Clips[i].frameEvent = frameEvent;
            }
        }
    }
}