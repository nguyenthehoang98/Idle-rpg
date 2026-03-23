using System.Collections.Generic;
using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesSave : MonoBehaviour
    {
        private async void Start()
        {
            var userId = SystemInfo.deviceUniqueIdentifier;
            var loginResult = await CloudAPIUtils.LoginWithId(userId);
            if (!loginResult.result.success)
            {
                Debug.LogError(
                    "login failed. " + loginResult.result.message + ". Code: " + loginResult.result.errorCode);
                return;
            }

            PlayerProgress progressData = new PlayerProgress
            {
                name = "hoang nguyen", level = 10
            };
            PlayerEquipment equipmentData = new PlayerEquipment
            {
                equipments = new List<int> { 2, 3, 5, 6, 7 }
            };
            var saveResult = await CloudAPIUtils.SaveData(progressData, equipmentData);
            if (saveResult.success)
            {
                Debug.Log("save data success");
            }
            else
            {
                Debug.LogError("save data failed. " + saveResult.message + ". Code: " + saveResult.errorCode);
            }
        }
    }
}