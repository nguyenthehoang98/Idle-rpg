// Decompiled with JetBrains decompiler
// Type: CodeBuddy.FileService
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System.IO;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal class FileService
  {
    public string SaveScript(
      string content,
      string fileName,
      string extension,
      bool askWhereToSave = false)
    {
      if (string.IsNullOrEmpty(fileName))
        fileName = "NewFile";
      string path1;
      if (askWhereToSave)
      {
        path1 = FileService.SaveFileDialog(content, fileName, extension);
      }
      else
      {
        string path2 = Application.dataPath + "/" + ScriptableSingleton<Settings>.instance.SaveFolderName;
        if (!Directory.Exists(path2))
          Directory.CreateDirectory(path2);
        path1 = path2 + "/" + fileName + "." + extension;
        if (File.Exists(path1) && !EditorUtility.DisplayDialog("File already exists", "File " + Path.Combine("Assets", ScriptableSingleton<Settings>.instance.SaveFolderName, fileName + "." + extension) + " already exists.\n\rAre you sure you want to overwrite it?", "Overwrite", "Save As New..."))
          return FileService.SaveFileDialog(content, fileName, extension);
        File.WriteAllText(path1, content);
      }
      return path1;
    }

    private static string SaveFileDialog(string content, string fileName, string extension)
    {
      string path = EditorUtility.SaveFilePanel("Save Script", "Assets", fileName + "." + extension, extension);
      if (!string.IsNullOrEmpty(path))
        File.WriteAllText(path, content);
      return path;
    }

    public string CreateScriptFile(
      string scriptContent,
      string fileName,
      string extension,
      bool forceAsk = false)
    {
      return this.SaveScript(scriptContent, fileName, extension, forceAsk);
    }

    internal void UpdateScriptFile(string codeText, string filePath)
    {
      if (!EditorUtility.DisplayDialog("Update", "Are you sure you want to overwrite " + filePath + "?", "Yes", "Cancel"))
        return;
      File.WriteAllText(Application.dataPath.Substring(0, Application.dataPath.Length - 6) + filePath, codeText);
      AssetDatabase.Refresh();
    }
  }
}
