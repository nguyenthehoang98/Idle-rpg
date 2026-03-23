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

                    try
                    {
                        CloudScriptResult cloudScriptResult = JsonUtility.FromJson<CloudScriptResult>(execute.FunctionResult.ToString());
                        result.Success = cloudScriptResult.success;
                        result.ErrorCode = cloudScriptResult.error.code;
                        result.ErrorMessage = cloudScriptResult.error.message;
                        Debug.Log(execute.FunctionResult.ToString());
                        tcs.TrySetResult((result, execute.FunctionResult));
                    }
                    catch (Exception e)
                    {
                        result.Success = false;
                        result.ErrorMessage = "Error parse data";
                        result.ErrorCode = (int)ErrorCode.Unknown;
                        tcs.TrySetResult((result, execute.FunctionResult));
                    }
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