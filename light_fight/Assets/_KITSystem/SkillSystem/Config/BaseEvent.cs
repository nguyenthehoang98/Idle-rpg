using System;
using _KITSystem.SkillSystem.Config;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class BaseEvent
    {
        [TabGroup("$TriggerType"), HideLabel]
        public Trigger trigger = new Trigger();
        
        [TabGroup("$actionType"), OnValueChanged("ActionTypeChanged"), HideLabel]
        public BaseAction.ActionType actionType;
        [TabGroup("$actionType")]
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseAction action;

        public BaseEvent()
        {
            ActionTypeChanged();
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

        private string TriggerType => trigger.type.ToString();
    }
}