using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface IDataSaveLoad
    {
        UniTask<RequestResult> SaveData(LoginSessionResult session, KeyObjectData[] clients);
        UniTask<(RequestResult, KeyObjectData[])> GetData(LoginSessionResult session);
    }
}