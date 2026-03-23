using _Games.CloudAPI.Playfab;
using CloudAPI.Model;
using CloudAPI.Utils;
using UnityEngine;

namespace _Games.CloudClient.View
{
    public class FindOpponent : MonoBehaviour
    {
        [SerializeField] private string userId = "e4ed7e56-f50e-49ba-9fd7-fd32ae3dd104";
        
        async void Start()
        {
            RequestResult login = (await CloudUtils.LoginCustomId(userId)).result;
            if (!login.Success)
            {
                Debug.LogError($"Error login: {login.ErrorMessage}, code: {login.ErrorCode}");
                return;
            }
            var execute = await PlayFabCloudScript.ExecuteCloudScript("findOpponent", null);
            if (execute.result.Success)
            {
                Debug.Log("find opponent success.");
                Debug.Log("result: " + execute.data.ToString());
            }
            else
            {
                Debug.LogError("find opponent failed. Code:" + execute.result.ErrorCode + ". Message: " + execute.result.ErrorMessage);
            }
        }
    }
}