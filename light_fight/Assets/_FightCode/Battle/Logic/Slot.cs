using _FightCode.Battle.Model;
using _FightCode.Battle.View;
using _FightCode.Utils;
using _KITSystem.Entity;
using UnityEngine;

namespace _FightCode.Battle.Logic
{
    [System.Serializable]
    public class Slot
    {
        private BattleSetting setting;
        private BattleShare share;
        private Weapon weapon;
        private SlotView view;

        public bool IsPlaying { get; private set; }

        public Slot(int order, SlotView slotView, BattleShare share, BattleSetting setting, IQuery query)
        {
            this.share = share;
            this.setting = setting;
            float angle = -360f / Const.MAX_DICE_NUMBER * order + 90;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            view = slotView;
            weapon = new Weapon(order, view, share, setting, query, dir);
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
    }
}