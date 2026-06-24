using UnityEngine;

namespace _Game.GamePlay.Utils
{
    public static class GizmosLine
    {
        public static void Circle(Vector3 center, float radius, Color color, float deltaTime, int segments = 12)
        {
#if UNITY_EDITOR
            Vector3 prev = center + Vector3.right * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Debug.DrawLine(prev, next, color, deltaTime);
                prev = next;
            }
#endif
        }
    }
}