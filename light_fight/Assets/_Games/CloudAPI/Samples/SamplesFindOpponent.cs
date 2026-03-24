using _Games.CloudAPI.Model;
using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesFindOpponent : MonoBehaviour
    {
        [SerializeField] private bool win;
        
        private async void Start()
        {
            var userId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log(userId);
            var loginResult = await CloudAPIUtils.LoginWithId(userId);
            if (!loginResult.result.success)
            {
                Debug.LogError("login failed. " + loginResult.result.message + ". Code: " + loginResult.result.errorCode);
                return;
            }
            
            var joinLeaderBoardResult = await CloudAPIUtils.JoinLeaderboard();
            if (joinLeaderBoardResult.result.success)
            {
                var o = joinLeaderBoardResult.rankResult;
                Debug.Log($"Successfully joined leaderboard: Season: {o.Season}, Rank: {o.Rank}, Tier: {o.Tier}, Score: {o.Score}, NewPlayer: {o.IsNew}");
            }
            else
            {
                Debug.LogError($"Error join leaderboard: {joinLeaderBoardResult.result.message}");
                return;
            }

            var findOpponentResult = await CloudAPIUtils.FindOpponent();
            if (findOpponentResult.result.success)
            {
                var o = findOpponentResult.opponent;
                Debug.Log($"find opponent success: MatchId: {o.MatchId}, OpponentId: {o.OpponentId}");
            }
            else
            {
                Debug.LogError("find opponent failed. " + findOpponentResult.result.message + ". Code: " + findOpponentResult.result.errorCode);
                return;
            }

            var submitResult = await CloudAPIUtils.SubmitResultBattle(new MatchingSubmitRequest
            {
                MatchId = findOpponentResult.opponent.MatchId,
                IsWin = win
            });
            if (submitResult.result.success)
            {
                var o = submitResult.rankResult;
                Debug.Log($"submit battle success: Season: {o.Season}, Rank: {o.Rank}, Tier: {o.Tier}, Score: {o.Score}");
            }
            else
            {
                Debug.LogError("submit battle failed. " + submitResult.result.message + ". Code: " + submitResult.result.errorCode);
            }

            CloudAPIUtils.Logout();
        }
    }
}