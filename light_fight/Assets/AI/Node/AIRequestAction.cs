using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

[Category("AI")]
[Description("Send request to AI with system + prompt + format (structured output)")]
public class AIRequestAction : ActionTask
{
    // ===== INPUT =====
    public BBParameter<string> ip;
    public BBParameter<string> prompt;
    public BBParameter<string> systemPrompt;

    public BBParameter<string> format; // JSON schema (optional)
    public BBParameter<int> timeout = 10;

    // ===== OUTPUT =====
    public BBParameter<bool> boolVariable;
    public BBParameter<string> response;
    public BBParameter<string> error;

    // Optional parsed JSON
    public BBParameter<string> jsonRaw;

    protected override void OnExecute()
    {
        StartCoroutine(RequestAI());
    }

    IEnumerator RequestAI()
    {
        string url = ip.value;

        // ===== BUILD REQUEST BODY =====
        AIRequestData payload = new AIRequestData
        {
            system = systemPrompt.value,
            prompt = prompt.value,
            format = string.IsNullOrEmpty(format.value) ? null : format.value
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = timeout.value;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string result = req.downloadHandler.text;

                response.value = result;

                // ===== TRY PARSE JSON =====
                TryParseJson(result);

                EndAction(true);
            }
            else
            {
                error.value = req.error;
                Debug.LogError($"[AIRequest] Error: {req.error}");
                Debug.LogError(url);

                EndAction(false);
            }

            boolVariable.value = true;
        }
    }

    void TryParseJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        text = text.Trim();

        // Check simple JSON
        if ((text.StartsWith("{") && text.EndsWith("}")) ||
            (text.StartsWith("[") && text.EndsWith("]")))
        {
            jsonRaw.value = text;
        }
        else
        {
            // Try extract JSON inside text
            int start = text.IndexOf("{");
            int end = text.LastIndexOf("}");

            if (start >= 0 && end > start)
            {
                string sub = text.Substring(start, end - start + 1);
                jsonRaw.value = sub;
            }
        }
    }

    // ===== DATA STRUCT =====
    [Serializable]
    class AIRequestData
    {
        public string system;
        public string prompt;
        public string format; // optional
    }
}