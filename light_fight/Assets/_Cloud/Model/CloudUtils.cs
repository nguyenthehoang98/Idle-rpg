using _Cloud.Playfab;
using Cysharp.Threading.Tasks;

namespace _Cloud.Model
{
    public static class CloudUtils
    {
        private static LoginSessionData sessionData;
        private static readonly ICloudSaveLoad CloudSaveLoad = new PlayFabSaveLoad();
        private static readonly ICloudLogin CloudLogin = new PlayFabLogin();

        public static async UniTask<(RequestResult result, LoginSessionData session)> LoginCustomId(string customId)
        {
            (RequestResult result, LoginSessionData session) result = await CloudLogin.LoginCustomId(customId);
            sessionData = result.session;
            return result;
        }

        public static UniTask<RequestResult> SaveObjectAsync(CloudObjectData objectData)
        {
            return CloudSaveLoad.SaveObjectAsync(sessionData, objectData);
        }

        public static UniTask<(RequestResult, CloudObjectData)> GetObjectsAsync(string title)
        {
            return CloudSaveLoad.GetObjectsAsync(sessionData, title);
        }
    }
}