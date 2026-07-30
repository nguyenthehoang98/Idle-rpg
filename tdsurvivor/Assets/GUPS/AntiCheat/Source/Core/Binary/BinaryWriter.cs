// System
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// Unity
using UnityEngine;

// Allow the internal classes to be accessed by the test assembly.
[assembly: InternalsVisibleTo("GUPS.AntiCheat.Tests")]

namespace GUPS.AntiCheat.Core.Binary
{
    /// <summary>
    /// Writes primitive C# and common Unity types to a byte buffer in little-endian order.
    /// </summary>
    internal class BinaryWriter
    {
        /// <summary>
        /// Union of a <see cref="float"/> and a <see cref="uint"/> sharing the same memory, used for bit-exact conversion without allocations.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct UIntFloat
        {
            /// <summary>
            /// The float view of the shared memory.
            /// </summary>
            [FieldOffset(0)]
            public float FloatValue;

            /// <summary>
            /// The uint view of the shared memory.
            /// </summary>
            [FieldOffset(0)]
            public uint IntValue;
        }

        /// <summary>
        /// Union of a <see cref="double"/> and a <see cref="ulong"/> sharing the same memory, used for bit-exact conversion without allocations.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct LongDouble
        {
            /// <summary>
            /// The double view of the shared memory.
            /// </summary>
            [FieldOffset(0)]
            public double DoubleValue;

            /// <summary>
            /// The ulong view of the shared memory.
            /// </summary>
            [FieldOffset(0)]
            public ulong LongValue;
        }

        /// <summary>
        /// The maximum allowed encoded length, in bytes, for a single string.
        /// </summary>
        private const int MAX_STRING_LENGTH = 65535;

        /// <summary>
        /// The maximum allowed byte array length.
        /// </summary>
        private const int MAX_BYTE_LENGTH = 2147483647;

        /// <summary>
        /// The underlying buffer receiving the written bytes.
        /// </summary>
        private Buffer buffer;

        /// <summary>
        /// The encoding used when writing strings (UTF-8).
        /// </summary>
        private static Encoding encoding;

        /// <summary>
        /// Shared scratch buffer reused across string write operations.
        /// </summary>
        private static byte[] stringWriteBuffer;

        /// <summary>
        /// Scratch union for bit-exact float to uint conversion.
        /// </summary>
        private static UIntFloat floatConverter;

        /// <summary>
        /// Scratch union for bit-exact double to ulong conversion.
        /// </summary>
        private static LongDouble doubleConverter;

        /// <summary>
        /// Gets the current write position in the underlying buffer.
        /// </summary>
        public uint Position
        {
            get
            {
                return this.buffer.Position;
            }
        }

        /// <summary>
        /// Initializes a new writer backed by an empty buffer.
        /// </summary>
        public BinaryWriter()
        {
            this.buffer = new Buffer();
            if (BinaryWriter.encoding == null)
            {
                BinaryWriter.encoding = new UTF8Encoding();
                BinaryWriter.stringWriteBuffer = new byte[MAX_STRING_LENGTH];
            }
        }

        /// <summary>
        /// Initializes a new writer backed by the supplied byte array (not copied).
        /// </summary>
        /// <param name="_Buffer">The byte array to write into.</param>
        public BinaryWriter(byte[] _Buffer)
        {
            this.buffer = new Buffer(_Buffer);
            if (BinaryWriter.encoding == null)
            {
                BinaryWriter.encoding = new UTF8Encoding();
                BinaryWriter.stringWriteBuffer = new byte[MAX_STRING_LENGTH];
            }
        }

        /// <summary>
        /// Returns a newly allocated array containing only the bytes that have been written.
        /// </summary>
        /// <returns>A copy of the written portion of the buffer.</returns>
        public byte[] ToArray()
        {
            byte[] array = new byte[this.buffer.AsArraySegment().Count];
            Array.Copy(this.buffer.AsArraySegment().Array, array, this.buffer.AsArraySegment().Count);
            return array;
        }

