// System
using System;
using System.IO;
using System.Runtime.CompilerServices;

// Unity
using UnityEngine;
using UnityEngine.Internal;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Hash;
using GUPS.AntiCheat.Core.Protected;
using GUPS.AntiCheat.Core.Storage;

// GUPS - AntiCheat
using GUPS.AntiCheat.Settings;
using System.Threading.Tasks;

// Allow the internal classes to be accessed by the test assembly.
[assembly: InternalsVisibleTo("GUPS.AntiCheat.Tests")]

namespace GUPS.AntiCheat.Protected.Storage.Prefs
{
    /// <summary>
    /// Thread-safe, file-backed drop-in replacement for <see cref="UnityEngine.PlayerPrefs"/> with owner binding and optional encryption.
    /// </summary>
    /// <remarks>
    /// All entries are kept in a single <see cref="StorageContainer"/> file at <see cref="FilePath"/>, owned by
    /// <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>. The file is lazily loaded on first access,
    /// optionally XOR-encrypted with the secret from <see cref="GlobalSettings"/>, and access is serialized via
    /// an internal lock. See <see cref="ProtectedIntPref"/> for a usage example.
    /// </remarks>
    public static class ProtectedFileBasedPlayerPrefs
    {
        #region Error Messages

        internal const String ERROR_NO_STORAGE = "The storage was not loaded yet, nothing to save.";
        internal const String ERROR_OWNER = "You are not the owner of the storage item.";

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets the on-disk file path for the storage container.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>Application.persistentDataPath + DirectorySeparatorChar + "playerprefs.dat"</c>.
        /// Must be set before <see cref="Load"/> is called for the first time to take effect.
        /// </remarks>
        public static String FilePath { get; set; } = Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar + "playerprefs.dat";

        /// <summary>
        /// The lock used to serialize all read, write and load operations on <see cref="storage"/>.
        /// </summary>
        private static object lockHandle = new object();

        /// <summary>
        /// The lazily loaded storage container; <c>null</c> until <see cref="Load"/> succeeds.
        /// </summary>
        private static StorageContainer storage = null;

        /// <summary>
        /// Gets and sets whether each <c>Set*</c> or <see cref="DeleteKey"/> call should immediately persist via <see cref="Save"/>.
        /// </summary>
        public static bool AutoSave { get; set; } = true;

        #endregion

        #region Key

        /// <summary>
        /// Returns the effective key used in the storage container, optionally hashed.
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
        /// Returns whether a value is stored for the given key, loading the storage file on first access.
        /// </summary>
        /// <param name="_Key">The logical key to check.</param>
        /// <returns><c>true</c> if a value exists for the key; otherwise <c>false</c>.</returns>
        public static bool HasKey(String _Key)
        {
            Load();

            String var_Key = GetKeyName(_Key);

            return storage.Has(var_Key);
        }

        /// <summary>
        /// Asynchronously runs <see cref="HasKey"/> on a background thread.
        /// </summary>
        /// <param name="_Key">The logical key to check.</param>
        /// <returns>A task whose result is <c>true</c> if a value exists for the key; otherwise <c>false</c>.</returns>
        public static Task<bool> HasKeyAsync(String _Key)
        {
            return Task.Run(() =>
            {
                return HasKey(_Key);
            });
        }

        #endregion

