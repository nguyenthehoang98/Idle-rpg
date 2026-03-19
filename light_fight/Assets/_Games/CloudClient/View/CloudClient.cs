using System;
using System.Collections.Generic;
using CloudAPI.Model;
using CloudAPI.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CloudClient : MonoBehaviour
{
    public PlayerData PlayerData;
    public ProgressData ProgressData;

    private async void Start()
    {
        RequestResult login = await Login();
        if (!login.Success) return;

        await Save(CloudConverter.CreateObjectData(CloudAPIConfig.TYPE_PLAYER_ACCOUNT, PlayerData, ProgressData));
        await GetAll(CloudAPIConfig.TYPE_PLAYER_ACCOUNT);
    }

    async UniTask<RequestResult> Login()
    {
        var result = await CloudUtils.LoginCustomId(SystemInfo.deviceUniqueIdentifier);
        return result.result;
    }

    async UniTask<RequestResult> Save(CloudObjectData objectData)
    {
        List<string> titles = new List<string>();
        foreach (var data in objectData.Objects)
            titles.Add(data.Name);

        var result = await CloudUtils.SaveObjectAsync(objectData);
        if (result.Success)
        {
            Debug.Log("Saved successfully: " + string.Join(',', titles));
        }
        else
        {
            Debug.Log("Save failed: " + string.Join(',', titles) +
                      "\nMessage: " + result.ErrorMessage + ", Code: " + result.ErrorCode
            );
        }
        
        return result;
    }

    async UniTask<(RequestResult, CloudObjectData)> GetAll(string title)
    {
        var result = await CloudUtils.GetObjectsAsync(title);
        if (result.Item1.Success)
        {
            Debug.Log("Save successful");
            
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
            Debug.Log("Save failed: " + title +
                      "\nMessage: " + result.Item1.ErrorMessage + ", Code: " + result.Item1.ErrorCode
            );
        }
        return (result.Item1, result.Item2);
    }
}