        /// <summary>
        /// Returns the underlying byte array directly (no copy, may be larger than the written length).
        /// </summary>
        /// <returns>A reference to the underlying byte array.</returns>
        public byte[] AsArray()
        {
            return this.AsArraySegment().Array;
        }

        /// <summary>
        /// Returns an <see cref="ArraySegment{T}"/> spanning the written portion of the underlying buffer.
        /// </summary>
        /// <returns>An array segment over the bytes that have been written.</returns>
        internal ArraySegment<byte> AsArraySegment()
        {
            return this.buffer.AsArraySegment();
        }

        /// <summary>
        /// Dispatches to the appropriate <c>Write</c> overload for the supplied runtime type.
        /// </summary>
        /// <param name="_Type">The runtime type of <paramref name="_Value"/>.</param>
        /// <param name="_Value">The value to write; must be assignable to <paramref name="_Type"/>.</param>
        public void Write(Type _Type, System.Object _Value)
        {
            if (_Type == typeof(System.Byte))
                this.Write((System.Byte)_Value);
            else if (_Type == typeof(System.Boolean))
                this.Write((System.Boolean)_Value);
            else if (_Type == typeof(System.Int16))
                this.Write((System.Int16)_Value);
            else if (_Type == typeof(System.Int32))
                this.Write((System.Int32)_Value);
            else if (_Type == typeof(System.Int64))
                this.Write((System.Int64)_Value);
            else if (_Type == typeof(System.UInt16))
                this.Write((System.UInt16)_Value);
            else if (_Type == typeof(System.UInt32))
                this.Write((System.UInt32)_Value);
            else if (_Type == typeof(System.UInt64))
                this.Write((System.UInt64)_Value);
            else if (_Type == typeof(System.Single))
                this.Write((System.Single)_Value);
            else if (_Type == typeof(System.Double))
                this.Write((System.Double)_Value);
            else if (_Type == typeof(System.Decimal))
                this.Write((System.Decimal)_Value);
            else if (_Type == typeof(System.Char))
                this.Write((System.Char)_Value);
            else if (_Type == typeof(System.String))
                this.Write((System.String)_Value);
            else if (_Type == typeof(UnityEngine.Color))
                this.Write((UnityEngine.Color)_Value);
            else if (_Type == typeof(UnityEngine.Color32))
                this.Write((UnityEngine.Color32)_Value);
            else if (_Type == typeof(UnityEngine.Vector2))
                this.Write((UnityEngine.Vector2)_Value);
            else if (_Type == typeof(UnityEngine.Vector2Int))
                this.Write((UnityEngine.Vector2Int)_Value);
            else if (_Type == typeof(UnityEngine.Vector3))
                this.Write((UnityEngine.Vector3)_Value);
            else if (_Type == typeof(UnityEngine.Vector3Int))
                this.Write((UnityEngine.Vector3Int)_Value);
            else if (_Type == typeof(UnityEngine.Vector4))
                this.Write((UnityEngine.Vector4)_Value);
            else if (_Type == typeof(UnityEngine.Quaternion))
                this.Write((UnityEngine.Quaternion)_Value);
            else if (_Type == typeof(UnityEngine.Rect))
                this.Write((UnityEngine.Rect)_Value);
            else if (_Type == typeof(UnityEngine.Plane))
                this.Write((UnityEngine.Plane)_Value);
            else if (_Type == typeof(UnityEngine.Ray))
                this.Write((UnityEngine.Ray)_Value);
            else if (_Type == typeof(UnityEngine.Matrix4x4))
                this.Write((UnityEngine.Matrix4x4)_Value);
        }

        /// <summary>
        /// Writes a single 8-bit char to the stream.
        /// </summary>
        /// <param name="_Value">The char value to write.</param>
        public void Write(char _Value)
        {
            this.buffer.WriteByte((byte)_Value);
        }

        /// <summary>
        /// Writes a single unsigned byte to the stream.
        /// </summary>
        /// <param name="_Value">The byte value to write.</param>
        public void Write(byte _Value)
        {
            this.buffer.WriteByte(_Value);
        }

