using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Echo.Scripts.AnimationSystem
{
    [Serializable]
    public class SpriteAnimClip
    {
        public Direction8 direction;
        public Texture textureHDR;
        public Sprite[] frames;
        public float fps = 12;
        public bool loop = true;
        public bool flip = false;
    }
}