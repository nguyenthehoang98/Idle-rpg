using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CodeBuddy.Services
{
    internal class GeminiService : IAiService
    {
        private static GeminiService instance;

        private HttpClient httpClient;
        private string baseUrl;
        private bool isStreaming = false;

        private ServiceStatus status = ServiceStatus.NotInitialized;
        private GeminiSettings settings;

        public static GeminiService Instance
        {
            get
            {
                if (instance == null)
                    instance = new GeminiService();

                instance.Init();
                return instance;
            }
        }

        public GeminiService()
        {
            settings = ScriptableSingleton<CodeBuddy.Settings>.instance.GeminiSettings;
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
                ? "https://generativelanguage.googleapis.com/"
                : settings.BaseUrl;

            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", settings.ApiKey);

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
                Debug.LogError("GeminiService not ready");
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
                            Role = "user",
                            Content = "Unity version: " + Application.unityVersion
                        });

                        chat.Name = await GenerateChatTitle(message.Content);

                        EditorMainThreadDispatcher.Enqueue(() =>
                        {
                            ScriptableSingleton<CodeBuddyHistory>.instance.Save();
                        });
                    }

                    var body = new
                    {
                        contents = MessageConverterFactory
                            .GetConverter(this)
                            .ConvertMessages(chat.Messages),
                        generationConfig = new
                        {
                            temperature = settings.Temperature
                        }
                    };

                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{baseUrl}v1beta/models/{settings.Model}:streamGenerateContent?alt=sse");

                    request.Content = new StringContent(
                        JsonConvert.SerializeObject(body),
                        Encoding.UTF8,
                        "application/json");

                    var response = await httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        Debug.LogError($"Gemini error: {response.StatusCode}\n{error}");
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

                        if (json == "[DONE]")
                            break;

                        try
                        {
                            var obj = JObject.Parse(json);

                            var text = obj["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                            if (!string.IsNullOrEmpty(text))
                                onDataReceived?.Invoke(text);
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

        private async Task<string> GenerateChatTitle(string input)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-goog-api-key", settings.ApiKey);

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = "Create short title: " + input }
                            }
                        }
                    }
                };

                var res = await client.PostAsync(
                    $"{baseUrl}v1beta/models/{settings.Model}:generateContent",
                    new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

                var json = await res.Content.ReadAsStringAsync();

                var text = JObject.Parse(json)["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                return Regex.Replace(text ?? "Chat", "<.*?>", "").Trim();
            }
            catch
            {
                return "Chat";
            }
        }

        public string[] GetModelList()
        {
            try
            {
                var json = httpClient.GetStringAsync(
                    $"{baseUrl}v1beta/models?key={settings.ApiKey}").Result;

                var arr = JObject.Parse(json)["models"];

                var list = new List<string>();

                foreach (var item in arr)
                {
                    var name = item["name"].ToString().Replace("models/", "");

                    if (!name.Contains("embedding") &&
                        !name.Contains("vision") &&
                        !name.Contains("imagen"))
                    {
                        list.Add(name);
                    }
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