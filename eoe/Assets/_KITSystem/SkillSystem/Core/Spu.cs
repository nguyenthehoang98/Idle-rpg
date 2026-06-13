using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace _KITSystem.SkillSystem.Core
{
    [Serializable]
    /// <summary>
    /// Logic tương tự MPU. nhưng khác 1 chút về skill
    /// Thay vì có 1 lớp Skill xử lý logic của các action thì mỗi action lại có 1 logic riêng, tương tự như request rồi có ref đến parent là skillId
    /// </summary>
    public partial class Spu
    {
        private int version;
        private int nextSkillInstanceId = 1;
        private int nextActionId = 1;
        private Queue<Action> pendingCommands = new Queue<Action>();

        // {Key:Value}={SkillId:List_ActionId}
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
            FlushCommands();

            int startVersion = version;

            int totalActionFinished = 0;
            for (int i = 0; i < activeActions.Count; i++)
            {
                ActionRuntime a = activeActions[i];
                a.Action.Tick(deltaTime);
                activeActions[i] = a;
                
                if (a.Action.IsFinished)
                {
                    if (pendingActionRemoved.Count > totalActionFinished)
                    {
                        pendingReasonActionRemoved[totalActionFinished] = a.Action.Reason;
                        pendingActionRemoved[totalActionFinished] = a.ActionInstanceId;
                    }
                    else
                    {
                        pendingReasonActionRemoved.Add(a.Action.Reason);
                        pendingActionRemoved.Add(a.ActionInstanceId);
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
                ActionCompleteReason reason = pendingReasonActionRemoved[i];
                if (reason == ActionCompleteReason.EndLifeCycle)
                    RequestRemoveActionEndLifeCycle(pendingActionRemoved[i]);
                else if (reason == ActionCompleteReason.Interrupt)
                    RequestRemoveAction(pendingActionRemoved[i]);
                else Debug.LogError($"Not define reason '{reason}'");
            }

            FlushCommands();
        }
        
        private void FlushCommands()
        {
            while (pendingCommands.Count > 0)
            {
                pendingCommands.Dequeue().Invoke();
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
                id = nextSkillInstanceId++;
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

        public bool TryGetAction(int actionId, out IAction action)
        {
            if (mapActionIdToIndex.TryGetValue(actionId, out int index))
            {
                if ((uint)index < activeActions.Count)
                {
                    action = activeActions[index].Action;
                    return true;
                }
            }

            action = null;
            return false;
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

            if (!mapActionsIndex.TryGetValue(skillId, out List<int> list))
            {
                list = new List<int>();
                mapActionsIndex[skillId] = list;
            }
            
            action.Start();

            int index = activeActions.Count;

            ActionRuntime runtime = new ActionRuntime
            {
                SkillInstanceId = skillId,
                ActionInstanceId = actionId,
                Action = action
            };

            activeActions.Add(runtime);
            list.Add(actionId);
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

            ActionRuntime removed = activeActions[idx];

            if (interrupted) removed.Action.Interrupt();

            removed.Action.Stop();
            
            // remove khỏi skill map
            if (mapActionsIndex.TryGetValue(removed.SkillInstanceId, out List<int> list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == actionId)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }

                if (list.Count == 0)
                {
                    mapActionsIndex.Remove(removed.SkillInstanceId);
                    freeIds.Push(removed.SkillInstanceId);
                }
            }
            
            int lastIdx = activeActions.Count - 1;

            if (idx != lastIdx)
            {
                ActionRuntime last = activeActions[lastIdx];
                activeActions[idx] = last;

                mapActionIdToIndex[last.ActionInstanceId] = idx;
            }

            activeActions.RemoveAt(lastIdx);
            mapActionIdToIndex.Remove(actionId);
            
            version++;
            return true;
        }
        
        public void Dispose()
        {
            pendingCommands = null;
            mapActionsIndex = null;
            mapActionIdToIndex = null;;
            freeIds = null;
            activeActions = null;
            pendingActionRemoved = null;
            pendingReasonActionRemoved = null;
        }
    }
    
    public partial class Spu
    {
        struct ActionRuntime
        {
            public int SkillInstanceId;
            public int ActionInstanceId;
            public float ElapsedTime;
            public bool MarkedForRemoval;
            public IAction Action;
        }
    }
}
