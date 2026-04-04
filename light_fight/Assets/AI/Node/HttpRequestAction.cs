using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[Category("HTTP")]
[Description("Generic HTTP Request")]
public class HttpRequestAction : ActionTask
{
    // ===== INPUT =====
    public BBParameter<string> url;
    public BBParameter<string> method = "POST";

    [TextArea]
    public BBParameter<string> body;

    public BBParameter<string> contentType = "application/json";
    public BBParameter<int> timeout = 10;

    // ===== OUTPUT =====
    public BBParameter<string> response;
    public BBParameter<string> error;
    public BBParameter<long> statusCode;

    protected override void OnExecute()
    {
        StartCoroutine(SendRequest());
    }

    IEnumerator SendRequest()
    {
        string m = method.value.ToUpper();

        UnityWebRequest req;

        if (m == "GET")
        {
            req = UnityWebRequest.Get(url.value);
        }
        else
        {
            req = new UnityWebRequest(url.value, m);

            if (!string.IsNullOrEmpty(body.value))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body.value);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            req.downloadHandler = new DownloadHandlerBuffer();
        }

        req.SetRequestHeader("Content-Type", contentType.value);
        req.timeout = timeout.value;

        yield return req.SendWebRequest();

        statusCode.value = req.responseCode;

        if (req.result == UnityWebRequest.Result.Success)
        {
            response.value = req.downloadHandler.text;
            EndAction(true);
        }
        else
        {
            error.value = req.error;
            Debug.LogError($"[HTTP] Error: {req.error}");
            EndAction(false);
        }
    }
}