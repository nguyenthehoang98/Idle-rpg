using System;
using System.Collections.Generic;
using _KITSystem.Resource;
using _KITSystem.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace _KITSystem.Popup
{
    public sealed class PopupManager : Singleton<PopupManager>
    {
        private readonly List<PopupBase> popups = new List<PopupBase>();

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                PopupBase popup = popups[i];
                bool dirty = false;
                if (popup == null) dirty = true;
                else if (popup.SceneId == scene.buildIndex && popup.IsDestroyOnLoadScene)
                {
                    dirty = true;
                    Type type = popup.GetType();
                    Object.Destroy(popup.gameObject);
                    KitLoaded.UnCache(GetPathOfPopup(type));
                }

                if (dirty) popups.RemoveAt(i);
            }
        }

        public int CountPopupActive()
        {
            int count = 0;
            foreach (var popup in popups)
            {
                if (popup != null && popup.gameObject.activeSelf) count++;
            }

            return count;
        }

        public async void Push<T>(bool destroyOnLoadScene = true) where T : PopupBase
        {
            await PushAsync<T>(destroyOnLoadScene);
        }

        public async UniTask<T> PushAsync<T>(bool destroyOnLoadScene = true) where T : PopupBase
        {
            if (TryGetPopup(out T popup))
            {
                popups.Remove(popup);
            }
            else
            {
                GameObject go = await KitLoaded.LoadAsync<GameObject>(GetPathOfPopup(typeof(T)));
                popup = KitPool.Instantiate(go).GetComponent<T>();
            }

#if UNITY_EDITOR
            Debug.Log($"[Popup]: PushAsync '{typeof(T).Name}', popup='{popup}'");
#endif

            SetupPopup(popup, destroyOnLoadScene);
            popups.Add(popup);
            popup.Open();
            return popup;
        }

        public PopupBase Push(PopupBase popup, bool destroyOnLoadScene = true)
        {
            PopupBase instance = KitPool.Instantiate(popup);

            SetupPopup(instance, destroyOnLoadScene);
            popups.Add(instance);
            instance.Open();
            return instance;
        }

        private void SetupPopup(PopupBase popup, bool destroyOnLoadScene)
        {
            popup.transform.SetParent(transform);

            RectTransform rect = popup.GetComponent<RectTransform>();
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
            rect.anchoredPosition3D = Vector3.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;

            popup.IsDestroyOnLoadScene = destroyOnLoadScene;
            popup.SceneId = SceneManager.GetActiveScene().buildIndex;
            popup.transform.SetAsLastSibling();
        }

        public bool TryGetPopup<T>(out T popup) where T : PopupBase
        {
            foreach (var p in popups)
            {
                if (p is T)
                {
                    popup = (T)p;
                    return true;
                }
            }

            popup = null;
            return false;
        }

        static string GetPathOfPopup(Type type) => type.Name;
    }
}