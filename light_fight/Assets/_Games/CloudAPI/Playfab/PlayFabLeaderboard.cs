using System.Collections.Generic;
using CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

namespace CloudAPI.Playfab
{
    public class PlayFabLeaderboard : ICloudLeaderboard
    {
        public UniTask<RequestResult> UpdateStatistics(LoginSessionData session, StatisticData[] statisticsData)
        {
            var tcs = new UniTaskCompletionSource<RequestResult>();
            var result = new RequestResult { Method = "PlayFab" };

            if (session.AuthenticationContext is PlayFabAuthenticationContext context)
            {
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Invalid AuthenticationContext";
                tcs.TrySetResult(result);
                return tcs.Task;
            }

            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
            {
                Statistics = ConvertToSetStatistic(statisticsData),
                AuthenticationContext = context
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request, success =>
            {
                result.Success = true;
                tcs.TrySetResult(result);
            }, error =>
            {
                result.Success = false;
                result.ErrorMessage = error.ErrorMessage;
                result.ErrorCode = (int)error.Error;
                tcs.TrySetResult(result);
            });
            return tcs.Task;
        }
        
        List<StatisticUpdate> ConvertToSetStatistic(StatisticData[] statisticsData)
        {
            List<StatisticUpdate> list = new List<StatisticUpdate>();
            foreach (var data in statisticsData)
            {
                list.Add(new StatisticUpdate
                {
                    StatisticName = data.Name,
                    Value = data.Value
                });
            }

            return list;
        }
    }
}