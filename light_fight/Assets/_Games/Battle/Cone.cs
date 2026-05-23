using System.Collections;
using _KITSystem.Resource;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class Cone
    {
        private BattleMode mode;
        private Weapon weapon;
        private ConeView view;
#if UNITY_EDITOR
        private Vector3 left1;
        private Vector3 right1;
        private Vector3 left2;
        private Vector3 right2;
#endif

        public bool IsPlaying { get; private set; }

        public Cone(BattleShare share, int order, BattleSetting setting, IQuery query)
        {
            float angle = -360f / BattleConst.MAX_DICE_NUMBER * order + 90;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

#if UNITY_EDITOR
            Vector2 center = setting.center;
            Vector2 right = new Vector2(dir.y, -dir.x);
            
            float length1 = 2;
            float length2 = 0.4f;
            
            Vector2 baseCenter1 = center + dir * length1;
            left1 = baseCenter1 - right * length1/2f;
            right1 = baseCenter1 + right * length1/2f;
            
            Vector2 baseCenter2 = center + dir *length2;
            left2 = baseCenter2 - right * length2/2f;
            right2 = baseCenter2 + right * length2/2f;
#endif
           
            weapon = new Weapon(setting, query, dir);
            mode = setting.mode;
            if (mode == BattleMode.Default)
            {
                view = Object.Instantiate(setting.coneView, share.coneParent);
                view.transform.localRotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }

        public float Init()
        {
            if (mode == BattleMode.Default)
            {
                view.initFeedback.PlayFeedbacks();
                return view.initFeedback.TotalDuration; 
            }

            return 0;
        }

        public float Play()
        {
            if (mode == BattleMode.Default)
            {
               view.playFeedback.PlayFeedbacks();
               return view.playFeedback.TotalDuration;
            }

            return 0;
        }

        public void Active()
        {
            IsPlaying = true;
            weapon.Active();
        }
        
        public void Inactive()
        { 
            IsPlaying = false;
            weapon.Inactive();
        }

        public void Tick(float dt)
        {
            weapon.Tick(dt);
        }

        public void Draw()
        {
#if UNITY_EDITOR
            Vector2 v2 = left1;
            Vector2 v3 = right1;
            Vector2 v4 = right2;
            Vector2 v5 = left2;
            
            float scale = IsPlaying ? 1.3f : 1f;
            v2 *= scale;
            v3 *= scale;
            v4 *= scale;
            v5 *= scale;

            Color color = IsPlaying ? Color.red : Color.green;
            Debug.DrawLine(v2, v3, color);
            Debug.DrawLine(v3, v4, color);
            Debug.DrawLine(v4, v5, color);
            Debug.DrawLine(v5, v2, color);
            
            weapon.Draw(scale, color);
#endif
        }
    }
}