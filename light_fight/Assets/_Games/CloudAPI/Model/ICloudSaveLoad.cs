using Cysharp.Threading.Tasks;

namespace CloudAPI.Model
{
    interface ICloudSaveLoad
    {
        UniTask<RequestResult> SaveObjectAsync(LoginSessionData session, CloudObjectData objectData);
        UniTask<(RequestResult, CloudObjectData)> GetObjectsAsync(LoginSessionData session, string title);
    }
}