// System
using System;
using System.Runtime.CompilerServices;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Binary;

// Allow the internal classes to be accessed by the test assembly.
[assembly: InternalsVisibleTo("GUPS.AntiCheat.Tests")]

namespace GUPS.AntiCheat.Core.Storage
{
    /// <summary>
    /// A single typed value held in a <see cref="StorageContainer"/>, encoded as a one-byte type tag followed by the value bytes.
    /// </summary>
    internal class StorageItem : IReadAble, IWriteAble
    {
        /// <summary>
        /// The storage type tag identifying how <see cref="Value"/> is encoded.
        /// </summary>
        public EStorageType Type;

        /// <summary>
        /// The boxed value; its runtime type must match <see cref="Type"/>.
        /// </summary>
        public Object Value;

        /// <summary>
        /// Initializes an empty storage item to be populated by <see cref="Read"/>.
        /// </summary>
        public StorageItem()
        {
        }

        /// <summary>
        /// Initializes a storage item from a value, inferring <see cref="Type"/> from its runtime type.
        /// </summary>
        /// <param name="_Value">The value to wrap; its runtime type must map to a supported <see cref="EStorageType"/>.</param>
        public StorageItem(Object _Value)
        {
            this.Type = StorageHelper.GetStorageType(_Value);

            this.Value = _Value;
        }

        /// <inheritdoc cref="IReadAble.Read"/>
        public virtual void Read(BinaryReader _Reader)
        {
            this.Type = (EStorageType)_Reader.ReadByte();

            this.Value = StorageHelper.Read(_Reader, this.Type);
        }

        /// <inheritdoc cref="IWriteAble.Write"/>
        public virtual void Write(BinaryWriter _Writer)
        {
            _Writer.Write((Byte)this.Type);

            StorageHelper.Write(_Writer, this.Value);
        }
    }
}
