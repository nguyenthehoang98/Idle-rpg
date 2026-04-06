using System;
using System.IO;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("API")]
public class TelegramPostConditionTask : BaseAPIConditionTask<TelegramPostConditionTask.ResponseData>
{
    public BBParameter<string> webhook = new BBParameter<string>() { name = "TELEGRAM_WEBHOOK" };
    public BBParameter<string> chatID = new BBParameter<string>() { name = "TELEGRAM_CHAT_ID" }; // Lưu vào config hoặc có 1 hàm reload để lấy chatId thay vì điền tay.
    public BBParameter<string> content = new BBParameter<string>() { name = "RESPONSE_SUCCESS" };

    protected override string Url
    {
        get => $"https://api.telegram.org/bot{webhook.value}/sendMessage";
    }

    protected override string Json
    {
        get => JsonUtility.ToJson(new RequestData
        {
            chat_id = chatID.value,
            text = content.value,
        });
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
        public string chat_id;
        public string text;
        public string parse_mode = "HTML";
    }
}