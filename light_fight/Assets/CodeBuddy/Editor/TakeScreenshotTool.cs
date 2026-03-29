// Decompiled with JetBrains decompiler
// Type: CodeBuddy.TakeScreenshotTool
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class TakeScreenshotTool : ToolBase<TakeScreenshotTool.TakeScreenshotParameters>
  {
    public static string LatestScreenshot = "";

    public TakeScreenshotTool()
      : base("TakeScreenshot", "Takes a screenshot from a specified camera position and direction to provide visual context about the scene. You must use this tool when asked about visuals of the level or to validate your actions in the scene.")
    {
    }

    public override void Execute(
      string toolCallId,
      TakeScreenshotTool.TakeScreenshotParameters parameters)
    {
      string[] strArray = parameters.Resolution.Split(',', StringSplitOptions.None);
      int result1 = 1920;
      int result2 = 1080;
      if (strArray.Length == 2)
      {
        int.TryParse(strArray[0], out result1);
        int.TryParse(strArray[1], out result2);
      }
      GameObject gameObject = new GameObject("CodeBuddy_Eyes");
      Camera camera = gameObject.AddComponent<Camera>();
      camera.fieldOfView = parameters.FieldOfView;
      camera.nearClipPlane = 0.01f;
      camera.farClipPlane = 1000f;
      gameObject.transform.position = parameters.Position;
      if (parameters.LookDirection != Vector3.zero)
        gameObject.transform.rotation = Quaternion.LookRotation(parameters.LookDirection);
      RenderTexture renderTexture = new RenderTexture(result1, result2, 24);
      camera.targetTexture = renderTexture;
      Texture2D tex = new Texture2D(result1, result2, TextureFormat.RGB24, false);
      camera.Render();
      RenderTexture.active = renderTexture;
      tex.ReadPixels(new Rect(0.0f, 0.0f, (float) result1, (float) result2), 0, 0);
      camera.targetTexture = (RenderTexture) null;
      RenderTexture.active = (RenderTexture) null;
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObject);
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) renderTexture);
      TakeScreenshotTool.LatestScreenshot = Convert.ToBase64String(tex.EncodeToPNG());
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) tex);
      this.InvokeExecutionFinished(toolCallId, "Screenshot taken successfully. I will send it with the next message.");
    }

    public class TakeScreenshotParameters : ToolParameters
    {
      [JsonProperty("position", Required = Required.Always)]
      [Description("X,Y,Z position of the camera.")]
      [JsonConverter(typeof (UnityTypeConverter))]
      public Vector3 Position;
      [JsonProperty("lookDirection", Required = Required.Always)]
      [Description("X,Y,Z direction that the camera should look towards.")]
      [JsonConverter(typeof (UnityTypeConverter))]
      public Vector3 LookDirection;
      [JsonProperty("fieldOfView", Required = Required.Default)]
      [Description("Field of view of the camera in degrees. Default is 60.")]
      public float FieldOfView = 60f;
      [JsonProperty("resolution", Required = Required.Default)]
      [Description("Width,Height resolution of the screenshot. Default is 1920,1080.")]
      public string Resolution = "1920,1080";
    }
  }
}
