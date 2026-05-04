using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _KITSystem.Schedule;
using UnityEngine;

namespace _KITSystem.SkillSystem.Runtime
{
    /// <summary>
    /// Logic tương tự MPU. nhưng khác 1 chút về skill
    /// Thay vì có 1 lớp Skill xử lý logic của các action thì mỗi action lại có 1 logic riêng, tương tự như request rồi có ref đến parent là skillId
    /// </summary>
    public partial class SPU : ITickable
    {
        private int version;
        private int nextActionId = 1;
        private Queue<Action> pendingCommands = new Queue<Action>();

        // {Key:Value}={SkillId:List_Action_Index->'activeActions'}
        private Dictionary<int, List<int>> mapActionsIndex = new Dictionary<int, List<int>>();
        // {Key:Value}={ActionId:Action_Index->'activeActions'}
        private Dictionary<int, int> mapActionIdToIndex = new Dictionary<int, int>();
        // reuse SkillID
        private Stack<int> freeIds = new Stack<int>();
        // all action on going
        private List<ActionRuntime> activeActions = new List<ActionRuntime>();
        
        private List<int> pendingActionRemoved = new List<int>();
        private List<ActionCompleteReason> pendingReasonActionRemoved = new List<ActionCompleteReason>();
        
        public void Tick(float deltaTime)
        {
            while (pendingCommands.Count > 0)
            {
                pendingCommands.Dequeue().Invoke();
            }

            int startVersion = version;

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

#if UNITY_EDITOR
                if (startVersion != version)
                {
                    Debug.LogError("❌ SPU State changed during Tick!");
                    return;
                }
#endif
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

        public int GenerateSkillInstanceId()
        {
            int id;

            // reuse id nếu có
            if (freeIds.Count > 0)
            {
                id = freeIds.Pop();
            }
            else
            {
                id = mapActionIdToIndex.Count;
            }

            version++; 
            
            return id;
        }
        
        public void RequestAddAction(int skillId, IAction action, Action<int> callback = null)
        {
            pendingCommands.Enqueue(() =>
            {
                int id = AddAction_Internal(skillId, action);
                callback?.Invoke(id);
            });
        }

        public void RequestRemoveAction(int actionId, Action<bool> callback = null)
        {
            pendingCommands.Enqueue(() =>
            {
                bool result = RemoveAction_Internal(actionId, true);
                callback?.Invoke(result);
            });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAction(int actionId)
        {
            return mapActionIdToIndex.ContainsKey(actionId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasSkill(int skillId, out List<int> list)
        {
            if (mapActionsIndex.TryGetValue(skillId, out list))
            {
                return list.Count > 0;
            }

            return false;
        }

        private void RequestRemoveActionEndLifeCycle(int actionId)
        {
            pendingCommands.Enqueue(() =>
            {
                RemoveAction_Internal(actionId, false);
            });
        }

        private int AddAction_Internal(int skillId, IAction action)
        {
            int actionId = nextActionId++;

            if (!mapActionsIndex.TryGetValue(skillId, out var list))
            {
                list = new List<int>();
                mapActionsIndex[skillId] = list;
            }
            
            action.Start();

            int index = activeActions.Count;

            ActionRuntime runtime = new ActionRuntime
            {
                skillInstanceId = skillId,
                actionInstanceId = actionId,
                action = action
            };

            activeActions.Add(runtime);
            list.Add(index);
            mapActionIdToIndex[actionId] = index;

            version++;
            return actionId;
        }

        private bool RemoveAction_Internal(int actionId, bool interrupted)
        {
            if (!mapActionIdToIndex.TryGetValue(actionId, out int idx))
                return false;

            if ((uint)idx >= activeActions.Count)
                return false;

            var removed = activeActions[idx];

            if (interrupted) removed.action.Interrupt();

            removed.action.Stop();
            
            // remove khỏi skill map
            if (mapActionsIndex.TryGetValue(removed.skillInstanceId, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == idx)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }

                if (list.Count == 0)
                {
                    mapActionsIndex.Remove(removed.skillInstanceId);
                    freeIds.Push(removed.skillInstanceId);
                }
            }
            
            int lastIdx = activeActions.Count - 1;

            if (idx != lastIdx)
            {
                var last = activeActions[lastIdx];
                activeActions[idx] = last;

                mapActionIdToIndex[last.actionInstanceId] = idx;

                if (mapActionsIndex.TryGetValue(last.skillInstanceId, out var list2))
                {
                    for (int i = 0; i < list2.Count; i++)
                    {
                        if (list2[i] == lastIdx)
                        {
                            list2[i] = idx;
                            break;
                        }
                    }
                }
            }

            
            activeActions.RemoveAt(lastIdx);
            mapActionIdToIndex.Remove(actionId);

            version++;
            return true;
        }
    }
    
    public partial class SPU
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