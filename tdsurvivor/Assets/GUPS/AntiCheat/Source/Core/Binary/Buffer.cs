// System
using System;

namespace GUPS.AntiCheat.Core.Binary
{
    /// <summary>
    /// A dynamic byte buffer that grows on demand as data is written.
    /// </summary>
    internal class Buffer
    {
        /// <summary>
        /// The underlying byte array used for storage.
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// The current read/write position in the buffer.
        /// </summary>
        private uint position;

        /// <summary>
        /// The initial capacity of a freshly constructed buffer, in bytes.
        /// </summary>
        private const int INIT_BUFFER_SIZE = 64;

        /// <summary>
        /// The multiplicative factor used when resizing the buffer.
        /// </summary>
        private const float GROWTH_FACTOR = 1.5f;

        /// <summary>
        /// Gets the current position in the buffer.
        /// </summary>
        public uint Position
        {
            get { return this.position; }
        }

        /// <summary>
        /// Gets the length of the buffer.
        /// </summary>
        public int Length
        {
            get { return this.buffer.Length; }
        }

        /// <summary>
        /// Initializes a new buffer with the default initial capacity.
        /// </summary>
        public Buffer()
        {
            this.buffer = new byte[INIT_BUFFER_SIZE];
        }

        /// <summary>
        /// Initializes a new buffer wrapping the supplied byte array (not copied).
        /// </summary>
        /// <param name="_Buffer">The backing byte array.</param>
        public Buffer(byte[] _Buffer)
        {
            this.buffer = _Buffer;
        }

        /// <summary>
        /// Reads a single byte from the buffer and advances the position.
        /// </summary>
        /// <returns>The byte read from the buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when attempting to read beyond the buffer's length.</exception>
        public byte ReadByte()
        {
            if (this.position >= this.buffer.Length)
            {
                throw new IndexOutOfRangeException("ByteReader:ReadByte out of range:" + this.ToString());
            }
            return this.buffer[this.position++];
        }

        /// <summary>
        /// Reads a specified number of bytes from the buffer into the provided byte array.
        /// </summary>
        /// <param name="_Buffer">The byte array to read into.</param>
        /// <param name="_Count">The number of bytes to read.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown when attempting to read beyond the buffer's length.</exception>
        public void ReadBytes(byte[] _Buffer, int _Count)
        {
            if (this.position + _Count > this.buffer.Length)
            {
                throw new IndexOutOfRangeException("ByteReader:ReadBytes out of range: (" + _Count + ") " + this.ToString());
            }
            for (int num = 0; num < _Count; num = (int)(num + 1))
            {
                _Buffer[num] = this.buffer[this.position + num];
            }
            this.position += (uint)_Count;
        }

        /// <summary>
        /// Returns an ArraySegment representing the current buffer contents.
        /// </summary>
        /// <returns>An ArraySegment containing the buffer data up to the current position.</returns>
        internal ArraySegment<byte> AsArraySegment()
        {
            return new ArraySegment<byte>(this.buffer, 0, (int)this.position);
        }

        /// <summary>
        /// Writes a single byte to the buffer and advances the position.
        /// </summary>
        /// <param name="_Value">The byte to write.</param>
        public void WriteByte(byte _Value)
        {
            this.WriteCheckForSpace(1);
            this.buffer[this.position] = _Value;
            this.position += 1u;
        }

        /// <summary>
        /// Writes two bytes to the buffer and advances the position.
        /// </summary>
        /// <param name="_Value0">The first byte to write.</param>
        /// <param name="_Value1">The second byte to write.</param>
        public void WriteByte2(byte _Value0, byte _Value1)
        {
            this.WriteCheckForSpace(2);
            this.buffer[this.position] = _Value0;
            this.buffer[this.position + 1] = _Value1;
            this.position += 2u;
        }

        /// <summary>
        /// Writes four bytes to the buffer and advances the position.
        /// </summary>
        /// <param name="_Value0">The first byte to write.</param>
        /// <param name="_Value1">The second byte to write.</param>
        /// <param name="_Value2">The third byte to write.</param>
        /// <param name="_Value3">The fourth byte to write.</param>
        public void WriteByte4(byte _Value0, byte _Value1, byte _Value2, byte _Value3)
        {
            this.WriteCheckForSpace(4);
            this.buffer[this.position] = _Value0;
            this.buffer[this.position + 1] = _Value1;
            this.buffer[this.position + 2] = _Value2;
            this.buffer[this.position + 3] = _Value3;
            this.position += 4u;
        }

