using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class Cone
    {
        [SerializeField] private int order;
        [SerializeField] private Weapon weapon = new Weapon();
        
        private Vector3 position;
        public bool IsPlaying { get; private set; }
        public Weapon Weapon => weapon;

        public Cone(int order, Vector3 position)
        {
            this.order = order;
            this.position = position;
        }

        public void Active() => IsPlaying = true;
        
        public void Inactive() => IsPlaying = false;

        public void Focus(Vector3 goalPosition)
        {
        }

        public void Draw()
        {
            float angle = -360f / BattleConst.MAX_DICE_NUMBER * order + 90;
            float height = 2;
            Vector2 v3 = position;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector2 right = new Vector2(dir.y, -dir.x);
            float halfBase = height / 2;

            Vector2 baseCenter = v3 + dir * height;
            Vector2 v1 = baseCenter - right * halfBase;
            Vector2 v2 = baseCenter + right * halfBase;

            float scale = IsPlaying ? 1.0f : 0.8f;
            v1 *= scale;
            v2 *= scale;
            v3 *= scale;

            Color color = IsPlaying ? Color.red : Color.green;
            
            Debug.DrawLine(v1, v2, color);
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v1, color);
            
            weapon.Draw(position, dir, scale, color);
        }
    }
}