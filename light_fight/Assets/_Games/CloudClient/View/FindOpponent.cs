using System;
using CloudAPI.Model;
using CloudAPI.Utils;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Serialization.Json;
using UnityEngine;

namespace _Games.CloudClient.View
{
    public class FindOpponent : MonoBehaviour
    {
        async void Start()
        {
            string userId = "B85366EFBCE25BB4";
            RequestResult login = await Login(userId);
            if (!login.Success)
            {
                Debug.LogError($"Error login: {login.ErrorMessage}, code: {login.ErrorCode}");
                return;
            }
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "findOpponent",
                FunctionParameter = new {
                    rank = 100,
                    score = 1500,
                    rankOffset = 5,
                    scoreOffset = 200
                }
            }, result => {
                Debug.Log("Opponent: " + result.FunctionResult);
                Debug.Log("Opponent: " + JsonSerialization.ToJson(result));
            }, error => {
                Debug.LogError(error.GenerateErrorReport());
            });
        }
        
        async UniTask<RequestResult> Login(string userId)
        {
            var result = await CloudUtils.LoginCustomId(userId);
            return result.result;
        }
    }
}