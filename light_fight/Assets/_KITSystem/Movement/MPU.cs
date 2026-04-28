using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using _KITSystem.Schedule;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _KITSystem.Movement
{
    [Serializable]
    //~ Movement Processing Unit
    public sealed partial class MPU : ITickable
    {
        [SerializeField, Tooltip("Các modifier có kiểu khác danh sách này sẽ không được thêm vào hệ thống")]
        private ModifierName[] flagModifiers = new ModifierName[0];

        // Đánh version giúp kiểm tra debug
        private int version;
        // biến kiểm tra đã khởi tạo chưa
        private bool isInitialized = false;
        // Kiểu collection khác, giúp kiểm tra nhanh hơn
        private HashSet<ModifierName> flags = new HashSet<ModifierName>();
        // Danh sách chứa vị trí của các unit. unit id = index
        private List<Vector3> positions = new List<Vector3>();
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
        private Dictionary<int, int> handleToGlobalIndex = new Dictionary<int, int>();
        // Hàng chờ các command, tránh conflic data
        private Queue<Action> pendingCommands = new Queue<Action>();

        public MPU()
        {
        }

        public MPU(params ModifierName[] additionalModifiers)
        {
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

                isInitialized = true;
            }
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized) return;
            
            int startVersion = version;

            for (int i = 0; i < activeModifiers.Count; i++)
            {
                var m = activeModifiers[i];

                // update logic...

#if UNITY_EDITOR
                if (startVersion != version)
                {
                    Debug.LogError("❌ State changed during Tick!");
                    return;
                }
#endif
            }

            while (pendingCommands.Count > 0)
            {
                pendingCommands.Dequeue().Invoke();
            }
        }

        public void RequestAddUnit(Vector3 position, Action<int> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(AddUnit_Internal(position)); });
        }
        
        public void RequestAddUnit(Vector3 position)
        {
            pendingCommands.Enqueue(() => { AddUnit_Internal(position); });
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
     
        public void RequestRemoveModifier(int unitId, Action<bool> callback)
        {
            pendingCommands.Enqueue(() => { callback.Invoke(RemoveModifier_Internal(unitId)); });
        }
     
        public void RequestRemoveModifier(int unitId)
        {
            pendingCommands.Enqueue(() => { RemoveModifier_Internal(unitId); });
        }

        private int AddUnit_Internal(Vector3 position)
        {
            if (!isInitialized)
                return -2;
            
            int id;

            // reuse id nếu có
            if (freeIds.Count > 0)
            {
                id = freeIds.Pop();

                positions[id] = position;
                alives[id] = true;
            }
            else
            {
                id = positions.Count;

                positions.Add(position);
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

                    handles.Add(activeModifiers[idx].UniqueModifierId);
                }
                
                // remove bằng handle (an toàn)
                for (int i = 0; i < handles.Count; i++)
                {
                    RemoveModifier_Internal(handles[i]);
                }

                unitModifiers.Remove(unitId);
            }

            // đưa id vào free list
            freeIds.Push(unitId);
            
            // đoạn này đặt về như thế cho dễ debug nếu ần thôi
            positions[unitId] = default;
            
            version++; 

            return true;
        }

        private int AddModifier_Internal(int unitId, IModifier modifier)
        {
            if (!isInitialized) 
                return -2;

            if (!flags.Contains(modifier.Name))
                return -3;
            
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
            
            // Nếu modifier mới có priority cao hơn → huỷ modifier cũ
            if (modifier.OverrideOthers && list.Count > 0)
            {
                var temp = new List<int>(list);
                
                for (int i = 0; i < temp.Count; i++)
                {
                    int idx = temp[i];

                    if (idx >= activeModifiers.Count) continue;

                    var m = activeModifiers[idx];
                    
                    if (!IsValidModifierIndex(idx)) continue;

                    if (modifier.Priority > m.Modifier.Priority)
                    {
                        RemoveModifier_Internal(m.UniqueModifierId);
                    }
                }
            }
            
            // ~ start ->
            modifier.OnStart(positions[unitId]);
            
            int globalIndex = activeModifiers.Count;
            int uniqueModifierId = nextUniqueModifierId++;
            
            // Add modifier vào hệ thống
            ModifierRuntime runtime = new ModifierRuntime
            {
                UnitId = unitId,
                Modifier = modifier,
                ElapsedTime = 0f,
                ModifierGlobalIndex = globalIndex,
                UniqueModifierId = uniqueModifierId
            };

            activeModifiers.Add(runtime);

            list.Add(globalIndex);
            
            handleToGlobalIndex[uniqueModifierId] = globalIndex;

            ValidateState();
            
            version++; 
            
            return uniqueModifierId;
        }

        private bool RemoveModifier_Internal(int uniqueId)
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
            
            ModifierRuntime runtime = activeModifiers[idx];
            runtime.Modifier.OnInterrupt();

            // lấy index của modifier bên trong danh sách
            int lastIdx = activeModifiers.Count - 1;
            
            if (idx != lastIdx)
            {
                // đảo chỗ phần tử cần xóa cho phần tử cuối -> thay vì xóa ở list (giảm độ phức tạp)
                ModifierRuntime last = activeModifiers[lastIdx];
                activeModifiers[idx] = last;

                // cập nhật lại các phần tử
                handleToGlobalIndex[last.UniqueModifierId] = idx;
                last.ModifierGlobalIndex = idx;
                activeModifiers[idx] = last;
            }
            
            // xóa tại index cuối (thay vì xóa ở index nào đó -> phải sort list)
            activeModifiers.RemoveAt(lastIdx);
            
            // xoá mapping của thằng bị remove
            handleToGlobalIndex.Remove(uniqueId);

            // xóa modifier ở danh sách theo unitId
            if (unitModifiers.TryGetValue(runtime.UnitId, out List<int> list))
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
                if (list.Count == 0) unitModifiers.Remove(runtime.UnitId);
            }

            ValidateState();
            
            version++; 

            return true;
        }
        
        private bool TryGetIndexModifier(int handle, out int idx)
        {
            if (!handleToGlobalIndex.TryGetValue(handle, out idx))
                return false;

            if (!IsValidModifierIndex(idx)) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasModifier(int handle)
        {
            return handleToGlobalIndex.ContainsKey(handle);
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
        
        [Conditional("UNITY_EDITOR")]
        private void ValidateState()
        {
            // mapping ↔ global phải khớp
            foreach (var kv in handleToGlobalIndex)
            {
                int idx = kv.Value;

                if (!IsValidModifierIndex(idx))
                {
                    Debug.LogError("Invalid index");
                    continue;
                }

                if (activeModifiers[idx].UniqueModifierId != kv.Key)
                {
                    Debug.LogError("Mapping mismatch");
                }
            }
        }
    }
    
    public partial class MPU
    {
        struct ModifierRuntime
        {
            public int UnitId;
            public float ElapsedTime;
            public IModifier Modifier;
            public int ModifierGlobalIndex; // Dùng để xóa ngược lại modifieractives mà ko phải duyệt toàn bộ
            public int UniqueModifierId; // Dùng lưu để xóa
        }
    }
}
