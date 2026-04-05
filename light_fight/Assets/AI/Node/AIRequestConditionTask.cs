using System;
using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Networking;

[Category("API")]
[Description("Send request to AI with system + prompt + format (structured output)")]
public class AIRequestConditionTask : ConditionTask
{
    [ParadoxNotion.Design.Header("Input")]
    public BBParameter<string> ip;
    public BBParameter<string> prompt;
    public BBParameter<string> systemPrompt;
    public BBParameter<string> format;

    [ParadoxNotion.Design.Header("Output")]
    public BBParameter<string> response;
    
    private Coroutine coroutine;
    private bool isTaskCompleted = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        coroutine = StartCoroutine(SendRequest());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (coroutine != null) StopCoroutine(coroutine);
    }

    protected override bool OnCheck()
    {
        return isTaskCompleted;
    }

    IEnumerator SendRequest()
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

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string result = req.downloadHandler.text;

                response.value = result;

                // ===== TRY PARSE JSON =====
                TryParseJson(result);
                EndAction();
            }
            else
            {
                response.value = req.error;
                EndAction();
            }
        }
    }

    void EndAction()
    {
        isTaskCompleted = true;
    }

    void TryParseJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        text = text.Trim();

        // Check simple JSON
        if ((text.StartsWith("{") && text.EndsWith("}")) ||
            (text.StartsWith("[") && text.EndsWith("]")))
        {
            response.value = text;
        }
        else
        {
            // Try extract JSON inside text
            int start = text.IndexOf("{");
            int end = text.LastIndexOf("}");

            if (start >= 0 && end > start)
            {
                string sub = text.Substring(start, end - start + 1);
                response.value = sub;
            }
        }
    }

    // ===== DATA STRUCT =====
    [Serializable]
    struct AIRequestData
    {
        public string system;
        public string prompt;
        public string format; // optional
    }
}