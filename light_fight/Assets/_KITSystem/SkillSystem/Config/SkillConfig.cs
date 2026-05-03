using System;
using _KITSystem.SkillSystem.Config.Skill;
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
        public SkillType skillType;
        [SerializeReference, HideReferenceObjectPicker, HideLabel] public DefineSkill defineSkill = new DefineSkill();
        
        [Title("Events")]
        [Tooltip("True: Các events sẽ kết thúc ngay khi skill kết thúc." +
                 "\nFalse: Các events hoạt động độc lập")]
        public bool isRelativeEventsBySkill = false;
        [Tooltip("Các events được định nghĩa từ skill")]
        [Searchable]
        public BaseEvent[] events;

        private void SkillTypeChanged()
        {
            if (defineSkill == null || defineSkill.Type != skillType)
            {
                Type[] types = TypeUtils.GetAllTypeThatImplement<DefineSkill>();
                foreach (var type in types)
                {
                    DefineSkill instance = Activator.CreateInstance(type) as DefineSkill;
                    if (instance != null && instance.Type == skillType)
                    {
                        defineSkill = instance;
                        return;
                    }
                }
            }
        }
    }
}
