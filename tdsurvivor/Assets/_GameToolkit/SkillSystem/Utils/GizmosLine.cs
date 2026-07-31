using UnityEngine;

namespace _GameToolkit.SkillSystem.Utils
{
    /// <summary>
    /// Debug draw helpers (Debug.DrawLine wrappers). Ported from _Game.GamePlay.Utils
    /// để SkillSystem không phụ thuộc vào project game.
    /// </summary>
    public static class GizmosLine
    {
        public static void Line(Vector3 center, Vector3 to, Color color, float deltaTime)
        {
            Debug.DrawLine(center, to, color, deltaTime);
        }

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

        public static void Rectangle(Vector2 center, Vector2 direction, Vector2 size, Color color, float deltaTime)
        {
            Vector2 bl, br, tr, tl;

            float halfForward = size.x * 0.5f;
            float halfRight = size.y * 0.5f;

            Vector2 right = new Vector2(direction.y, -direction.x);
            Vector2 hf = direction * halfForward;
            Vector2 hr = right * halfRight;

            bl = center - hf - hr;
            br = center - hf + hr;
            tr = center + hf + hr;
            tl = center + hf - hr;

            Debug.DrawLine(bl, br, color, deltaTime);
            Debug.DrawLine(br, tr, color, deltaTime);
            Debug.DrawLine(tr, tl, color, deltaTime);
            Debug.DrawLine(tl, bl, color, deltaTime);
        }

        public static void Rectangle(Vector2 center, Vector2 size, Color color, float deltaTime)
        {
            Vector2 bl, br, tr, tl;

            Vector2 half = size * 0.5f;

            bl = center + new Vector2(-half.x, -half.y); // Bottom Left
            br = center + new Vector2( half.x, -half.y); // Bottom Right
            tr = center + new Vector2( half.x,  half.y); // Top Right
            tl = center + new Vector2(-half.x,  half.y); // Top Left

            Debug.DrawLine(bl, br, color, deltaTime);
            Debug.DrawLine(br, tr, color, deltaTime);
            Debug.DrawLine(tr, tl, color, deltaTime);
            Debug.DrawLine(tl, bl, color, deltaTime);
        }
    }
}
