using System.Collections.Generic;
using _Cloud.Model;
using _Cloud.Playfab;
using PlayFab.Internal;
using UnityEngine;

public class PlayFabExecute : MonoBehaviour
{
    public string customId = "Absadsa132412iu3o12";
    public bool useDeviceId = false;

    async void Start()
    {
        string userId = useDeviceId ? SystemInfo.deviceUniqueIdentifier : customId;
        (RequestResult result, LoginSessionData session) login = await CloudUtils.LoginCustomId(userId);
        if (login.result.Success)
        {
            Debug.Log("Login successful: " + login.session.UserId + ", new_player: " + login.session.IsNewPlayer);
        }
        else
        {
            Debug.Log("Login failed: " + login.result.ErrorMessage + ", Code: " + login.result.ErrorCode);
        }

        /*if (login.result.Success)
        {
            ObjectData[] objects = new ObjectData[]
            {
                new ObjectData() { Name = "Name", Object = "Nguyen The Hoang" },
                new ObjectData() { Name = "Old", Object = 28 }
            };
            CloudObjectData objectData = new CloudObjectData
            {
                Title = PlayFabUtils.TYPE_PLAYER_ACCOUNT,
                Objects = objects,
            };
            
            RequestResult saveResult = await CloudUtils.SaveObjectAsync(objectData);
            if (saveResult.Success)
            {
                Debug.Log("Save successful");
            }
            else
            {
                Debug.Log("Save failed: " + saveResult.ErrorMessage + ", Code: " + saveResult.ErrorCode);
            }
        }*/

        if (login.result.Success)
        {
            (RequestResult, CloudObjectData) getResult = await CloudUtils.GetObjectsAsync(PlayFabUtils.TYPE_PLAYER_ACCOUNT);
            RequestResult result = getResult.Item1;
            if (result.Success)
            {
                Debug.Log("Save successful: " + JsonUtility.ToJson(getResult.Item2));
            }
            else
            {
                Debug.Log("Save failed: " + result.ErrorMessage + ", Code: " + result.ErrorCode);
            }
        }
    }
}
