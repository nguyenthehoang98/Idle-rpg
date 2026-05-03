using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _KITSystem.Schedule;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _KITSystem.Movement
{
    [Serializable]
    //~ Movement Processing Unit
    public sealed partial class Mpu : ITickable
    {
        [SerializeField, Tooltip("Các modifier có kiểu khác danh sách này sẽ không được thêm vào hệ thống")]
        private ModifierName[] flagModifiers = new ModifierName[0];
        [SerializeField, Tooltip("Các phương pháp xử lý vận tốc của unit"), SerializeReference]
        private IResolver resolver;

        // Đánh version giúp kiểm tra debug
        private int version;
        // biến kiểm tra đã khởi tạo chưa
        private bool isInitialized = false;
        // Kiểu collection khác, giúp kiểm tra nhanh hơn
        private HashSet<ModifierName> flags = new HashSet<ModifierName>();
        // Danh sách chứa vị trí của các unit. unit id = index
        private List<Vector3> positions = new List<Vector3>();
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
        private Dictionary<int, List<int>> unitModifiers = new Dictionary<int, List<int>>();
        // Danh sách chứa các modifiers đang hoạt động (sử dụng loops)
        private List<ModifierRuntime> activeModifiers = new List<ModifierRuntime>();
        // Cờ id cho phần thêm, xóa modifier
        private int nextUniqueModifierId = 1;
        // Danh sách các cờ modifier được lưu ở đây. Map [uniqueId]->[globalid] giúp tìm modifier trực tiếp
        private Dictionary<int, int> mapModifierIdToIndex = new Dictionary<int, int>();
        // Hàng chờ các command, tránh conflic data
        private Queue<Action> pendingCommands = new Queue<Action>();
        // Danh sách chứa các modifier đã finish để chờ remove.
        private List<int> pendingModifierRemoved = new List<int>();
        private List<ModifierCompleteReason> pendingReasonModifierRemoved = new List<ModifierCompleteReason>();

        public Mpu()
        {
        }

        public Mpu(params ModifierName[] additionalModifiers)
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

            // Apply pending commands
            while (pendingCommands.Count > 0)
            {
                pendingCommands.Dequeue().Invoke();
            }

            // Update modifier
            int totalModifierFinished = 0;
            int startVersion = version;
            for (int i = 0; i < activeModifiers.Count; i++)
            {
                ModifierRuntime m = activeModifiers[i];
                m.modifier.Process(positions[m.unitId], deltaTime);
                activeModifiers[i] = m;

                // Xử lý để giảm việc cấp phát bộ nhớ (clear || new) liên tục
                if (m.modifier.IsFinished)
                {
                    if (pendingModifierRemoved.Count > totalModifierFinished)
                    {
                        pendingReasonModifierRemoved[totalModifierFinished] = m.modifier.Reason;
                        pendingModifierRemoved[totalModifierFinished] = m.modifierId;
                    }
                    else
                    {
                        pendingReasonModifierRemoved.Add(m.modifier.Reason);
                        pendingModifierRemoved.Add(m.modifierId);
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
                
                if (!unitModifiers.TryGetValue(unitId, out var list)) // modifiers list
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
                    ModifierRuntime m = activeModifiers[idx];

                    if (m.modifier.IsFinished) continue;
                    if (m.modifier.Priority > maxPriority && m.modifier.OverrideOthers)
                    {
                        maxPriority = m.modifier.Priority;
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    int idx = list[i];
                    ModifierRuntime m = activeModifiers[idx];
                    
                    if (m.modifier.IsFinished) continue;
                    if (maxPriority != int.MinValue)
                    {
                        if (m.modifier.Priority < maxPriority)
                            continue;
                    }
                    
                    desiredPosition += m.modifier.EvaluatePosition(deltaTime);
                    desiredVelocity += m.modifier.EvaluateVelocity(deltaTime);
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
                ModifierCompleteReason reason = pendingReasonModifierRemoved[i];
                if (reason == ModifierCompleteReason.EndLifeCycle)
                    RequestRemoveModifierEndLifeCycle(pendingModifierRemoved[i]);
                else if (reason == ModifierCompleteReason.Interrupt)
                    RequestRemoveModifier(pendingModifierRemoved[i]);
                else Debug.LogError($"Not define reason '{reason}'");
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

        public void RequestAddModifier(int unitId, IModifier modifier, Action<int> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(AddModifier_Internal(unitId, modifier)); });
        }
        
        public void RequestAddModifier(int unitId, IModifier modifier)
        {
            pendingCommands.Enqueue(() => { AddModifier_Internal(unitId, modifier); });
        }
     
        public void RequestRemoveModifier(int uniqueId, Action<bool> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(RemoveModifier_Internal(uniqueId, true)); });
        }
     
        public void RequestRemoveModifier(int uniqueId)
        {
            pendingCommands.Enqueue(() => { RemoveModifier_Internal(uniqueId, true); });
        }
     
        private void RequestRemoveModifierEndLifeCycle(int uniqueId)
        {
            pendingCommands.Enqueue(() => { RemoveModifier_Internal(uniqueId, false); });
        }

        private int AddUnit_Internal(Vector3 position, Vector3 destination)
        {
            if (!isInitialized)
                return -2;
            
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
                return false;

            // mark dead
            alives[unitId] = false;

            // cleanup modifiers của unit này
            if (unitModifiers.TryGetValue(unitId, out List<int> list))
            {
                List<int> handles = new List<int>(list.Count);

                for (int i = 0; i < list.Count; i++)
                {
                    int idx = list[i];

                    if (!IsValidModifierIndex(idx)) continue;

                    handles.Add(activeModifiers[idx].modifierId);
                }
                
                // remove bằng handle (an toàn)
                for (int i = 0; i < handles.Count; i++)
                {
                    RemoveModifier_Internal(handles[i], true);
                }

                unitModifiers.Remove(unitId);
            }

            // đưa id vào free list
            freeIds.Push(unitId);
            
            // đoạn này đặt về như thế cho dễ debug nếu ần thôi
            positions[unitId] = default;
            desiredVelocities[unitId] = Vector3.zero;
            
            version++; 

            return true;
        }

        private int AddModifier_Internal(int unitId, IModifier modifier)
        {
            if (!isInitialized) 
                return -2;

            if (!flags.Contains(modifier.Name))
            {
                Debug.LogError($"Modifier '{modifier.Name}' is not registered to system.");
                return -3;
            }
            
            /*
             * Ý tưởng khi thêm 1 modifier vào thì kiểm tra nó có override modifier nào ko? có -> xóa
             * Sau đó fill các modifier các đối tượng để lưu trữ
             * Trả về 1 mark, nó sẽ dùng để query nhanh modifier bất kì
             */
            
            if (!IsValidUnit(unitId))
                return -1;
            
            // Lấy danh sách modifier của unit
            if (!unitModifiers.TryGetValue(unitId, out var list))
            {
                list = new List<int>();
                unitModifiers[unitId] = list;
            }
            
            // ~ start ->
            modifier.OnStart(positions[unitId]);
            
            int modifierIndex = activeModifiers.Count;
            int modifierId = nextUniqueModifierId++;
            
            // Add modifier vào hệ thống
            ModifierRuntime runtime = new ModifierRuntime
            {
                unitId = unitId,
                modifier = modifier,
                modifierId = modifierId
            };

            activeModifiers.Add(runtime);

            list.Add(modifierIndex);
            
            mapModifierIdToIndex[modifierId] = modifierIndex;

            version++;

            return modifierId;
        }

        private bool RemoveModifier_Internal(int uniqueId, bool hasInterrupted)
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

            if (!TryGetIndexModifier(uniqueId, out int idx))
                return false;
            
            ModifierRuntime removed = activeModifiers[idx];
            if(hasInterrupted) removed.modifier.OnInterrupt();
            removed.modifier.OnEnd();
            
            // xóa modifier ở danh sách theo unitId
            if (unitModifiers.TryGetValue(removed.unitId, out List<int> list))
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
                    unitModifiers.Remove(removed.unitId);
            }

            // lấy index của modifier bên trong danh sách
            int lastIdx = activeModifiers.Count - 1;
            if (idx != lastIdx)
            {
                // đảo chỗ phần tử cần xóa cho phần tử cuối -> thay vì xóa ở list (giảm độ phức tạp)
                ModifierRuntime last = activeModifiers[lastIdx];
                activeModifiers[idx] = last;

                // cập nhật lại các phần tử
                mapModifierIdToIndex[last.modifierId] = idx;
                activeModifiers[idx] = last;
                
                // update unitModifiers của thằng bị swap
                if (unitModifiers.TryGetValue(last.unitId, out List<int> list2))
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
            activeModifiers.RemoveAt(lastIdx);
            
            // xoá mapping của thằng bị remove
            mapModifierIdToIndex.Remove(uniqueId);

            version++;

            return true;
        }
        
        private bool TryGetIndexModifier(int handle, out int idx)
        {
            if (!mapModifierIdToIndex.TryGetValue(handle, out idx))
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
            return mapModifierIdToIndex.ContainsKey(handle);
        }

        public bool HasModifierType(int unitId, ModifierName name)
        {
            if (IsValidUnit(unitId) && unitModifiers.TryGetValue(unitId, out var list))
            {
                foreach (var idx in list)
                {
                    if(IsValidModifierIndex(idx) && activeModifiers[idx].modifier.Name == name)
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasModifier(int handle, out int idx)
        {
            return mapModifierIdToIndex.TryGetValue(handle, out idx);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidUnit(int id)
        {
            return id >= 0 && id < alives.Count && alives[id];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidModifierIndex(int idx)
        {
            return (uint)idx < (uint)activeModifiers.Count;
        }
    }
    
    public partial class Mpu
    {
#if UNITY_EDITOR
        [Serializable]
#endif
        struct ModifierRuntime
        {
            public int unitId;
            public float elapsedTime;
            public bool markedForRemoval;
            public IModifier modifier;
            public int modifierId; // Dùng lưu để xóa
        }
    }
}
