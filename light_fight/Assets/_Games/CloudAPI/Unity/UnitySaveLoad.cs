using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.CloudAPI.Unity
{
    public class UnitySaveLoad : IDataSaveLoad
    {
        // Phần này có thể fake thời gian save chậm 1 chút = cách delay cho giống server
        public UniTask<RequestResult> SaveData(LoginSessionResult session, params IObjectData[] clients)
        {
            for (int i = 0; i < clients.Length; i++)
            {
                PlayerPrefs.SetString(CloudConverter.FormatClientKey(clients[i].Name()), JsonUtility.ToJson(clients[i]));
            }
            TriggerSave += 1;
            return new UniTask<RequestResult>(new RequestResult { success = true });
        }

        // Phần này có thể fake thời gian load chậm 1 chút = cách delay cho giống server
        public UniTask<(RequestResult result, IObjectData[] objects)> GetData(LoginSessionResult session)
        {
            RequestResult result = new RequestResult();
            string[] keys = new string[]
            {
                CloudConfig.USER_DATA_EQUIPMENT
            };
            IObjectData[] objects = new IObjectData[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (PlayerPrefs.HasKey(CloudConverter.FormatClientKey(keys[i])))
                {
                    objects[i] = new DefaultObjectData(keys[i], PlayerPrefs.GetString(CloudConverter.FormatClientKey(keys[i])));
                }
                else
                {
                    result.success = false;
                    result.errorCode = (int)ErrorCode.CLIENT_USER_DATA_ERROR_LOAD_DATA;
                    result.message = $"Error load data: {keys[i]}";
                    return new UniTask<(RequestResult result, IObjectData[] objects)>((result, null));
                }
            }

            TriggerGet += 1;
            result.success = true;
            return new UniTask<(RequestResult result, IObjectData[] objects)>((result, objects));
        }

        public int TriggerSave
        {
            get => PlayerPrefs.GetInt(CloudConverter.FormatClientKey("UserData_SaveTrigger"), 0);
            set => PlayerPrefs.SetInt(CloudConverter.FormatClientKey("UserData_SaveTrigger"), value);
        }

        public int TriggerGet
        {
            get => PlayerPrefs.GetInt(CloudConverter.FormatClientKey("UserData_GetTrigger"), 0);
            set => PlayerPrefs.SetInt(CloudConverter.FormatClientKey("UserData_GetTrigger"), value);
        }
    }
}