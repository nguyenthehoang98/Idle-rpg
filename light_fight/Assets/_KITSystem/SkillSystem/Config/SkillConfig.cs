using System;
using _KITSystem.SkillSystem.Config;
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
        public DefaultSkill.SkillType skillType;
        [FormerlySerializedAs("defineSkill")] [SerializeReference, HideReferenceObjectPicker, HideLabel] public DefaultSkill defaultSkill = new DefaultSkill();
        
        [Title("Events")]
        [Tooltip("True: Các events sẽ kết thúc ngay khi skill kết thúc." +
                 "\nFalse: Các events hoạt động độc lập")]
        public bool isRelativeEventsBySkill = false;
        [Tooltip("Các events được định nghĩa từ skill")]
        [Searchable]
        public BaseEvent[] events;

        private void SkillTypeChanged()
        {
            if (defaultSkill == null || defaultSkill.Type != skillType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<DefaultSkill>();
                foreach (var type in types)
                {
                    DefaultSkill instance = Activator.CreateInstance(type) as DefaultSkill;
                    if (instance != null && instance.Type == skillType)
                    {
                        defaultSkill = instance;
                        return;
                    }
                }
            }
        }
    }
}
