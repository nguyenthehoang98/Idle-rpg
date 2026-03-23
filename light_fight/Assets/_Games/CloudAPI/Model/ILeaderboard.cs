using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface ILeaderboard
    {
        UniTask<RequestResult> UpdateLeaderBoard(LoginSessionResult session, KeyObjectData data);

        UniTask<(RequestResult, LeaderboardResult)> GetLeaderboardData(LoginSessionResult session, LeaderBoardRequest request);

        UniTask<(RequestResult, FindOpponentResult)> FindOpponents(LoginSessionResult session);
    }
}