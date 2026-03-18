using System;
using _Cloud.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

namespace _Cloud.Playfab
{
    public class PlayFabLogin : ICloudLogin
    {
        public async UniTask<(ResultArg result, LoginSessionData session)> LoginCustomId(string customId)
        {
            var login = await LoginWithParameter(customId, false);
            if (login.result.Result)
            {
                return login;
            }
            else
            {
                PlayFabErrorCode errorCode = (PlayFabErrorCode)login.result.ErrorCode;
                switch (errorCode)
                {
                    case PlayFabErrorCode.AccountNotFound:
                        return await LoginWithParameter(customId, true);
                    default:
                        return login;
                }
            }
        }

        UniTask<(ResultArg result, LoginSessionData session)> LoginWithParameter(string customId, bool createAccount)
        {
            var tcs = new UniTaskCompletionSource<(ResultArg, LoginSessionData)>();

            ResultArg result = new ResultArg { Method = "PlayFab" };
            LoginSessionData session = new LoginSessionData();

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

                    result.Result = true;

                    tcs.TrySetResult((result, session));
                },
                error =>
                {
                    result.Result = false;
                    result.ErrorMessage = error.ErrorMessage;
                    result.ErrorCode = (int)error.Error;

                    tcs.TrySetResult((result, session));
                }
            );

            return tcs.Task;
        }
    }
}