using System;
using _KIT.Utils;
using CloudAPI.Model;
using CloudAPI.Utils;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace _Games.CloudClient.View
{
    public class FakeCloudFunction : MonoBehaviour
    {
        [SerializeField] private string[] usersId;

        private async void Start()
        {
            Application.runInBackground = true;
            for (int i = 0; i < usersId.Length; i++)
            {
                int score = RandomUtils.Range(100, 100000);
                UploadData(usersId[i], score);
                await UniTask.WaitForSeconds(10);
            }
        }

        async void UploadData(string userId, int score)
        {
            RequestResult login = await Login(userId);
            if (!login.Success)
            {
                Debug.LogError($"Error login: {login.ErrorMessage}, code: {login.ErrorCode}");
                return;
            }

            UpdateLeaderboard(new StatisticData { Name = "Score", Value = score });
        }

        async UniTask<RequestResult> Login(string userId)
        {
            var result = await CloudUtils.LoginCustomId(userId);
            return result.result;
        }
        
        void UpdateLeaderboard(StatisticData statisticsData)
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "updateLeaderboard",
                FunctionParameter = new { value = statisticsData.Value }
            }, result =>
            {
                Debug.Log("Updated!");
            }, error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }
    }
}