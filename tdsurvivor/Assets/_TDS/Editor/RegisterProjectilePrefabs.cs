using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace _TDS.Editor
{
    /// <summary>
    /// Đăng ký mọi prefab trong Prefabs/Projectiles vào Addressables (address = tên file).
    /// Idempotent - entry đã có thì bỏ qua. Chạy lại mỗi khi thêm prefab đạn mới.
    /// </summary>
    public static class RegisterProjectilePrefabs
    {
        private const string ProjectileFolder = "Assets/_TDS assets/Prefabs/Projectiles";
        private const string GroupName = "Default Local Group";

        [MenuItem("Tools/Setup/Register Projectile Prefabs")]
        public static void Register()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[Setup] No AddressableAssetSettings found");
                return;
            }

            AddressableAssetGroup group = settings.FindGroup(GroupName);
            if (group == null) group = settings.DefaultGroup;

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { ProjectileFolder });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[Setup] No prefabs found under '{ProjectileFolder}'");
                return;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string address = System.IO.Path.GetFileNameWithoutExtension(path);

                if (settings.FindAssetEntry(guid) != null)
                {
                    Debug.Log($"[Setup] '{address}' already addressable, skip");
                    continue;
                }

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(address, settings);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

                Debug.Log($"[Setup] Registered '{path}' as addressable '{address}'");
            }
        }
    }
}
