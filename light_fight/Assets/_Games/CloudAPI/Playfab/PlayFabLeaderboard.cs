/*using System;
using System.Collections.Generic;
using _Games.CloudAPI.Playfab;
using CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

namespace CloudAPI.Playfab
{
    public class PlayFabLeaderboard : ICloudLeaderboard
    {
        /// <summary>
        /// Sau chuyển thành update kết quả trận đấu. với match Id
        /// </summary>
        /// <param name="session"></param>
        /// <param name="statisticsData"></param>
        /// <returns></returns>
        public async UniTask<RequestResult> UpdateLeaderboard(LoginSessionData session, StatisticData statisticsData)
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

            var execute = await PlayFabCloudScript.ExecuteCloudScript("updateScore",
                new Dictionary<string, object>
                {
                    { statisticsData.Name, statisticsData.Value }
                });
            if (execute.result.Success)
            {
                
            }
            else
            {
                tcs.TrySetResult(execute.result);
            }
            
            return tcs.Task;
        }

        public UniTask<(RequestResult, LeaderboardData)> GetLeaderboardData(LoginSessionData session, LeaderboardRequestData requestData)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, LeaderboardData)>();
            var result = new RequestResult { Method = "PlayFab" };

            string entityId = "";
            if (session.AuthenticationContext is PlayFabAuthenticationContext context)
            {
                entityId = context.EntityId;
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Invalid AuthenticationContext";
                tcs.TrySetResult((result, default));
                return tcs.Task;
            }

            GetLeaderboardRequest request = new GetLeaderboardRequest
            {
                AuthenticationContext = context,
                StatisticName = requestData.StatisticName,
                StartPosition = requestData.StartIndex,
                MaxResultsCount = requestData.MaxResultsCount,
            };

            PlayFabClientAPI.GetLeaderboard(request, success =>
            {
                LeaderboardData leaderboardData = new LeaderboardData();
                leaderboardData.NextTimeReset = success.NextReset != null
                    ? ((DateTimeOffset)success.NextReset.Value).ToUnixTimeSeconds()
                    : 0;
                leaderboardData.Entries = new LeaderboardEntryData[success.Leaderboard.Count];
                for (int i = 0; i < leaderboardData.Entries.Length; i++)
                {
                    PlayerLeaderboardEntry data = success.Leaderboard[i];
                    leaderboardData.Entries[i] = new LeaderboardEntryData
                    {
                        Profile = data.Profile,
                        Index = data.Position,
                        Stat = data.StatValue,
                        UserId = data.PlayFabId,
                        DisplayName = data.DisplayName
                    };
                }
                
                result.Success = true;
                tcs.TrySetResult((result, leaderboardData));
            }, error =>
            {
                result.Success = false;
                result.ErrorMessage = error.ErrorMessage;
                result.ErrorCode = (int)error.Error;
                tcs.TrySetResult((result, default));
            });
            
            return tcs.Task;
        }

        [Serializable]
        struct FindOpponentResult
        {
            public string PlayFabId;
            public int StatValue;
            public int Position;
        }
    }
}*/