// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetSceneHierarchyTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable
namespace CodeBuddy
{
  public class GetSceneHierarchyTool : ToolBase<ToolParameters>
  {
    public GetSceneHierarchyTool()
      : base("GetSceneHierarchy", "Retrieves the hierarchy of the current scene and returns a tree-like view of the objects with names and instance IDs.")
    {
    }

    public override void Execute(string toolCallId, ToolParameters parameters)
    {
      try
      {
        string hierarchyString = this.GetHierarchyString(SceneManager.GetActiveScene().GetRootGameObjects(), 0);
        this.InvokeExecutionFinished(toolCallId, hierarchyString);
      }
      catch (Exception ex)
      {
        this.InvokeExecutionFinished(toolCallId, "[FATAL ERROR] Exception during execution : '" + ex.Message + "'");
      }
    }

    private string GetHierarchyString(GameObject[] objects, int indentLevel)
    {
      string hierarchyString = string.Empty;
      string str = new string(' ', indentLevel * 2);
      foreach (GameObject parent in objects)
        hierarchyString = hierarchyString + string.Format("{0}- {1} (ID: {2})\n", (object) str, (object) parent.name, (object) parent.GetInstanceID()) + this.GetHierarchyString(this.GetChildren(parent), indentLevel + 1);
      return hierarchyString;
    }

    private GameObject[] GetChildren(GameObject parent)
    {
      int childCount = parent.transform.childCount;
      GameObject[] children = new GameObject[childCount];
      for (int index = 0; index < childCount; ++index)
        children[index] = parent.transform.GetChild(index).gameObject;
      return children;
    }
  }
}
