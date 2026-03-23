using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface IDataSaveLoad
    {
        UniTask<RequestResult> SaveData(LoginSessionResult session, params IObjectData[] clients);
        UniTask<(RequestResult result, IObjectData[] objects)> GetData(LoginSessionResult session);
    }
}