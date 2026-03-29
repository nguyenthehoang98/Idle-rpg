// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ChatMessageEditor
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#nullable enable
namespace CodeBuddy
{
  [CustomEditor(typeof (ChatMessageComponent))]
  internal class ChatMessageEditor : Editor
  {
    public override VisualElement CreateInspectorGUI()
    {
      VisualElement inspectorGui = new VisualElement();
      PropertyField child = new PropertyField(this.serializedObject.FindProperty("_messageContent"), "Message Content");
      inspectorGui.Add((VisualElement) child);
      return inspectorGui;
    }
  }
}
