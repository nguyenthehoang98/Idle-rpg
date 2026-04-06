using System;
using System.Collections;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BaseAPIConditionTask<T> : ConditionTask where T : IAPIResponse
{
    [ParadoxNotion.Design.Header("API-Response")]
    public BBParameter<string> success = new BBParameter<string>() { name = "RESPONSE_SUCCESS" };
    public BBParameter<string> error = new BBParameter<string>() { name = "RESPONSE_ERROR" };
    public BBParameter<bool> result = new BBParameter<bool>() { name = "RESPONSE_RESULT" };
    
    private Coroutine coroutine;
    private bool isTaskCompleted;

    protected override void OnEnable()
    {
        base.OnEnable();
        coroutine = StartCoroutine(SendRequest());
        result.value = false;
        error.value = String.Empty;
        success.value = String.Empty;
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
    
    protected abstract string Url { get; }
    protected abstract string Json { get; }
    protected abstract UnityWebRequestType RequestType { get; }
    protected virtual bool IsJsonResponse
    {
        get => true;
    }
    
    private IEnumerator SendRequest()
    {
        using UnityWebRequest req = new UnityWebRequest(Url, RequestType.ToString());
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(Json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            if (IsJsonResponse)
            {
                var data = JsonUtility.FromJson<T>(req.downloadHandler.text);
                if (data != null)
                {
                    success.value = data.Content;
                    result.value = true;
                }
                else
                {
                    error.value = string.Format("Error parse json.\n{0}\n", req.downloadHandler.text);
                }   
            }
            else
            {
                success.value = string.Format("<color=green>[Done] {0}", name);
                result.value = true;
            }
        }
        else
        {
            error.value = req.error;
        }

        isTaskCompleted = true;
    }
}

public enum UnityWebRequestType
{
    POST,
}

public interface IAPIResponse
{
    string Content { get; }
}