// Decompiled with JetBrains decompiler
// Type: CodeBuddy.Services.DeepSeekService
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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace CodeBuddy.Services
{
  internal class DeepSeekService : IAiService
  {
    private static DeepSeekService instance;
    private HttpClient httpClient;
    private string baseUrl;
    private bool isStreaming = false;
    private ServiceStatus status = ServiceStatus.NotInitialized;
    private DeepSeekSettings settings;

    public static DeepSeekService Instance
    {
      get
      {
        if (DeepSeekService.instance == null)
          DeepSeekService.instance = new DeepSeekService();
        DeepSeekService.instance.Init();
        return DeepSeekService.instance;
      }
    }

    public DeepSeekService()
    {
      this.settings = ScriptableSingleton<CodeBuddy.Settings>.instance.DeepSeekSettings;
    }

    public void Init()
    {
      if (this.Status != 0)
        return;
      string apiKey = this.settings.ApiKey;
      this.baseUrl = this.settings.BaseUrl ?? "https://api.deepseek.com/";
      this.httpClient = new HttpClient();
      this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
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
            string str = await OpenAiNaming.GenerateChatTitleWithDeepSeek(message.Content);
            serviceChat.Name = str;
            serviceChat = (ServiceChat) null;
            str = (string) null;
            EditorMainThreadDispatcher.Enqueue((Action) (() => ScriptableSingleton<CodeBuddyHistory>.instance.Save()));
          }));
        }
        CompletionMessageBase[] source = MessageConverterFactory.GetConverter((IAiService) this).ConvertMessages(chat.Messages);
        StringContent stringContent = new StringContent(JsonConvert.SerializeObject((object) new ChatCompletionRequest()
        {
          messages = ((IEnumerable<CompletionMessageBase>) source).ToArray<CompletionMessageBase>(),
          model = this.settings.Model,
          temperature = this.settings.Temperature,
          stream = true,
          tools = (this.settings.Model == "deepseek-chat" ? ToolManager.Instance.GetToolsJson((IAiService) this) : (object[]) null)
        }), Encoding.UTF8, "application/json");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, this.baseUrl + "chat/completions")
        {
          Content = (HttpContent) stringContent
        };
        ToolCall toolCall1 = (ToolCall) null;
        using (HttpResponseMessage result1 = this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
        {
          if (result1.IsSuccessStatusCode)
          {
            Stream result2 = result1.Content.ReadAsStreamAsync().Result;
            string str1 = "";
            using (StreamReader streamReader = new StreamReader(result2))
            {
              while (!streamReader.EndOfStream && this.isStreaming)
              {
                string str2 = streamReader.ReadLine();
                if (str2 != null && str2.StartsWith("data:") && str2.Contains("chat.completion.chunk"))
                {
                  string json = str2.Substring(6);
                  if (json.Contains("tool_calls"))
                  {
                    JToken jtoken = JObject.Parse(json)["choices"][(object) 0][(object) "delta"];
                    if (jtoken.HasValues && jtoken[(object) "tool_calls"] != null)
                    {
                      ToolCall toolCall2 = JsonConvert.DeserializeObject<ToolCall>(jtoken[(object) "tool_calls"][(object) 0].ToString());
                      if (toolCall1 == null)
                        toolCall1 = toolCall2;
                      toolCall1.function.arguments += toolCall2.function.arguments;
                    }
                  }
                  else
                  {
                    string content = JsonConvert.DeserializeObject<CompletionMessageData>(json).choices[0].delta.content;
                    onDataReceived(content);
                    str1 += content;
                  }
                }
              }
            }
          }
          else
          {
            string result3 = result1.Content.ReadAsStringAsync().Result;
            Debug.LogError((object) string.Format("Request unsuccessful! \r\nResponse code: {0}\r\nResponse text: {1}", (object) result1.StatusCode, (object) result3));
          }
        }
        this.isStreaming = false;
        if (toolCall1 == null)
          return;
        CodeBuddyMessageBus.Instance.Publish<MToolCall>(new MToolCall(toolCall1));
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
      if (string.IsNullOrEmpty(this.settings.ApiKey))
      {
        this.Status = ServiceStatus.Failed;
        this.StatusMessage = "API key is not set. Please check Code Buddy settings.";
      }
      else if (string.IsNullOrEmpty(this.settings.BaseUrl))
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
            this.StatusMessage = this.settings.BaseUrl + " server cannot be reached.";
            return;
          }
        }
        catch
        {
          this.Status = ServiceStatus.Failed;
          this.StatusMessage = this.settings.BaseUrl + " server cannot be reached.";
          return;
        }
        ChatCompletionRequest completionRequest1 = new ChatCompletionRequest();
        ChatCompletionRequest completionRequest2 = completionRequest1;
        CompletionMessageText[] completionMessageTextArray = new CompletionMessageText[1];
        CompletionMessageText completionMessageText = new CompletionMessageText();
        completionMessageText.role = "user";
        completionMessageText.content = "This is a test message. Respond OK";
        completionMessageTextArray[0] = completionMessageText;
        CompletionMessageBase[] completionMessageBaseArray = (CompletionMessageBase[]) completionMessageTextArray;
        completionRequest2.messages = completionMessageBaseArray;
        completionRequest1.model = "deepseek-chat";
        completionRequest1.temperature = 0.01f;
        completionRequest1.stream = true;
        if (this.httpClient.PostAsync(this.settings.BaseUrl + "/chat/completions", (HttpContent) new StringContent(JsonConvert.SerializeObject((object) completionRequest1), Encoding.UTF8, "application/json")).Result.StatusCode == HttpStatusCode.Unauthorized)
        {
          this.Status = ServiceStatus.Failed;
          this.StatusMessage = "Not authorized. Please check that your API key is correct.";
        }
        else
        {
          this.Status = ServiceStatus.Active;
          this.StatusMessage = "OK";
        }
      }
    }
  }
}
