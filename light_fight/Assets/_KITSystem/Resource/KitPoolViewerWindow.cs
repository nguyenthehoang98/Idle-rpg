#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _KITSystem.Resource
{
    public class KitPoolViewerWindow : EditorWindow
    {
        private Vector2 scroll;
        private bool autoRefresh = true;
        private double lastRefreshTime;

        [MenuItem("Tools/KIT/Kit Pool Viewer")]
        public static void Open()
        {
            GetWindow<KitPoolViewerWindow>("Kit Pool Viewer");
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (!autoRefresh) return;

            if (EditorApplication.timeSinceStartup - lastRefreshTime > 0.05f)
            {
                lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var pools = KitPool.EditorPools;
            if (pools == null || pools.Count == 0)
            {
                EditorGUILayout.HelpBox("Pool is empty", MessageType.Info);
            }
            else
            {
                foreach (var kv in pools)
                {
                    DrawPool(kv.Key, kv.Value);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPool(string key, Pool pool)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"Pool: {key}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Total", pool.Total.ToString());
            EditorGUILayout.LabelField("Active", pool.ActiveCount.ToString());

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Ping Objects"))
            {
                foreach (var obj in pool.allObjects)
                {
                    if (obj != null)
                        EditorGUIUtility.PingObject(obj);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
#endif