        /// <summary>
        /// Writes eight bytes to the buffer and advances the position.
        /// </summary>
        /// <param name="_Value0">The first byte to write.</param>
        /// <param name="_Value1">The second byte to write.</param>
        /// <param name="_Value2">The third byte to write.</param>
        /// <param name="_Value3">The fourth byte to write.</param>
        /// <param name="_Value4">The fifth byte to write.</param>
        /// <param name="_Value5">The sixth byte to write.</param>
        /// <param name="_Value6">The seventh byte to write.</param>
        /// <param name="_Value7">The eighth byte to write.</param>
        public void WriteByte8(byte _Value0, byte _Value1, byte _Value2, byte _Value3, byte _Value4, byte _Value5, byte _Value6, byte _Value7)
        {
            this.WriteCheckForSpace(8);
            this.buffer[this.position] = _Value0;
            this.buffer[this.position + 1] = _Value1;
            this.buffer[this.position + 2] = _Value2;
            this.buffer[this.position + 3] = _Value3;
            this.buffer[this.position + 4] = _Value4;
            this.buffer[this.position + 5] = _Value5;
            this.buffer[this.position + 6] = _Value6;
            this.buffer[this.position + 7] = _Value7;
            this.position += 8u;
        }

        /// <summary>
        /// Writes bytes to the buffer at a specified offset.
        /// </summary>
        /// <param name="_Buffer">The byte array to write from.</param>
        /// <param name="_TargetOffset">The offset in the target buffer to start writing at.</param>
        /// <param name="_Count">The number of bytes to write.</param>
        public void WriteBytesAtOffset(byte[] _Buffer, int _TargetOffset, int _Count)
        {
            uint num = (uint)(_Count + _TargetOffset);
            this.WriteCheckForSpace((int)num);
            if (_TargetOffset == 0 && _Count == _Buffer.Length)
            {
                _Buffer.CopyTo(this.buffer, (int)this.position);
            }
            else
            {
                for (int i = 0; i < _Count; i++)
                {
                    this.buffer[_TargetOffset + i] = _Buffer[i];
                }
            }
            if (num > this.position)
            {
                this.position = num;
            }
        }

        /// <summary>
        /// Writes bytes to the buffer and advances the position.
        /// </summary>
        /// <param name="_Buffer">The byte array to write from.</param>
        /// <param name="_Count">The number of bytes to write.</param>
        public void WriteBytes(byte[] _Buffer, int _Count)
        {
            this.WriteCheckForSpace(_Count);
            if (_Count == _Buffer.Length)
            {
                _Buffer.CopyTo(this.buffer, (int)this.position);
            }
            else
            {
                for (int i = 0; i < _Count; i++)
                {
                    this.buffer[this.position + i] = _Buffer[i];
                }
            }
            this.position += (uint)_Count;
        }

        /// <summary>
        /// Ensures the buffer has enough free space for the specified number of bytes, growing it by <see cref="GROWTH_FACTOR"/> if needed.
        /// </summary>
        /// <param name="_Count">The number of additional bytes that must fit from the current position.</param>
        private void WriteCheckForSpace(int _Count)
        {
            if (this.position + _Count >= this.buffer.Length)
            {
                int num = (int)Math.Ceiling((double)((float)this.buffer.Length * GROWTH_FACTOR));
                while (this.position + _Count >= num)
                {
                    num = (int)Math.Ceiling((double)((float)num * GROWTH_FACTOR));
                }
                byte[] array = new byte[num];
                this.buffer.CopyTo(array, 0);
                this.buffer = array;
            }
        }

        /// <summary>
        /// Resets the buffer position to zero.
        /// </summary>
        public void SeekZero()
        {
            this.position = 0u;
        }

        /// <summary>
        /// Replaces the current buffer with a new byte array and resets the position.
        /// </summary>
        /// <param name="_Buffer">The new byte array to use as the buffer.</param>
        public void Replace(byte[] _Buffer)
        {
            this.buffer = _Buffer;
            this.position = 0u;
        }

        /// <summary>
        /// Returns a diagnostic string describing the buffer's size and position.
        /// </summary>
        /// <returns>A short description of the buffer state.</returns>
        public override string ToString()
        {
            return string.Format("Buffer sz:{0} pos:{1}", this.buffer.Length, this.position);
        }
    }
}
