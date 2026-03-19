using CloudAPI.Model;
using CloudAPI.Playfab;
using Cysharp.Threading.Tasks;

namespace CloudAPI.Utils
{
    public static class CloudUtils
    {
        private static LoginSessionData sessionData;
        private static readonly ICloudSaveLoad SaveLoad = new PlayFabSaveLoad();
        private static readonly ICloudLogin Login = new PlayFabLogin();
        private static readonly ICloudLeaderboard Leaderboard = new PlayFabLeaderboard();

        public static async UniTask<(RequestResult result, LoginSessionData session)> LoginCustomId(string customId)
        {
            (RequestResult result, LoginSessionData session) result = await Login.LoginCustomId(customId);
            sessionData = result.session;
            return result;
        }

        public static UniTask<RequestResult> SaveObjectAsync(CloudObjectData objectData)
        {
            return SaveLoad.SaveObjectAsync(sessionData, objectData);
        }

        public static UniTask<(RequestResult, CloudObjectData)> GetObjectsAsync(string title)
        {
            return SaveLoad.GetObjectsAsync(sessionData, title);
        }

        public static UniTask<RequestResult> UpdateLeaderboard(StatisticData[] statisticsData)
        {
            return Leaderboard.UpdateLeaderboard(sessionData, statisticsData);
        }

        public static UniTask<(RequestResult, LeaderboardData)> GetLeaderboardData(LeaderboardRequestData requestData)
        {
            return Leaderboard.GetLeaderboardData(sessionData, requestData);
        }
    }
}