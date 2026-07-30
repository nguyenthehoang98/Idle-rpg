// System
using System;
using System.Runtime.CompilerServices;

// Unity
using UnityEngine;
using UnityEngine.Internal;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Binary;
using GUPS.AntiCheat.Core.Hash;
using GUPS.AntiCheat.Core.Protected;
using GUPS.AntiCheat.Core.Storage;

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;

// Allow the internal classes to be accessed by the test assembly.
[assembly: InternalsVisibleTo("GUPS.AntiCheat.Tests")]

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Drop-in replacement for <see cref="UnityEngine.PlayerPrefs"/> that adds XOR encryption, SHA1
    /// key hashing, SHA256 integrity signatures and owner binding to <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>.
    /// </summary>
    /// <remarks>
    /// Values are serialized via <see cref="PreferenceStorageItem"/>, optionally XOR-encrypted with the
    /// secret configured in <see cref="GlobalSettings"/>, base64-encoded and finally stored as a string
    /// under <see cref="UnityEngine.PlayerPrefs"/>. See <see cref="ProtectedIntPref"/> for a usage example.
    /// </remarks>
    public sealed class ProtectedPlayerPrefs
    {
        #region Error Messages

        internal const String ERROR_TYPE = "The type of the value read does not match the requested type.";
        internal const String ERROR_INTEGRITY = "The integrity of the storage item is not valid.";
        internal const String ERROR_OWNER = "You are not the owner of the storage item.";

        #endregion

        #region Key

        /// <summary>
        /// Returns the effective key used in <see cref="UnityEngine.PlayerPrefs"/>, optionally hashed.
        /// </summary>
        /// <param name="_Key">The caller-supplied logical key name.</param>
        /// <returns>The original key, or a SHA1 hash of it (base64) when <c>PlayerPreferences_Hash_Key</c> is enabled.</returns>
        private static String GetKeyName(String _Key)
        {
            String var_Key = _Key;

            // Optional key obfuscation: SHA1 the UTF8 bytes of the logical key and base64-encode the digest.
            if (GlobalSettings.Instance.PlayerPreferences_Hash_Key)
            {
                byte[] var_KeyBytes = System.Text.Encoding.UTF8.GetBytes(_Key);
                byte[] var_Hash = HashHelper.ComputeHash(EHashAlgorithm.SHA1, var_KeyBytes);
                var_Key = System.Convert.ToBase64String(var_Hash);
            }

            return var_Key;
        }

        /// <summary>
        /// Returns whether a value is stored for the given key (after optional key hashing).
        /// </summary>
        /// <param name="_Key">The logical key to check.</param>
        /// <returns><c>true</c> if a value exists for the key; otherwise <c>false</c>.</returns>
        public static bool HasKey(String _Key)
        {
            String var_Key = GetKeyName(_Key);

            return PlayerPrefs.HasKey(var_Key);
        }

        #endregion

        #region Set

        /// <summary>
        /// Writes a pre-built <see cref="PreferenceStorageItem"/> to <see cref="UnityEngine.PlayerPrefs"/> for testing.
        /// </summary>
        /// <param name="_Key">The key under which the item is stored.</param>
        /// <param name="_Item">The storage item to serialize, optionally XOR-encrypt and store.</param>
        /// <remarks>
        /// Intended for tests that need to inject crafted signatures or owners; production code should use
        /// the typed <c>Set*</c>/<c>Get*</c> methods.
        /// </remarks>
        internal static void SetRaw(String _Key, PreferenceStorageItem _Item)
        {
            String var_Key = GetKeyName(_Key);

            BinaryWriter var_Writer = new BinaryWriter();
            _Item.Write(var_Writer);

            byte[] var_ValueBytes = var_Writer.ToArray();

            // Optional XOR encryption using the secret configured in GlobalSettings.
            if (!String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
            {
                byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);

                for (int i = 0; i < var_ValueBytes.Length; i++)
                {
                    var_ValueBytes[i] ^= var_EncryptionKeyBytes[i % var_EncryptionKeyBytes.Length];
                }
            }

            // Base64-encode the (possibly encrypted) blob and persist as a PlayerPrefs string.
            String var_Value = System.Convert.ToBase64String(var_ValueBytes);
            PlayerPrefs.SetString(var_Key, var_Value);
        }

        /// <summary>
        /// Stores the unwrapped value of an <see cref="IProtected"/> container under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <param name="_Value">The protected container whose inner value is persisted.</param>
        public static void Set(String _Key, IProtected _Value)
        {
            Set(_Key, _Value.Value);
        }

        /// <summary>
        /// Stores an object under the given key, supporting any type in <see cref="EStorageType"/>.
        /// </summary>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <param name="_Value">The value to serialize and store.</param>
        /// <remarks>
        /// The value is wrapped in a <see cref="PreferenceStorageItem"/> (which adds owner and SHA256 signature),
        /// serialized to a byte buffer, XOR-encrypted with the configured secret when one is set, base64-encoded
        /// and finally written via <see cref="UnityEngine.PlayerPrefs.SetString(String, String)"/>.
        /// </remarks>
        public static void Set(String _Key, System.Object _Value)
        {
            String var_Key = GetKeyName(_Key);

            // Wrap the value so the on-disk bytes are bound to the device owner and SHA256 signature.
            PreferenceStorageItem var_Item = new PreferenceStorageItem(_Value);

            BinaryWriter var_Writer = new BinaryWriter();
            var_Item.Write(var_Writer);

            byte[] var_ValueBytes = var_Writer.ToArray();

            // Optional XOR encryption using the secret configured in GlobalSettings.
            if(!String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
            {
                byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);

                for (int i = 0; i < var_ValueBytes.Length; i++)
                {
                    var_ValueBytes[i] ^= var_EncryptionKeyBytes[i % var_EncryptionKeyBytes.Length];
                }
            }

            // Base64-encode the (possibly encrypted) blob and persist as a PlayerPrefs string.
            String var_Value = System.Convert.ToBase64String(var_ValueBytes);
            PlayerPrefs.SetString(var_Key, var_Value);

            if(AutoSave)
            {
                Save();
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Retrieves the raw <see cref="PreferenceStorageItem"/> for the given key without owner or signature checks.
        /// </summary>
        /// <param name="_Key">The key under which the item is stored.</param>
        /// <returns>The deserialized storage item, or <c>null</c> if the key does not exist.</returns>
        /// <remarks>
        /// Intended for tests and debugging. Decryption (when a secret is configured) is still applied,
        /// but signature and owner verification are skipped.
        /// </remarks>
        internal static PreferenceStorageItem GetRaw(String _Key)
        {
            String var_Key = GetKeyName(_Key);

            if (!PlayerPrefs.HasKey(var_Key))
            {
                return null;
            }

            String var_Value = PlayerPrefs.GetString(var_Key);
            byte[] var_ValueBytes = System.Convert.FromBase64String(var_Value);

            // Reverse the optional XOR encryption applied at write time.
            if (!String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
            {
                byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);

                for (int i = 0; i < var_ValueBytes.Length; i++)
                {
                    var_ValueBytes[i] ^= var_EncryptionKeyBytes[i % var_EncryptionKeyBytes.Length];
                }
            }

            BinaryReader var_Reader = new BinaryReader(var_ValueBytes);
            PreferenceStorageItem var_Item = new PreferenceStorageItem();
            var_Item.Read(var_Reader);

            return var_Item;
        }

        /// <summary>
        /// Reads a stored object of the given runtime type, supporting any type in <see cref="EStorageType"/>.
        /// </summary>
        /// <param name="_Type">The expected runtime type of the stored value.</param>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <returns>The deserialized value, or the default of <paramref name="_Type"/> when the key does not exist.</returns>
        /// <remarks>
        /// Decryption (when configured), storage-type mismatch detection, SHA256 integrity verification and
        /// device-owner verification are applied according to <see cref="GlobalSettings"/>; any failure raises
        /// an <see cref="Exception"/>.
        /// </remarks>
        public static System.Object Get(Type _Type, String _Key)
        {
            String var_Key = GetKeyName(_Key);

            if (!PlayerPrefs.HasKey(var_Key))
            {
                // Value types fall back to their default instance; reference types fall back to null.
                if (_Type.IsValueType)
                {
                    return Activator.CreateInstance(_Type);
                }

                return null;
            }

            String var_Value = PlayerPrefs.GetString(var_Key);
            byte[] var_ValueBytes = System.Convert.FromBase64String(var_Value);

            // Reverse the optional XOR encryption applied at write time.
            if (!String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
            {
                byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);

                for (int i = 0; i < var_ValueBytes.Length; i++)
                {
                    var_ValueBytes[i] ^= var_EncryptionKeyBytes[i % var_EncryptionKeyBytes.Length];
                }
            }

            BinaryReader var_Reader = new BinaryReader(var_ValueBytes);
            PreferenceStorageItem var_Item = new PreferenceStorageItem();
            var_Item.Read(var_Reader);

            if(var_Item.Type != StorageHelper.GetStorageType(_Type))
            {
                throw new Exception(ERROR_TYPE);
            }

            // SHA256 integrity check: signature is recomputed and compared against the on-disk one.
            if(GlobalSettings.Instance.PlayerPreferences_Verify_Integrity)
            {
                if(!var_Item.VerifySignature())
                {
                    throw new Exception(ERROR_INTEGRITY);
                }
            }

            // Owner binding: reject entries copied from a different device.
            if(!GlobalSettings.Instance.PlayerPreferences_Allow_Read_Any_Owner)
            {
                if(var_Item.Owner != UnityEngine.SystemInfo.deviceUniqueIdentifier)
                {
                    throw new Exception(ERROR_OWNER);
                }
            }

            return var_Item.Value;
        }

        /// <summary>
        /// Reads a stored object as <typeparamref name="T"/>, supporting any type in <see cref="EStorageType"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the stored value.</typeparam>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <returns>The deserialized value cast to <typeparamref name="T"/>, or <c>default(T)</c> when the key does not exist.</returns>
        public static T Get<T>(String _Key)
        {
            Type var_Type = typeof(T);

            return (T)Get(var_Type, _Key);
        }

        /// <summary>
        /// Reads a stored object without a runtime type check, supporting any type in <see cref="EStorageType"/>.
        /// </summary>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <returns>The deserialized value as <see cref="System.Object"/>, or <c>null</c> when the key does not exist.</returns>
        /// <remarks>
        /// Decryption, SHA256 integrity verification and device-owner verification still run according to
        /// <see cref="GlobalSettings"/>; only the storage-type check is skipped.
        /// </remarks>
        public static System.Object Get(String _Key)
        {
            String var_Key = GetKeyName(_Key);

            if (!PlayerPrefs.HasKey(var_Key))
            {
                return null;
            }

            String var_Value = PlayerPrefs.GetString(var_Key);
            byte[] var_ValueBytes = System.Convert.FromBase64String(var_Value);

            // Reverse the optional XOR encryption applied at write time.
            if (!String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
            {
                byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);

                for (int i = 0; i < var_ValueBytes.Length; i++)
                {
                    var_ValueBytes[i] ^= var_EncryptionKeyBytes[i % var_EncryptionKeyBytes.Length];
                }
            }

            BinaryReader var_Reader = new BinaryReader(var_ValueBytes);
            PreferenceStorageItem var_Item = new PreferenceStorageItem();
            var_Item.Read(var_Reader);

            // SHA256 integrity check: signature is recomputed and compared against the on-disk one.
            if (GlobalSettings.Instance.PlayerPreferences_Verify_Integrity)
            {
                if (!var_Item.VerifySignature())
                {
                    throw new Exception(ERROR_INTEGRITY);
                }
            }

            // Owner binding: reject entries copied from a different device.
            if (!GlobalSettings.Instance.PlayerPreferences_Allow_Read_Any_Owner)
            {
                if (var_Item.Owner != UnityEngine.SystemInfo.deviceUniqueIdentifier)
                {
                    throw new Exception(ERROR_OWNER);
                }
            }

            return var_Item.Value;
        }

        #endregion

        #region Int

        /// <summary>
        /// Stores an <see cref="Int32"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the integer is stored.</param>
        /// <param name="_Value">The integer value to store.</param>
        public static void SetInt(String _Key, int _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Int32"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the integer is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored integer, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static int GetInt(String _Key, [DefaultValue("0")] int _DefaultValue)
        {
            if(HasKey(_Key))
            {
                return Get<Int32>(_Key);
            }

            return PlayerPrefs.GetInt(_Key, _DefaultValue);
        }

        /// <summary>
        /// Reads the <see cref="Int32"/> value stored under the given key, or <c>0</c> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the integer is stored.</param>
        /// <returns>The stored integer, or <c>0</c> when the key is missing.</returns>
        public static int GetInt(String _Key)
        {
            return GetInt(_Key, 0);
        }

        #endregion

        #region Bool

        /// <summary>
        /// Stores a <see cref="Boolean"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the boolean is stored.</param>
        /// <param name="_Value">The boolean value to store.</param>
        public static void SetBool(String _Key, bool _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Boolean"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the boolean is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored boolean, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static bool GetBool(String _Key, [DefaultValue("false")] bool _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<bool>(_Key);
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Reads the <see cref="Boolean"/> value stored under the given key, or <c>false</c> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the boolean is stored.</param>
        /// <returns>The stored boolean, or <c>false</c> when the key is missing.</returns>
        public static bool GetBool(String _Key)
        {
            return GetBool(_Key, false);
        }

        #endregion

        #region Float

        /// <summary>
        /// Stores a <see cref="Single"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the float is stored.</param>
        /// <param name="_Value">The float value to store.</param>
        public static void SetFloat(String _Key, float _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Single"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the float is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored float, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static float GetFloat(String _Key, [DefaultValue("0.0f")] float _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<float>(_Key);
            }
            return PlayerPrefs.GetFloat(_Key, _DefaultValue);
        }

        /// <summary>
        /// Reads the <see cref="Single"/> value stored under the given key, or <c>0.0f</c> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the float is stored.</param>
        /// <returns>The stored float, or <c>0.0f</c> when the key is missing.</returns>
        public static float GetFloat(String _Key)
        {
            return GetFloat(_Key, 0.0f);
        }

        #endregion

        #region String

        /// <summary>
        /// Stores a <see cref="String"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the string is stored.</param>
        /// <param name="_Value">The string value to store.</param>
        public static void SetString(String _Key, string _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="String"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the string is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored string, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static string GetString(String _Key, [DefaultValue("")] string _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<string>(_Key);
            }
            return PlayerPrefs.GetString(_Key, _DefaultValue);
        }

        /// <summary>
        /// Reads the <see cref="String"/> value stored under the given key, or the empty string when missing.
        /// </summary>
        /// <param name="_Key">The key under which the string is stored.</param>
        /// <returns>The stored string, or <see cref="String.Empty"/> when the key is missing.</returns>
        public static string GetString(String _Key)
        {
            return GetString(_Key, "");
        }
        #endregion

        #region Vector2

        /// <summary>
        /// Stores a <see cref="Vector2"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the Vector2 is stored.</param>
        /// <param name="_Value">The Vector2 value to store.</param>
        public static void SetVector2(String _Key, Vector2 _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Vector2"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the Vector2 is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored Vector2, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static Vector2 GetVector2(String _Key, Vector2 _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<Vector2>(_Key);
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Reads the <see cref="Vector2"/> value stored under the given key, or <see cref="Vector2.zero"/> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the Vector2 is stored.</param>
        /// <returns>The stored Vector2, or <see cref="Vector2.zero"/> when the key is missing.</returns>
        public static Vector2 GetVector2(String _Key)
        {
            return GetVector2(_Key, Vector2.zero);
        }
        #endregion

        #region Vector3

        /// <summary>
        /// Stores a <see cref="Vector3"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the Vector3 is stored.</param>
        /// <param name="_Value">The Vector3 value to store.</param>
        public static void SetVector3(String _Key, Vector3 _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Vector3"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the Vector3 is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored Vector3, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static Vector3 GetVector3(String _Key, Vector3 _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<Vector3>(_Key);
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Reads the <see cref="Vector3"/> value stored under the given key, or <see cref="Vector3.zero"/> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the Vector3 is stored.</param>
        /// <returns>The stored Vector3, or <see cref="Vector3.zero"/> when the key is missing.</returns>
        public static Vector3 GetVector3(String _Key)
        {
            return GetVector3(_Key, Vector3.zero);
        }

        #endregion

        #region Vector4

        /// <summary>
        /// Stores a <see cref="Vector4"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the Vector4 is stored.</param>
        /// <param name="_Value">The Vector4 value to store.</param>
        public static void SetVector4(String _Key, Vector4 _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Vector4"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the Vector4 is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored Vector4, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static Vector4 GetVector4(String _Key, Vector4 _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<Vector4>(_Key);
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Reads the <see cref="Vector4"/> value stored under the given key, or <see cref="Vector4.zero"/> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the Vector4 is stored.</param>
        /// <returns>The stored Vector4, or <see cref="Vector4.zero"/> when the key is missing.</returns>
        public static Vector4 GetVector4(String _Key)
        {
            return GetVector4(_Key, Vector4.zero);
        }

        #endregion

        #region Quaternion

        /// <summary>
        /// Stores a <see cref="Quaternion"/> value under the given key.
        /// </summary>
        /// <param name="_Key">The key under which the Quaternion is stored.</param>
        /// <param name="_Value">The Quaternion value to store.</param>
        public static void SetQuaternion(String _Key, Quaternion _Value)
        {
            Set(_Key, _Value);
        }

        /// <summary>
        /// Reads the <see cref="Quaternion"/> value stored under the given key, or <paramref name="_DefaultValue"/>.
        /// </summary>
        /// <param name="_Key">The key under which the Quaternion is stored.</param>
        /// <param name="_DefaultValue">The value to return when the key does not exist.</param>
        /// <returns>The stored Quaternion, or <paramref name="_DefaultValue"/> when the key is missing.</returns>
        public static Quaternion GetQuaternion(String _Key, Quaternion _DefaultValue)
        {
            if (HasKey(_Key))
            {
                return Get<Quaternion>(_Key);
            }
            return _DefaultValue;
        }

        /// <summary>
        /// Reads the <see cref="Quaternion"/> value stored under the given key, or <see cref="Quaternion.identity"/> when missing.
        /// </summary>
        /// <param name="_Key">The key under which the Quaternion is stored.</param>
        /// <returns>The stored Quaternion, or <see cref="Quaternion.identity"/> when the key is missing.</returns>
        public static Quaternion GetQuaternion(String _Key)
        {
            return GetQuaternion(_Key, Quaternion.identity);
        }

        #endregion

        #region Save

        /// <summary>
        /// When <c>true</c>, every <c>Set*</c> call automatically invokes <see cref="Save"/>.
        /// </summary>
        public static bool AutoSave = false;

        /// <summary>
        /// Flushes any pending changes to disk via <see cref="UnityEngine.PlayerPrefs.Save"/>.
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        #endregion

        #region Delete

        /// <summary>
        /// Removes the key and its stored value from <see cref="UnityEngine.PlayerPrefs"/>.
        /// </summary>
        /// <param name="_Key">The logical key to remove; the actual stored key is hashed when configured.</param>
        public static void DeleteKey(String _Key)
        {
            String var_Key = GetKeyName(_Key);

            PlayerPrefs.DeleteKey(var_Key);
        }

        #endregion
    }
}
