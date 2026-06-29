#if UNITY_INCLUDE_TESTS && UNITY_EDITOR
using System;
using System.Collections;
using _KITSystem.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

namespace _Game.Configs
{
    public class LoadBundleConfigTest
    {
        [UnityTest]
        public IEnumerator LoadAddressable_MonsterConfig()
        {
            yield return LoadBundle<MonsterConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_LevelConfig()
        {
            yield return LoadBundle<LevelConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_WeaponConfig()
        {
            yield return LoadBundle<WeaponConfig>();
        }

        [UnityTest]
        public IEnumerator LoadAddressable_PlayerConfig()
        {
            yield return LoadBundle<PlayerConfig>();
        }

        private IEnumerator LoadBundle<T>(Action<T> callback = null) where T : IGameConfig
        {
            string name = typeof(T).Name;

            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(name);

            yield return handle;

            Assert.AreEqual(AsyncOperationStatus.Succeeded, handle.Status);

            Assert.NotNull(handle.Result);

            try
            {
                T data = JsonUtility.FromJson<T>(handle.Result.text);

                data.OnMappingValue();

                callback?.Invoke(data);
            }
            catch (Exception e)
            {
                Assert.Fail($"Parse error game_config '{name}'");
            }

            Addressables.Release(handle);
        }
    }
}
#endif