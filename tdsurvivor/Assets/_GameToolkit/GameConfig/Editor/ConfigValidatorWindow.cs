using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace _KITSystem.Config.Editor
{
    public class ConfigValidatorWindow : EditorWindow
    {
        private string dataFolder = "Assets/_FightCode/Config/Downloads";
        private Vector2 scrollPos;
        private List<ValidationError> errors = new List<ValidationError>();
        private bool hasScanned;
        private bool isScanning;

        private const string EditorPrefsFolderKey = "ConfigValidator_Folder";
        private static readonly string RelationsFile = "Assets/_FightSource/Configs/ConfigRelations.json";

        private static readonly Dictionary<string, string> FieldAlias = new Dictionary<string, string>
        {
            { "EquipmentsPool", "EquipmentsID" },
            { "SkillBuffsPool", "SkillBuffsID" },
            { "PortalsID", "SpawnPortalsID" },
        };

        [MenuItem("Tools/Config/Validator")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigValidatorWindow>("Config Validator");
            window.Show();
        }

        private void OnEnable()
        {
            dataFolder = EditorPrefs.GetString(EditorPrefsFolderKey, dataFolder);
        }

        private void OnGUI()
        {
            GUILayout.Space(8);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Data folder:", GUILayout.Width(80));
                var newFolder = EditorGUILayout.TextField(dataFolder);
                if (newFolder != dataFolder)
                {
                    dataFolder = newFolder;
                    EditorPrefs.SetString(EditorPrefsFolderKey, dataFolder);
                }

                using (new EditorGUI.DisabledGroupScope(isScanning))
                {
                    if (GUILayout.Button("Scan", GUILayout.Width(80)))
                        Scan();
                }
            }

            GUILayout.Space(4);

            if (isScanning)
            {
                EditorGUILayout.HelpBox("Scanning...", MessageType.Info);
                return;
            }

            if (!hasScanned) return;

            GUILayout.Space(4);

            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox("All OK — no missing references", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Found {errors.Count} missing reference(s)", MessageType.Error);
            }

            GUILayout.Space(4);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var error in errors)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(error.SourceType, EditorStyles.boldLabel, GUILayout.Width(150));
                        GUILayout.Label($"Row {error.RowIndex}", GUILayout.Width(60));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Ping", GUILayout.Width(50)))
                            PingConfig(error.SourceType);
                    }

                    EditorGUILayout.LabelField("Field", error.SourceField);
                    EditorGUILayout.LabelField("Value", error.MissingValue.ToString());
                    EditorGUILayout.LabelField("Expected in", $"{error.TargetType}.{error.TargetField}");
                }

                GUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void PingConfig(string typeName)
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            var rel = Path.Combine(dataFolder, typeName + ".json").Replace("\\", "/");
            var abs = Path.Combine(projectPath, rel.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(abs))
            {
                AssetDatabase.ImportAsset(rel);
                var a = AssetDatabase.LoadAssetAtPath<TextAsset>(rel);
                if (a != null) EditorGUIUtility.PingObject(a);
            }
        }

        // ----------------------------------------------------------------
        // SCAN
        // ----------------------------------------------------------------
        private void Scan()
        {
            isScanning = true;
            errors.Clear();
            hasScanned = false;
            Repaint();

            try
            {
                var projectPath = Path.GetDirectoryName(Application.dataPath);
                var assetFolder = Path.Combine(projectPath,
                    dataFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!Directory.Exists(assetFolder))
                {
                    EditorUtility.DisplayDialog("Error", $"Folder not found: {dataFolder}", "OK");
                    return;
                }

                // 1. Parse ConfigRelations.json
                var rawRelations = ParseRelationsFile();
                if (rawRelations.Count == 0)
                {
                    EditorUtility.DisplayDialog("Error", "ConfigRelations.json is empty or not found", "OK");
                    return;
                }

                // 2. Collect unique config type names from relations
                var neededTypes = new HashSet<string>();
                foreach (var (srcKey, tgtKey) in rawRelations)
                {
                    neededTypes.Add(srcKey.config);
                    neededTypes.Add(tgtKey.config);
                }

                // 3. Find actual C# types via TypeCache
                var allConfigTypes = TypeCache.GetTypesDerivedFrom<IGameConfig>()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .ToList();

                var typeMap = new Dictionary<string, Type>();
                foreach (var name in neededTypes)
                {
                    var t = allConfigTypes.FirstOrDefault(x => x.Name == name);
                    if (t != null) typeMap[name] = t;
                    else Debug.LogWarning($"Type not found: {name}");
                }

                // 4. Load JSON files for those types
                var loaded = new Dictionary<Type, object>();
                foreach (var kv in typeMap)
                {
                    var jsonPath = Path.Combine(assetFolder, kv.Key + ".json");
                    if (!File.Exists(jsonPath))
                    {
                        Debug.LogWarning($"JSON not found: {jsonPath}");
                        continue;
                    }

                    var json = File.ReadAllText(jsonPath);
                    var obj = JsonUtility.FromJson(json, kv.Value);
                    if (obj != null) loaded[kv.Value] = obj;
                }

                // 5. Run each relation
                foreach (var (srcKey, tgtKey) in rawRelations)
                {
                    if (!typeMap.TryGetValue(srcKey.config, out var srcType)) continue;
                    if (!typeMap.TryGetValue(tgtKey.config, out var tgtType)) continue;
                    if (!loaded.TryGetValue(srcType, out var srcObj)) continue;
                    if (!loaded.TryGetValue(tgtType, out var tgtObj)) continue;

                    var srcList = GetListField(srcObj, srcKey.list);
                    if (srcList == null || srcList.Count == 0) continue;

                    var tgtList = GetListField(tgtObj, tgtKey.list);
                    if (tgtList == null || tgtList.Count == 0) continue;

                    // Build target set: collect all values of the target field
                    var tgtSet = new HashSet<int>();
                    var tgtElemType = tgtList[0].GetType();
                    var tgtField = tgtElemType.GetField(tgtKey.field,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (tgtField == null) continue;
                    foreach (var item in tgtList)
                    {
                        if (tgtField.FieldType == typeof(int))
                            tgtSet.Add((int)tgtField.GetValue(item));
                    }

                    if (tgtSet.Count == 0) continue;

                    // Resolve source field alias
                    var srcElemType = srcList[0].GetType();
                    var srcFieldName = ResolveAlias(srcElemType, srcKey.field);
                    var srcField = srcElemType.GetField(srcFieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (srcField == null) continue;

                    int row = 0;
                    foreach (var item in srcList)
                    {
                        row++;

                        if (srcField.FieldType == typeof(int))
                        {
                            var v = (int)srcField.GetValue(item);
                            if (v <= 0) continue;
                            if (!tgtSet.Contains(v))
                                errors.Add(Err(srcKey, srcFieldName, v, row, tgtKey));
                        }
                        else if (srcField.FieldType == typeof(int[]))
                        {
                            var arr = (int[])srcField.GetValue(item);
                            if (arr == null || arr.Length == 0) continue;
                            foreach (var v in arr)
                            {
                                if (v <= 0) continue;
                                if (!tgtSet.Contains(v))
                                    errors.Add(Err(srcKey, srcFieldName + "[]", v, row, tgtKey));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Validation error: {ex}");
                EditorUtility.DisplayDialog("Error", ex.Message, "OK");
            }
            finally
            {
                isScanning = false;
                hasScanned = true;
                Repaint();
            }
        }

        // ----------------------------------------------------------------
        // HELPERS
        // ----------------------------------------------------------------

        private static ValidationError Err(
            (string config, string list, string field) src, string srcField, int v, int row,
            (string config, string list, string field) tgt)
        {
            return new ValidationError
            {
                SourceType = src.config,
                SourceField = $"{src.list}[].{srcField}",
                MissingValue = v,
                RowIndex = row,
                TargetType = tgt.config,
                TargetField = $"{tgt.list}[].{tgt.field}",
            };
        }

        private static string ResolveAlias(Type elemType, string fieldName)
        {
            var f = elemType.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) return fieldName;
            return FieldAlias.TryGetValue(fieldName, out var a) ? a : fieldName;
        }

        private static IList GetListField(object obj, string fieldName)
        {
            var f = obj.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return f?.GetValue(obj) as IList;
        }

        // ----------------------------------------------------------------
        // PARSE ConfigRelations.json
        // ----------------------------------------------------------------

        private static List<((string config, string list, string field), (string config, string list, string field))>
            ParseRelationsFile()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Application.dataPath),
                RelationsFile.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Relations file not found: {path}");
                return new List<((string, string, string), (string, string, string))>();
            }

            var json = File.ReadAllText(path);
            var pairs = ParseFlatJson(json);
            var result = new List<((string, string, string), (string, string, string))>();

            foreach (var (key, val) in pairs)
            {
                var s = key.Split('.');
                var t = val.Split('.');
                if (s.Length < 3 || t.Length < 3)
                {
                    Debug.LogWarning($"Skip invalid: {key} → {val}");
                    continue;
                }

                result.Add((
                    (s[0], s[1], string.Join(".", s.Skip(2))),
                    (t[0], t[1], string.Join(".", t.Skip(2)))
                ));
            }

            return result;
        }

        private static List<(string Key, string Value)> ParseFlatJson(string json)
        {
            var result = new List<(string, string)>();
            json = json.Trim();

            if (json.StartsWith("{")) json = json.Substring(1);
            if (json.EndsWith("}")) json = json.Substring(0, json.Length - 1);

            int depth = 0, start = 0;
            var parts = new List<string>();
            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{' || json[i] == '[') depth++;
                else if (json[i] == '}' || json[i] == ']') depth--;
                else if (json[i] == ',' && depth == 0)
                {
                    parts.Add(json.Substring(start, i - start));
                    start = i + 1;
                }
            }
            var last = json.Substring(start).Trim();
            if (last.Length > 0) parts.Add(last);

            foreach (var p in parts)
            {
                var ci = p.IndexOf(':');
                if (ci < 0) continue;
                result.Add((p.Substring(0, ci).Trim().Trim('"'),
                            p.Substring(ci + 1).Trim().Trim('"')));
            }

            return result;
        }

        // ----------------------------------------------------------------
        // DATA CLASSES
        // ----------------------------------------------------------------

        public class ValidationError
        {
            public string SourceType;
            public string SourceField;
            public int MissingValue;
            public int RowIndex;
            public string TargetType;
            public string TargetField;
        }
    }
}
