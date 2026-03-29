using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CodeBuddy.Services
{
    internal class ClaudeService : IAiService
    {
        private static ClaudeService instance;

        private HttpClient httpClient;
        private string baseUrl;
        private bool isStreaming = false;

        private ServiceStatus status = ServiceStatus.NotInitialized;
        private ClaudeSettings settings;

        public static ClaudeService Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClaudeService();

                instance.Init();
                return instance;
            }
        }

        public ClaudeService()
        {
            settings = ScriptableSingleton<CodeBuddy.Settings>.instance.ClaudeSettings;
        }

        public void Init()
        {
            if (Status != ServiceStatus.NotInitialized)
                return;

            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                Status = ServiceStatus.Failed;
                StatusMessage = "Missing API Key";
                return;
            }

            baseUrl = string.IsNullOrEmpty(settings.BaseUrl)
                ? "https://api.anthropic.com/"
                : settings.BaseUrl;

            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-api-key", settings.ApiKey);
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            RunDiagnostics();
        }

        public void Reset()
        {
            httpClient?.Dispose();
            httpClient = null;

            isStreaming = false;
            Status = ServiceStatus.NotInitialized;
            StatusMessage = "Reset";

            Init();
        }

        public void SendMessageToChat(
            ServiceChat chat,
            ServiceMessage message,
            Action<string> onDataReceived)
        {
            if (Status != ServiceStatus.Active)
            {
                Debug.LogError("ClaudeService not ready");
                return;
            }

            if (isStreaming)
            {
                Debug.LogWarning("Already streaming");
                return;
            }

            isStreaming = true;

            Task.Run(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(chat.Id))
                    {
                        chat.Id = Guid.NewGuid().ToString();
                        chat.Messages.Insert(0, new ServiceMessage
                        {
                            Role = "system",
                            Content = "Unity version: " + Application.unityVersion
                        });

                        chat.Name = await GenerateChatTitle(message.Content);

                        EditorMainThreadDispatcher.Enqueue(() =>
                        {
                            ScriptableSingleton<CodeBuddyHistory>.instance.Save();
                        });
                    }

                    var requestData = new
                    {
                        model = settings.Model,
                        messages = MessageConverterFactory
                            .GetConverter(this)
                            .ConvertMessages(chat.Messages),
                        max_tokens = settings.MaxTokens,
                        stream = true
                    };

                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        baseUrl + "v1/messages");

                    request.Content = new StringContent(
                        JsonConvert.SerializeObject(requestData),
                        Encoding.UTF8,
                        "application/json");

                    var response = await httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        Debug.LogError($"Claude error: {response.StatusCode}\n{error}");
                        return;
                    }

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new System.IO.StreamReader(stream);

                    while (!reader.EndOfStream && isStreaming)
                    {
                        var line = await reader.ReadLineAsync();

                        if (string.IsNullOrEmpty(line) || !line.StartsWith("data:"))
                            continue;

                        string json = line.Substring(5).Trim();

                        try
                        {
                            var obj = JObject.Parse(json);

                            var delta = obj["delta"];
                            if (delta != null && delta["text"] != null)
                            {
                                string text = delta["text"].ToString();
                                onDataReceived?.Invoke(text);
                            }
                        }
                        catch
                        {
                            // ignore broken chunks
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                finally
                {
                    isStreaming = false;
                }
            });
        }

        private async Task<string> GenerateChatTitle(string input)
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.ApiKey);

                var body = new
                {
                    model = "claude-3-5-haiku-latest",
                    messages = new[]
                    {
                        new { role = "user", content = "Create short title" },
                        new { role = "user", content = input }
                    },
                    max_tokens = 50
                };

                var res = await client.PostAsync(
                    baseUrl + "v1/messages",
                    new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

                var json = await res.Content.ReadAsStringAsync();

                var text = JObject.Parse(json)["content"]?[0]?["text"]?.ToString() ?? "Chat";

                return Regex.Replace(text, "<.*?>", "").Trim();
            }
            catch
            {
                return "Chat";
            }
        }

        public void CancelRequest()
        {
            isStreaming = false;
        }

        public ServiceStatus Status
        {
            get => status;
            private set
            {
                if (status == value) return;
                status = value;
                CodeBuddyMessageBus.Instance.Publish(status);
            }
        }

        public bool IsStreaming => isStreaming;

        public string StatusMessage { get; private set; } = "OK";

        private async void RunDiagnostics()
        {
            Status = ServiceStatus.Validating;

            try
            {
                var res = await httpClient.GetAsync(baseUrl);

                if (res.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Status = ServiceStatus.Failed;
                    StatusMessage = "Unauthorized";
                    return;
                }

                Status = ServiceStatus.Active;
                StatusMessage = "OK";
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Failed;
                StatusMessage = ex.Message;
            }
        }

        public string[] GetModelList()
        {
            try
            {
                var json = httpClient.GetStringAsync(baseUrl + "v1/models").Result;
                var arr = JObject.Parse(json)["data"];

                var list = new List<string>();

                foreach (var item in arr)
                {
                    list.Add(item["id"].ToString());
                }

                return list.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
    }
}