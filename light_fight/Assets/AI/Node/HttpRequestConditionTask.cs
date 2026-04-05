using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Networking;

[Category("API")]
public class HttpRequestConditionTask : ConditionTask
{
    [ParadoxNotion.Design.Header("Input")]
    public BBParameter<string> url = new BBParameter<string>("https://jsonplaceholder.typicode.com/todos/1");

    [ParadoxNotion.Design.Header("Output")]
    public BBParameter<string> response;
    public BBParameter<string> error;
    public BBParameter<bool> result;
    
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
        using (UnityWebRequest request = UnityWebRequest.Get(url.value))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                response.value = request.downloadHandler.text;
            }
            else
            {
                error.value = request.error;
            }
            
            isTaskCompleted = true;
            result.value = request.result == UnityWebRequest.Result.Success;
        }
    }
}