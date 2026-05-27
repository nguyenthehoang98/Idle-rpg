using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _FightCode.Utils;
using _KITSystem.SkillSystem.Runtime;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    [System.Serializable]
    public class Slot
    {
        private BattleSetting setting;
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

        public Slot(int order, BattleShare share, BattleSetting setting, IQuery query)
        {
            this.share = share;
            this.setting = setting;
            float angle = -360f / Const.MAX_DICE_NUMBER * order + 90;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

#if UNITY_EDITOR
            Vector2 worldCenter = setting.worldCenter;
            Vector2 right = new Vector2(dir.y, -dir.x);
            
            float length1 = 2;
            float length2 = 0.4f;
            
            Vector2 baseCenter1 = worldCenter + dir * length1;
            left1 = baseCenter1 - right * length1/2f;
            right1 = baseCenter1 + right * length1/2f;
            
            Vector2 baseCenter2 = worldCenter + dir *length2;
            left2 = baseCenter2 - right * length2/2f;
            right2 = baseCenter2 + right * length2/2f;
#endif
           
            view = setting.slot.Instantiate(share.coneParent, new Vector3(0, 0, angle - 90));
            weapon = new Weapon(order, view, share, setting, query, dir);
            share.slots.Add(view);
        }

        public void Initialize() => view.Initialize(share.timeScale);

        public float Play()
        {
            weapon.Play();
            return view.Play(share.timeScale);
        }

        public void Activate(float delayActivate)
        {
            weapon.Activate(delayActivate);
            
            view.Activate(delayActivate, share.timeScale);
            
            IsPlaying = true;
        }
        
        public void Deactivate(float delayDeactivate)
        {
            weapon.Deactivate(delayDeactivate);
            
            view.Deactivate(delayDeactivate, share.timeScale);
            
            IsPlaying = false;
        }

        public void Tick(float dt)
        {
            weapon.Tick(dt);
        }

        public void DoStack(int stack) => view.Stack(stack, setting.slotLerpColorDuration, share.timeScale);

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