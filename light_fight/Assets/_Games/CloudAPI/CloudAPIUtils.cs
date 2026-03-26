using _Games.CloudAPI.Model;
using _Games.CloudAPI.PlayFab;
using _Games.CloudAPI.Unity;
using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI
{
    public static class CloudAPIUtils
    {
        private static LoginSessionResult session;

        #region Server API

        private static ILogin login = new PlayFabLogin();
        private static IDataSaveLoad cloudSaveLoad = new PlayFabDataSaveLoad();
        private static ILeaderboard cloudLeaderboard = new PlayFabLeaderboard();

        #endregion

        #region Client API

        private static UnitySaveLoad clientSaveLoad = new UnitySaveLoad();
        private static UnityLeaderboard clientLeaderboard = new UnityLeaderboard();

        #endregion

        public static async UniTask<(RequestResult result, LoginSessionResult session)> LoginWithId(string userId)
        {
            var result = await login.LoginWithId(userId);
            session = result.session;
            return result;
        }

        public static void Logout()
        {
            session = null;
            login.Logout();
        }

        public static async UniTask<RequestResult> SaveData(params IObjectData[] clients)
        {
            if (session.IsNewPlayer || clientSaveLoad.TriggerSave >= CloudConfig.CACHE_USER_DATA_SAVE_THRESHOLD)
            {
                RequestResult result = await cloudSaveLoad.SaveData(session, clients);
                if (result.success) 
                    clientSaveLoad.TriggerSave = 0;
                return result;
            }
            
            return await clientSaveLoad.SaveData(session, clients);
        }

        public static async UniTask<(RequestResult result, IObjectData[] objects)> GetData()
        {
            if (clientSaveLoad.TriggerGet >= CloudConfig.CACHE_USER_DATA_GET_THRESHOLD)
            {
                var tuple = await cloudSaveLoad.GetData(session);
                if (tuple.result.success) 
                    clientSaveLoad.TriggerSave = 0;
                return tuple;
            }
            
            return await clientSaveLoad.GetData(session);
        }

        // trước khi quít game cũng nên save 1 lần. Hoặc idle quá lâu, tránh việc server ko nhận được dữ liệu quá lâu dẫn đến sai lệch quá nhiều.
        public static async UniTask<(RequestResult result, PlayerRankResult rankResult)> SubmitResultBattle(
            MatchingSubmitRequest submitRequest)
        {
            var tuple = await clientLeaderboard.SubmitResultBattle(session, new[] { submitRequest });
            MatchingSubmitRequest[] allRequests = clientLeaderboard.GetAllSubmitRequests();
            if (allRequests.Length >= CloudConfig.CACHE_LEADERBOARD_SUBMIT_THRESHOLD)
            {
                tuple = await cloudLeaderboard.SubmitResultBattle(session, allRequests);
                if (tuple.result.success)
                    clientLeaderboard.ClearAllSubmitRequests();
                return tuple;
            }
            return tuple;
        }

        public static async UniTask<(RequestResult result, FindOpponentResult opponent)> FindOpponent()
        {
            // todo: check ở client trước
            var tuple = await clientLeaderboard.FindOpponent(session);
            if (tuple.result.success)
            {
                FindOpponentResult opponent = tuple.opponents[0];
                return (tuple.result, opponent);
            }
            
            // todo: tìm ở server
            tuple = await cloudLeaderboard.FindOpponent(session);
            if (tuple.result.success)
            {
                // todo: cập nhật client
                clientLeaderboard.SaveOpponent(tuple.opponents);
                
                // todo: tìm lại ở client
                tuple = await clientLeaderboard.FindOpponent(session);
                if (tuple.result.success)
                {
                    FindOpponentResult opponent = tuple.opponents[0];
                    return (tuple.result, opponent);
                }
            }
            
            // todo: không tìm thấy
            return (tuple.result, null);
        }

        // Gọi khi player=new hoặc season hết thời gian
        public static UniTask<(RequestResult result, PlayerRankResult rankResult)> JoinLeaderboard()
        {
            return cloudLeaderboard.JoinLeaderboard(session);   
        }

        static bool LastCallLeaderBoardFromServer = false;
        public static async UniTask<(RequestResult result, LeaderboardResult leaderboard)> GetLeaderboardData(LeaderBoardRequest request)
        {
            // chưa có logic xử lý load client & server như nào. chắc là theo số trận đấu hoặc gì gì đó
            if (clientLeaderboard.TriggerSubmit >= CloudConfig.CACHE_USER_DATA_GET_THRESHOLD && !LastCallLeaderBoardFromServer)
            {
                var tuple = await cloudLeaderboard.GetLeaderboardData(session, request);
                if (tuple.result.success)
                {
                    LastCallLeaderBoardFromServer = true;
                    clientLeaderboard.SaveLeaderboard(tuple.leaderboard);
                }
                return tuple;
            }
            
            LastCallLeaderBoardFromServer = false;
            return await clientLeaderboard.GetLeaderboardData(session, request);
        }
    }
}