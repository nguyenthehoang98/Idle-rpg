using System;
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
        [FormerlySerializedAs("skill")]
        [Title("Core")]
        [OnValueChanged("SkillTypeChanged"), HideLabel]
        public DefaultSkillConfig.SkillType skillType;
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public DefaultSkillConfig defaultSkillConfig = new DefaultSkillConfig();
        
        [Title("Events")]
        [Tooltip("Các events được định nghĩa từ skill")]
        [Searchable]
        public EventConfig[] events;

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
    }
}
