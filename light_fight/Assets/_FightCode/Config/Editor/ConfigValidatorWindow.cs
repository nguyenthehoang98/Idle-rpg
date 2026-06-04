using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using _KITSystem.Data;
using UnityEditor;
using UnityEngine;

namespace _FightCode.Config.Editor
{
    public class ConfigValidatorWindow : EditorWindow
    {
        private string dataFolder = "Assets/_FightCode/Config/Downloads";
        private Vector2 scrollPos;
        private List<ValidationError> errors = new List<ValidationError>();
        private bool hasScanned;
        private bool isScanning;
        private HashSet<string> missingConfigs = new HashSet<string>();
        private int scannedCount;

        private const string EditorPrefsFolderKey = "ConfigValidator_Folder";

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
                    {
                        Scan();
                    }
                }
            }

            GUILayout.Space(4);

            if (isScanning)
            {
                EditorGUILayout.HelpBox("Scanning...", MessageType.Info);
            }
            else if (hasScanned)
            {
                foreach (var c in missingConfigs)
                {
                    EditorGUILayout.HelpBox($"MISSING: {c} not found in {dataFolder}", MessageType.Warning);
                }

                if (missingConfigs.Count > 0)
                    GUILayout.Space(4);

                if (errors.Count == 0)
                {
                    EditorGUILayout.HelpBox($"All OK - {scannedCount} configs, no missing references", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"Found {errors.Count} missing reference(s) across {scannedCount} configs",
                        MessageType.Error);
                }
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
                        {
                            PingConfig(error.SourceType);
                        }
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
            var relativePath = Path.Combine(dataFolder, typeName + ".json").Replace("\\", "/");
            var fullPath = Path.Combine(projectPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(fullPath))
            {
                AssetDatabase.ImportAsset(relativePath);
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            }
        }

        private void Scan()
        {
            isScanning = true;
            errors.Clear();
            missingConfigs.Clear();
            hasScanned = false;
            Repaint();

            try
            {
                var projectPath = Path.GetDirectoryName(Application.dataPath);
                var assetFolder = Path.Combine(projectPath, dataFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!Directory.Exists(assetFolder))
                {
                    EditorUtility.DisplayDialog("Error", $"Folder not found: {dataFolder}", "OK");
                    return;
                }

                var configTypes = TypeCache.GetTypesDerivedFrom<IGameConfig>()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .ToList();

                var loaded = new Dictionary<Type, object>();

                foreach (var type in configTypes)
                {
                    var jsonPath = Path.Combine(assetFolder, type.Name + ".json");
                    if (!File.Exists(jsonPath))
                    {
                        missingConfigs.Add(type.Name);
                        continue;
                    }

                    var json = File.ReadAllText(jsonPath);
                    var obj = JsonUtility.FromJson(json, type);
                    if (obj != null)
                        loaded[type] = obj;
                }

                scannedCount = loaded.Count;
                var lookups = BuildLookups(loaded);
                var relations = DefineRelations(configTypes);

                foreach (var rel in relations)
                {
                    if (!loaded.TryGetValue(rel.SourceType, out var sourceObj)) continue;
                    if (!loaded.TryGetValue(rel.TargetType, out _))
                    {
                        missingConfigs.Add(rel.TargetType.Name);
                        continue;
                    }

                    var sourceList = GetListField(sourceObj, rel.SourceListField);
                    if (sourceList == null) continue;

                    var targetSet = rel.TargetType == rel.SourceType
                        ? BuildSetFromList(sourceList, rel.TargetField)
                        : lookups.GetValueOrDefault((rel.TargetType, rel.TargetListField, rel.TargetField));

                    if (targetSet == null) continue;

                    var rowIndex = 0;
                    foreach (var item in sourceList)
                    {
                        rowIndex++;

                        if (rel.IsArray)
                        {
                            var arr = GetIntArrayField(item, rel.SourceField);
                            if (arr == null) continue;
                            foreach (var val in arr)
                            {
                                if (val <= 0) continue;
                                if (!targetSet.Contains(val))
                                {
                                    errors.Add(new ValidationError
                                    {
                                        SourceType = rel.SourceType.Name,
                                        SourceField = $"{rel.SourceListField}[].{rel.SourceField}[]",
                                        MissingValue = val,
                                        RowIndex = rowIndex,
                                        TargetType = rel.TargetType.Name,
                                        TargetField = $"{rel.TargetListField}[].{rel.TargetField}"
                                    });
                                }
                            }
                        }
                        else
                        {
                            var val = GetIntField(item, rel.SourceField);
                            if (val <= 0) continue;
                            if (!targetSet.Contains(val))
                            {
                                errors.Add(new ValidationError
                                {
                                    SourceType = rel.SourceType.Name,
                                    SourceField = $"{rel.SourceListField}[].{rel.SourceField}",
                                    MissingValue = val,
                                    RowIndex = rowIndex,
                                    TargetType = rel.TargetType.Name,
                                    TargetField = $"{rel.TargetListField}[].{rel.TargetField}"
                                });
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

        private static Dictionary<(Type, string, string), HashSet<int>> BuildLookups(
            Dictionary<Type, object> loaded)
        {
            var result = new Dictionary<(Type, string, string), HashSet<int>>();

            foreach (var (type, obj) in loaded)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (!field.FieldType.IsGenericType ||
                        field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
                        continue;

                    var list = field.GetValue(obj) as IList;
                    if (list == null || list.Count == 0) continue;

                    var elementType = field.FieldType.GetGenericArguments()[0];
                    var candidates = new[] { "ID", "Level", "SpawnGroupID" };

                    foreach (var candidate in candidates)
                    {
                        var idField = elementType.GetField(candidate,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (idField == null || idField.FieldType != typeof(int)) continue;

                        var key = (type, field.Name, candidate);
                        if (!result.ContainsKey(key))
                            result[key] = new HashSet<int>();

                        foreach (var item in list)
                        {
                            var val = (int)idField.GetValue(item);
                            result[key].Add(val);
                        }

                        break;
                    }
                }
            }

            return result;
        }

        private static HashSet<int> BuildSetFromList(IList list, string fieldName)
        {
            var set = new HashSet<int>();
            if (list == null || list.Count == 0) return set;

            var elementType = list[0].GetType();
            var field = elementType.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(int)) return set;

            foreach (var item in list)
            {
                var val = (int)field.GetValue(item);
                set.Add(val);
            }

            return set;
        }

        private static IList GetListField(object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(obj) as IList;
        }

        private static int GetIntField(object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) return -1;
            return (int)field.GetValue(obj);
        }

        private static int[] GetIntArrayField(object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(obj) as int[];
        }

        private static List<Relation> DefineRelations(List<Type> allTypes)
        {
            var monster = allTypes.FirstOrDefault(t => t.Name == "MonsterConfig");
            var monsterClass = allTypes.FirstOrDefault(t => t.Name == "MonsterClassConfig");
            var monsterLevel = allTypes.FirstOrDefault(t => t.Name == "MonsterLevelConfig");
            var skill = allTypes.FirstOrDefault(t => t.Name == "SkillConfig");
            var equipment = allTypes.FirstOrDefault(t => t.Name == "EquipmentConfig");
            var level = allTypes.FirstOrDefault(t => t.Name == "LevelConfig");
            var spawn = allTypes.FirstOrDefault(t => t.Name == "SpawnConfig");

            var list = new List<Relation>();

            if (monster != null && monsterClass != null)
                list.Add(new Relation(monster, "monsters", "ClassID",
                    monsterClass, "classes", "ID"));

            if (monster != null && skill != null)
            {
                list.Add(new Relation(monster, "monsters", "ActiveSkill",
                    skill, "Overview", "ID"));
                list.Add(new Relation(monster, "monsters", "PassiveSkill",
                    skill, "Overview", "ID"));
            }

            if (equipment != null && skill != null)
            {
                list.Add(new Relation(equipment, "Overview", "ActiveSkillID",
                    skill, "Overview", "ID"));
                list.Add(new Relation(equipment, "Overview", "PassiveSkillID",
                    skill, "Overview", "ID"));
            }

            if (level != null && spawn != null)
                list.Add(new Relation(level, "levels", "SpawnGroupID",
                    spawn, "spawns", "SpawnGroupID"));

            if (level != null && equipment != null)
                list.Add(new Relation(level, "levels", "EquipmentsPool",
                    equipment, "Overview", "ID") { IsArray = true });

            if (spawn != null && monster != null)
                list.Add(new Relation(spawn, "spawns", "MonsterID",
                    monster, "monsters", "ID"));

            if (spawn != null && monsterLevel != null)
                list.Add(new Relation(spawn, "spawns", "MonsterLevel",
                    monsterLevel, "levels", "Level"));

            return list;
        }

        public class ValidationError
        {
            public string SourceType;
            public string SourceField;
            public int MissingValue;
            public int RowIndex;
            public string TargetType;
            public string TargetField;
        }

        public class Relation
        {
            public Type SourceType;
            public string SourceListField;
            public string SourceField;
            public Type TargetType;
            public string TargetListField;
            public string TargetField;
            public bool IsArray;

            public Relation(Type sourceType, string sourceList, string sourceField,
                Type targetType, string targetList, string targetField)
            {
                SourceType = sourceType;
                SourceListField = sourceList;
                SourceField = sourceField;
                TargetType = targetType;
                TargetListField = targetList;
                TargetField = targetField;
            }
        }
    }
}
