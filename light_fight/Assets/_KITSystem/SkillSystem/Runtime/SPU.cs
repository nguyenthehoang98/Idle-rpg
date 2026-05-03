using System;
using System.Collections.Generic;
using _KITSystem.Schedule;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    /// <summary>
    /// Logic tương tự MPU. nhưng khác 1 chút về skill
    /// Thay vì có 1 lớp Skill xử lý logic của các action thì mỗi action lại có 1 logic riêng, tương tự như request rồi có ref đến parent là skillId
    /// </summary>
    public partial class Spu : ITickable
    {
        // Map Id -> Skill Instance
        private Dictionary<int, ISkill> mapSkills = new Dictionary<int, ISkill>();
        // Map Id -> List Action của skill (index activeEvents)
        private Dictionary<int, List<int>> mapActionsIndex = new Dictionary<int, List<int>>();
        // Danh sách các action đang được chạy
        private List<ActionRuntime> activeActions = new List<ActionRuntime>();
        // Danh sách chứa các action đã finish để chờ remove.
        private List<int> pendingActionRemoved = new List<int>();
        private List<ActionCompleteReason> pendingReasonActionRemoved = new List<ActionCompleteReason>();
        
        public void Tick(float deltaTime)
        {
            int totalActionFinished = 0;
            for (int i = 0; i < activeActions.Count; i++)
            {
                ActionRuntime a = activeActions[i];
                a.action.Tick(deltaTime);
                activeActions[i] = a;

                if (a.action.IsFinished)
                {
                    if (pendingActionRemoved.Count > totalActionFinished)
                    {
                        pendingReasonActionRemoved[totalActionFinished] = a.action.Reason;
                        pendingActionRemoved[totalActionFinished] = a.actionInstanceId;
                    }
                    else
                    {
                        pendingReasonActionRemoved.Add(a.action.Reason);
                        pendingActionRemoved.Add(a.actionInstanceId);
                    }

                    totalActionFinished++;
                }
            }
            
            // todo: xử lý các logic khác
            
            // todo: xóa các action đã xong
            for (int i = 0; i < totalActionFinished; i++)
            {
                var reason = pendingReasonActionRemoved[i];
                if (reason == ActionCompleteReason.EndLifeCycle)
                    RequestRemoveActionEndLifeCycle(pendingActionRemoved[i]);
                else if (reason == ActionCompleteReason.Interrupt)
                    RequestRemoveAction(pendingActionRemoved[i]);
                else Debug.LogError($"Not define reason '{reason}'");
            }
        }
        
        public void RequestRemoveAction(int uniqueId)
        {
        }
     
        private void RequestRemoveActionEndLifeCycle(int uniqueId)
        {
        }
    }
    

    public partial class Spu
    {
#if UNITY_EDITOR
        [Serializable]
#endif
        struct ActionRuntime
        {
            public int skillInstanceId;
            public int actionInstanceId;
            public float elapsedTime;
            public bool markedForRemoval;
            public IAction action;
        }
    }
}