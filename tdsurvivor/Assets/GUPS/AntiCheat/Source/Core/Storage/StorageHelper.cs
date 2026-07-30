// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Binary;

namespace GUPS.AntiCheat.Core.Storage
{
    /// <summary>
    /// Maps runtime types to <see cref="EStorageType"/> values and reads or writes storage items via <see cref="BinaryReader"/> and <see cref="BinaryWriter"/>.
    /// </summary>
    internal static class StorageHelper
    {
        /// <summary>
        /// Returns the <see cref="EStorageType"/> that matches the runtime type of <paramref name="_Object"/>.
        /// </summary>
        /// <param name="_Object">The non-null object to classify.</param>
        /// <returns>The matching storage type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="_Object"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the runtime type is unsupported.</exception>
        public static EStorageType GetStorageType(Object _Object)
        {
            if (_Object == null)
            {
                throw new ArgumentNullException(nameof(_Object));
            }

            return GetStorageType(_Object.GetType());
        }

        /// <summary>
        /// Returns the <see cref="EStorageType"/> that matches the supplied <see cref="Type"/>.
        /// </summary>
        /// <param name="_Type">The non-null type to classify.</param>
        /// <returns>The matching storage type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="_Type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the type is unsupported.</exception>
        public static EStorageType GetStorageType(Type _Type)
        {
            if (_Type == null)
            {
                throw new ArgumentNullException(nameof(_Type));
            }

            // Primitive types
            if (_Type == typeof(byte)) return EStorageType.Byte;
            if (_Type == typeof(byte[])) return EStorageType.ByteArray;
            if (_Type == typeof(bool)) return EStorageType.Boolean;
            if (_Type == typeof(short)) return EStorageType.Int16;
            if (_Type == typeof(int)) return EStorageType.Int32;
            if (_Type == typeof(long)) return EStorageType.Int64;
            if (_Type == typeof(ushort)) return EStorageType.UInt16;
            if (_Type == typeof(uint)) return EStorageType.UInt32;
            if (_Type == typeof(ulong)) return EStorageType.UInt64;
            if (_Type == typeof(float)) return EStorageType.Single;
            if (_Type == typeof(double)) return EStorageType.Double;
            if (_Type == typeof(decimal)) return EStorageType.Decimal;
            if (_Type == typeof(char)) return EStorageType.Char;
            if (_Type == typeof(string)) return EStorageType.String;

            // Unity-specific types
            if (_Type == typeof(UnityEngine.Color)) return EStorageType.Color;
            if (_Type == typeof(UnityEngine.Color32)) return EStorageType.Color32;
            if (_Type == typeof(UnityEngine.Vector2)) return EStorageType.Vector2;
            if (_Type == typeof(UnityEngine.Vector2Int)) return EStorageType.Vector2Int;
            if (_Type == typeof(UnityEngine.Vector3)) return EStorageType.Vector3;
            if (_Type == typeof(UnityEngine.Vector3Int)) return EStorageType.Vector3Int;
            if (_Type == typeof(UnityEngine.Vector4)) return EStorageType.Vector4;
            if (_Type == typeof(UnityEngine.Quaternion)) return EStorageType.Quaternion;
            if (_Type == typeof(UnityEngine.Rect)) return EStorageType.Rect;
            if (_Type == typeof(UnityEngine.Plane)) return EStorageType.Plane;
            if (_Type == typeof(UnityEngine.Ray)) return EStorageType.Ray;
            if (_Type == typeof(UnityEngine.Matrix4x4)) return EStorageType.Matrix4x4;

            // If the type is not supported, throw an exception.
            throw new ArgumentException($"Unsupported type: {_Type.FullName}", nameof(_Type));
        }

        /// <summary>
        /// Reads a value of the specified storage type from <paramref name="_Reader"/>.
        /// </summary>
        /// <param name="_Reader">The non-null binary reader to read from.</param>
        /// <param name="_StorageType">The storage type to decode.</param>
        /// <returns>The decoded value, boxed as <see cref="object"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="_Reader"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="_StorageType"/> is unsupported.</exception>
        public static object Read(BinaryReader _Reader, EStorageType _StorageType)
        {
            if (_Reader == null)
            {
                throw new ArgumentNullException(nameof(_Reader));
            }

