
using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("API")]
public class TelegramListenConditionTask : BaseAPIConditionTask<TelegramListenConditionTask.ResponseData>
{
    public BBParameter<string> webhook = new BBParameter<string>() { name = "TELEGRAM_WEBHOOK" };
    public BBParameter<string> content = new BBParameter<string>() { name = "RESPONSE_SUCCESS" };
    
    // string url = $";
    protected override string Url
    {
        get => $"https://api.telegram.org/bot{webhook.value}/getUpdates?offset={lastUpdateId + 1}&timeout=30";
    }

    protected override string Json { get; }
    protected override UnityWebRequestType RequestType => UnityWebRequestType.GET;

    private long lastUpdateId;

    protected override void OnEnable()
    {
        lastUpdateId = GetLastUpdateId();
        base.OnEnable();
    }

    long GetLastUpdateId()
    {
        string stringValue = PlayerPrefs.GetString(Key(), "0");
        return long.Parse(stringValue);
    }

    protected override void OnExecute(ResponseData response)
    {
        Debug.Log("respone");
    }

    void SetLastUpdateId(long value)
    {
        PlayerPrefs.SetString(Key(), value.ToString());
        PlayerPrefs.Save();
    }

    string Key() => webhook.value;
    
    [Serializable]
    public class ResponseData : IAPIResponse
    {
        public string Content => String.Empty;
    }
}