        /// <summary>
        /// Writes a single signed byte to the stream.
        /// </summary>
        /// <param name="_Value">The signed byte value to write.</param>
        public void Write(sbyte _Value)
        {
            this.buffer.WriteByte((byte)_Value);
        }

        /// <summary>
        /// Writes a signed 16-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 16-bit signed integer to write.</param>
        public void Write(short _Value)
        {
            this.buffer.WriteByte2((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF));
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 16-bit unsigned integer to write.</param>
        public void Write(ushort _Value)
        {
            this.buffer.WriteByte2((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF));
        }

        /// <summary>
        /// Writes a signed 32-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 32-bit signed integer to write.</param>
        public void Write(int _Value)
        {
            this.buffer.WriteByte4((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF), (byte)(_Value >> 16 & 0xFF), (byte)(_Value >> 24 & 0xFF));
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 32-bit unsigned integer to write.</param>
        public void Write(uint _Value)
        {
            this.buffer.WriteByte4((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF), (byte)(_Value >> 16 & 0xFF), (byte)(_Value >> 24 & 0xFF));
        }

        /// <summary>
        /// Writes a signed 64-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 64-bit signed integer to write.</param>
        public void Write(long _Value)
        {
            this.buffer.WriteByte8((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF), (byte)(_Value >> 16 & 0xFF), (byte)(_Value >> 24 & 0xFF),
                                     (byte)(_Value >> 32 & 0xFF), (byte)(_Value >> 40 & 0xFF), (byte)(_Value >> 48 & 0xFF), (byte)(_Value >> 56 & 0xFF));
        }

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream (little-endian).
        /// </summary>
        /// <param name="_Value">The 64-bit unsigned integer to write.</param>
        public void Write(ulong _Value)
        {
            this.buffer.WriteByte8((byte)(_Value & 0xFF), (byte)(_Value >> 8 & 0xFF), (byte)(_Value >> 16 & 0xFF), (byte)(_Value >> 24 & 0xFF),
                                     (byte)(_Value >> 32 & 0xFF), (byte)(_Value >> 40 & 0xFF), (byte)(_Value >> 48 & 0xFF), (byte)(_Value >> 56 & 0xFF));
        }

        /// <summary>
        /// Writes a single-precision float to the stream.
        /// </summary>
        /// <param name="_Value">The float value to write.</param>
        public void Write(float _Value)
        {
            BinaryWriter.floatConverter.FloatValue = _Value;
            this.Write(BinaryWriter.floatConverter.IntValue);
        }

        /// <summary>
        /// Writes a double-precision float to the stream.
        /// </summary>
        /// <param name="_Value">The double value to write.</param>
        public void Write(double _Value)
        {
            BinaryWriter.doubleConverter.DoubleValue = _Value;
            this.Write(BinaryWriter.doubleConverter.LongValue);
        }

        /// <summary>
        /// Writes a <see cref="decimal"/> to the stream as four 32-bit integers.
        /// </summary>
        /// <param name="_Value">The decimal value to write.</param>
        public void Write(decimal _Value)
        {
            int[] bits = decimal.GetBits(_Value);
            this.Write(bits[0]);
            this.Write(bits[1]);
            this.Write(bits[2]);
            this.Write(bits[3]);
        }

        /// <summary>
        /// Writes a UTF-8 string prefixed by its encoded byte length, or a -1 length prefix when <paramref name="_Value"/> is <c>null</c>.
        /// </summary>
        /// <param name="_Value">The string to write, or <c>null</c>.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when the encoded length is at least <see cref="MAX_STRING_LENGTH"/>.</exception>
        public void Write(string _Value)
        {
            if (_Value == null)
            {
                this.Write((int)-1);
            }
            else
            {
                int byteCount = BinaryWriter.encoding.GetByteCount(_Value);
                if (byteCount >= MAX_STRING_LENGTH)
                {
                    throw new IndexOutOfRangeException("Serialize(string) too long: " + _Value.Length + "! Maximal length: " + MAX_STRING_LENGTH);
                }
                this.Write((int)byteCount);
                int bytes = BinaryWriter.encoding.GetBytes(_Value, 0, _Value.Length, BinaryWriter.stringWriteBuffer, 0);
                this.buffer.WriteBytes(BinaryWriter.stringWriteBuffer, (int)bytes);
            }
        }

        /// <summary>
        /// Writes a boolean to the stream as 1 for <c>true</c> or 0 for <c>false</c>.
        /// </summary>
        /// <param name="_Value">The boolean value to write.</param>
        public void Write(bool _Value)
        {
            if (_Value)
            {
                this.buffer.WriteByte(1);
            }
            else
            {
                this.buffer.WriteByte(0);
            }
        }

        /// <summary>
        /// Writes the first <paramref name="_Count"/> bytes of <paramref name="_Buffer"/> directly to the stream, without a length prefix.
        /// </summary>
        /// <param name="_Buffer">The source byte array.</param>
        /// <param name="_Count">The number of bytes to copy from the start of <paramref name="_Buffer"/>.</param>
        public void Write(byte[] _Buffer, int _Count)
        {
            if (_Count > MAX_BYTE_LENGTH)
            {
                Debug.LogError("BinaryWriter Write: buffer is too large (" + _Count + ") bytes. The maximum buffer size is 2000M bytes.");
            }
            else
            {
                this.buffer.WriteBytes(_Buffer, (int)_Count);
            }
        }

        /// <summary>
        /// Writes <paramref name="_Count"/> bytes from <paramref name="_Buffer"/> at the given offset, without a length prefix.
        /// </summary>
        /// <param name="_Buffer">The source byte array.</param>
        /// <param name="_Offset">The target offset in the underlying buffer.</param>
        /// <param name="_Count">The number of bytes to write.</param>
        public void Write(byte[] _Buffer, int _Offset, int _Count)
        {
            if (_Count > MAX_BYTE_LENGTH)
            {
                Debug.LogError("BinaryWriter Write: buffer is too large (" + _Count + ") bytes. The maximum buffer size is 2000M bytes.");
            }
            else
            {
                this.buffer.WriteBytesAtOffset(_Buffer, (int)_Offset, (int)_Count);
            }
        }

        /// <summary>
        /// Writes a 32-bit length prefix followed by <paramref name="_Count"/> bytes from <paramref name="_Buffer"/>.
        /// </summary>
        /// <param name="_Buffer">The source byte array, or <c>null</c>.</param>
        /// <param name="_Count">The number of bytes to write; a zero count writes only a zero-length prefix.</param>
        public void WriteBytesAndSize(byte[] _Buffer, int _Count)
        {
            if (_Buffer == null || _Count == 0)
            {
                this.Write((int)0);
            }
            else if (_Count > MAX_BYTE_LENGTH)
            {
                Debug.LogError("BinaryWriter WriteBytesAndSize: buffer is too large (" + _Count + ") bytes. The maximum buffer size is 2000M bytes.");
            }
            else
            {
                this.Write((int)_Count);
                this.buffer.WriteBytes(_Buffer, (int)_Count);
            }
        }

        /// <summary>
        /// Writes a 32-bit length prefix followed by the full contents of <paramref name="_Buffer"/>, or a zero-length prefix when it is <c>null</c>.
        /// </summary>
        /// <param name="_Buffer">The byte array to write, or <c>null</c>.</param>
        public void WriteBytesFull(byte[] _Buffer)
        {
            if (_Buffer == null)
            {
                this.Write((int)0);
            }
            else if (_Buffer.Length > MAX_BYTE_LENGTH)
            {
                Debug.LogError("BinaryWriter WriteBytes: buffer is too large (" + _Buffer.Length + ") bytes. The maximum buffer size is 2000M bytes.");
            }
            else
            {
                this.Write((int)_Buffer.Length);
                this.buffer.WriteBytes(_Buffer, (int)_Buffer.Length);
            }
        }

        /// <summary>
        /// Writes a <see cref="Vector2"/> as two floats (x, y).
        /// </summary>
        /// <param name="_Value">The Vector2 value to write.</param>
        public void Write(Vector2 _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
        }

        /// <summary>
        /// Writes a <see cref="Vector2Int"/> as two 32-bit integers (x, y).
        /// </summary>
        /// <param name="_Value">The Vector2Int value to write.</param>
        public void Write(Vector2Int _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
        }

        /// <summary>
        /// Writes a <see cref="Vector3"/> as three floats (x, y, z).
        /// </summary>
        /// <param name="_Value">The Vector3 value to write.</param>
        public void Write(Vector3 _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
            this.Write(_Value.z);
        }

        /// <summary>
        /// Writes a <see cref="Vector3Int"/> as three 32-bit integers (x, y, z).
        /// </summary>
        /// <param name="_Value">The Vector3Int value to write.</param>
        public void Write(Vector3Int _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
            this.Write(_Value.z);
        }

        /// <summary>
        /// Writes a <see cref="Vector4"/> as four floats (x, y, z, w).
        /// </summary>
        /// <param name="_Value">The Vector4 value to write.</param>
        public void Write(Vector4 _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
            this.Write(_Value.z);
            this.Write(_Value.w);
        }

        /// <summary>
        /// Writes a <see cref="Color"/> as four floats (RGBA).
        /// </summary>
        /// <param name="_Value">The Color value to write.</param>
        public void Write(Color _Value)
        {
            this.Write(_Value.r);
            this.Write(_Value.g);
            this.Write(_Value.b);
            this.Write(_Value.a);
        }

        /// <summary>
        /// Writes a <see cref="Color32"/> as four bytes (RGBA).
        /// </summary>
        /// <param name="_Value">The Color32 value to write.</param>
        public void Write(Color32 _Value)
        {
            this.Write(_Value.r);
            this.Write(_Value.g);
            this.Write(_Value.b);
            this.Write(_Value.a);
        }

        /// <summary>
        /// Writes a <see cref="Quaternion"/> as four floats (x, y, z, w).
        /// </summary>
        /// <param name="_Value">The Quaternion value to write.</param>
        public void Write(Quaternion _Value)
        {
            this.Write(_Value.x);
            this.Write(_Value.y);
            this.Write(_Value.z);
            this.Write(_Value.w);
        }

        /// <summary>
        /// Writes a <see cref="Rect"/> as four floats (xMin, yMin, width, height).
        /// </summary>
        /// <param name="_Value">The Rect value to write.</param>
        public void Write(Rect _Value)
        {
            this.Write(_Value.xMin);
            this.Write(_Value.yMin);
            this.Write(_Value.width);
            this.Write(_Value.height);
        }

        /// <summary>
        /// Writes a <see cref="Plane"/> as its normal vector followed by its distance.
        /// </summary>
        /// <param name="_Value">The Plane value to write.</param>
        public void Write(Plane _Value)
        {
            this.Write(_Value.normal);
            this.Write(_Value.distance);
        }

        /// <summary>
        /// Writes a <see cref="Ray"/> as origin followed by direction.
        /// </summary>
        /// <param name="_Value">The Ray value to write.</param>
        public void Write(Ray _Value)
        {
            this.Write(_Value.origin);
            this.Write(_Value.direction);
        }

        /// <summary>
        /// Writes a <see cref="Matrix4x4"/> as 16 floats in row-major order.
        /// </summary>
        /// <param name="_Value">The Matrix4x4 value to write.</param>
        public void Write(Matrix4x4 _Value)
        {
            this.Write(_Value.m00);
            this.Write(_Value.m01);
            this.Write(_Value.m02);
            this.Write(_Value.m03);
            this.Write(_Value.m10);
            this.Write(_Value.m11);
            this.Write(_Value.m12);
            this.Write(_Value.m13);
            this.Write(_Value.m20);
            this.Write(_Value.m21);
            this.Write(_Value.m22);
            this.Write(_Value.m23);
            this.Write(_Value.m30);
            this.Write(_Value.m31);
            this.Write(_Value.m32);
            this.Write(_Value.m33);
        }
    }
}
