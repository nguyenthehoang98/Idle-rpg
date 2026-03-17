using System;
using System.Collections.Generic;
using System.Threading;

namespace _KIT.Utils
{
    public static class TypeID
    {
        private static int global = 0;
        private static readonly Dictionary<Type, int> typeMap = new Dictionary<Type, int>();

        internal static int GetID(Type t)
        {
            // Nếu type đã có ID → trả về luôn
            if (typeMap.TryGetValue(t, out var id))
                return id;

            // Nếu chưa có → tạo mới ID và lưu lại
            id = Interlocked.Increment(ref global);
            typeMap[t] = id;
            return id;
        }
    }

    public static class TypeID<T>
    {
        public static readonly int Id = TypeID.GetID(typeof(T));
    }
}