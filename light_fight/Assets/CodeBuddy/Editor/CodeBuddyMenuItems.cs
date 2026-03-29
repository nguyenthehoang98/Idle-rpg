// Decompiled with JetBrains decompiler
// Type: CodeBuddy.CodeBuddyMenuItems
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public static class CodeBuddyMenuItems
  {
    [MenuItem("Window/Code Buddy/Code with Buddy", false, 10000)]
    public static void ShowWindow()
    {
      EditorWindow.GetWindow<CodeBuddyGenerateWindow>("Code with Buddy");
    }

    [MenuItem("Assets/Edit Script with Buddy", true)]
    public static bool CanShowEditWindow() => Selection.activeObject is MonoScript;

    [MenuItem("Assets/Edit Script with Buddy")]
    public static void ShowEditWindow()
    {
      CodeBuddyGenerateWindow window = EditorWindow.GetWindow<CodeBuddyGenerateWindow>("Code with Buddy");
      window.NewChatButton_clicked();
      window.SetScriptToEdit(Selection.activeObject as MonoScript);
    }

    [MenuItem("CONTEXT/MonoBehaviour/Edit Script with Buddy")]
    public static void ContextShowEditWindow(MenuCommand context)
    {
      CodeBuddyGenerateWindow window = EditorWindow.GetWindow<CodeBuddyGenerateWindow>("Code with Buddy");
      window.NewChatButton_clicked();
      window.SetScriptToEdit(MonoScript.FromMonoBehaviour(context.context as MonoBehaviour));
    }
  }
}
