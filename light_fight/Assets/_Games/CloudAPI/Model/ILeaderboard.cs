using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface ILeaderboard
    {
        UniTask<RequestResult> UpdateLeaderBoard(LoginSessionResult session, IObjectData data);
        
        UniTask<(RequestResult result, PlayerRankResult rankResult)> JoinLeaderboard(LoginSessionResult session);

        UniTask<(RequestResult result, LeaderboardResult leaderboard)> GetLeaderboardData(LoginSessionResult session, LeaderBoardRequest request);

        UniTask<(RequestResult result, FindOpponentResult opponent)> FindOpponent(LoginSessionResult session);
    }
}