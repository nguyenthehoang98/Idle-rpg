// System
using System;
using System.IO;
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
    /// Reads primitive C# and common Unity types from a byte buffer in little-endian order.
    /// </summary>
    internal class BinaryReader
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
        /// The underlying buffer holding the bytes to read.
        /// </summary>
        private Buffer buffer;

        /// <summary>
        /// The maximum supported byte length.
        /// </summary>
        private const int MAX_BYTE_LENGTH = 2147483647;

        /// <summary>
        /// The initial capacity of the shared string read buffer.
        /// </summary>
        private const int INITIAL_STRING_BUFFER_SIZE = 1024;

        /// <summary>
        /// Shared scratch buffer reused across string read operations.
        /// </summary>
        private static byte[] stringReaderBuffer;

        /// <summary>
        /// The encoding used when decoding strings (UTF-8).
        /// </summary>
        private static Encoding encoding;

        /// <summary>
        /// Scratch union for bit-exact uint to float conversion.
        /// </summary>
        private static UIntFloat floatConverter;

        /// <summary>
        /// Scratch union for bit-exact ulong to double conversion.
        /// </summary>
        private static LongDouble doubleConverter;

        /// <summary>
        /// Gets the current read position in the underlying buffer.
        /// </summary>
        public uint Position
        {
            get { return this.buffer.Position; }
        }

        /// <summary>
        /// Gets the total length of the underlying buffer.
        /// </summary>
        public int Length
        {
            get { return this.buffer.Length; }
        }

        /// <summary>
        /// Initializes a new reader backed by an empty buffer.
        /// </summary>
        public BinaryReader()
        {
            this.buffer = new Buffer();
            BinaryReader.Initialize();
        }

        /// <summary>
        /// Initializes a new reader backed by the supplied byte array (not copied).
        /// </summary>
        /// <param name="_Buffer">The byte array to read from.</param>
        public BinaryReader(byte[] _Buffer)
        {
            this.buffer = new Buffer(_Buffer);
            BinaryReader.Initialize();
        }

        /// <summary>
        /// Performs first-time initialization of the shared static state.
        /// </summary>
        static BinaryReader()
        {
            BinaryReader.Initialize();
        }

        /// <summary>
        /// Lazily initializes the shared string buffer and encoding on first use.
        /// </summary>
        private static void Initialize()
        {
            if (BinaryReader.encoding == null)
            {
                BinaryReader.stringReaderBuffer = new byte[INITIAL_STRING_BUFFER_SIZE];
                BinaryReader.encoding = new UTF8Encoding();
            }
        }

        /// <summary>
        /// Resets the read position to the start of the buffer.
        /// </summary>
        public void SeekZero()
        {
            this.buffer.SeekZero();
        }

        /// <summary>
        /// Replaces the underlying buffer with the supplied byte array and resets the position.
        /// </summary>
        /// <param name="_Buffer">The new backing byte array.</param>
        internal void Replace(byte[] _Buffer)
        {
            this.buffer.Replace(_Buffer);
        }

        /// <summary>
        /// Reads a value of the specified runtime type from the stream.
        /// </summary>
        /// <param name="_Type">The runtime type to read.</param>
        /// <returns>The decoded value, a default-constructed value type for unsupported structs, or <c>null</c> for unsupported reference types.</returns>
        public System.Object Read(Type _Type)
        {
            if (_Type == typeof(System.Byte))
                return this.ReadByte();
            else if (_Type == typeof(System.Boolean))
                return this.ReadBoolean();
            else if (_Type == typeof(System.Int16))
                return this.ReadInt16();
            else if (_Type == typeof(System.Int32))
                return this.ReadInt32();
            else if (_Type == typeof(System.Int64))
                return this.ReadInt64();
            else if (_Type == typeof(System.UInt16))
                return this.ReadUInt16();
            else if (_Type == typeof(System.UInt32))
                return this.ReadUInt32();
            else if (_Type == typeof(System.UInt64))
                return this.ReadUInt64();
            else if (_Type == typeof(System.Single))
                return this.ReadSingle();
            else if (_Type == typeof(System.Double))
                return this.ReadDouble();
            else if (_Type == typeof(System.Decimal))
                return this.ReadDecimal();
            else if (_Type == typeof(System.Char))
                return this.ReadChar();
            else if (_Type == typeof(System.String))
                return this.ReadString();
            else if (_Type == typeof(UnityEngine.Color))
                return this.ReadColor();
            else if (_Type == typeof(UnityEngine.Color32))
                return this.ReadColor32();
            else if (_Type == typeof(UnityEngine.Vector2))
                return this.ReadVector2();
            else if (_Type == typeof(UnityEngine.Vector2Int))
                return this.ReadVector2Int();
            else if (_Type == typeof(UnityEngine.Vector3))
                return this.ReadVector3();
            else if (_Type == typeof(UnityEngine.Vector2Int))
                return this.ReadVector3Int();
            else if (_Type == typeof(UnityEngine.Vector4))
                return this.ReadVector4();
            else if (_Type == typeof(UnityEngine.Quaternion))
                return this.ReadQuaternion();
            else if (_Type == typeof(UnityEngine.Rect))
                return this.ReadRect();
            else if (_Type == typeof(UnityEngine.Plane))
                return this.ReadPlane();
            else if (_Type == typeof(UnityEngine.Ray))
                return this.ReadRay();
            else if (_Type == typeof(UnityEngine.Matrix4x4))
                return this.ReadMatrix4x4();

            // Is a struct, create the default instance.
            if (_Type.IsValueType)
            {
                return Activator.CreateInstance(_Type);
            }

            // Is a class, return null.
            return null;
        }

        /// <summary>
        /// Reads a single unsigned byte from the stream.
        /// </summary>
        /// <returns>The byte read.</returns>
        public byte ReadByte()
        {
            return this.buffer.ReadByte();
        }

        /// <summary>
        /// Reads a single signed byte from the stream.
        /// </summary>
        /// <returns>The signed byte read.</returns>
        public sbyte ReadSByte()
        {
            return (sbyte)this.buffer.ReadByte();
        }

        /// <summary>
        /// Reads a signed 16-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 16-bit signed integer read.</returns>
        public short ReadInt16()
        {
            ushort num = 0;
            num = (ushort)(num | this.buffer.ReadByte());
            num = (ushort)(num | (ushort)(this.buffer.ReadByte() << 8));
            return (short)num;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 16-bit unsigned integer read.</returns>
        public ushort ReadUInt16()
        {
            ushort num = 0;
            num = (ushort)(num | this.buffer.ReadByte());
            return (ushort)(num | (ushort)(this.buffer.ReadByte() << 8));
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 32-bit signed integer read.</returns>
        public int ReadInt32()
        {
            uint num = 0u;
            num |= this.buffer.ReadByte();
            num = (uint)((int)num | this.buffer.ReadByte() << 8);
            num = (uint)((int)num | this.buffer.ReadByte() << 16);
            return (int)num | this.buffer.ReadByte() << 24;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 32-bit unsigned integer read.</returns>
        public uint ReadUInt32()
        {
            uint num = 0u;
            num |= this.buffer.ReadByte();
            num = (uint)((int)num | this.buffer.ReadByte() << 8);
            num = (uint)((int)num | this.buffer.ReadByte() << 16);
            return (uint)((int)num | this.buffer.ReadByte() << 24);
        }

        /// <summary>
        /// Reads a signed 64-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 64-bit signed integer read.</returns>
        public long ReadInt64()
        {
            ulong num = 0uL;
            ulong num2 = this.buffer.ReadByte();
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 8;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 16;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 24;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 32;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 40;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 48;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 56;
            return (long)(num | num2);
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream (little-endian).
        /// </summary>
        /// <returns>The 64-bit unsigned integer read.</returns>
        public ulong ReadUInt64()
        {
            ulong num = 0uL;
            ulong num2 = this.buffer.ReadByte();
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 8;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 16;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 24;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 32;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 40;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 48;
            num |= num2;
            num2 = (ulong)this.buffer.ReadByte() << 56;
            return num | num2;
        }

        /// <summary>
        /// Reads a <see cref="decimal"/> value from the stream as four 32-bit integers.
        /// </summary>
        /// <returns>The decimal value read.</returns>
        public decimal ReadDecimal()
        {
            return new decimal(new int[4]
            {
                this.ReadInt32(),
                this.ReadInt32(),
                this.ReadInt32(),
                this.ReadInt32()
            });
        }

        /// <summary>
        /// Reads a single-precision float from the stream.
        /// </summary>
        /// <returns>The float value read.</returns>
        public float ReadSingle()
        {
            floatConverter.IntValue = this.ReadUInt32();
            return floatConverter.FloatValue;
        }

        /// <summary>
        /// Reads a double-precision float from the stream.
        /// </summary>
        /// <returns>The double value read.</returns>
        public double ReadDouble()
        {
            doubleConverter.LongValue = this.ReadUInt64();
            return doubleConverter.DoubleValue;
        }

        /// <summary>
        /// Reads a UTF-8 string written with a leading length prefix.
        /// </summary>
        /// <returns>The decoded string, an empty string when the length is zero, or <c>null</c> when the length prefix is -1.</returns>
        /// <exception cref="IOException">Thrown when the encoded length exceeds the maximum scratch buffer size.</exception>
        public string ReadString()
        {
            int num = this.ReadInt32();
            if (num == -1)
            {
                return null;
            }
            if (num == 0)
            {
                return "";
            }
            while (num > BinaryReader.stringReaderBuffer.Length)
            {
                if (BinaryReader.stringReaderBuffer.Length >= Int32.MaxValue / 2)
                {
                    throw new IOException("Required array size of " + num + " too large");
                }

                BinaryReader.stringReaderBuffer = new byte[BinaryReader.stringReaderBuffer.Length * 2];
            }
            this.buffer.ReadBytes(BinaryReader.stringReaderBuffer, num);
            char[] chars = BinaryReader.encoding.GetChars(BinaryReader.stringReaderBuffer, 0, num);
            return new string(chars);
        }

        /// <summary>
        /// Reads a single 8-bit char from the stream.
        /// </summary>
        /// <returns>The char value read.</returns>
        public char ReadChar()
        {
            return (char)this.buffer.ReadByte();
        }

        /// <summary>
        /// Reads a boolean from the stream (1 = true, 0 = false).
        /// </summary>
        /// <returns>The boolean value read.</returns>
        public bool ReadBoolean()
        {
            return this.buffer.ReadByte() == 1;
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream into a new array.
        /// </summary>
        /// <param name="_Count">The number of bytes to read; must be non-negative.</param>
        /// <returns>A newly allocated array containing the bytes read.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="_Count"/> is negative or exceeds the remaining buffer.</exception>
        public byte[] ReadBytes(int _Count)
        {
            if (_Count < 0)
            {
                throw new IndexOutOfRangeException("BinaryReader ReadBytes " + _Count);
            }
            byte[] array = new byte[_Count];
            this.buffer.ReadBytes(array, (int)_Count);
            return array;
        }

        /// <summary>
        /// Reads a 32-bit length prefix followed by that many bytes.
        /// </summary>
        /// <returns>The bytes read, or an empty array when the length prefix is zero.</returns>
        public byte[] ReadBytesAndSize()
        {
            int num = this.ReadInt32();
            if (num == 0)
            {
                return new byte[0];
            }
            return this.ReadBytes(num);
        }

        /// <summary>
        /// Reads a <see cref="Vector2"/> from the stream.
        /// </summary>
        /// <returns>The Vector2 read.</returns>
        public Vector2 ReadVector2()
        {
            return new Vector2(this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Vector2Int"/> from the stream.
        /// </summary>
        /// <returns>The Vector2Int read.</returns>
        public Vector2Int ReadVector2Int()
        {
            return new Vector2Int(this.ReadInt32(), this.ReadInt32());
        }

        /// <summary>
        /// Reads a <see cref="Vector3"/> from the stream.
        /// </summary>
        /// <returns>The Vector3 read.</returns>
        public Vector3 ReadVector3()
        {
            return new Vector3(this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Vector3Int"/> from the stream.
        /// </summary>
        /// <returns>The Vector3Int read.</returns>
        public Vector3Int ReadVector3Int()
        {
            return new Vector3Int(this.ReadInt32(), this.ReadInt32(), this.ReadInt32());
        }

        /// <summary>
        /// Reads a <see cref="Vector4"/> from the stream.
        /// </summary>
        /// <returns>The Vector4 read.</returns>
        public Vector4 ReadVector4()
        {
            return new Vector4(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Color"/> from the stream as four floats (RGBA).
        /// </summary>
        /// <returns>The Color read.</returns>
        public Color ReadColor()
        {
            return new Color(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Color32"/> from the stream as four bytes (RGBA).
        /// </summary>
        /// <returns>The Color32 read.</returns>
        public Color32 ReadColor32()
        {
            return new Color32(this.ReadByte(), this.ReadByte(), this.ReadByte(), this.ReadByte());
        }

        /// <summary>
        /// Reads a <see cref="Quaternion"/> from the stream as four floats (x, y, z, w).
        /// </summary>
        /// <returns>The Quaternion read.</returns>
        public Quaternion ReadQuaternion()
        {
            return new Quaternion(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Rect"/> from the stream as four floats (x, y, width, height).
        /// </summary>
        /// <returns>The Rect read.</returns>
        public Rect ReadRect()
        {
            return new Rect(this.ReadSingle(), this.ReadSingle(), this.ReadSingle(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Plane"/> from the stream as a normal vector followed by a distance.
        /// </summary>
        /// <returns>The Plane read.</returns>
        public Plane ReadPlane()
        {
            return new Plane(this.ReadVector3(), this.ReadSingle());
        }

        /// <summary>
        /// Reads a <see cref="Ray"/> from the stream as origin followed by direction.
        /// </summary>
        /// <returns>The Ray read.</returns>
        public Ray ReadRay()
        {
            return new Ray(this.ReadVector3(), this.ReadVector3());
        }

        /// <summary>
        /// Reads a <see cref="Matrix4x4"/> from the stream as 16 floats in row-major order.
        /// </summary>
        /// <returns>The Matrix4x4 read.</returns>
        public Matrix4x4 ReadMatrix4x4()
        {
            Matrix4x4 result = default(Matrix4x4);
            result.m00 = this.ReadSingle();
            result.m01 = this.ReadSingle();
            result.m02 = this.ReadSingle();
            result.m03 = this.ReadSingle();
            result.m10 = this.ReadSingle();
            result.m11 = this.ReadSingle();
            result.m12 = this.ReadSingle();
            result.m13 = this.ReadSingle();
            result.m20 = this.ReadSingle();
            result.m21 = this.ReadSingle();
            result.m22 = this.ReadSingle();
            result.m23 = this.ReadSingle();
            result.m30 = this.ReadSingle();
            result.m31 = this.ReadSingle();
            result.m32 = this.ReadSingle();
            result.m33 = this.ReadSingle();
            return result;
        }

        /// <summary>
        /// Returns a diagnostic string describing the underlying buffer.
        /// </summary>
        /// <returns>A short description of the reader state.</returns>
        public override string ToString()
        {
            return this.buffer.ToString();
        }

        /// <summary>
        /// Returns a default-constructed value for the specified type.
        /// </summary>
        /// <param name="_Type">The type to produce a default for.</param>
        /// <returns>A new instance for value types, or <c>null</c> for reference types.</returns>
        private object GetDefaultValue(Type _Type)
        {
            if (_Type.IsValueType)
                return Activator.CreateInstance(_Type);

            return null;
        }
    }
}