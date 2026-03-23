using _Games.CloudAPI.Model;
using _Games.CloudAPI.PlayFab;
using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI
{
    public static class CloudAPIUtils
    {
        private static LoginSessionResult session;
        private static ILogin login = new PlayFabLogin();
        private static IDataSaveLoad saveLoad = new PlayFabDataSaveLoad();
        private static ILeaderboard leaderboard = new PlayFabLeaderboard();

        public static async UniTask<(RequestResult result, LoginSessionResult session)> LoginWithId(string userId)
        {
            var result = await login.LoginWithId(userId);
            session = result.session;
            return result;
        }

        public static UniTask<RequestResult> Logout()
        {
            return login.Logout();
        }

        public static UniTask<RequestResult> SaveData(params IObjectData[] clients)
        {
            return saveLoad.SaveData(session, clients);
        }

        public static UniTask<(RequestResult result, IObjectData[] objects)> GetData()
        {
            return saveLoad.GetData(session);
        }

        public static UniTask<RequestResult> UpdateLeaderBoard(IObjectData data)
        {
            return leaderboard.UpdateLeaderBoard(session, data);
        }
    }
}