using System;
using System.Collections.Generic;
using CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Serialization.Json;
using UnityEngine;

namespace _Games.CloudAPI.Playfab
{
    public static class PlayFabCloudScript
    {
        public static UniTask<(RequestResult result, object data)> ExecuteCloudScript(
            string functionName, Dictionary<string, object> parameters)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, object)>();
            var result = new RequestResult { Method = "PlayFab" };
            
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                {
                    FunctionName = functionName,
                    FunctionParameter = parameters
                },
                execute =>
                {
                    Debug.Log($"{functionName}: " + JsonSerialization.ToJson(execute));

                    result.Success = true;
                    tcs.TrySetResult((result, execute.FunctionResult));
                },
                error =>
                {
                    result.Success = false;
                    result.ErrorMessage = error.GenerateErrorReport();
                    result.ErrorCode = (int)error.Error;
                    tcs.TrySetResult((result, default));
                });

            return tcs.Task;
        }

        public static bool TryGetResult(string json, out RequestResult result)
        {
            try
            {
                CloudScriptResult script = JsonUtility.FromJson<CloudScriptResult>(json);
                result = new RequestResult
                {
                    Success = script.success,
                    ErrorCode = script.error.code,
                    ErrorMessage = script.error.message
                };
                return result.Success;
            }
            catch (Exception e)
            {
                result = new RequestResult
                {
                    Success = false,
                    ErrorCode = (int)ErrorCode.Unknown,
                    ErrorMessage = "Error parse data\n" + e.Message
                };
                return false;
            }
        }
        
        [Serializable]
        struct CloudScriptResult
        {
            public bool success;
            public CloudScriptError error;
        }

        [Serializable]
        struct CloudScriptError
        {
            public int code;
            public string message;
        }
    }
}

