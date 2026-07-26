// System
using System.Runtime.InteropServices;

namespace GUPS.AntiCheat.Protected
{
    /// <summary>
    /// Reinterpret-cast helper that aliases a 64-bit unsigned long and a 64-bit double at the same memory offset, used to obfuscate double-precision primitives without lossy conversions.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct ULongDouble
    {
        /// <summary>
        /// The double view of the shared 64-bit storage.
        /// </summary>
        [FieldOffset(0)]
        public double doubleValue;

        /// <summary>
        /// The unsigned long view of the shared 64-bit storage (same bits as <see cref="doubleValue"/>).
        /// </summary>
        [FieldOffset(0)]
        public ulong longValue;
    }
}
