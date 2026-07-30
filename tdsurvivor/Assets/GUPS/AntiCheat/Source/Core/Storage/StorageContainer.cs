// System
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Binary;
using GUPS.AntiCheat.Core.Hash;

// Allow the internal classes to be accessed by the test assembly.
[assembly: InternalsVisibleTo("GUPS.AntiCheat.Tests")]

namespace GUPS.AntiCheat.Core.Storage
{
    /// <summary>
    /// Key/value container that can be optionally encrypted with an XOR keystream and signed with a SHA-256 hash, useful for save files and similar persisted data.
    /// </summary>
    public class StorageContainer : IReadAble, IWriteAble
    {
        internal const String ERROR_SIGNATURE = "Invalid signature.";
        internal const String ERROR_DUPLICATE = "An item with the same key has already been added.";
        internal const String ERROR_TYPE = "The type of the value read does not match the requested type.";

        /// <summary>
        /// The keyed storage items held by the container.
        /// </summary>
        private Dictionary<String, StorageItem> items;

        /// <summary>
        /// Gets or sets the keyed storage items held by the container.
        /// </summary>
        internal Dictionary<String, StorageItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        /// <summary>
        /// The optional owner identifier (for example a player name or device id).
        /// </summary>
        private String owner;

        /// <summary>
        /// Gets or sets the owner identifier (for example a player name or device id).
        /// </summary>
        public String Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        /// <summary>
        /// The optional symmetric key used for XOR encryption/decryption.
        /// </summary>
        private byte[] encryptionKey;

        /// <summary>
        /// The signature hash that authenticates the container's contents.
        /// </summary>
        private String signature;

        /// <summary>
        /// Gets the signature computed during the last write, used to verify authenticity on read.
        /// </summary>
        public String Signature
        {
            get { return signature; }
        }

        /// <summary>
        /// Initializes an empty, unowned storage container.
        /// </summary>
        public StorageContainer()
        {
            this.items = new Dictionary<String, StorageItem>();
        }

        /// <summary>
        /// Initializes an empty storage container with the supplied owner identifier (for example a player name or <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>).
        /// </summary>
        /// <param name="_Owner">The owner identifier to associate with this container.</param>
        public StorageContainer(String _Owner)
        {
            this.items = new Dictionary<String, StorageItem>();
            this.owner = _Owner;
        }

        /// <summary>
        /// Initializes an empty storage container with the supplied owner identifier and a symmetric XOR encryption key.
        /// </summary>
        /// <param name="_Owner">The owner identifier to associate with this container.</param>
        /// <param name="_EncryptionKey">The symmetric key applied to the serialized payload via XOR.</param>
        public StorageContainer(String _Owner, byte[] _EncryptionKey)
        {
            this.items = new Dictionary<String, StorageItem>();
            this.owner = _Owner;
            this.encryptionKey = _EncryptionKey;
        }

        /// <summary>
        /// Returns whether the container holds an item with the specified key.
        /// </summary>
        /// <param name="_Key">The key to look up.</param>
        /// <returns><c>true</c> if an item with <paramref name="_Key"/> exists; otherwise <c>false</c>.</returns>
        public bool Has(String _Key)
        {
           return this.Items.ContainsKey(_Key);
        }

        /// <summary>
        /// Adds a new item to the container.
        /// </summary>
        /// <param name="_Key">The unique key for the new item.</param>
        /// <param name="_Value">The value to store; its runtime type must map to a supported <see cref="EStorageType"/>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="_Key"/> is already present.</exception>
        public void Add(String _Key, Object _Value)
        {
            if(this.Items.ContainsKey(_Key))
            {
                throw new ArgumentException(ERROR_DUPLICATE);
            }

            this.Items.Add(_Key, new StorageItem(_Value));
        }

        /// <summary>
        /// Adds or replaces the item at <paramref name="_Key"/> with the supplied value.
        /// </summary>
        /// <param name="_Key">The key to write to.</param>
        /// <param name="_Value">The value to store; its runtime type must map to a supported <see cref="EStorageType"/>.</param>
        public void Set(String _Key, Object _Value)
        {
            this.Items[_Key] = new StorageItem(_Value);
        }

        /// <summary>
        /// Removes the item with the specified key, if present.
        /// </summary>
        /// <param name="_Key">The key of the item to remove.</param>
        public void Remove(String _Key)
        {
            this.Items.Remove(_Key);
        }

        /// <summary>
        /// Returns the value stored under <paramref name="_Key"/> as <see cref="object"/>.
        /// </summary>
        /// <param name="_Key">The key of the item to read.</param>
        /// <returns>The stored value.</returns>
        public Object Get(String _Key)
        {
            return this.Items[_Key].Value;
        }

