using System;
using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

namespace _Games.CloudAPI.PlayFab
{
    public class PlayFabLogin : ILogin
    {
        public async UniTask<(RequestResult result, LoginSessionResult session)> LoginWithId(string userId)
        {
            var login = await LoginWithParameter(userId, false);
            if (login.result.success)
            {
                return login;
            }
            else
            {
                PlayFabErrorCode errorCode = (PlayFabErrorCode)login.result.errorCode;
                switch (errorCode)
                {
                    case PlayFabErrorCode.AccountNotFound:
                        return await LoginWithParameter(userId, true);
                    default:
                        return login;
                }
            }
        }

        public UniTask<(RequestResult result, LoginSessionResult session)> LoginWithToken(string token)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<RequestResult> Logout()
        {
            PlayFabClientAPI.ForgetAllCredentials();
            RequestResult result = new RequestResult{success = true};
            return new UniTask<RequestResult>(result);
        }

        private UniTask<(RequestResult result, LoginSessionResult session)> LoginWithParameter(string customId,
            bool createAccount)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, LoginSessionResult)>();
            var result = new RequestResult();
            var session = new LoginSessionResult();
            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest
                {
                    CustomId = customId,
                    CreateAccount = createAccount
                },
                success =>
                {
                    result.success = true;
                    session.UserId = success.PlayFabId;
                    session.IsNewPlayer = success.NewlyCreated;
                    session.LoginTime = (success.LastLoginTime ?? DateTime.UtcNow);
                    session.Context = success.AuthenticationContext;
                    tcs.TrySetResult((result, session));
                },
                error =>
                {
                    result.success = false;
                    result.message = error.ErrorMessage;
                    result.errorCode = (int)error.Error;
                    tcs.TrySetResult((result, session));
                }
            );

            return tcs.Task;
        }
    }
}