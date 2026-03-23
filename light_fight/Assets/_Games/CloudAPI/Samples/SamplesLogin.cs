using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesLogin : MonoBehaviour
    {
        private async void Start()
        {
            var userId = SystemInfo.deviceUniqueIdentifier;
            var loginResult = await CloudAPIUtils.LoginWithId(userId);
            if (!loginResult.result.success)
            {
                Debug.LogError("login failed. " + loginResult.result.message + ". Code: " + loginResult.result.errorCode);
                return;
            }
            else
            {
                Debug.Log("login success");
            }
        }
    }
}