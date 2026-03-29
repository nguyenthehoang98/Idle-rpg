// Decompiled with JetBrains decompiler
// Type: CodeBuddy.UrlDetectorFetcher
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  public class UrlDetectorFetcher
  {
    private static readonly Regex UrlRegex = new Regex("https?://[^\\s]+", RegexOptions.Compiled);

    public List<string> DetectUrls(string text)
    {
      MatchCollection matchCollection = UrlDetectorFetcher.UrlRegex.Matches(text);
      List<string> stringList = new List<string>();
      foreach (Match match in matchCollection)
        stringList.Add(match.Value);
      return stringList;
    }

    public async Task<string> FetchUrlContentAsync(string url)
    {
      using (HttpClient httpClient = new HttpClient())
      {
        try
        {
          HttpResponseMessage response = await httpClient.GetAsync(url);
          response.EnsureSuccessStatusCode();
          string str = await response.Content.ReadAsStringAsync();
          return str;
        }
        catch (Exception ex)
        {
          Debug.LogError((object) ("Failed to fetch URL content: " + ex.Message));
          return string.Empty;
        }
      }
    }
  }
}
