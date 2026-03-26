using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.CloudAPI.Unity
{
    public class UnityLeaderboard : ILeaderboard
    {
        public async UniTask<(RequestResult result, PlayerRankResult rankResult)> SubmitResultBattle(LoginSessionResult session, MatchingSubmitRequest[] requests)
        {
            for (int i = 0; i < requests.Length; i++)
            {
                SubmitResultBattle(requests[i]);
            }
            
            Debug.Log(@"Ở đây phải có logic tính +- điểm up/down rank giông server. nên có logic cập nhật lại leaderboard ở client cho giống Fake thật                ");
            return (new RequestResult { success = true }, null);
        }

        public UniTask<(RequestResult result, PlayerRankResult rankResult)> JoinLeaderboard(LoginSessionResult session)
        {
            throw new System.NotImplementedException();
        }

        public async UniTask<(RequestResult result, LeaderboardResult leaderboard)> GetLeaderboardData(LoginSessionResult session, LeaderBoardRequest request)
        {
            string key = CloudConverter.FormatClientKey(LEADERBOARD_RANK);
            if (PlayerPrefs.HasKey(key))
            {
                LeaderboardResult result = JsonUtility.FromJson<LeaderboardResult>(key);
                return (new RequestResult { success = true }, result);
            }
            else
            {
                return (new RequestResult
                {
                    success = false,
                    errorCode = (int)ErrorCode.CLIENT_GET_LEADERBOARD_EMPTY_DATA,
                    message = "Leaderboard is empty"
                }, null);
            }
        }

        public async UniTask<(RequestResult result, FindOpponentResult[] opponents)> FindOpponent(LoginSessionResult session)
        {
            string key = CloudConverter.FormatClientKey(LEADERBOARD_OPPONENT);
            if (!PlayerPrefs.HasKey(key))
            {
                RequestResult result = new RequestResult();
                result.success = false;
                result.errorCode = (int)ErrorCode.CLIENT_FIND_OPPONENT_ERROR_LOAD_DATA;
                result.message = "Failed to find opponent";
                return (result, null);
            }

            string json = PlayerPrefs.GetString(key);
            string[] splits = json.Split(',');
            List<FindOpponentResult> list = new List<FindOpponentResult>();
            for (int i = 0; i < splits.Length; i++)
            {
                try
                {
                    FindOpponentResult result = JsonUtility.FromJson<FindOpponentResult>(splits[i]);
                    list.Add(result);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (list.Count == 0)
            {
                RequestResult result = new RequestResult();
                result.success = false;
                result.errorCode = (int)ErrorCode.CLIENT_FIND_OPPONENT_EMPTY_DATA;
                result.message = "Opponent is empty";
                return (result, null);
            }
            
            FindOpponentResult opponent = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            SaveOpponent(list.ToArray());
            return (new RequestResult { success = true }, new[] { opponent });
        }

        public void SaveLeaderboard(LeaderboardResult leaderboard)
        {
            string key = CloudConverter.FormatClientKey(LEADERBOARD_RANK);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(leaderboard));
        }
        
        public void SaveOpponent(FindOpponentResult[] results)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < results.Length; i++)
            {
                sb.AppendJoin(',', JsonUtility.ToJson(results[i]));
            }

            PlayerPrefs.SetString(CloudConverter.FormatClientKey(LEADERBOARD_OPPONENT), sb.ToString());
        }

        private void SubmitResultBattle(MatchingSubmitRequest request)
        {
            List<MatchingSubmitRequest> submitRequests = GetAllSubmitRequests().ToList();
            submitRequests.Add(request);
            
            StringBuilder sb = new StringBuilder();
            foreach (var submitRequest in submitRequests)
            {
                sb.AppendJoin(',', JsonUtility.ToJson(submitRequest));
            }

            TriggerSubmit += 1;
            PlayerPrefs.SetString(CloudConverter.FormatClientKey(LEADERBOARD_SUBMIT), sb.ToString());
        }

        public MatchingSubmitRequest[] GetAllSubmitRequests()
        {
            string key = CloudConverter.FormatClientKey(LEADERBOARD_SUBMIT);
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                string[] splits = json.Split(',');
                List<MatchingSubmitRequest> list = new List<MatchingSubmitRequest>();
                foreach (var split in splits)
                {
                    try
                    {
                        MatchingSubmitRequest submitRequest = JsonUtility.FromJson<MatchingSubmitRequest>(split);
                        list.Add(submitRequest);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                return list.ToArray();
            }

            return new MatchingSubmitRequest[0];
        }

        public void ClearAllSubmitRequests()
        {
            TriggerSubmit = 0;
            PlayerPrefs.DeleteKey(CloudConverter.FormatClientKey(LEADERBOARD_SUBMIT));
        }

        public int TriggerSubmit
        {
            get => PlayerPrefs.GetInt(CloudConverter.FormatClientKey("Leaderboard_SubmitTrigger"), 0);
            set => PlayerPrefs.SetInt(CloudConverter.FormatClientKey("Leaderboard_SubmitTrigger"), value);
        }
        
        const string LEADERBOARD_OPPONENT = "Leaderboard_Opponent";
        const string LEADERBOARD_SUBMIT = "Leaderboard_Submit";
        const string LEADERBOARD_RANK = "Leaderboard_Rank";
    }
}