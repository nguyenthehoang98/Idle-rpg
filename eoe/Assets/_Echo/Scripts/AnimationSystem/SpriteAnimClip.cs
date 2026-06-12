using System;
using UnityEngine;

namespace _Echo.Scripts.AnimationSystem
{
    [Serializable]
    public class SpriteAnimClip
    {
        public Direction direction;
        public Texture textureHDR;
        public Sprite[] frames;
        public float fps = 12;
        public bool loop = true;
        public bool flip = false;
    }
}