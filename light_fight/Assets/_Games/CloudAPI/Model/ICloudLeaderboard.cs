using Cysharp.Threading.Tasks;

namespace CloudAPI.Model
{
    interface ICloudLeaderboard
    {
        UniTask<RequestResult> UpdateStatistics(LoginSessionData session, StatisticData[] statisticsData);
    }
}