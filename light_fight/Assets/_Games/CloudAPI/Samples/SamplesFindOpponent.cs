using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesFindOpponent : MonoBehaviour
    {
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

            var findOpponentResult = await CloudAPIUtils.FindOpponent();
            if (findOpponentResult.result.success)
            {
                var o = findOpponentResult.opponent;
                Debug.Log($"find opponent success: MatchId: {o.MatchId}, OpponentId: {o.OpponentId}");
            }
            else
            {
                Debug.LogError("find opponent failed. " + findOpponentResult.result.message + ". Code: " + findOpponentResult.result.errorCode);
            }
        }
    }
}