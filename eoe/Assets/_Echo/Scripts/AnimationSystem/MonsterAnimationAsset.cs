using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    [CreateAssetMenu]
    public class MonsterAnimationAsset : ScriptableObject
    {
        public int frameEvent = -1;
        public SpriteAnimClip[] Clips = new SpriteAnimClip[0];

        private void OnValidate()
        {
            for (int i = 0; i < Clips.Length; i++)
            {
                Clips[i].frameEvent = frameEvent;
            }
        }
    }
}