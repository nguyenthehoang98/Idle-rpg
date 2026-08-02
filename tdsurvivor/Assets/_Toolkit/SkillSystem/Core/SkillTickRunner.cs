using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _Toolkit.Updater;
using UnityEngine;

namespace _Toolkit.SkillSystem.Core
{
    public class SkillTickRunner : BaseTickRunner
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
        private List<SkillActionCompleteReason> pendingReasonActionRemoved = new List<SkillActionCompleteReason>();

        public override void Tick(float deltaTime)
        {
            FlushCommands();

            int startVersion = version;

            int totalActionFinished = 0;
            
            for (int i = 0; i < activeActions.Count; i++)
            {
                ActionRuntime a = activeActions[i];
                a.Action.Tick(deltaTime);
                activeActions[i] = a;

                if (a.Action.IsCompleted)
                {
                    if (pendingActionRemoved.Count > totalActionFinished)
                    {
                        pendingReasonActionRemoved[totalActionFinished] = a.Action.CompleteReason;
                        pendingActionRemoved[totalActionFinished] = a.ActionInstanceId;
                    }
                    else
                    {
                        pendingReasonActionRemoved.Add(a.Action.CompleteReason);
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
                var reason = pendingReasonActionRemoved[i];

                if (reason == SkillActionCompleteReason.EndLifeCycle)
                {
                    QueueRemoveSkillActionByEndLifeCycle(pendingActionRemoved[i]);
                }
                else if (reason == SkillActionCompleteReason.Interrupt)
                {
                    QueueRemoveSkillAction(pendingActionRemoved[i]);
                }
                else
                {
                    Debug.LogError($"Not define reason '{reason}'");
                }
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

        public int NewActionSkillId()
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

        public void QueueAddSkillAction(int skillId, BaseSkillAction action, Action<int> callback = null)
        {
            pendingCommands.Enqueue(() =>
            {
                int id = AddSkillActionInternal(skillId, action);
                callback?.Invoke(id);
            });
        }

        public void QueueRemoveSkillAction(int actionId, Action<bool> callback = null)
        {
            pendingCommands.Enqueue(() =>
            {
                bool result = RemoveSkillActionInternal(actionId, true);
                callback?.Invoke(result);
            });
        }

        public bool TryGetSkillAction(int actionId, out BaseSkillAction action)
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
        public bool HasSkillAction(int actionId)
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

        private void QueueRemoveSkillActionByEndLifeCycle(int id)
        {
            pendingCommands.Enqueue(() => { RemoveSkillActionInternal(id, false); });
        }

        private int AddSkillActionInternal(int id, BaseSkillAction action)
        {
            int actionId = nextActionId++;

            if (!mapActionsIndex.TryGetValue(id, out List<int> list))
            {
                list = new List<int>();
                mapActionsIndex[id] = list;
            }

            action.Startup();

            int index = activeActions.Count;

            ActionRuntime runtime = new ActionRuntime
            {
                SkillInstanceId = id,
                ActionInstanceId = actionId,
                Action = action
            };

            activeActions.Add(runtime);
            list.Add(actionId);
            mapActionIdToIndex[actionId] = index;

            version++;
            return actionId;
        }

        private bool RemoveSkillActionInternal(int id, bool interrupted)
        {
            if (!mapActionIdToIndex.TryGetValue(id, out int idx))
                return false;

            if ((uint)idx >= activeActions.Count)
                return false;

            ActionRuntime removed = activeActions[idx];

            if (interrupted) removed.Action.Interrupt();

            removed.Action.Shutdown();

            // remove khỏi skill map
            if (mapActionsIndex.TryGetValue(removed.SkillInstanceId, out List<int> list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == id)
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
            mapActionIdToIndex.Remove(id);

            version++;
            return true;
        }

        private class ActionRuntime
        {
            public int SkillInstanceId;
            public int ActionInstanceId;
            public BaseSkillAction Action;
        }
    }
}