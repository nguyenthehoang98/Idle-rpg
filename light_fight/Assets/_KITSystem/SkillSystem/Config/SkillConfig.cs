using System;
using System.Text;
using _KITSystem.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _KITSystem.SkillSystem.Config
{
    /// <summary>
    /// Lớp này có thể override data từ excel. override trực tiếp luôn
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
    public sealed class SkillConfig : SerializedScriptableObject
    {
        [Title("Core"), OnValueChanged("SkillTypeChanged"), HideLabel]
        public DefaultSkillConfig.SkillType skillType;
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public DefaultSkillConfig defaultSkillConfig = new DefaultSkillConfig();
        
        [InfoBox("$GetStringNotify", InfoMessageType.Warning, "$EnableStringNotify")]
        [Title("Events"), Tooltip("Các events được định nghĩa từ skill")]
        [Searchable]
        public EventConfig[] events = new EventConfig[0];

        private void SkillTypeChanged()
        {
            if (defaultSkillConfig == null || defaultSkillConfig.Type != skillType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<DefaultSkillConfig>();
                foreach (var type in types)
                {
                    DefaultSkillConfig instance = Activator.CreateInstance(type) as DefaultSkillConfig;
                    if (instance != null && instance.Type == skillType)
                    {
                        defaultSkillConfig = instance;
                        return;
                    }
                }
            }
        }

        private string GetStringNotify()
        {
            StringBuilder sb = new StringBuilder();
            float duration = defaultSkillConfig.lifeTimeInSeconds;
            for (var i = 0; i < events.Length; i++)
            {
                var e = events[i];
                if (e.actionConfig.Duration > duration)
                    sb.AppendLine(
                        $"[Index={i}, Duration={e.actionConfig.Duration}, Action_Type={e.actionConfig.Type}]");
                if (e.triggerConfig.type == TriggerConfig.TriggerType.Timeline && e.triggerConfig.timer > duration)
                    sb.AppendLine($"[Index={i}, Timer={e.triggerConfig.timer}, Trigger]");
            }

            return sb.ToString();
        }

        private bool EnableStringNotify
        {
            get
            {
                float max = 0;
                foreach (var e in events)
                {
                    max = Mathf.Max(max, e.actionConfig.Duration);
                    if (e.triggerConfig.type == TriggerConfig.TriggerType.Timeline)
                    {
                        max = Mathf.Max(max, e.triggerConfig.timer);
                    }
                }

                return max > defaultSkillConfig.lifeTimeInSeconds;
            }
        }
    }
}
