#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace _KITSystem.SkillSystem.Entity
{
    public sealed class EntityComponentDataCheckerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Skill System/Entity Data Checker";
        private const double AutoRefreshSeconds = 0.5d;

        private readonly List<ComponentSnapshot> componentSnapshots = new();
        private readonly List<Issue> issues = new();
        private readonly List<int> aliveEntityIds = new();
        private readonly HashSet<int> entitiesWithComponents = new();
        private readonly Dictionary<string, bool> foldouts = new();

        private Vector2 scroll;
        private string search = string.Empty;
        private bool autoRefresh = true;
        private bool showOnlyIssues;
        private double lastRefreshTime;
        private int activeCount;
        private int totalComponentRows;
        private int entitiesWithoutComponents;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            GetWindow<EntityComponentDataCheckerWindow>("Entity Data Checker");
        }

        private void OnEnable()
        {
            RefreshData();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Update()
        {
            if (!autoRefresh)
                return;

            double now = EditorApplication.timeSinceStartup;
            if (now - lastRefreshTime < AutoRefreshSeconds)
                return;

            RefreshData();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "This checker reads the in-memory EntityManager and ComponentManager<T> data. Open Play Mode to inspect live runtime data.",
                    MessageType.Info);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSummary();
            DrawIssues();
            DrawEntitySnapshots();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                RefreshData();

            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto", EditorStyles.toolbarButton, GUILayout.Width(48));
            showOnlyIssues = GUILayout.Toggle(showOnlyIssues, "Only Issues", EditorStyles.toolbarButton, GUILayout.Width(85));

            GUILayout.Space(8);
            GUILayout.Label("Search", GUILayout.Width(45));
            search = GUILayout.TextField(search, EditorStyles.toolbarTextField, GUILayout.MinWidth(120));

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48)))
            {
                search = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Copy Report", EditorStyles.toolbarButton, GUILayout.Width(90)))
                EditorGUIUtility.systemCopyBuffer = BuildReport();

            GUI.enabled = activeCount > 0 || totalComponentRows > 0;
            if (GUILayout.Button("Clear ECS", EditorStyles.toolbarButton, GUILayout.Width(72))
                && EditorUtility.DisplayDialog(
                    "Clear Entity Data",
                    "This will call EntityManager.Clear() and remove all runtime entities/components. Continue?",
                    "Clear",
                    "Cancel"))
            {
                EntityManager.Clear();
                RefreshData();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSummary()
        {
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Active Count", $"{activeCount} (scanned alive ids: {aliveEntityIds.Count})");
            EditorGUILayout.LabelField("Registered Component Types", componentSnapshots.Count.ToString());
            EditorGUILayout.LabelField("Total Component Rows", totalComponentRows.ToString());
            EditorGUILayout.LabelField("Alive Entities Without Components", entitiesWithoutComponents.ToString());
            EditorGUILayout.LabelField("Issues", issues.Count.ToString());
            EditorGUILayout.EndVertical();
        }

        private void DrawIssues()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Issues", EditorStyles.boldLabel);

            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("No entity/component consistency issues found.", MessageType.None);
                return;
            }

            foreach (Issue issue in issues)
            {
                EditorGUILayout.HelpBox(issue.Message, issue.Type);
            }
        }

        private void DrawEntitySnapshots()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Entity Data", EditorStyles.boldLabel);

            List<EntitySnapshot> entitySnapshots = BuildEntitySnapshots();
            if (entitySnapshots.Count == 0)
            {
                EditorGUILayout.HelpBox("No live entities or registered component rows yet. ComponentRegistry is filled when ComponentManager<T>.Add() is called.", MessageType.Info);
                return;
            }

            foreach (EntitySnapshot entity in entitySnapshots)
            {
                if (!ShouldDrawEntity(entity))
                    continue;

                string key = $"entity:{entity.EntityId}";
                if (!foldouts.ContainsKey(key))
                    foldouts[key] = true;

                Color previousColor = GUI.color;
                if (entity.HasIssue)
                    GUI.color = new Color(1f, 0.85f, 0.35f);

                string title = $"Entity {entity.EntityId}  Alive: {entity.IsAlive}  Components: {entity.Components.Count}";
                foldouts[key] = EditorGUILayout.Foldout(foldouts[key], title, true);
                GUI.color = previousColor;

                if (!foldouts[key])
                    continue;

                EditorGUILayout.BeginVertical(GUI.skin.box);

                if (!entity.IsAlive)
                    EditorGUILayout.HelpBox("Entity is dead/missing but still has component rows.", MessageType.Warning);

                if (entity.Components.Count == 0)
                    EditorGUILayout.LabelField("No components.");

                foreach (EntityComponentSnapshot component in entity.Components)
                {
                    if (showOnlyIssues && !component.HasIssue)
                        continue;

                    DrawEntityComponent(component);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawEntityComponent(EntityComponentSnapshot component)
        {
            Color previousColor = GUI.color;
            if (component.HasIssue)
                GUI.color = new Color(1f, 0.85f, 0.35f);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField(GetTypeName(component.ComponentType), EditorStyles.boldLabel);
            GUI.color = previousColor;

            if (!string.IsNullOrEmpty(component.Entry.Error))
                EditorGUILayout.HelpBox(component.Entry.Error, MessageType.Warning);

            EditorGUILayout.LabelField(component.Entry.ValueText, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
        }

        private List<EntitySnapshot> BuildEntitySnapshots()
        {
            Dictionary<int, EntitySnapshot> entities = new();

            foreach (int entityId in aliveEntityIds)
            {
                if (!entities.ContainsKey(entityId))
                    entities.Add(entityId, new EntitySnapshot(entityId, true));
            }

            foreach (ComponentSnapshot snapshot in componentSnapshots)
            {
                foreach (ComponentEntry entry in snapshot.Entries)
                {
                    if (!entities.TryGetValue(entry.EntityId, out EntitySnapshot entity))
                    {
                        entity = new EntitySnapshot(entry.EntityId, entry.IsAlive);
                        entities.Add(entry.EntityId, entity);
                    }

                    entity.Components.Add(new EntityComponentSnapshot(snapshot.ComponentType, entry));
                }
            }

            return entities.Values
                .OrderBy(entity => entity.EntityId)
                .ToList();
        }

        private void RefreshData()
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            componentSnapshots.Clear();
            issues.Clear();
            aliveEntityIds.Clear();
            entitiesWithComponents.Clear();
            totalComponentRows = 0;
            entitiesWithoutComponents = 0;

            activeCount = EntityManager.ActiveCount;
            ReadAliveEntityIds();

            if (aliveEntityIds.Count != activeCount)
            {
                issues.Add(new Issue(
                    $"EntityManager.ActiveCount is {activeCount}, but scanned alive entity ids is {aliveEntityIds.Count}.",
                    MessageType.Warning));
            }

            Type[] componentTypes = ComponentRegistry.Pools.Keys
                .OrderBy(type => type.FullName)
                .ToArray();

            foreach (Type componentType in componentTypes)
            {
                ComponentSnapshot snapshot = ReadComponentSnapshot(componentType);
                componentSnapshots.Add(snapshot);
                totalComponentRows += snapshot.Entries.Count;

                foreach (ComponentEntry entry in snapshot.Entries)
                    entitiesWithComponents.Add(entry.EntityId);
            }

            entitiesWithoutComponents = aliveEntityIds.Count(id => !entitiesWithComponents.Contains(id));
        }

        private void ReadAliveEntityIds()
        {
            FieldInfo versionsField = typeof(EntityManager).GetField("entityVersions", BindingFlags.Static | BindingFlags.NonPublic);
            if (versionsField?.GetValue(null) is not int[] versions)
            {
                issues.Add(new Issue("Could not read EntityManager entityVersions field.", MessageType.Warning));
                return;
            }

            for (int entityId = 1; entityId < versions.Length; entityId++)
            {
                if (versions[entityId] >= 0)
                    aliveEntityIds.Add(entityId);
            }
        }

        private ComponentSnapshot ReadComponentSnapshot(Type componentType)
        {
            ComponentSnapshot snapshot = new(componentType);

            try
            {
                Type managerType = typeof(ComponentManager<>).MakeGenericType(componentType);
                PropertyInfo countProperty = managerType.GetProperty("Count", BindingFlags.Static | BindingFlags.Public);
                PropertyInfo entityIdsProperty = managerType.GetProperty("EntityIds", BindingFlags.Static | BindingFlags.Public);

                snapshot.Count = countProperty?.GetValue(null) is int count ? count : 0;

                if (entityIdsProperty?.GetValue(null) is not IEnumerable entityIds)
                {
                    AddSnapshotIssue(snapshot, $"Could not read EntityIds for {GetTypeName(componentType)}.");
                    return snapshot;
                }

                HashSet<int> seenEntityIds = new();
                foreach (object entityIdObject in entityIds)
                {
                    int entityId = Convert.ToInt32(entityIdObject);
                    ComponentEntry entry = ReadComponentEntry(managerType, componentType, entityId);
                    snapshot.Entries.Add(entry);

                    if (!string.IsNullOrEmpty(entry.Error))
                        AddSnapshotIssue(snapshot, $"{GetTypeName(componentType)} entity {entityId}: {entry.Error}");

                    if (!seenEntityIds.Add(entityId))
                    {
                        entry.Error = AppendError(entry.Error, "Duplicate component row for this entity.");
                        AddSnapshotIssue(snapshot, $"{GetTypeName(componentType)} has duplicate row for entity {entityId}. Check if ComponentManager<{componentType.Name}>.Add() was called twice without Remove().");
                    }

                    if (entityId <= 0)
                    {
                        entry.Error = AppendError(entry.Error, $"Invalid entity id {entityId}.");
                        AddSnapshotIssue(snapshot, $"{GetTypeName(componentType)} has invalid entity id {entityId}.");
                    }

                    if (!entry.IsAlive)
                    {
                        entry.Error = AppendError(entry.Error, "Component exists on dead/missing entity.");
                        AddSnapshotIssue(snapshot, $"{GetTypeName(componentType)} exists on dead/missing entity {entityId}.");
                    }
                }

                if (snapshot.Count != snapshot.Entries.Count)
                {
                    AddSnapshotIssue(snapshot, $"{GetTypeName(componentType)} Count is {snapshot.Count}, but EntityIds returned {snapshot.Entries.Count} rows.");
                }
            }
            catch (Exception exception)
            {
                AddSnapshotIssue(snapshot, $"Failed to inspect {GetTypeName(componentType)}: {Unwrap(exception).Message}");
            }

            return snapshot;
        }

        private ComponentEntry ReadComponentEntry(Type managerType, Type componentType, int entityId)
        {
            ComponentEntry entry = new(entityId, EntityManager.IsAlive(entityId));

            try
            {
                MethodInfo tryGetMethod = managerType.GetMethod("TryGet", BindingFlags.Static | BindingFlags.Public);
                if (tryGetMethod == null)
                {
                    entry.Error = "Could not find TryGet method.";
                    entry.ValueText = "No value.";
                    return entry;
                }

                object[] args = { entityId, Activator.CreateInstance(componentType) };
                bool hasComponent = tryGetMethod.Invoke(null, args) is true;
                if (!hasComponent)
                {
                    entry.Error = "EntityIds contains this entity, but TryGet returned false.";
                    entry.ValueText = "No value.";
                    return entry;
                }

                entry.Value = args[1];
                entry.ValueText = FormatComponentValue(entry.Value, componentType);
            }
            catch (Exception exception)
            {
                entry.Error = Unwrap(exception).Message;
                entry.ValueText = "Failed to read value.";
            }

            return entry;
        }

        private void AddSnapshotIssue(ComponentSnapshot snapshot, string message)
        {
            snapshot.HasIssue = true;
            issues.Add(new Issue(message, MessageType.Warning));
        }

        private bool ShouldDrawEntity(EntitySnapshot entity)
        {
            if (showOnlyIssues && !entity.HasIssue)
                return false;

            if (string.IsNullOrWhiteSpace(search))
                return true;

            string searchText = search.Trim();
            if (entity.EntityId.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return entity.Components.Any(component => EntityComponentMatchesSearch(component, searchText));
        }

        private bool EntityComponentMatchesSearch(EntityComponentSnapshot component, string searchText)
        {
            if (GetTypeName(component.ComponentType).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return component.Entry.ValueText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string BuildReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("Entity Component Data Checker");
            builder.AppendLine($"Active Count: {activeCount}");
            builder.AppendLine($"Scanned Alive Ids: {aliveEntityIds.Count}");
            builder.AppendLine($"Registered Component Types: {componentSnapshots.Count}");
            builder.AppendLine($"Total Component Rows: {totalComponentRows}");
            builder.AppendLine($"Alive Entities Without Components: {entitiesWithoutComponents}");
            builder.AppendLine($"Issues: {issues.Count}");

            if (issues.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Issues");
                foreach (Issue issue in issues)
                    builder.AppendLine($"- {issue.Message}");
            }

            builder.AppendLine();
            builder.AppendLine("Entity Data");

            foreach (EntitySnapshot entity in BuildEntitySnapshots())
            {
                builder.AppendLine($"Entity {entity.EntityId} Alive: {entity.IsAlive} Components: {entity.Components.Count}");
                foreach (EntityComponentSnapshot component in entity.Components)
                    builder.AppendLine($"  {GetTypeName(component.ComponentType)}: {component.Entry.ValueText}");
            }

            return builder.ToString();
        }

        private static string AppendError(string current, string message)
        {
            return string.IsNullOrEmpty(current) ? message : $"{current} {message}";
        }

        private static string FormatComponentValue(object value, Type componentType)
        {
            if (value == null)
                return "null";

            FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .ToArray();

            if (fields.Length == 0 && properties.Length == 0)
                return value.ToString();

            StringBuilder builder = new();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (i > 0)
                    builder.Append(", ");

                builder.Append(field.Name);
                builder.Append(": ");
                builder.Append(FormatScalar(field.GetValue(value)));
            }

            foreach (PropertyInfo property in properties)
            {
                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(property.Name);
                builder.Append(": ");
                builder.Append(FormatPropertyValue(property, value));
            }

            return builder.ToString();
        }

        private static string FormatPropertyValue(PropertyInfo property, object value)
        {
            try
            {
                return FormatScalar(property.GetValue(value));
            }
            catch (Exception exception)
            {
                return $"<error: {Unwrap(exception).Message}>";
            }
        }

        private static string FormatScalar(object value)
        {
            if (value == null)
                return "null";

            return value is string text ? $"\"{text}\"" : value.ToString();
        }

        private static string GetTypeName(Type type)
        {
            return type.Name;
        }

        private static Exception Unwrap(Exception exception)
        {
            return exception is TargetInvocationException { InnerException: not null } ? exception.InnerException : exception;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            RefreshData();
            Repaint();
        }

        private sealed class ComponentSnapshot
        {
            public ComponentSnapshot(Type componentType)
            {
                ComponentType = componentType;
            }

            public Type ComponentType { get; }
            public int Count { get; set; }
            public bool HasIssue { get; set; }
            public List<ComponentEntry> Entries { get; } = new();
        }

        private sealed class ComponentEntry
        {
            public ComponentEntry(int entityId, bool isAlive)
            {
                EntityId = entityId;
                IsAlive = isAlive;
            }

            public int EntityId { get; }
            public bool IsAlive { get; }
            public bool HasIssue => !IsAlive || !string.IsNullOrEmpty(Error);
            public object Value { get; set; }
            public string ValueText { get; set; } = string.Empty;
            public string Error { get; set; }
        }

        private sealed class EntitySnapshot
        {
            public EntitySnapshot(int entityId, bool isAlive)
            {
                EntityId = entityId;
                IsAlive = isAlive;
            }

            public int EntityId { get; }
            public bool IsAlive { get; }
            public bool HasIssue => !IsAlive || Components.Any(component => component.HasIssue);
            public List<EntityComponentSnapshot> Components { get; } = new();
        }

        private sealed class EntityComponentSnapshot
        {
            public EntityComponentSnapshot(Type componentType, ComponentEntry entry)
            {
                ComponentType = componentType;
                Entry = entry;
            }

            public Type ComponentType { get; }
            public ComponentEntry Entry { get; }
            public bool HasIssue => Entry.HasIssue;
        }

        private readonly struct Issue
        {
            public Issue(string message, MessageType type)
            {
                Message = message;
                Type = type;
            }

            public string Message { get; }
            public MessageType Type { get; }
        }
    }
}
#endif
