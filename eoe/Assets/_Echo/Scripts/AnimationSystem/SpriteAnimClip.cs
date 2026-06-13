using System;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    [Serializable]
    public class SpriteAnimClip
    {
        public Direction direction;
        public Texture textureHDR;
        [HideInInspector] public int frameEvent = -1;
        public Sprite[] frames;
        public float fps = 12;
        public bool loop = true;
        public bool flip = false;
    }
}