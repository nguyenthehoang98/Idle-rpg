// Decompiled with JetBrains decompiler
// Type: CodeBuddy.AttachmentHelper
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal static class AttachmentHelper
  {
    internal static async Task<string> GetAttachmentsString(List<string> attachmentGuids)
    {
      if (attachmentGuids == null || attachmentGuids.Count == 0)
        return "";
      StringBuilder sb = new StringBuilder();
      foreach (string attachmentGuid in attachmentGuids)
      {
        string attachment = attachmentGuid;
        if (!string.IsNullOrEmpty(attachment))
          await EditorMainThreadDispatcher.EnqueueAndWait((Action) (() =>
          {
            string assetPath = AssetDatabase.GUIDToAssetPath(attachment);
            if (string.IsNullOrEmpty(assetPath))
              return;
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if ((UnityEngine.Object) textAsset != (UnityEngine.Object) null)
            {
              sb.AppendLine(textAsset.text);
              sb.AppendLine();
            }
          }));
      }
      return sb.ToString();
    }

    internal static async Task<string[]> GetAttachmentsTexture(List<string> attachmentGuids)
    {
      if (attachmentGuids == null || attachmentGuids.Count == 0)
        return new string[0];
      List<string> base64Strings = new List<string>();
      foreach (string attachmentGuid in attachmentGuids)
      {
        string attachment = attachmentGuid;
        if (!string.IsNullOrEmpty(attachment))
          await EditorMainThreadDispatcher.EnqueueAndWait((Action) (() =>
          {
            string assetPath = AssetDatabase.GUIDToAssetPath(attachment);
            if (string.IsNullOrEmpty(assetPath))
              return;
            Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if ((UnityEngine.Object) originalTexture != (UnityEngine.Object) null)
            {
              base64Strings.Add(Convert.ToBase64String(AttachmentHelper.GetReadableTexture(originalTexture).EncodeToPNG()));
            }
            else
            {
              Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
              if ((UnityEngine.Object) sprite != (UnityEngine.Object) null)
                base64Strings.Add(Convert.ToBase64String(AttachmentHelper.GetReadableSpriteTexture(sprite).EncodeToPNG()));
            }
          }));
      }
      return base64Strings.ToArray();
    }

    private static Texture2D GetReadableTexture(Texture2D originalTexture)
    {
      RenderTexture temporary = RenderTexture.GetTemporary(originalTexture.width, originalTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
      Graphics.Blit((Texture) originalTexture, temporary);
      RenderTexture active = RenderTexture.active;
      RenderTexture.active = temporary;
      Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height);
      readableTexture.ReadPixels(new Rect(0.0f, 0.0f, (float) temporary.width, (float) temporary.height), 0, 0);
      readableTexture.Apply();
      RenderTexture.active = active;
      RenderTexture.ReleaseTemporary(temporary);
      return readableTexture;
    }

    public static Texture2D GetReadableSpriteTexture(Sprite sprite)
    {
      Texture2D texture = sprite.texture;
      RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
      Graphics.Blit((Texture) texture, temporary);
      RenderTexture active = RenderTexture.active;
      RenderTexture.active = temporary;
      Texture2D readableSpriteTexture = new Texture2D(texture.width, texture.height);
      readableSpriteTexture.ReadPixels(new Rect(0.0f, 0.0f, (float) temporary.width, (float) temporary.height), 0, 0);
      readableSpriteTexture.Apply();
      RenderTexture.active = active;
      RenderTexture.ReleaseTemporary(temporary);
      return readableSpriteTexture;
    }
  }
}
