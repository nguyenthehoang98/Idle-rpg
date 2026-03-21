using System.Collections.Generic;
using CloudAPI.Model;
using CloudAPI.Utils;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class CloudClient : MonoBehaviour
{
    public PlayerData PlayerData;
    public ProgressData ProgressData;
    public StatisticData[] Statistics;

    private async void Start()
    {
        RequestResult login = await Login();
        if (!login.Success) return;

        await SaveObjectAsync(CloudConverter.CreateObjectData(CloudAPIConfig.TYPE_PLAYER_ACCOUNT, PlayerData, ProgressData));
        await GetObjectsAsync(CloudAPIConfig.TYPE_PLAYER_ACCOUNT);
        await UpdateLeaderboard(Statistics);
        await GetLeaderboardData(new LeaderboardRequestData
        {
            StartIndex = 0,
            MaxResultsCount = 10,
            StatisticName = Statistics[0].Name
        });
    }

    async UniTask<RequestResult> Login()
    {
        var result = await CloudUtils.LoginCustomId(SystemInfo.deviceUniqueIdentifier);
        return result.result;
    }

    async UniTask<RequestResult> SaveObjectAsync(CloudObjectData objectData)
    {
        var result = await CloudUtils.SaveObjectAsync(objectData);
        if (result.Success)
        {
            Debug.Log("SaveObjectAsync successfully");
        }
        else
        {
            Debug.Log("SaveObjectAsync failed: Message: " + result.ErrorMessage + ", Code: " + result.ErrorCode
            );
        }
        
        return result;
    }

    async UniTask<(RequestResult, CloudObjectData)> GetObjectsAsync(string title)
    {
        var result = await CloudUtils.GetObjectsAsync(title);
        if (result.Item1.Success)
        {
            Debug.Log("GetObjectsAsync successful");
            
            if (CloudConverter.TryGetObject(result.Item2, out PlayerData playerData))
            {
                Debug.Log("PlayerData [Name:" + playerData.Name + "] has been loaded");
            }

            if (CloudConverter.TryGetObject(result.Item2, out ProgressData progressData))
            {
                Debug.Log("ProgressData [Chapter:" + progressData.Chapter + "] has been loaded");
            }
        }
        else
        {
            Debug.Log("GetObjectsAsync failed: Message: " + result.Item1.ErrorMessage + ", Code: " + result.Item1.ErrorCode);
        }
        return (result.Item1, result.Item2);
    }

    async UniTask<RequestResult> UpdateLeaderboard(StatisticData[] statisticsData)
    {
        var result = await CloudUtils.UpdateLeaderboard(statisticsData);
        if (result.Success)
        {
            Debug.Log("UpdateStatistics successfully");
        }
        else
        {
            Debug.Log("UpdateStatistics failed: Message: " + result.ErrorMessage + ", Code: " + result.ErrorCode);
        }
        
        return result;
    }

    async UniTask<(RequestResult, LeaderboardData)> GetLeaderboardData(LeaderboardRequestData requestData)
    {
        var result = await CloudUtils.GetLeaderboardData(requestData);
        if (result.Item1.Success)
        {
            Debug.Log("GetLeaderboardData successfully");
            foreach (var entry in result.Item2.Entries)
            {
                Debug.Log($"{entry.UserId}_{entry.Index}_{entry.Stat}");
            }
        }
        else
        {
            Debug.Log("GetLeaderboardData failed: Message: " + result.Item1.ErrorMessage + ", Code: " + result.Item1.ErrorCode);
        }

        return result;
    }
}
