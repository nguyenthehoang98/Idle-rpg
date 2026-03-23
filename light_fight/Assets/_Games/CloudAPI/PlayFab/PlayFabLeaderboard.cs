using System;
using System.Collections.Generic;
using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Serialization.Json;
using UnityEngine;

namespace _Games.CloudAPI.PlayFab
{
    public class PlayFabLeaderboard : ILeaderboard
    {
        public async UniTask<RequestResult> UpdateLeaderBoard(LoginSessionResult session, IObjectData data)
        {
            var result = new RequestResult();
            if (session.Context is PlayFabAuthenticationContext context)
            {
            }
            else
            {
                result.success = false;
                result.errorCode = (int)PlayFabErrorCode.NotAuthenticated;
                result.message = "Invalid AuthenticationContext";
                return result;
            }

            var execute = await ExecuteScript("updateLeaderboard", new Dictionary<string, object>
            {
                { data.Name(), data.ToString() }
            });
            if (execute.Item1.success)
            {
                result.success = true;
                return result;
            }
            else
            {
                return execute.Item1;
            }
        }

        public UniTask<(RequestResult result, LeaderboardResult leaderboard)> GetLeaderboardData(LoginSessionResult session, LeaderBoardRequest request)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, LeaderboardResult)>();
            var result = new RequestResult();
            if (session.Context is PlayFabAuthenticationContext context)
            {
            }
            else
            {
                result.success = false;
                result.errorCode = (int)PlayFabErrorCode.NotAuthenticated;
                result.message = "Invalid AuthenticationContext";
                tcs.TrySetResult((result, null));
                return tcs.Task;
            }

            PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
            {
                AuthenticationContext = context,
                StatisticName = request.StatisticName,
                StartPosition = request.StartIndex,
                MaxResultsCount = request.MaxResultsCount,
            }, success =>
            {
                LeaderboardResult leaderboardResult = new LeaderboardResult
                {
                    NextTimeReset = success.NextReset,
                    Entries = new LeaderboardEntry[success.Leaderboard.Count]
                };
                for (int i = 0; i < success.Leaderboard.Count; i++)
                {
                    PlayerLeaderboardEntry data = success.Leaderboard[i];
                    leaderboardResult.Entries[i] = new LeaderboardEntry
                    {
                        Index = data.Position,
                        Stat = data.StatValue,
                        Profile = data.Profile,
                        UserId = data.PlayFabId,
                        DisplayName = data.DisplayName
                    };
                }
                
                result.success = true;
                tcs.TrySetResult((result, leaderboardResult));
            }, error =>
            {
                result.success = false;
                result.message = error.ErrorMessage;
                result.errorCode = (int)error.Error;
                tcs.TrySetResult((result, null));
            });
            
            return tcs.Task;
        }

        public async UniTask<(RequestResult result, FindOpponentResult opponent)> FindOpponent(LoginSessionResult session)
        {
            var result = new RequestResult();
            if (session.Context is PlayFabAuthenticationContext context)
            {
            }
            else
            {
                result.success = false;
                result.errorCode = (int)PlayFabErrorCode.NotAuthenticated;
                result.message = "Invalid AuthenticationContext";
                return (result, null);
            }
            
            var execute = await ExecuteScript("findOpponent", null);
            if (execute.Item1.success)
            {
                try
                {
                    JsonObject json = JsonSerialization.FromJson<JsonObject>(execute.Item2.ToString());
                    if (json.TryGetValue("PlayFabId", out object playFabId) &&
                        json.TryGetValue("Position", out object position))
                    {
                        string userId = playFabId.ToString();
                        int index = int.Parse(position.ToString());
                        result.success = true;
                        return (result, new FindOpponentResult { UserId = userId, Index = index });
                    }
                    else
                    {
                        result.errorCode = (int)ErrorCode.Unknown;
                        result.message = "Invalid PlayFabId or Position";
                        result.success = false;
                        return (result, null);
                    }
                }
                catch (Exception e)
                {
                    result.errorCode = (int)ErrorCode.Unknown;
                    result.message = e.Message;
                    result.success = false;
                    return (result, null);
                }
            }

            return (execute.Item1, null);
        }

        UniTask<(RequestResult, object)> ExecuteScript(string functionName, Dictionary<string, object> objects)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, object)>();
            var result = new RequestResult();
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                {
                    FunctionName = functionName,
                    FunctionParameter = objects
                },
                execute =>
                {
#if DEBUG
                    Debug.Log($"{functionName}: " + JsonSerialization.ToJson(execute));
#endif
                    if (execute.Error != null)
                    {
                        result.success = false;
                        result.message = execute.Error.Message;
                        result.errorCode = (int)ErrorCode.Unknown;
                        tcs.TrySetResult((result, default));
                    }
                    else
                    {
                        result.success = true;
                        tcs.TrySetResult((result, execute.FunctionResult));                        
                    }
                },
                error =>
                {
                    result.success = false;
                    result.message = error.ErrorMessage;
                    result.errorCode = (int)error.Error;
                    tcs.TrySetResult((result, default));
                });

            return tcs.Task;
        }
    }
}