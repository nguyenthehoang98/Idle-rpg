using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _KITSystem.Schedule;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace _KITSystem.Movement
{
    [Serializable]
    //~ Movement Processing Unit
    public sealed partial class MPU : ITickable
    {
        [SerializeField, Tooltip("Các modifier có kiểu khác danh sách này sẽ không được thêm vào hệ thống")]
        private ModifierName[] flagModifiers = new ModifierName[0];
        [SerializeField, Tooltip("Các phương pháp xử lý vận tốc của unit"), SerializeReference]
        private IResolver resolver;

        // Đánh version giúp kiểm tra debug
        private int version;
        // biến kiểm tra đã khởi tạo chưa
        [HideInEditorMode, DisableInPlayMode]
        [SerializeField] private bool isInitialized = false;
        // Kiểu collection khác, giúp kiểm tra nhanh hơn
        private HashSet<ModifierName> flags = new HashSet<ModifierName>();
        // Danh sách chứa vị trí của các unit. unit id = index
        [HideInEditorMode, DisableInPlayMode]
        [SerializeField] private List<Vector3> positions = new List<Vector3>();
        private List<Vector3> destinations = new List<Vector3>();
        // Tương tự poistions list nhưng lưu vận tốc
        private List<Vector3> desiredVelocities = new List<Vector3>();
        // Danh sách đánh dấu unit nào không còn hoạt động.
        private List<bool> alives = new List<bool>();
        // Chứa các id (resue), nếu danh sách này trống thì tăng thêm size của positions
        private Stack<int> freeIds = new Stack<int>();
        /*
         * Map các unit Id -> Modifier Runtime [index]
         * Muốn tìm 1 modifier thì sẽ unitModifiers[unitId][x] x là index => activeModifiers[x]
         */
        private Dictionary<int, List<int>> units = new Dictionary<int, List<int>>();
        // Danh sách chứa các modifiers đang hoạt động (sử dụng loops)
        [HideInEditorMode, DisableInPlayMode]
        [SerializeField] private List<ModifierRuntime> activeActions = new List<ModifierRuntime>();
        // Cờ id cho phần thêm, xóa modifier
        private int nextUniqueModifierId = 1;
        // Danh sách các cờ modifier được lưu ở đây. Map [uniqueId]->[globalid] giúp tìm modifier trực tiếp
        private Dictionary<int, int> mapActionIdToIndex = new Dictionary<int, int>();
        // Hàng chờ các command, tránh conflic data
        private Queue<Action> pendingCommands = new Queue<Action>();
        // Danh sách chứa các modifier đã finish để chờ remove.
        private List<int> pendingActionRemoved = new List<int>();
        private List<ModifierCompleteReason> pendingReasonActionRemoved = new List<ModifierCompleteReason>();

        public MPU()
        {
        }

        public MPU(params ModifierName[] additionalModifiers)
        {
            resolver = new AvoidanceResolver();
            foreach (var m in additionalModifiers)
            {
                flags.Add(m);
            }
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                flags.Add(ModifierName.Default);
                foreach (ModifierName modifierName in flagModifiers)
                {
                    flags.Add(modifierName);
                }

                resolver.Initialize();
                
                isInitialized = true;
            }
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized) return;
            FlushCommands();

            // Update modifier
            int totalModifierFinished = 0;
            int startVersion = version;
            for (int i = 0; i < activeActions.Count; i++)
            {
                ModifierRuntime m = activeActions[i];
                m.action.Process(positions[m.unitId], deltaTime);
                activeActions[i] = m;

                // Xử lý để giảm việc cấp phát bộ nhớ (clear || new) liên tục
                if (m.action.IsFinished)
                {
                    if (pendingActionRemoved.Count > totalModifierFinished)
                    {
                        pendingReasonActionRemoved[totalModifierFinished] = m.action.Reason;
                        pendingActionRemoved[totalModifierFinished] = m.actionId;
                    }
                    else
                    {
                        pendingReasonActionRemoved.Add(m.action.Reason);
                        pendingActionRemoved.Add(m.actionId);
                    }

                    totalModifierFinished++;
                }
#if UNITY_EDITOR
                if (startVersion != version)
                {
                    Debug.LogError("❌ State changed during Tick!");
                    return;
                }
#endif
            }

            // Tính vận tốc của các unit
            for (int unitId = 0; unitId < positions.Count; unitId++)
            {
                if (!alives[unitId])
                    continue;
                
                if (!units.TryGetValue(unitId, out var list)) // modifiers list
                {
                    desiredVelocities[unitId] = Vector3.zero;
                    continue;
                }

                Vector3 desiredPosition = Vector3.zero;
                Vector3 desiredVelocity = Vector3.zero;
                int maxPriority = int.MinValue;
                for (int i = 0; i < list.Count; i++)
                {
                    int idx = list[i];
                    ModifierRuntime m = activeActions[idx];

                    if (m.action.IsFinished) continue;
                    if (m.action.Priority > maxPriority && m.action.OverrideOthers)
                    {
                        maxPriority = m.action.Priority;
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    int idx = list[i];
                    ModifierRuntime m = activeActions[idx];
                    
                    if (m.action.IsFinished) continue;
                    if (maxPriority != int.MinValue)
                    {
                        if (m.action.Priority < maxPriority)
                            continue;
                    }
                    
                    desiredPosition += m.action.EvaluatePosition(deltaTime);
                    desiredVelocity += m.action.EvaluateVelocity(deltaTime);
                }

                positions[unitId] += desiredPosition;
                desiredVelocities[unitId] = desiredVelocity;
            }

            // Giải quyết / xử lý va chạm
            var finalVelocities = resolver.Resolve(
                positions, destinations, desiredVelocities
            );
            
            // tính lại vị trí
            for (int unitId = 0; unitId < positions.Count; unitId++)
            {
                if (!alives[unitId]) continue;

                positions[unitId] += finalVelocities[unitId];
            }

            // Remove all modifier finished
            for (int i = 0; i < totalModifierFinished; i++)
            {
                ModifierCompleteReason reason = pendingReasonActionRemoved[i];
                if (reason == ModifierCompleteReason.EndLifeCycle)
                    RequestRemoveActionEndLifeCycle(pendingActionRemoved[i]);
                else if (reason == ModifierCompleteReason.Interrupt)
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

        public void RequestAddUnit(Vector3 position, Vector3 destination, Action<int> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(AddUnit_Internal(position, destination)); });
        }
        
        public void RequestAddUnit(Vector3 position, Vector3 destination)
        {
            pendingCommands.Enqueue(() => { AddUnit_Internal(position, destination); });
        }
        
        public void RequestRemoveUnit(int unitId, Action<bool> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(RemoveUnit_Internal(unitId)); });
        }
        
        public void RequestRemoveUnit(int unitId)
        {
            pendingCommands.Enqueue(() => { RemoveUnit_Internal(unitId); });
        }

        public void RequestAddAction(int unitId, IMovementAction movementAction, Action<int> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(AddAction_Internal(unitId, movementAction)); });
        }
        
        public void RequestAddAction(int unitId, IMovementAction movementAction)
        {
            pendingCommands.Enqueue(() => { AddAction_Internal(unitId, movementAction); });
        }
     
        public void RequestRemoveAction(int uniqueId, Action<bool> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(RemoveAction_Internal(uniqueId, true)); });
        }
     
        public void RequestRemoveAction(int uniqueId)
        {
            pendingCommands.Enqueue(() => { RemoveAction_Internal(uniqueId, true); });
        }
     
        private void RequestRemoveActionEndLifeCycle(int uniqueId)
        {
            pendingCommands.Enqueue(() => { RemoveAction_Internal(uniqueId, false); });
        }

        private int AddUnit_Internal(Vector3 position, Vector3 destination)
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogError("MPU not initialized");
#endif
                return -2;
            }
            
            int id;

            // reuse id nếu có
            if (freeIds.Count > 0)
            {
                id = freeIds.Pop();

                positions[id] = position;
                destinations[id] = destination;
                desiredVelocities[id] = Vector3.zero;
                alives[id] = true;
            }
            else
            {
                id = positions.Count;

                positions.Add(position);
                destinations.Add(destination);
                desiredVelocities.Add(Vector3.zero);
                alives.Add(true);
            }

            version++; 
            
            return id;
        }

        private bool RemoveUnit_Internal(int unitId)
        {
            if (!IsValidUnit(unitId))
            {
#if UNITY_EDITOR
                Debug.LogError($"MPU not registered as a valid unit '{unitId}'");
#endif
                return false;
            }

            // mark dead
            alives[unitId] = false;

            // cleanup modifiers của unit này
            if (units.TryGetValue(unitId, out List<int> list))
            {
                List<int> handles = new List<int>(list.Count);

                for (int i = 0; i < list.Count; i++)
                {
                    int idx = list[i];

                    if (!IsValidModifierIndex(idx)) continue;

                    handles.Add(activeActions[idx].actionId);
                }
                
                // remove bằng handle (an toàn)
                for (int i = 0; i < handles.Count; i++)
                {
                    RemoveAction_Internal(handles[i], true);
                }

                units.Remove(unitId);
            }

            // đưa id vào free list
            freeIds.Push(unitId);
            
            // đoạn này đặt về như thế cho dễ debug nếu ần thôi
            positions[unitId] = default;
            desiredVelocities[unitId] = Vector3.zero;
            
            version++; 

            return true;
        }

        private int AddAction_Internal(int unitId, IMovementAction movementAction)
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogError("MPU not initialized");
#endif
                return -2;
            }

            if (!flags.Contains(movementAction.Name))
            {
#if UNITY_EDITOR
                Debug.LogError($"Modifier '{movementAction.Name}' is not registered to system.");
#endif
                return -3;
            }
            
            /*
             * Ý tưởng khi thêm 1 modifier vào thì kiểm tra nó có override modifier nào ko? có -> xóa
             * Sau đó fill các modifier các đối tượng để lưu trữ
             * Trả về 1 mark, nó sẽ dùng để query nhanh modifier bất kì
             */
            
            if (!IsValidUnit(unitId))
            {
#if UNITY_EDITOR
                Debug.LogError($"MPU not registered as a valid unit '{unitId}'");
#endif
                return -1;
            }
            
            // Lấy danh sách modifier của unit
            if (!units.TryGetValue(unitId, out var list))
            {
                list = new List<int>();
                units[unitId] = list;
            }
            
            // ~ start ->
            movementAction.Start(positions[unitId]);
            
            int modifierIndex = activeActions.Count;
            int modifierId = nextUniqueModifierId++;
            
            // Add modifier vào hệ thống
            ModifierRuntime runtime = new ModifierRuntime
            {
                unitId = unitId,
                action = movementAction,
                actionId = modifierId
            };

            activeActions.Add(runtime);

            list.Add(modifierIndex);
            
            mapActionIdToIndex[modifierId] = modifierIndex;

            version++;

            return modifierId;
        }

        private bool RemoveAction_Internal(int uniqueId, bool hasInterrupted)
        {
            /*
             * Áp dụng kĩ thuật trong môn Thuật toán & ứng dụng
             * Ta sẽ đảo chỗ item cần xóa về cuối, rồi xóa phần tử cuối
             *
             * Ví dụ
             * activeModifiers: [A, B, C, D]
             * remove B:
             * → swap D vào vị trí B → [A, D, C, D]
             * → remove cuối → [A, D, C]
             *
             * Độ phức tạp
             * Swap + Remove global: 0(1)
             * UpdateUnitModifier O(k) với K là số modifier hiện tại (khá ít) 
             */

            if (!TryGetIndexAction(uniqueId, out int idx))
                return false;
            
            ModifierRuntime removed = activeActions[idx];
            if(hasInterrupted) removed.action.Interrupt();
            removed.action.Stop();
            
            // xóa modifier ở danh sách theo unitId
            if (units.TryGetValue(removed.unitId, out List<int> list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    // so sánh bằng GlobalIndex để tìm đúng modifier
                    if (list[i] == idx)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }

                // xóa key nếu ko còn phần tử nào cả
                if (list.Count == 0)
                    units.Remove(removed.unitId);
            }

            // lấy index của modifier bên trong danh sách
            int lastIdx = activeActions.Count - 1;
            if (idx != lastIdx)
            {
                // đảo chỗ phần tử cần xóa cho phần tử cuối -> thay vì xóa ở list (giảm độ phức tạp)
                ModifierRuntime last = activeActions[lastIdx];
                activeActions[idx] = last;

                // cập nhật lại các phần tử
                mapActionIdToIndex[last.actionId] = idx;
                activeActions[idx] = last;
                
                // update unitModifiers của thằng bị swap
                if (units.TryGetValue(last.unitId, out List<int> list2))
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
            
            // xóa tại index cuối (thay vì xóa ở index nào đó -> phải sort list)
            activeActions.RemoveAt(lastIdx);
            
            // xoá mapping của thằng bị remove
            mapActionIdToIndex.Remove(uniqueId);

            version++;

            return true;
        }
        
        private bool TryGetIndexAction(int handle, out int idx)
        {
            if (!mapActionIdToIndex.TryGetValue(handle, out idx))
                return false;

            if (!IsValidModifierIndex(idx)) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetUnitPosition(int unitId)
        {
            return positions[unitId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetUnitVelocity(int unitId)
        {
            return desiredVelocities[unitId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasModifier(int handle)
        {
            return mapActionIdToIndex.ContainsKey(handle);
        }

        public bool HasModifierType(int unitId, ModifierName name)
        {
            if (IsValidUnit(unitId) && units.TryGetValue(unitId, out var list))
            {
                foreach (var idx in list)
                {
                    if(IsValidModifierIndex(idx) && activeActions[idx].action.Name == name)
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasModifier(int handle, out int idx)
        {
            return mapActionIdToIndex.TryGetValue(handle, out idx);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidUnit(int id)
        {
            return id >= 0 && id < alives.Count && alives[id];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidModifierIndex(int idx)
        {
            return (uint)idx < (uint)activeActions.Count;
        }
    }
    
    public partial class MPU
    {
#if UNITY_EDITOR
        [Serializable]
#endif
        struct ModifierRuntime
        {
            public int unitId;
            public float elapsedTime;
            public bool markedForRemoval;
#if UNITY_EDITOR
            [SerializeReference]
#endif
            public IMovementAction action;
            public int actionId; // Dùng lưu để xóa
        }
    }
}
