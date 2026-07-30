// System
using System.Runtime.InteropServices;

namespace GUPS.AntiCheat.Protected
{
    /// <summary>
    /// Reinterpret-cast helper that aliases a 32-bit unsigned int and a 32-bit float at the same memory offset, used to obfuscate floating-point primitives without lossy conversions.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloat
    {
        /// <summary>
        /// The float view of the shared 32-bit storage.
        /// </summary>
        [FieldOffset(0)]
        public float floatValue;

        /// <summary>
        /// The unsigned int view of the shared 32-bit storage (same bits as <see cref="floatValue"/>).
        /// </summary>
        [FieldOffset(0)]
        public uint intValue;
    }
}
