using System;
using System.Collections;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace AI
{
    public abstract class AIBash : EditorWindow
    {
        protected virtual string API => "http://{0}:11434/api/generate";
        protected virtual string IP => "100.100.181.25";
        protected abstract string Model { get; } 
        
        protected IEnumerator SendRequest(string prompt, Action<(bool success, string response)> callback)
        {
            var data = new RequestData(prompt, Model);
            var json = JsonUtility.ToJson(data);
            var url = string.Format(API, IP);

            using var req = new UnityWebRequest(url, "POST");

            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 120;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<ResponseData>(req.downloadHandler.text);
                if (res != null)
                {
                    callback?.Invoke((true, res.response));
                }
                else
                {
                    callback?.Invoke((false, "Parse error"));
                }
            }
            else
            {
                callback?.Invoke((false, req.error));
            }

            Repaint();
        }
        
        [Serializable]
        class RequestData
        {
            public string model;
            public string prompt;
            public bool stream;

            public RequestData(string prompt, string model)
            {
                this.prompt = prompt;
                this.model = model;
            }
        }

        [Serializable]
        class ResponseData
        {
            public string response;
        }
    }
}