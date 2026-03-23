using Cysharp.Threading.Tasks;

namespace CloudAPI.Model
{
    interface ICloudLeaderboard
    {
        UniTask<RequestResult> UpdateLeaderboard(LoginSessionData session, StatisticData statisticsData);
        UniTask<(RequestResult, LeaderboardData)> GetLeaderboardData(LoginSessionData session, LeaderboardRequestData requestData);
    }
}