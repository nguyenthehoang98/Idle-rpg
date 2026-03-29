// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ProjectInfoGenerator
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal class ProjectInfoGenerator : ScriptableSingleton<ProjectInfoGenerator>
  {
    private bool isCacheInitialized = false;
    [SerializeField]
    private Dictionary<string, string> scriptFileCache = new Dictionary<string, string>();
    [SerializeField]
    private Dictionary<string, string> classToFilePathCache = new Dictionary<string, string>();
    [SerializeField]
    private List<string> asmdefFileCache = new List<string>();
    private ConcurrentDictionary<string, DateTime> fileLastWriteTime = new ConcurrentDictionary<string, DateTime>();
    private ConcurrentDictionary<string, Dictionary<string, string>> fileCommentsCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
    private ConcurrentDictionary<string, List<System.Type>> fileTypesCache = new ConcurrentDictionary<string, List<System.Type>>();

    private void OnEnable()
    {
      this.isCacheInitialized = false;
      this.InitializeCache();
    }

    public void UpdateCacheForImportedAssets(string[] importedAssets)
    {
      foreach (string importedAsset in importedAssets)
      {
        if (!this.IsPathExcluded(importedAsset) && importedAsset.EndsWith(".cs"))
          this.AddOrUpdateCacheEntry(importedAsset);
      }
    }

    public void UpdateCacheForDeletedAssets(string[] deletedAssets)
    {
      foreach (string deletedAsset in deletedAssets)
      {
        if (this.scriptFileCache.ContainsValue(deletedAsset))
          this.RemoveCacheEntry(deletedAsset);
      }
    }

    public void UpdateCacheForMovedAssets(string[] movedAssets, string[] movedFromAssetPaths)
    {
      for (int index = 0; index < movedAssets.Length; ++index)
      {
        string movedAsset = movedAssets[index];
        string movedFromAssetPath = movedFromAssetPaths[index];
        if (this.scriptFileCache.ContainsValue(movedFromAssetPath))
        {
          this.RemoveCacheEntry(movedFromAssetPath);
          this.AddOrUpdateCacheEntry(movedAsset);
        }
      }
    }

    private void EnsureCacheInitialized()
    {
      if (this.isCacheInitialized)
        return;
      this.InitializeCache();
    }

    private void AddOrUpdateCacheEntry(string assetPath)
    {
      this.scriptFileCache[Path.GetFileName(assetPath)] = assetPath;
      foreach (MemberInfo type in this.ExtractTypes(assetPath))
        this.classToFilePathCache[type.Name] = assetPath;
    }

    private void RemoveCacheEntry(string assetPath)
    {
      this.scriptFileCache.Remove(Path.GetFileName(assetPath));
      foreach (MemberInfo type in this.ExtractTypes(assetPath))
        this.classToFilePathCache.Remove(type.Name);
    }

    public string GetClassesWithComments()
    {
      this.EnsureCacheInitialized();
      using (StringWriter writer = new StringWriter())
      {
        foreach (KeyValuePair<string, string> keyValuePair in this.scriptFileCache)
        {
          string str = keyValuePair.Value;
          if (str.StartsWith("Assets") && (ScriptableSingleton<Settings>.instance.IncludeEditorFolder || !str.Contains("/Editor/")) && !this.IsPathExcluded(str))
          {
            Dictionary<string, string> comments = this.ExtractComments(str);
            foreach (System.Type type in this.ExtractTypes(str))
            {
              this.WriteTypeInfo(writer, type, comments);
              writer.WriteLine();
            }
          }
        }
        return writer.ToString();
      }
    }

    public string GetClassDescriptionsWithComments(string[] classNames)
    {
      this.EnsureCacheInitialized();
      using (StringWriter writer = new StringWriter())
      {
        foreach (KeyValuePair<string, string> keyValuePair in this.scriptFileCache)
        {
          string str = keyValuePair.Value;
          if (str.StartsWith("Assets") && (ScriptableSingleton<Settings>.instance.IncludeEditorFolder || !str.Contains("/Editor/")) && !this.IsPathExcluded(str))
          {
            Dictionary<string, string> comments = this.ExtractComments(str);
            foreach (System.Type type in this.ExtractTypes(str))
            {
              if (((IEnumerable<string>) classNames).Contains<string>(type.Name))
                this.WriteClassInfo(writer, type, comments);
            }
          }
        }
        return writer.ToString();
      }
    }

    private void WriteTypeInfo(
      StringWriter writer,
      System.Type type,
      Dictionary<string, string> commentsDictionary)
    {
      if (type == (System.Type) null)
        return;
      string str = "Class";
      if (type.IsInterface)
        str = "Interface";
      if (type.IsEnum)
        str = "Enum";
      else if (type.IsValueType)
        str = "Struct";
      writer.Write(str + ": " + type.Name);
      if (type.BaseType != (System.Type) null && type.BaseType != typeof (object))
        writer.Write(" : " + type.BaseType.Name);
      System.Type[] interfaces = type.GetInterfaces();
      for (int index = 0; index < interfaces.Length; ++index)
      {
        System.Type type1 = interfaces[index];
        if (index == 0 && (type.BaseType == (System.Type) null || type.BaseType == typeof (object)))
          writer.Write(" : " + type1.Name);
        else
          writer.Write(", " + type1.Name);
      }
      writer.Write(writer.NewLine);
      if (!string.IsNullOrEmpty(type.Namespace))
        writer.WriteLine("Namespace: " + type.Namespace);
      string commentsInDictionary = ProjectInfoGenerator.FindCommentsInDictionary(type.Name, commentsDictionary);
      if (string.IsNullOrWhiteSpace(commentsInDictionary))
        return;
      writer.WriteLine("Description: " + commentsInDictionary);
    }

    internal void InitializeCache()
    {
      this.scriptFileCache.Clear();
      this.classToFilePathCache.Clear();
      this.fileTypesCache.Clear();
      this.fileLastWriteTime.Clear();
      this.fileCommentsCache.Clear();
      foreach (string asset in AssetDatabase.FindAssets("t:Script"))
      {
        string assetPath = AssetDatabase.GUIDToAssetPath(asset);
        if (!this.IsPathExcluded(assetPath) && assetPath.StartsWith("Assets"))
          this.AddOrUpdateCacheEntry(assetPath);
      }
      this.asmdefFileCache.Clear();
      string[] searchInFolders = new string[1]{ "Assets" };
      foreach (string asset in AssetDatabase.FindAssets("t:asmdef", searchInFolders))
      {
        string assemblyName = this.ExtractAssemblyName(AssetDatabase.GUIDToAssetPath(asset));
        if (!string.IsNullOrEmpty(assemblyName) && !this.scriptFileCache.ContainsKey(assemblyName))
          this.asmdefFileCache.Add(assemblyName);
      }
      this.asmdefFileCache.Add("Assembly-CSharp");
      if (ScriptableSingleton<Settings>.instance.IncludeEditorFolder)
        this.asmdefFileCache.Add("Assembly-CSharp-Editor");
      this.isCacheInitialized = true;
    }

    public bool TryGetCachedPath(string fileName, out string cachedPath)
    {
      if (this.scriptFileCache.ContainsKey(fileName))
      {
        cachedPath = this.scriptFileCache[fileName];
        return true;
      }
      cachedPath = (string) null;
      return false;
    }

    public string GetFilePathForClass(string className)
    {
      if (!this.classToFilePathCache.ContainsKey(className))
        this.InitializeCache();
      return this.classToFilePathCache.ContainsKey(className) ? this.classToFilePathCache[className] : (string) null;
    }

    private bool IsPathExcluded(string assetPath)
    {
      if (ScriptableSingleton<Settings>.instance.UseWhitelist)
      {
        foreach (string includedFolder in ScriptableSingleton<Settings>.instance.IncludedFolders)
        {
          if (!string.IsNullOrEmpty(includedFolder) && assetPath.StartsWith(includedFolder, StringComparison.OrdinalIgnoreCase))
            return false;
        }
        return true;
      }
      foreach (string excludedFolder in ScriptableSingleton<Settings>.instance.ExcludedFolders)
      {
        if (!string.IsNullOrEmpty(excludedFolder) && assetPath.StartsWith(excludedFolder, StringComparison.OrdinalIgnoreCase))
          return true;
      }
      return false;
    }

    private void WriteClassInfo(
      StringWriter writer,
      System.Type type,
      Dictionary<string, string> commentsDictionary)
    {
      if (type == (System.Type) null)
        return;
      this.WriteTypeInfo(writer, type, commentsDictionary);
      FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
      PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
      IEnumerable<MethodInfo> source = ((IEnumerable<MethodInfo>) type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => !m.IsSpecialName));
      string empty = string.Empty;
      if (((IEnumerable<FieldInfo>) fields).Any<FieldInfo>())
      {
        writer.WriteLine("Fields:");
        foreach (FieldInfo fieldInfo in fields)
        {
          string commentsInDictionary = ProjectInfoGenerator.FindCommentsInDictionary(fieldInfo.Name, commentsDictionary);
          string str = fieldInfo.IsStatic ? "- static" : "-";
          if (!string.IsNullOrWhiteSpace(commentsInDictionary))
            writer.WriteLine(str + " " + fieldInfo.Name + " : " + ProjectInfoGenerator.GetFriendlyTypeName(fieldInfo.FieldType) + " - " + commentsInDictionary);
          else
            writer.WriteLine(str + " " + fieldInfo.Name + " : " + ProjectInfoGenerator.GetFriendlyTypeName(fieldInfo.FieldType));
        }
      }
      if (((IEnumerable<PropertyInfo>) properties).Any<PropertyInfo>())
      {
        writer.WriteLine("Properties:");
        foreach (PropertyInfo property in properties)
        {
          string str = ProjectInfoGenerator.isItStatic(property) ? "- static" : "-";
          string commentsInDictionary = ProjectInfoGenerator.FindCommentsInDictionary(property.Name, commentsDictionary);
          if (!string.IsNullOrWhiteSpace(commentsInDictionary))
            writer.WriteLine(str + " " + property.Name + " : " + ProjectInfoGenerator.GetFriendlyTypeName(property.PropertyType) + " - " + commentsInDictionary);
          else
            writer.WriteLine(str + " " + property.Name + " : " + ProjectInfoGenerator.GetFriendlyTypeName(property.PropertyType));
        }
      }
      if (source.Any<MethodInfo>())
      {
        writer.WriteLine("Methods:");
        foreach (MethodInfo methodInfo in source)
        {
          string str1 = string.Join(", ", ((IEnumerable<System.Reflection.ParameterInfo>) methodInfo.GetParameters()).Select<System.Reflection.ParameterInfo, string>((Func<System.Reflection.ParameterInfo, string>) (p => ProjectInfoGenerator.GetFriendlyTypeName(p.ParameterType) + " " + p.Name)));
          string friendlyTypeName = ProjectInfoGenerator.GetFriendlyTypeName(methodInfo.ReturnType);
          string str2 = methodInfo.IsStatic ? "- static" : "-";
          string commentsInDictionary = ProjectInfoGenerator.FindCommentsInDictionary(methodInfo.Name, commentsDictionary);
          if (!string.IsNullOrWhiteSpace(commentsInDictionary))
            writer.WriteLine(str2 + " " + methodInfo.Name + "(" + str1 + ") : " + friendlyTypeName + " - " + commentsInDictionary);
          else
            writer.WriteLine(str2 + " " + methodInfo.Name + "(" + str1 + ") : " + friendlyTypeName);
        }
      }
      writer.WriteLine();
    }

    private string ExtractAssemblyName(string filePath)
    {
      return !filePath.EndsWith(".asmdef") ? (string) null : JsonConvert.DeserializeObject<ProjectInfoGenerator.AsmdefJson>(File.ReadAllText(filePath)).name;
    }

    private static bool isItStatic(PropertyInfo property)
    {
      MethodInfo getMethod = property.GetGetMethod(true);
      int num;
      // ISSUE: explicit non-virtual call
      if (((object) getMethod != null ? ( (getMethod.IsStatic) ? 1 : 0) : 0) == 0)
      {
        MethodInfo setMethod = property.GetSetMethod(true);
        // ISSUE: explicit non-virtual call
        num = (object) setMethod != null ? ( (setMethod.IsStatic) ? 1 : 0) : 0;
      }
      else
        num = 1;
      return num != 0;
    }

    private static string FindCommentsInDictionary(
      string name,
      Dictionary<string, string> commentsDictionary)
    {
      KeyValuePair<string, string> keyValuePair = commentsDictionary.Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (kvp => kvp.Key.Contains(" " + name))).FirstOrDefault<KeyValuePair<string, string>>();
      if (keyValuePair.Key == null)
        return (string) null;
      int num1 = keyValuePair.Key.IndexOf(" " + name);
      int num2 = name.Length + 1;
      return num1 + num2 < keyValuePair.Key.Length && char.IsLetterOrDigit(keyValuePair.Key[num1 + num2]) ? "" : keyValuePair.Value;
    }

    private Dictionary<string, string> ExtractComments(string filePath)
    {
      DateTime lastWriteTime = File.GetLastWriteTime(filePath);
      if (this.fileLastWriteTime.ContainsKey(filePath) && this.fileLastWriteTime[filePath] == lastWriteTime && this.fileCommentsCache[filePath] != null && this.fileCommentsCache[filePath].Count > 0)
        return this.fileCommentsCache[filePath];
      this.UpdateTypesAndCommentsCache(filePath);
      return this.fileCommentsCache[filePath];
    }

    private List<System.Type> ExtractTypes(string filePath)
    {
      DateTime lastWriteTime = File.GetLastWriteTime(filePath);
      if (this.fileLastWriteTime.ContainsKey(filePath) && this.fileLastWriteTime[filePath] == lastWriteTime && this.fileTypesCache[filePath] != null && this.fileTypesCache[filePath].Count > 0)
        return this.fileTypesCache[filePath];
      this.UpdateTypesAndCommentsCache(filePath);
      return this.fileTypesCache[filePath];
    }

    private void UpdateTypesAndCommentsCache(string filePath)
    {
      this.fileLastWriteTime[filePath] = File.GetLastWriteTime(filePath);
      string[] strArray = File.ReadAllLines(filePath);
      List<System.Type> typeList = new List<System.Type>();
      string str1 = "";
      Regex regex1 = new Regex("\\b(class|struct|interface)\\s+(\\w+)");
      Regex regex2 = new Regex("\\bnamespace\\s+(\\w+(\\.\\w+)*)");
      foreach (string str2 in strArray)
      {
        string input = str2.Trim();
        Match match1 = regex2.Match(input);
        if (match1.Success)
          str1 = match1.Groups[1].Value;
        Match match2 = regex1.Match(input);
        if (match2.Success)
        {
          string str3 = match2.Groups[1].Value;
          string str4 = match2.Groups[2].Value;
          foreach (string str5 in this.asmdefFileCache)
          {
            System.Type type1;
            if (!string.IsNullOrEmpty(str1))
              type1 = System.Type.GetType(str1 + "." + str4 + "," + str5);
            else
              type1 = System.Type.GetType(str4 + "," + str5);
            System.Type type2 = type1;
            if (type2 != (System.Type) null)
            {
              typeList.Add(type2);
              break;
            }
          }
        }
      }
      this.fileTypesCache[filePath] = typeList;
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      Regex regex3 = new Regex("<.*?>");
      bool flag = false;
      int num1 = 0;
      int num2 = 0;
      string str6 = "";
      string str7 = "";
      for (int index = 0; index < strArray.Length; ++index)
      {
        string line = strArray[index].Trim();
        if (line.Contains("{"))
          ++num1;
        if (line.Contains("}"))
          --num1;
        if (ProjectInfoGenerator.IsTypeOrNamespaceDeclaration(line))
          num2 = num1;
        if ((!ProjectInfoGenerator.IsMethodDeclaration(line) || num1 != num2 + 1) && num1 <= num2 + 1)
        {
          if (line.StartsWith("///"))
          {
            flag = true;
            string input = strArray[index].Replace("///", "").Trim();
            string str8 = regex3.Replace(input, "");
            if (!string.IsNullOrWhiteSpace(str8))
              str6 = str6 + str8 + " ";
          }
          else if (line.StartsWith("//"))
          {
            flag = true;
            string str9 = strArray[index].Replace("//", "").Trim();
            if (!string.IsNullOrWhiteSpace(str9))
              str6 = str6 + str9 + " ";
          }
          else if (flag)
          {
            if (!string.IsNullOrWhiteSpace(line))
            {
              string key = line;
              if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(str6))
                dictionary[key] = str6;
              str6 = "";
              str7 = "";
            }
            flag = false;
          }
        }
      }
      this.fileCommentsCache[filePath] = dictionary;
    }

    private static bool IsTypeOrNamespaceDeclaration(string line)
    {
      return ((IEnumerable<string>) new string[5]
      {
        "class",
        "struct",
        "enum",
        "interface",
        "namespace"
      }).Any<string>((Func<string, bool>) (keyword => line.Contains(keyword)));
    }

    private static bool IsMethodDeclaration(string line)
    {
      return line.Contains("(") && line.Contains(")") && !line.TrimEnd().EndsWith(";");
    }

    private static string GetFriendlyTypeName(System.Type type)
    {
      if (type == typeof (int))
        return "int";
      if (type == typeof (float))
        return "float";
      if (type == typeof (bool))
        return "bool";
      if (type == typeof (string))
        return "string";
      if (type == typeof (double))
        return "double";
      return type == typeof (void) ? "void" : type.Name;
    }

    [Serializable]
    private class AsmdefJson
    {
      public string name;
    }
  }
}
