using Cysharp.Threading.Tasks;

namespace CloudAPI.Model
{
    interface ICloudLogin
    {
        // Sẽ phải thêm method GG:FB...
        UniTask<(RequestResult result, LoginSessionData session)> LoginCustomId(string customId);
    }
}