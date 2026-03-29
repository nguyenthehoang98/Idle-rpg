// Decompiled with JetBrains decompiler
// Type: CodeBuddy.GetAssetsListTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEditor;

#nullable enable
namespace CodeBuddy
{
  public class GetAssetsListTool : ToolBase<GetAssetsListTool.GetAssetsListParameters>
  {
    public GetAssetsListTool()
      : base("GetAssetsList", "Retrieves a list of assets in the project based on a search string or type.")
    {
    }

    public override void Execute(
      string toolCallId,
      GetAssetsListTool.GetAssetsListParameters parameters)
    {
      StringBuilder stringBuilder = new StringBuilder();
      List<string> excludedFolders = ScriptableSingleton<Settings>.instance.ExcludedFolders;
      foreach (string asset in AssetDatabase.FindAssets(parameters.SearchString + " t:" + parameters.SearchType))
      {
        string assetPath = AssetDatabase.GUIDToAssetPath(asset);
        if (!excludedFolders.Any<string>((Func<string, bool>) (excludedFolder => assetPath.StartsWith(excludedFolder))))
          stringBuilder.AppendLine(assetPath);
      }
      if (stringBuilder.Length == 0)
        stringBuilder.AppendLine("No assets found matching the criteria.");
      this.InvokeExecutionFinished(toolCallId, stringBuilder.ToString());
    }

    public class GetAssetsListParameters : ToolParameters
    {
      [JsonProperty("searchString", Required = Required.Always)]
      [Description("The search string to filter assets.")]
      public string SearchString;
      [JsonProperty("searchType", Required = Required.Always)]
      [Description("The type of assets to filter (e.g., 'Texture', 'Material').")]
      public string SearchType;
    }
  }
}
