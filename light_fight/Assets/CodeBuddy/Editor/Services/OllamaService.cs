// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.OllamaService
// Assembly: CodeBuddy.Editor, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D84C74A1-B0EC-4416-9798-6553CF5A2D81
// Assembly location: C:\Users\Hoang PC\Documents\Idle-rpg\light_fight\Assets\CodeBuddy\Editor\CodeBuddy.Editor.dll

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy.Services
{
  internal class OllamaService : IAiService
  {
    private static OllamaService instance;
    private HttpClient httpClient;
    private string baseUrl;
    private bool isStreaming = false;
    private ServiceStatus status = ServiceStatus.NotInitialized;
    private OllamaSettings settings;

    public static OllamaService Instance
    {
      get
      {
        if (OllamaService.instance == null)
          OllamaService.instance = new OllamaService();
        OllamaService.instance.Init();
        return OllamaService.instance;
      }
    }

    public OllamaService() => this.settings = ScriptableSingleton<CodeBuddy.Settings>.instance.OllamaSettings;

    public void Init()
    {
      if (this.Status != 0)
        return;
      this.baseUrl = this.settings.BaseUrl;
      this.httpClient = new HttpClient();
      Task.Run((Action) (() => this.RunDiagnostics()));
    }

    public void Reset()
    {
      this.httpClient = (HttpClient) null;
      this.isStreaming = false;
      this.StatusMessage = "OK";
      this.Status = ServiceStatus.NotInitialized;
      this.Init();
    }

    public void SendMessageToChat(
      ServiceChat chat,
      ServiceMessage message,
      Action<string> onDataReceived)
    {
      if (this.Status != ServiceStatus.Active)
        Debug.LogError((object) "Code Buddy is not configured. Check settings.");
      else if (this.isStreaming)
      {
        Debug.LogWarning((object) "Streaming is already in progress.");
      }
      else
      {
        this.isStreaming = true;
        if (string.IsNullOrEmpty(chat.Id))
        {
          chat.Id = Guid.NewGuid().ToString();
          chat.Messages.Insert(0, new ServiceMessage()
          {
            Role = "system",
            Skip = true,
            Content = ScriptableSingleton<CodeBuddy.Settings>.instance.Instructions + "\r\nUnity version to use: " + Application.unityVersion
          });
          Task.Run((Func<Task>) (async () =>
          {
            ServiceChat serviceChat = chat;
            string str = await this.GenerateChatTitle(message.Content);
            serviceChat.Name = str;
            serviceChat = (ServiceChat) null;
            str = (string) null;
            EditorMainThreadDispatcher.Enqueue((Action) (() => ScriptableSingleton<CodeBuddyHistory>.instance.Save()));
          }));
        }
        CompletionMessageBase[] completionMessageBaseArray = MessageConverterFactory.GetConverter((IAiService) this).ConvertMessages(chat.Messages);
        StringContent stringContent = new StringContent(JsonConvert.SerializeObject((object) new OllamaChatRequest()
        {
          model = this.settings.Model,
          messages = completionMessageBaseArray,
          stream = true,
          tools = ToolManager.Instance.GetToolsJson((IAiService) this),
          options = new OllamaRequestOptions()
          {
            temperature = this.settings.Temperature,
            num_ctx = this.settings.ContextSize
          }
        }), Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.baseUrl + nameof (chat))
        {
          Content = (HttpContent) stringContent
        };
        ToolCall toolCall = (ToolCall) null;
        using (HttpResponseMessage result1 = this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
        {
          Stream result2 = result1.Content.ReadAsStreamAsync().Result;
          string str = "";
          using (StreamReader streamReader = new StreamReader(result2))
          {
            while (!streamReader.EndOfStream && this.isStreaming)
            {
              string json = streamReader.ReadLine();
              OllamaStreamingResponse streamingResponse = JsonConvert.DeserializeObject<OllamaStreamingResponse>(json);
              if (string.IsNullOrEmpty(streamingResponse.model))
              {
                OllamaError ollamaError = JsonConvert.DeserializeObject<OllamaError>(json);
                if (ollamaError != null)
                {
                  string message1 = "Ollama error: " + ollamaError.error;
                  Debug.LogError((object) message1);
                  onDataReceived("\n\r" + message1);
                  str = str + "\n\r" + message1;
                }
              }
              if (streamingResponse != null)
              {
                string content = streamingResponse.message.content;
                if (json.Contains("tool_calls"))
                {
                  JToken jtoken = JObject.Parse(json)[nameof (message)][(object) "tool_calls"];
                  if (jtoken.HasValues && toolCall == null)
                  {
                    toolCall = new ToolCall();
                    toolCall.id = Guid.NewGuid().ToString();
                    toolCall.function = new FunctionDetails()
                    {
                      name = jtoken[(object) 0][(object) "function"][(object) "name"].ToString(),
                      arguments = jtoken[(object) 0][(object) "function"][(object) "arguments"].ToString()
                    };
                  }
                }
                onDataReceived(content);
                str += content;
              }
            }
          }
        }
        this.isStreaming = false;
        if (toolCall == null)
          return;
        CodeBuddyMessageBus.Instance.Publish<MToolCall>(new MToolCall(toolCall));
      }
    }

    public void CancelRequest() => this.isStreaming = false;

    public ServiceStatus Status
    {
      get => this.status;
      private set
      {
        if (value == this.status)
          return;
        this.status = value;
        CodeBuddyMessageBus.Instance.Publish<ServiceStatus>(value);
      }
    }

    public bool IsStreaming => this.isStreaming;

    public string StatusMessage { get; private set; } = "OK";

    private void RunDiagnostics()
    {
      this.Status = ServiceStatus.Validating;
      this.StatusMessage = "";
      if (string.IsNullOrEmpty(this.settings.BaseUrl))
      {
        this.Status = ServiceStatus.Failed;
        this.StatusMessage = "Base url is not set.";
      }
      else
      {
        try
        {
          HttpResponseMessage result = this.httpClient.GetAsync(this.settings.BaseUrl).Result;
          if (result != null && (result.StatusCode == HttpStatusCode.RequestTimeout || result.StatusCode == HttpStatusCode.ServiceUnavailable))
          {
            this.Status = ServiceStatus.Failed;
            this.StatusMessage = this.settings.BaseUrl + " server cannot be reached. Please check that Ollama installed and running.";
            return;
          }
        }
        catch
        {
          this.Status = ServiceStatus.Failed;
          this.StatusMessage = this.settings.BaseUrl + " server cannot be reached. Please check that Ollama installed and running.";
          return;
        }
        this.Status = ServiceStatus.Active;
        this.settings.UpdateModelList().GetAwaiter().GetResult();
        if (this.settings.ModelList.Length == 0)
        {
          this.Status = ServiceStatus.Failed;
          this.StatusMessage = "No installed models found";
        }
        else
        {
          string model = this.settings.Model;
          CompletionMessageText[] completionMessageTextArray = new CompletionMessageText[1];
          CompletionMessageText completionMessageText = new CompletionMessageText();
          completionMessageText.role = "user";
          completionMessageText.content = "This is a test message. Respond OK";
          completionMessageTextArray[0] = completionMessageText;
          if (this.httpClient.PostAsync(this.settings.BaseUrl + "chat", (HttpContent) new StringContent(JsonConvert.SerializeObject((object) new
          {
            model = model,
            messages = completionMessageTextArray,
            stream = true
          }), Encoding.UTF8, "application/json")).Result.StatusCode == HttpStatusCode.Unauthorized)
          {
            this.Status = ServiceStatus.Failed;
            this.StatusMessage = "Not authorized. Please check that your API key is correct and Ollama account has sufficient funds.";
          }
          else
          {
            this.Status = ServiceStatus.Active;
            this.StatusMessage = "OK";
          }
        }
      }
    }

    internal string[] GetModelList()
    {
      if (this.Status != ServiceStatus.Active)
        return new string[0];
      try
      {
        HttpResponseMessage result = this.httpClient.GetAsync(this.baseUrl + "tags").Result;
        if (result.IsSuccessStatusCode)
          return ((IEnumerable<OllamaService.ModelInfo>) JsonConvert.DeserializeObject<OllamaService.ModelListResponse>(result.Content.ReadAsStringAsync().Result).models).Select<OllamaService.ModelInfo, string>((Func<OllamaService.ModelInfo, string>) (m => m.name)).ToArray<string>();
        Debug.LogError((object) ("Failed to retrieve model list: " + result.ReasonPhrase));
        return new string[0];
      }
      catch (Exception ex)
      {
        Debug.LogError((object) ("Exception occurred while retrieving model list: " + ex.Message));
        return new string[0];
      }
    }

    private async Task<string> GenerateChatTitle(string firstMessage)
    {
      string baseUrl = this.settings.BaseUrl;
      string json = "";
      HttpClient nameClient = new HttpClient();
      try
      {
        OllamaChatRequest ollamaChatRequest1 = new OllamaChatRequest();
        OllamaChatRequest ollamaChatRequest2 = ollamaChatRequest1;
        CompletionMessageText[] completionMessageTextArray = new CompletionMessageText[2];
        CompletionMessageText completionMessageText1 = new CompletionMessageText();
        completionMessageText1.role = "system";
        completionMessageText1.content = "You are an assistant that generates short, engaging conversation titles without special symbols. On every question return only the title";
        completionMessageTextArray[0] = completionMessageText1;
        CompletionMessageText completionMessageText2 = new CompletionMessageText();
        completionMessageText2.role = "user";
        completionMessageText2.content = "Summarize the following conversation into a short 100 characters max, engaging title: " + firstMessage;
        completionMessageTextArray[1] = completionMessageText2;
        CompletionMessageBase[] completionMessageBaseArray = (CompletionMessageBase[]) completionMessageTextArray;
        ollamaChatRequest2.messages = completionMessageBaseArray;
        ollamaChatRequest1.model = this.settings.Model;
        ollamaChatRequest1.stream = false;
        StringContent requestContent = new StringContent(JsonConvert.SerializeObject((object) ollamaChatRequest1), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await nameClient.PostAsync(baseUrl + "chat", (HttpContent) requestContent);
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        OllamaService.OllamaCompletionResponse cp = JsonConvert.DeserializeObject<OllamaService.OllamaCompletionResponse>(responseJson);
        json = responseJson;
        OllamaService.OllamaMessage message = cp.message;
        string content = message.content;
        content = Regex.Replace(content, "<think>.*?</think>", "", RegexOptions.Singleline);
        return content.Trim();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) json);
        Debug.LogError((object) ex);
        return ex.Message;
      }
    }

    [Serializable]
    private class ModelListResponse
    {
      public OllamaService.ModelInfo[] models;
    }

    [Serializable]
    private class ModelInfo
    {
      public string name;
    }

    [Serializable]
    internal class OllamaCompletionResponse
    {
      public OllamaService.OllamaMessage message;
    }

    [Serializable]
    internal class OllamaMessage
    {
      public string content;
    }
  }
}
