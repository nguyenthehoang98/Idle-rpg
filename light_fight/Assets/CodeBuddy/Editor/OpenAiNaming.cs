// Decompiled with JetBrains decompiler
// Type: CodeBuddy.OpenAiNaming
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy
{
  internal static class OpenAiNaming
  {
    internal static async Task<string> GenerateChatTitle(string firstMessage)
    {
      string apiKey = ScriptableSingleton<Settings>.instance.OpenAiCompletionsSettings.ApiKey;
      string baseUrl = ScriptableSingleton<Settings>.instance.OpenAiCompletionsSettings.BaseUrl ?? "https://api.openai.com/";
      HttpClient httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
      httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
      string json = "";
      try
      {
        CompletionMessageText[] completionMessageTextArray = new CompletionMessageText[2];
        CompletionMessageText completionMessageText1 = new CompletionMessageText();
        completionMessageText1.role = "system";
        completionMessageText1.content = "You are an assistant that generates short, engaging conversation titles without special simbols.";
        completionMessageTextArray[0] = completionMessageText1;
        CompletionMessageText completionMessageText2 = new CompletionMessageText();
        completionMessageText2.role = "user";
        completionMessageText2.content = "Summarize the following conversation into a short, engaging title: " + firstMessage;
        completionMessageTextArray[1] = completionMessageText2;
        StringContent requestContent = new StringContent(JsonConvert.SerializeObject((object) new
        {
          messages = completionMessageTextArray,
          model = "gpt-4o-mini",
          temperature = 0.01f,
          stream = false
        }), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(ScriptableSingleton<Settings>.instance.OpenAiCompletionsSettings.BaseUrl + "v1/chat/completions", (HttpContent) requestContent);
        string responseJson = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        string cleanedJson = Regex.Replace(responseJson, "\\\\\\\"", "\"");
        ChatCompletionResponse cp = JsonConvert.DeserializeObject<ChatCompletionResponse>(cleanedJson);
        json = responseJson;
        Choice[] choices = cp.choices;
        Choice choice = choices[0];
        Message message = choice.message;
        string content = message.content;
        return content;
      }
      catch (Exception ex)
      {
        Debug.LogError((object) json);
        Debug.LogError((object) ex);
        return ex.Message;
      }
    }

    internal static async Task<string> GenerateChatTitleWithDeepSeek(string firstMessage)
    {
      string apiKey = ScriptableSingleton<Settings>.instance.DeepSeekSettings.ApiKey;
      string baseUrl = ScriptableSingleton<Settings>.instance.DeepSeekSettings.BaseUrl ?? "https://api.deepseek.com/";
      HttpClient httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
      try
      {
        ChatCompletionRequest completionRequest1 = new ChatCompletionRequest();
        ChatCompletionRequest completionRequest2 = completionRequest1;
        CompletionMessageText[] completionMessageTextArray = new CompletionMessageText[2];
        CompletionMessageText completionMessageText1 = new CompletionMessageText();
        completionMessageText1.role = "system";
        completionMessageText1.content = "You are an assistant that generates short, engaging conversation titles without special symbols.";
        completionMessageTextArray[0] = completionMessageText1;
        CompletionMessageText completionMessageText2 = new CompletionMessageText();
        completionMessageText2.role = "user";
        completionMessageText2.content = "Summarize the following conversation into a short, engaging title: " + firstMessage;
        completionMessageTextArray[1] = completionMessageText2;
        CompletionMessageBase[] completionMessageBaseArray = (CompletionMessageBase[]) completionMessageTextArray;
        completionRequest2.messages = completionMessageBaseArray;
        completionRequest1.model = "deepseek-chat";
        completionRequest1.temperature = 0.01f;
        StringContent requestContent = new StringContent(JsonConvert.SerializeObject((object) completionRequest1), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(baseUrl + "chat/completions", (HttpContent) requestContent);
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        ChatCompletionResponse completionResponse = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);
        return completionResponse.choices[0].message.content;
      }
      catch (Exception ex)
      {
        Debug.LogError((object) ("Failed to generate chat title: " + ex.Message));
        return "New Chat";
      }
    }
  }
}
