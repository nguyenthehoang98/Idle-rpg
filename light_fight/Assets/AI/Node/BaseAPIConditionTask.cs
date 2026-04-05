using System;
using System.Collections;
using NodeCanvas.Framework;
using UnityEngine;
using UnityEngine.Networking;

public abstract class BaseAPIConditionTask<T> : ConditionTask where T : IAPIResponse
{
    [ParadoxNotion.Design.Header("API-Response")] 
    public BBParameter<string> response;
    public BBParameter<string> error;
    public BBParameter<bool> result;
    
    private Coroutine coroutine;
    private bool isTaskCompleted;

    protected override void OnEnable()
    {
        base.OnEnable();
        coroutine = StartCoroutine(SendRequest(Url, Json, RequestType));
        result.value = false;
        error.value = String.Empty;
        response.value = String.Empty;
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
    
    private IEnumerator SendRequest(string url, string json, UnityWebRequestType requestType)
    {
        using UnityWebRequest req = new UnityWebRequest(url, requestType.ToString());
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
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
                    response.value = data.Content;
                    result.value = true;
                }
                else
                {
                    error.value = string.Format("Error parse json.\n{0}\n", req.downloadHandler.text);
                }   
            }
            else
            {
                response.value = string.Format("<color=green>[Done] {0}", name);
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