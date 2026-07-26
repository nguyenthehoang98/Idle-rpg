// System
using System;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Binary;
using GUPS.AntiCheat.Core.Hash;
using GUPS.AntiCheat.Core.Storage;

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// A <see cref="StorageItem"/> that binds the stored value to an owner identifier and a SHA256 signature.
    /// </summary>
    /// <remarks>
    /// The owner is used to detect cross-device copies of the preference file, and the signature
    /// is recomputed on write and verified on read to detect tampering of the on-disk bytes.
    /// </remarks>
    internal class PreferenceStorageItem : StorageItem
    {
        /// <summary>
        /// The owner identifier (device unique identifier of the writer).
        /// </summary>
        private String owner;

        /// <summary>
        /// Gets and sets the owner identifier of this storage item.
        /// </summary>
        public String Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        /// <summary>
        /// The SHA256 signature over (type, value, owner) computed at write time.
        /// </summary>
        private byte[] signature;

        /// <summary>
        /// Gets the SHA256 signature computed over the item's type, value and owner.
        /// </summary>
        public byte[] Signature
        {
            get { return signature; }
        }

        /// <summary>
        /// Initializes a new <see cref="PreferenceStorageItem"/> with an empty value and the owner set to
        /// <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        public PreferenceStorageItem()
            :base()
        {
            // See https://docs.unity3d.com/ScriptReference/SystemInfo-deviceUniqueIdentifier.html.
            this.owner = UnityEngine.SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// Initializes a new <see cref="PreferenceStorageItem"/> with the specified value and the owner set to
        /// <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        /// <param name="_Value">The value to wrap.</param>
        public PreferenceStorageItem(Object _Value)
            :base(_Value)
        {
            // See https://docs.unity3d.com/ScriptReference/SystemInfo-deviceUniqueIdentifier.html.
            this.owner = UnityEngine.SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// Computes a SHA256 signature over the item's type, value and owner.
        /// </summary>
        /// <returns>The SHA256 hash bytes for the current item state.</returns>
        public byte[] ComputeSignature()
        {
            // Create a binary writer.
            BinaryWriter var_Writer = new BinaryWriter();

            // Write the storage type.
            var_Writer.Write((Byte)this.Type);

            // Write the value.
            StorageHelper.Write(var_Writer, this.Value);

            // Write the owner.
            var_Writer.Write(this.Owner);

            // Get the binary data.
            byte[] var_Binary = var_Writer.ToArray();

            // Return the hash of the binary data.
            return HashHelper.ComputeHash(EHashAlgorithm.SHA256, var_Binary);
        }

        /// <summary>
        /// Verifies the on-disk signature by recomputing it from the current state and comparing the two.
        /// </summary>
        /// <returns><c>true</c> if the stored signature matches the recomputed one; otherwise <c>false</c>.</returns>
        public bool VerifySignature()
        {
            // Compute the signature.
            byte[] var_Signature = this.ComputeSignature();

            // Compare the signatures.
            return HashHelper.CompareHashes(this.Signature, var_Signature);
        }

        /// <summary>
        /// Reads the type, value, owner and signature for this item from the given binary reader.
        /// </summary>
        /// <remarks>
        /// The on-disk layout is [content-bytes][signature]; content is then parsed as [type][value][owner].
        /// Integrity is not verified here; callers must invoke <see cref="VerifySignature"/> separately.
        /// </remarks>
        /// <param name="_Reader">The binary reader to read from.</param>
        public override void Read(BinaryReader _Reader)
        {
            // Read the binary data.
            byte[] var_Binary = _Reader.ReadBytesAndSize();

            // Read the signature.
            this.signature = _Reader.ReadBytesAndSize();

            // Read the content of the item.
            BinaryReader var_Reader = new BinaryReader(var_Binary);

            // Read the storage type.
            this.Type = (EStorageType)var_Reader.ReadByte();

            // Read the value.
            this.Value = StorageHelper.Read(var_Reader, this.Type);

            // Read the owner.
            this.Owner = var_Reader.ReadString();
        }

        /// <summary>
        /// Writes the type, value, owner and a freshly computed SHA256 signature for this item.
        /// </summary>
        /// <remarks>
        /// The on-disk layout is [content-bytes][signature], where content is [type][value][owner].
        /// The signature is computed over the content bytes and stored alongside them.
        /// </remarks>
        /// <param name="_Writer">The binary writer to write to.</param>
        public override void Write(BinaryWriter _Writer)
        {
            // Write the content of the item to an own writer allowing to calculate a hash on the stored data.
            BinaryWriter var_Writer = new BinaryWriter();

            // Write the storage type.
            var_Writer.Write((Byte)this.Type);

            // Write the value.
            StorageHelper.Write(var_Writer, this.Value);

            // Write the owner.
            var_Writer.Write(this.Owner);

            // Get the binary data.
            byte[] var_Binary = var_Writer.ToArray();

            // Compute the hash.
            this.signature = HashHelper.ComputeHash(EHashAlgorithm.SHA256, var_Binary);

            // Write the binary data.
            _Writer.WriteBytesFull(var_Binary);

            // Write the calculated signature.
            _Writer.WriteBytesFull(this.signature);
        }
    }
}
