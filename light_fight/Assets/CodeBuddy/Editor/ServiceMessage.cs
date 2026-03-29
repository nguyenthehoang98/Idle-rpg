// Decompiled with JetBrains decompiler
// Type: CodeBuddy.ServiceMessage
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Text;

#nullable enable
namespace CodeBuddy
{
  [Serializable]
  internal class ServiceMessage
  {
    public string Id;
    public string Role;
    public string Content;
    public ToolCall[] ToolCalls;
    public bool Skip = false;
    public List<string> AttachmentsGuids;
    public List<string> TexturesGuids;
    public Dictionary<string, string> Urls;
    private string[] imagesContent = (string[]) null;

    public string TextAttachmentsContent
    {
      get => AttachmentHelper.GetAttachmentsString(this.AttachmentsGuids).Result;
    }

    public string[] ImagesContent
    {
      set => this.imagesContent = value;
      get
      {
        return this.imagesContent == null ? AttachmentHelper.GetAttachmentsTexture(this.TexturesGuids).Result : this.imagesContent;
      }
    }

    public string UrlsContent
    {
      get
      {
        if (this.Urls == null || this.Urls.Count == 0)
          return "";
        StringBuilder stringBuilder = new StringBuilder("External docs and references:");
        foreach (KeyValuePair<string, string> url in this.Urls)
        {
          stringBuilder.AppendLine(url.Key);
          stringBuilder.AppendLine(url.Value);
          stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
      }
    }

    public string FullContent
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder(this.Content);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(this.TextAttachmentsContent);
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(this.UrlsContent);
        return stringBuilder.ToString();
      }
    }
  }
}
