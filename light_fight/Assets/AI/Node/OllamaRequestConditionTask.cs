using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("API")]
public class OllamaRequestConditionTask : BaseAPIConditionTask<OllamaRequestConditionTask.ResponseData>
{
    [ParadoxNotion.Design.Header("Input")] 
    public BBParameter<string> host = new BBParameter<string>() { name = "AI_HOST" };
    public BBParameter<string> model = new BBParameter<string>() { name = "AI_MODEL" };
    public BBParameter<string> prompt;
    public BBParameter<string> systemPrompt;

    protected override string Url
    {
        get => host.value + "/api/chat";
    }

    protected override string Json
    {
        get
        {
            return JsonUtility.ToJson(new RequestData
            {
                model = model.value,
                messages = new Message[]
                {
                    new Message { role = "system", content = systemPrompt.value },
                    new Message { role = "user", content = prompt.value },
                },
                stream = false,
            });
        }
    }

    protected override UnityWebRequestType RequestType
    {
        get => UnityWebRequestType.POST;
    }

    [Serializable]
    public class RequestData
    {
        public string model;
        public Message[] messages = new Message[0];
        public bool stream;
    }

    [Serializable]
    public class ResponseData : IAPIResponse
    {
        public Message message;

        public string Content => message.content;
    }

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
        public string thinking;
    }
}