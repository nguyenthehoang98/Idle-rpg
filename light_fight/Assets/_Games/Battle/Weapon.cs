using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class Weapon
    {
        private float forwardOffset = 1.5f;

        public void Draw(Vector3 position, Vector3 direction, float scale, Color color)
        {
            float size = 0.3f;

            Vector3 center = position + direction.normalized * forwardOffset * scale;
            direction.Normalize();

            Vector3 right = new Vector3(-direction.y, direction.x, 0f);
            float half = size * 0.5f;

            // 4 góc hình vuông
            Vector3 v1 = center - right * half - direction * half;
            Vector3 v2 = center + right * half - direction * half;
            Vector3 v3 = center + right * half + direction * half;
            Vector3 v4 = center - right * half + direction * half;

            // draw
            Debug.DrawLine(v1, v2, color);
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v4, color);
            Debug.DrawLine(v4, v1, color);
        }

        public void RotateTo(Vector3 worldPos, bool needUpdatePosition, object o)
        {
        }
    }
}