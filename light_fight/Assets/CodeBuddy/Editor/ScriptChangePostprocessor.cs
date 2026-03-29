// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ScriptChangePostprocessor
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class ScriptChangePostprocessor : AssetPostprocessor
  {
    private static void OnPostprocessAllAssets(
      string[] importedAssets,
      string[] deletedAssets,
      string[] movedAssets,
      string[] movedFromAssetPaths)
    {
      ScriptableSingleton<ProjectInfoGenerator>.instance.UpdateCacheForImportedAssets(importedAssets);
      ScriptableSingleton<ProjectInfoGenerator>.instance.UpdateCacheForDeletedAssets(deletedAssets);
      ScriptableSingleton<ProjectInfoGenerator>.instance.UpdateCacheForMovedAssets(movedAssets, movedFromAssetPaths);
    }
  }
}
