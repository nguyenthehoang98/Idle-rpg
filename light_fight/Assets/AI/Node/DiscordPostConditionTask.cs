using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("API")]
public class DiscordPostConditionTask : BaseAPIConditionTask<DiscordPostConditionTask.ResponseData>
{
    public BBParameter<string> webhook = new BBParameter<string>() { name = "DISCORD_WEBHOOK" };
    public BBParameter<string> content = new BBParameter<string>() { name = "RESPONSE_SUCCESS" };

    protected override string Url
    {
        get => $"https://discord.com/api/webhooks/{webhook.value}";
    }

    protected override string Json
    {
        get => JsonUtility.ToJson(new RequestData { content = content.value });
    }

    protected override bool IsJsonResponse => false;

    protected override UnityWebRequestType RequestType => UnityWebRequestType.POST;
    
    [Serializable]
    public class ResponseData : IAPIResponse
    {
        public string Content => String.Empty;
    }

    [Serializable]
    public class RequestData
    {
        public string content;
    }
}