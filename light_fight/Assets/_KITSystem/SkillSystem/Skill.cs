using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _KITSystem.SkillSystem
{
    /// <summary>
    /// Lớp này có thể override data từ excel. override trực tiếp luôn
    /// </summary>
    [CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
    public sealed class Skill : SerializedScriptableObject
    {
        [Title("Core")]
        [SerializeReference] public BaseSkill skill;
        
        [Title("Main")]
        [Tooltip("Thời gian sống của toàn bộ kĩ năng")]
        public float lifeTimeInSeconds = 1f;
        [Tooltip("Thời gian khóa hành vi kĩ năng, trong thời gian này kĩ năng được niệm và không nhận input")]
        public float channelingExitTimeInSeconds = 0f;
        public string animationName;
        public float scaleTime = 1f;
        
        [Title("Events")]
        [Tooltip("True: Các events sẽ kết thúc ngay khi skill kết thúc." +
                 "\nFalse: Các events hoạt động độc lập")]
        public bool isRelativeEventsBySkill = false;
        [Tooltip("Các events được định nghĩa từ skill")]
        [Searchable]
        public BaseEvent[] events;
    }
}
