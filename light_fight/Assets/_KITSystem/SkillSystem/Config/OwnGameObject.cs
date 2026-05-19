using System;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public sealed class OwnGameObject : IDisposable
    {
        private GameObject gameObject;
        private readonly int hash;

        private static int Count { get; set; }

        public OwnGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            hash = Count++;
        }

        private Vector3 position;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                if (gameObject != null)
                    gameObject.transform.position = position;
            }
        }

        private Quaternion rotation;

        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                if (gameObject != null)
                    gameObject.transform.rotation = rotation;
            }
        }

        public override int GetHashCode() => hash;

        public void Dispose()
        {

        }
    }
}