using System;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem.Config
{
    [Serializable]
    public class EventConfig
    {
        [TabGroup("$TriggerType"), HideLabel]
        public TriggerConfig triggerConfig = new TriggerConfig();
        
        [TabGroup("$actionType"), OnValueChanged("ActionTypeChanged"), HideLabel]
        public BaseActionConfig.ActionType actionType;
        [TabGroup("$actionType")]
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public BaseActionConfig actionConfig;

        public EventConfig()
        {
            ActionTypeChanged();
        }
        
        private void ActionTypeChanged()
        {
            if (actionConfig == null || actionConfig.Type != actionType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<BaseActionConfig>();
                foreach (var type in types)
                {
                    BaseActionConfig instance = Activator.CreateInstance(type) as BaseActionConfig;
                    if (instance != null && instance.Type == actionType)
                    {
                        actionConfig = instance;
                        return;
                    }
                }
            }
        }

        private string TriggerType => triggerConfig.type.ToString();
    }
}