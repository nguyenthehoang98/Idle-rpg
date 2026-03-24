using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface ILeaderboard
    {
        UniTask<(RequestResult result, PlayerRankResult rankResult)> SubmitResultBattle(LoginSessionResult session, MatchingSubmitRequest submitRequest);
        
        UniTask<(RequestResult result, PlayerRankResult rankResult)> JoinLeaderboard(LoginSessionResult session);

        // @Lấy giá trị leaderboard theo rank, tier
        UniTask<(RequestResult result, LeaderboardResult leaderboard)> GetLeaderboardData(LoginSessionResult session, LeaderBoardRequest request);

        UniTask<(RequestResult result, FindOpponentResult opponent)> FindOpponent(LoginSessionResult session);
    }
}