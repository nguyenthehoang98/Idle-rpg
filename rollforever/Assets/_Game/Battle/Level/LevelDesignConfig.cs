using UnityEngine;

namespace _Game.Battle.Level
{
    public class LevelDesignConfig : MonoBehaviour
    {
        [SerializeField] private Transform[] children;

        public Vector2[] LoopPoints()
        {
            Vector2[] points = new Vector2[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                points[i] = children[i].position;
            }

            return points;
        }
    }
}