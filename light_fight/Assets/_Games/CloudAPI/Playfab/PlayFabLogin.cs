using System;
using CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

namespace CloudAPI.Playfab
{
    public class PlayFabLogin : ICloudLogin
    {
        public async UniTask<(RequestResult result, LoginSessionData session)> LoginCustomId(string customId)
        {
            var login = await LoginWithParameter(customId, false);
            if (login.result.Success)
            {
                return login;
            }
            else
            {
                var errorCode = (PlayFabErrorCode)login.result.ErrorCode;
                switch (errorCode)
                {
                    case PlayFabErrorCode.AccountNotFound:
                        return await LoginWithParameter(customId, true);
                    default:
                        return login;
                }
            }
        }

        UniTask<(RequestResult result, LoginSessionData session)> LoginWithParameter(string customId, bool createAccount)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, LoginSessionData)>();

            var result = new RequestResult { Method = "PlayFab" };
            var session = new LoginSessionData();

            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest
                {
                    CustomId = customId,
                    CreateAccount = createAccount
                },
                success =>
                {
                    session.UserId = success.PlayFabId;
                    session.IsNewPlayer = success.NewlyCreated;
                    session.LoginTime = success.LastLoginTime ?? DateTime.UtcNow;
                    session.AuthenticationContext = success.AuthenticationContext;
                    result.Success = true;
                        
                    tcs.TrySetResult((result, session));
                },
                error =>
                {
                    result.Success = false;
                    result.ErrorMessage = error.ErrorMessage;
                    result.ErrorCode = (int)error.Error;

                    tcs.TrySetResult((result, session));
                }
            );

            return tcs.Task;
        }
    }
}