            switch (_StorageType)
            {
                case EStorageType.Byte:
                    return _Reader.ReadByte();
                case EStorageType.ByteArray:
                    return _Reader.ReadBytesAndSize();
                case EStorageType.Boolean:
                    return _Reader.ReadBoolean();
                case EStorageType.Int16:
                    return _Reader.ReadInt16();
                case EStorageType.Int32:
                    return _Reader.ReadInt32();
                case EStorageType.Int64:
                    return _Reader.ReadInt64();
                case EStorageType.UInt16:
                    return _Reader.ReadUInt16();
                case EStorageType.UInt32:
                    return _Reader.ReadUInt32();
                case EStorageType.UInt64:
                    return _Reader.ReadUInt64();
                case EStorageType.Single:
                    return _Reader.ReadSingle();
                case EStorageType.Double:
                    return _Reader.ReadDouble();
                case EStorageType.Decimal:
                    return _Reader.ReadDecimal();
                case EStorageType.Char:
                    return _Reader.ReadChar();
                case EStorageType.String:
                    return _Reader.ReadString();

                // Unity-specific types
                case EStorageType.Color:
                    return _Reader.ReadColor();
                case EStorageType.Color32:
                    return _Reader.ReadColor32();
                case EStorageType.Vector2:
                    return _Reader.ReadVector2();
                case EStorageType.Vector2Int:
                    return _Reader.ReadVector2Int();
                case EStorageType.Vector3:
                    return _Reader.ReadVector3();
                case EStorageType.Vector3Int:
                    return _Reader.ReadVector3Int();
                case EStorageType.Vector4:
                    return _Reader.ReadVector4();
                case EStorageType.Quaternion:
                    return _Reader.ReadQuaternion();
                case EStorageType.Rect:
                    return _Reader.ReadRect();
                case EStorageType.Plane:
                    return _Reader.ReadPlane();
                case EStorageType.Ray:
                    return _Reader.ReadRay();
                case EStorageType.Matrix4x4:
                    return _Reader.ReadMatrix4x4();

                default:
                    throw new ArgumentException($"Unsupported storage type: {_StorageType}");
            }
        }

        /// <summary>
        /// Writes <paramref name="_Object"/> to <paramref name="_Writer"/> using the appropriate encoding for its runtime type.
        /// </summary>
        /// <param name="_Writer">The non-null binary writer to write to.</param>
        /// <param name="_Object">The non-null value to write; must be one of the supported primitive or Unity types.</param>
        /// <exception cref="ArgumentNullException">Thrown when either argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the runtime type is unsupported.</exception>
        public static void Write(BinaryWriter _Writer, object _Object)
        {
            if (_Writer == null)
                throw new ArgumentNullException(nameof(_Writer));

            if (_Object == null)
                throw new ArgumentNullException(nameof(_Object));

            switch (_Object)
            {
                case byte value:
                    _Writer.Write(value);
                    break;
                case byte[] value:
                    _Writer.WriteBytesFull(value);
                    break;
                case bool value:
                    _Writer.Write(value);
                    break;
                case short value:
                    _Writer.Write(value);
                    break;
                case int value:
                    _Writer.Write(value);
                    break;
                case long value:
                    _Writer.Write(value);
                    break;
                case ushort value:
                    _Writer.Write(value);
                    break;
                case uint value:
                    _Writer.Write(value);
                    break;
                case ulong value:
                    _Writer.Write(value);
                    break;
                case float value:
                    _Writer.Write(value);
                    break;
                case double value:
                    _Writer.Write(value);
                    break;
                case decimal value:
                    _Writer.Write(value);
                    break;
                case char value:
                    _Writer.Write(value);
                    break;
                case string value:
                    _Writer.Write(value);
                    break;

                // Unity-specific types
                case UnityEngine.Color value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Color32 value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Vector2 value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Vector2Int value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Vector3 value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Vector3Int value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Vector4 value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Quaternion value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Rect value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Plane value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Ray value:
                    _Writer.Write(value);
                    break;
                case UnityEngine.Matrix4x4 value:
                    _Writer.Write(value);
                    break;
                default:
                    throw new ArgumentException($"Unsupported type: {_Object.GetType().FullName}", nameof(_Object));
            }
        }
    }
}
