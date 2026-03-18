using _Cloud.Model;
using UnityEngine;

public class Login : MonoBehaviour
{
    public string customId = "Absadsa132412iu3o12";
    public bool useDeviceId = false;

    async void Start()
    {
        string userId = useDeviceId ? SystemInfo.deviceUniqueIdentifier : customId;
        var login = await CloudUtils.GetCloudLogin().LoginCustomId(userId);
        if (login.result.Result)
        {
            Debug.Log("Login successful: " + login.session.UserId + ", new_player: " + login.session.IsNewPlayer);
        }
        else
        {
            Debug.Log("Login failed: " + login.result.ErrorMessage + ", Code: " + login.result.ErrorCode);
        }
    }
}
