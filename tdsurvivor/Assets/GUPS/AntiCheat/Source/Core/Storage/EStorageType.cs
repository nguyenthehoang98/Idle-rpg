// System
using System;

namespace GUPS.AntiCheat.Core.Storage
{
    /// <summary>
    /// Discriminator identifying the primitive or Unity type stored in a <see cref="StorageItem"/>.
    /// </summary>
    [Serializable]
    public enum EStorageType : byte
    {
        /// <summary>
        /// 8-bit unsigned integer.
        /// </summary>
        Byte,

        /// <summary>
        /// Array of 8-bit unsigned integers.
        /// </summary>
        ByteArray,

        /// <summary>
        /// Boolean value.
        /// </summary>
        Boolean,

        /// <summary>
        /// 16-bit signed integer.
        /// </summary>
        Int16,

        /// <summary>
        /// 32-bit signed integer.
        /// </summary>
        Int32,

        /// <summary>
        /// 64-bit signed integer.
        /// </summary>
        Int64,

        /// <summary>
        /// 16-bit unsigned integer.
        /// </summary>
        UInt16,

        /// <summary>
        /// 32-bit unsigned integer.
        /// </summary>
        UInt32,

        /// <summary>
        /// 64-bit unsigned integer.
        /// </summary>
        UInt64,

        /// <summary>
        /// Single-precision floating-point number.
        /// </summary>
        Single,

        /// <summary>
        /// Double-precision floating-point number.
        /// </summary>
        Double,

        /// <summary>
        /// High-precision decimal number.
        /// </summary>
        Decimal,

        /// <summary>
        /// Single Unicode character.
        /// </summary>
        Char,

        /// <summary>
        /// UTF-8 string.
        /// </summary>
        String,

        /// <summary>
        /// Unity <see cref="UnityEngine.Color"/>.
        /// </summary>
        Color,

        /// <summary>
        /// Unity <see cref="UnityEngine.Color32"/>.
        /// </summary>
        Color32,

        /// <summary>
        /// Unity <see cref="UnityEngine.Vector2"/>.
        /// </summary>
        Vector2,

        /// <summary>
        /// Unity <see cref="UnityEngine.Vector2Int"/>.
        /// </summary>
        Vector2Int,

        /// <summary>
        /// Unity <see cref="UnityEngine.Vector3"/>.
        /// </summary>
        Vector3,

        /// <summary>
        /// Unity <see cref="UnityEngine.Vector3Int"/>.
        /// </summary>
        Vector3Int,

        /// <summary>
        /// Unity <see cref="UnityEngine.Vector4"/>.
        /// </summary>
        Vector4,

        /// <summary>
        /// Unity <see cref="UnityEngine.Quaternion"/>.
        /// </summary>
        Quaternion,

        /// <summary>
        /// Unity <see cref="UnityEngine.Rect"/>.
        /// </summary>
        Rect,

        /// <summary>
        /// Unity <see cref="UnityEngine.Plane"/>.
        /// </summary>
        Plane,

        /// <summary>
        /// Unity <see cref="UnityEngine.Ray"/>.
        /// </summary>
        Ray,

        /// <summary>
        /// Unity <see cref="UnityEngine.Matrix4x4"/>.
        /// </summary>
        Matrix4x4
    }
}
