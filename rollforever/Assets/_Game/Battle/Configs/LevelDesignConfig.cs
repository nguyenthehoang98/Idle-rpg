using UnityEngine;

namespace _Game.Battle
{
    public class LevelDesignConfig : MonoBehaviour
    {
        [SerializeField] private Transform[] children;

        public Vector3[] LoopPoints()
        {
            Vector3[] points = new Vector3[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                points[i] = children[i].position;
            }

            return points;
        }

        private void OnDrawGizmos()
        {
            foreach (var child in children)
            {
                Gizmos.DrawCube(child.position, Vector3.one);
            }
        }
    }
}