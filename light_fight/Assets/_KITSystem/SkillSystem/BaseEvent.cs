using System;
using _KITSystem.SkillSystem.Action;
using _KITSystem.SkillSystem.Trigger;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    [Serializable]
    public class BaseEvent
    {
        [TabGroup("$triggerType"), OnValueChanged("TriggerTypeChanged"), HideLabel]
        public BaseTrigger.TriggerType triggerType;
        [TabGroup("$triggerType")]
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseTrigger trigger;
        
        [TabGroup("$actionType"), OnValueChanged("ActionTypeChanged"), HideLabel]
        public BaseAction.ActionType actionType;
        [TabGroup("$actionType")]
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseAction action;

        public BaseEvent()
        {
            ActionTypeChanged();
            TriggerTypeChanged();
        }
        
        private void ActionTypeChanged()
        {
            if (action == null || action.Type != actionType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseAction>();
                foreach (var type in types)
                {
                    BaseAction instance = Activator.CreateInstance(type) as BaseAction;
                    if (instance != null && instance.Type == actionType)
                    {
                        action = instance;
                        return;
                    }
                }
            }
        }
        
        private void TriggerTypeChanged()
        {
            if (trigger == null || trigger.Type != triggerType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseTrigger>();
                foreach (var type in types)
                {
                    BaseTrigger instance = Activator.CreateInstance(type) as BaseTrigger;
                    if (instance != null && instance.Type == triggerType)
                    {
                        trigger = instance;
                        return;
                    }
                }
            }
        }
    }
}