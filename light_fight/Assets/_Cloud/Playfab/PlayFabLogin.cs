using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    void Start()
    {
        Login();
    }

    void Login()
    {
        Debug.Log("Calling PlayFab Login...");

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                Invoke(() =>
                {
                    Debug.Log("✅ Login success!");
                });
            },
            error =>
            {
                Invoke(() =>
                {
                    Debug.LogError("❌ Login failed: " + error.GenerateErrorReport());
                });
            });
    }

    void Invoke(Action callback)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(callback);
    }
}