        #region Set

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
        /// Stores an object under the given key in the storage file, supporting any type in <see cref="EStorageType"/>.
        /// </summary>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <param name="_Value">The value to store; existing entries for the same key are overwritten.</param>
        /// <remarks>
        /// The storage file is loaded on first call. When <see cref="AutoSave"/> is <c>true</c>, the change is
        /// flushed to disk immediately via <see cref="Save"/>.
        /// </remarks>
        public static void Set(String _Key, System.Object _Value)
        {
            Load();

            String var_Key = GetKeyName(_Key);

            if (storage.Has(var_Key))
            {
                storage.Set(var_Key, _Value);
            }
            else
            {
                storage.Add(var_Key, _Value);
            }

            if (AutoSave)
            {
                Save();
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Reads a stored object of the given runtime type from the storage file.
        /// </summary>
        /// <param name="_Type">The expected runtime type of the stored value.</param>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <returns>The deserialized value, or the default of <paramref name="_Type"/> when the key does not exist.</returns>
        public static System.Object Get(Type _Type, String _Key)
        {
            Load();

            String var_Key = GetKeyName(_Key);

            if (!storage.Has(var_Key))
            {
                // Value types fall back to their default instance; reference types fall back to null.
                if (_Type.IsValueType)
                {
                    return Activator.CreateInstance(_Type);
                }

                return null;
            }

            System.Object var_Value = storage.Get(var_Key);

            return var_Value;
        }

        /// <summary>
        /// Reads a stored object as <typeparamref name="T"/> from the storage file.
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
        /// Reads a stored object from the storage file without a runtime type check.
        /// </summary>
        /// <param name="_Key">The key under which the value is stored.</param>
        /// <returns>The deserialized value as <see cref="System.Object"/>, or <c>null</c> when the key does not exist.</returns>
        public static System.Object Get(String _Key)
        {
            Load();

            String var_Key = GetKeyName(_Key);

            if (!storage.Has(var_Key))
            {
                return null;
            }

            System.Object var_Value = storage.Get(var_Key);

            return var_Value;
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
            if (HasKey(_Key))
            {
                return Get<Int32>(_Key);
            }
            return _DefaultValue;
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
            return _DefaultValue;
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
            return _DefaultValue;
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

        #region Delete

        /// <summary>
        /// Removes the key and its stored value from the storage file.
        /// </summary>
        /// <param name="_Key">The logical key to remove; the actual stored key is hashed when configured.</param>
        /// <remarks>
        /// When <see cref="AutoSave"/> is <c>true</c>, the change is flushed to disk immediately via <see cref="Save"/>.
        /// </remarks>
        public static void DeleteKey(String _Key)
        {
            Load();

            String var_Key = GetKeyName(_Key);

            storage.Remove(var_Key);

            if (AutoSave)
            {
                Save();
            }
        }

        #endregion

        #region Load

        /// <summary>
        /// Initializes the storage container and, if a file exists at <see cref="FilePath"/>, reads it into memory.
        /// </summary>
        /// <remarks>
        /// Runs exactly once per process; subsequent calls are no-ops. The container is created with the device
        /// identifier as owner and, if a secret is configured in <see cref="GlobalSettings"/>, with XOR encryption.
        /// When <c>PlayerPreferences_Allow_Read_Any_Owner</c> is disabled, the on-disk owner is compared against
        /// the device identifier and a mismatch throws.
        /// </remarks>
        /// <exception cref="Exception">Thrown when the on-disk owner does not match the current device identifier.</exception>
        public static void Load()
        {
            lock (lockHandle)
            {
                // Initialize the container exactly once per process; later calls are no-ops.
                if (storage == null)
                {
                    if(String.IsNullOrEmpty(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key))
                    {
                        // No encryption key configured: plain container owned by this device.
                        storage = new StorageContainer(UnityEngine.SystemInfo.deviceUniqueIdentifier);
                    } 
                    else
                    {
                        // XOR-encrypted container using the configured secret bytes.
                        byte[] var_EncryptionKeyBytes = System.Text.Encoding.UTF8.GetBytes(GlobalSettings.Instance.PlayerPreferences_Value_Encryption_Key);
                        storage = new StorageContainer(UnityEngine.SystemInfo.deviceUniqueIdentifier, var_EncryptionKeyBytes);
                    }
                }
                else
                {
                    return;
                }

                if (System.IO.File.Exists(FilePath))
                {
                    using (FileStream var_FileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        storage.Read(var_FileStream);
                    }
                }

                // Owner binding: reject files copied from a different device.
                if (!GlobalSettings.Instance.PlayerPreferences_Allow_Read_Any_Owner)
                {
                    if (storage.Owner != UnityEngine.SystemInfo.deviceUniqueIdentifier)
                    {
                        throw new Exception(ERROR_OWNER);
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously runs <see cref="Load"/> on a background thread.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation.</returns>
        /// <exception cref="Exception">Thrown when the on-disk owner does not match the current device identifier.</exception>
        public static async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                Load();
            });
        }

        #endregion

        #region Save

        /// <summary>
        /// Writes the in-memory storage container to the file at <see cref="FilePath"/>.
        /// </summary>
        /// <remarks>
        /// The file is opened with <see cref="FileMode.Create"/>, so existing contents are replaced atomically
        /// from the writer's point of view. <see cref="Load"/> must have been called previously.
        /// </remarks>
        /// <exception cref="Exception">Thrown when no storage container has been loaded yet.</exception>
        public static void Save()
        {
            lock (lockHandle)
            {
                if (storage == null)
                {
                    throw new Exception(ERROR_NO_STORAGE);
                }

                // FileMode.Create replaces the existing file in one shot from the writer's point of view.
                using (FileStream var_FileStream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    storage.Write(var_FileStream);
                }
            }
        }

        /// <summary>
        /// Asynchronously runs <see cref="Save"/> on a background thread.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        /// <exception cref="Exception">Thrown when no storage container has been loaded yet.</exception>
        public static async Task SaveAsync()
        {
            await Task.Run(() =>
            {
                Save();
            });
        }

        #endregion
    }
}