using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class Cone
    {
        private Vector3 center;
        private Vector3 left1;
        private Vector3 right1;
        private Vector3 left2;
        private Vector3 right2;

        public bool IsPlaying { get; private set; }
        public Weapon Weapon { get; private set; }

        public Cone(int order, Vector3 position)
        {
            center = position;
            float angle = -360f / BattleConst.MAX_DICE_NUMBER * order + 90;
            Vector3 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );
            Vector2 right = new Vector2(dir.y, -dir.x);
            
            float length1 = 2;
            float length2 = 0.4f;
            
            Vector2 baseCenter1 = position + dir * length1;
            left1 = baseCenter1 - right * length1/2f;
            right1 = baseCenter1 + right * length1/2f;
            
            Vector2 baseCenter2 = position + dir *length2;
            left2 = baseCenter2 - right * length2/2f;
            right2 = baseCenter2 + right * length2/2f;
           
            Weapon = new Weapon(position, dir);
        }

        public void Active() => IsPlaying = true;
        
        public void Inactive() => IsPlaying = false;

        public void Draw()
        {
            Vector2 v2 = left1;
            Vector2 v3 = right1;
            Vector2 v4 = right2;
            Vector2 v5 = left2;
            
            float scale = IsPlaying ? 1.0f : 0.8f;
            v2 *= scale;
            v3 *= scale;
            v4 *= scale;
            v5 *= scale;

            Color color = IsPlaying ? Color.red : Color.green;
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v4, color);
            Debug.DrawLine(v4, v5, color);
            Debug.DrawLine(v5, v2, color);
            
            Weapon.Draw(scale, color);
        }
    }
}