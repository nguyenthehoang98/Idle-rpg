using System.Text;
using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesLoad : MonoBehaviour
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

            var getResult = await CloudAPIUtils.GetData();
            if (getResult.result.success)
            {
                StringBuilder sb = new StringBuilder();
                if (CloudConverter.TryGetObject(getResult.objects, out PlayerProgress playerProgress))
                {
                    sb.AppendLine("PlayerProgress: " + JsonUtility.ToJson(playerProgress));
                }

                if (CloudConverter.TryGetObject(getResult.objects, out PlayerEquipment playerEquipment))
                {
                    sb.AppendLine("PlayerEquipment: " + JsonUtility.ToJson(playerEquipment));
                }
                
                Debug.Log("get data success.\n" + sb);
            }
            else
            {
                Debug.LogError("get data failed. " + getResult.result.message + ". Code: " + getResult.result.errorCode);
            }
        }
    }
}