        /// <summary>
        /// Returns the value stored under <paramref name="_Key"/> cast to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="_Key">The key of the item to read.</param>
        /// <returns>The stored value cast to <typeparamref name="T"/>.</returns>
        /// <exception cref="Exception">Thrown when the stored type does not match <typeparamref name="T"/>.</exception>
        public T Get<T>(String _Key)
        {
            StorageItem var_Item = this.Items[_Key];

            Type var_Type = typeof(T);

            if (var_Item.Type != StorageHelper.GetStorageType(var_Type))
            {
                throw new Exception(ERROR_TYPE);
            }

            return (T)this.Items[_Key].Value;
        }

        /// <summary>
        /// Removes all items from the container.
        /// </summary>
        public void Clear()
        {
            this.Items.Clear();
        }

        /// <summary>
        /// Reads and replaces the container's contents from the specified stream.
        /// </summary>
        /// <param name="_Stream">The stream positioned at the start of the encoded container.</param>
        public void Read(System.IO.Stream _Stream)
        {
            byte[] var_Binary = new byte[_Stream.Length];

            _Stream.Read(var_Binary, 0, var_Binary.Length);

            this.Read(var_Binary);
        }

        /// <summary>
        /// Reads and replaces the container's contents from the supplied binary blob.
        /// </summary>
        /// <param name="_Binary">The encoded container bytes.</param>
        public void Read(byte[] _Binary)
        {
            BinaryReader var_Reader = new BinaryReader(_Binary);

            ((IReadAble)this).Read(var_Reader);
        }

        /// <inheritdoc cref="IReadAble.Read"/>
        /// <remarks>
        /// Verifies the signature against a SHA-256 hash of the payload, optionally XOR-decrypts the payload with <c>encryptionKey</c>,
        /// then decodes the owner and the keyed storage items.
        /// </remarks>
        /// <exception cref="Exception">Thrown when the signature does not match the payload hash.</exception>
        void IReadAble.Read(BinaryReader _Reader)
        {
            byte[] var_Binary = _Reader.ReadBytesAndSize();

            byte[] var_Hash = HashHelper.ComputeHash(EHashAlgorithm.SHA256, var_Binary);

            this.signature = _Reader.ReadString();

            // Verify the payload hash matches the stored signature before any decoding.
            if (this.signature != "H_" + HashHelper.ToHex(var_Hash, false, false))
            {
                throw new Exception("Invalid signature.");
            }

            // Symmetric XOR decryption with the configured key.
            if (this.encryptionKey != null && this.encryptionKey.Length > 0)
            {
                for (int i = 0; i < this.encryptionKey.Length; i++)
                {
                    var_Binary[i] ^= this.encryptionKey[i % this.encryptionKey.Length];
                }
            }

            BinaryReader var_Reader = new BinaryReader(var_Binary);

            this.Owner = var_Reader.ReadString();

            int var_Count = var_Reader.ReadInt32();

            this.Items = new Dictionary<String, StorageItem>();

            for (int i = 0; i < var_Count; i++)
            {
                String var_Key = var_Reader.ReadString();
                StorageItem var_Item = new StorageItem();
                var_Item.Read(var_Reader);
                this.Items.Add(var_Key, var_Item);
            }
        }

        /// <summary>
        /// Serializes the container's contents to the specified stream.
        /// </summary>
        /// <param name="_Stream">The destination stream.</param>
        public void Write(System.IO.Stream _Stream)
        {
            _Stream.Write(this.Write());
        }

        /// <summary>
        /// Serializes the container's contents to a new byte array.
        /// </summary>
        /// <returns>The encoded container bytes.</returns>
        public byte[] Write()
        {
            BinaryWriter var_Writer = new BinaryWriter();

            ((IWriteAble)this).Write(var_Writer);

            return var_Writer.AsArray();
        }

        /// <inheritdoc cref="IWriteAble.Write"/>
        /// <remarks>
        /// Encodes owner and items into a payload, optionally XOR-encrypts it with <c>encryptionKey</c>,
        /// writes the payload, then writes a SHA-256-based signature so that <see cref="IReadAble.Read"/> can validate it.
        /// </remarks>
        void IWriteAble.Write(BinaryWriter _Writer)
        {
            // Serialize the payload into a separate writer so its bytes can be hashed and (optionally) encrypted before emission.
            BinaryWriter var_Writer = new BinaryWriter();

            var_Writer.Write(this.Owner);

            var_Writer.Write(this.Items.Count);

            foreach (var var_Pair in this.Items)
            {
                var_Writer.Write(var_Pair.Key);
                var_Pair.Value.Write(var_Writer);
            }

            byte[] var_Binary = var_Writer.ToArray();

            // Symmetric XOR encryption with the configured key.
            if (this.encryptionKey != null && this.encryptionKey.Length > 0)
            {
                for (int i = 0; i < this.encryptionKey.Length; i++)
                {
                    var_Binary[i] ^= this.encryptionKey[i % this.encryptionKey.Length];
                }
            }

            byte[] var_Hash = HashHelper.ComputeHash(EHashAlgorithm.SHA256, var_Binary);

            _Writer.WriteBytesFull(var_Binary);

            this.signature = "H_" + HashHelper.ToHex(var_Hash, false, false);

            _Writer.Write(this.signature);
        }
    }
}
