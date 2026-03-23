using System;
using System.Collections.Generic;
using _Games.CloudAPI.Playfab;
using _KIT.Utils;
using CloudAPI.Model;
using CloudAPI.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.CloudClient.View
{
    public class FakeCloudFunction : MonoBehaviour
    {
        [SerializeField] private string[] usersId;

        private void OnValidate()
        {
            if (usersId.Length > 0) return;
            int count = 20;
            usersId = new string[count];
            for (int i = 0; i < count; i++)
            {
                usersId[i] = Guid.NewGuid().ToString();
            }
        }

        private async void Start()
        {
            Application.runInBackground = true;
            for (int i = 0; i < usersId.Length; i++)
            {
                int score = RandomUtils.Range(100, 1000);
                UploadData(usersId[i], score);
                await UniTask.WaitForSeconds(10);
            }
        }

        async void UploadData(string userId, int score)
        {
            RequestResult login = (await CloudUtils.LoginCustomId(userId)).result;
            if (!login.Success)
            {
                Debug.LogError($"Error login: {login.ErrorMessage}, code: {login.ErrorCode}");
                return;
            }
            var execute = await PlayFabCloudScript.ExecuteCloudScript("updateScore", new Dictionary<string, object>
            {
                { "Score", score }
            });
            if (execute.result.Success)
            {
                Debug.Log("update leaderboard success.");
            }
            else
            {
                Debug.LogError("update leaderboard failed. Code:" + execute.result.ErrorCode + ". Message: " + execute.result.ErrorMessage);
            }
        }
    }
}