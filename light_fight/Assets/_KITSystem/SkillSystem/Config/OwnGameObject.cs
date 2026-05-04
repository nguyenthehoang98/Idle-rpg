using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    public sealed class OwnGameObject
    {
        private GameObject gameObject;

        public OwnGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        private Vector3 position;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
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
                gameObject.transform.rotation = rotation;
            }
        }
    }
}