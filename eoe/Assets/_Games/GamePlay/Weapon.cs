using UnityEngine;

namespace _Games.GamePlay
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private new SpriteRenderer renderer;

        public Color Color
        {
            get => renderer.color;
            set => renderer.color = value;
        }
        
        public Vector3 MuzzlePosition => muzzle.position;

        public float EulerAngleZ
        {
            get => transform.eulerAngles.z;
            set => transform.eulerAngles = new Vector3(0,0, value);
        }
    }
}