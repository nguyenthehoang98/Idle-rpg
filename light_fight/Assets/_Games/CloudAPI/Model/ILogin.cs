using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface ILogin
    {
        UniTask<(RequestResult result, LoginSessionResult session)> LoginWithId(string userId);
        UniTask<(RequestResult result, LoginSessionResult session)> LoginWithToken(string token);
        UniTask<RequestResult> Logout();
    }
}