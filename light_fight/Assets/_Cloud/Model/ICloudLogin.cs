using Cysharp.Threading.Tasks;

namespace _Cloud.Model
{
    public interface ICloudLogin
    {
        // Sẽ phải thêm method GG:FB...
        UniTask<(ResultArg result, LoginSessionData session)> LoginCustomId(string customId);
    }
}