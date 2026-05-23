using _Games.Battle.Logic;
using _Games.Battle.Model;
using _Games.Battle.View;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;

namespace _Games.Battle
{
    [System.Serializable]
    public class Slot
    {
        private BattleShare share;
        private Weapon weapon;
        private ISlotView view;
#if UNITY_EDITOR
        private Vector3 left1;
        private Vector3 right1;
        private Vector3 left2;
        private Vector3 right2;
#endif

        public bool IsPlaying { get; private set; }

        public Slot(BattleShare share, int order, BattleSetting setting, IQuery query)
        {
            this.share = share;
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
            view = setting.slot.Instantiate(share.coneParent, new Vector3(0, 0, angle - 90));
        }

        public void Initialize() => view.Initialize(share.timeScale);

        public float Play() => view.Play(share.timeScale);

        public void Activate()
        {
            weapon.Activate();
            view.Activate(share.timeScale);
            IsPlaying = true;
        }
        
        public void Deactivate()
        {
            weapon.Deactivate();
            view.Deactivate(share.timeScale);
            IsPlaying = false;
        }

        public void Tick(float dt)
        {
            weapon.Tick(dt);
        }

        public void DoStack(int stack) => view.Stack(stack, share.timeScale);

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