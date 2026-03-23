using Cysharp.Threading.Tasks;

namespace _Games.CloudAPI.Model
{
    internal interface IDataSaveLoad
    {
        UniTask<RequestResult> SaveData(LoginSessionResult session, ClientData[] clients);
        UniTask<(RequestResult, ClientData[])> GetData(LoginSessionResult session);
    }
}