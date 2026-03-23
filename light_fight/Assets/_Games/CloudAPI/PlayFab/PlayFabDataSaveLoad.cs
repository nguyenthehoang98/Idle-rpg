using System.Collections.Generic;
using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.DataModels;

namespace _Games.CloudAPI.PlayFab
{
    public class PlayFabDataSaveLoad : IDataSaveLoad
    {
        public UniTask<RequestResult> SaveData(LoginSessionResult session, KeyObjectData[] clients)
        {
            var tcs = new UniTaskCompletionSource<RequestResult>();
            var result = new RequestResult();
            string entityId = "";
            if (session.Context is PlayFabAuthenticationContext context)
            {
                entityId = context.EntityId;
            }
            else
            {
                result.success = false;
                result.errorCode = (int)PlayFabErrorCode.NotAuthenticated;
                result.message = "Invalid AuthenticationContext";
                tcs.TrySetResult(result);
                return tcs.Task;
            }

            SetObjectsRequest request = new SetObjectsRequest
            {
                Entity =  new EntityKey
                {
                    Id = entityId, Type = "title_player_account"
                },
                Objects = ConvertToSetObjects(clients),
                AuthenticationContext = context
            };

            PlayFabDataAPI.SetObjects(request, success =>
            {
                result.success = true;
                tcs.TrySetResult(result);
            }, error =>
            {
                result.success = false;
                result.message = error.ErrorMessage;
                result.errorCode = (int)error.Error;
                tcs.TrySetResult(result);
            });

            return tcs.Task;
        }

        public UniTask<(RequestResult, KeyObjectData[])> GetData(LoginSessionResult session)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, KeyObjectData[])>();
            var result = new RequestResult();

            string entityId = "";
            if (session.Context is PlayFabAuthenticationContext context)
            {
                entityId = context.EntityId;
            }
            else
            {
                result.success = false;
                result.errorCode = (int)PlayFabErrorCode.NotAuthenticated;
                result.message = "Invalid AuthenticationContext";
                return tcs.Task;
            }

            GetObjectsRequest request = new GetObjectsRequest
            {
                Entity =  new EntityKey
                {
                    Id = entityId, Type = "title_player_account"
                },
                AuthenticationContext = context
            };
            PlayFabDataAPI.GetObjects(request, success =>
            {
                KeyObjectData[] clients = new KeyObjectData[0];
                if (success.Objects != null)
                {
                    clients = new KeyObjectData[success.Objects.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, ObjectResult> kv in success.Objects)
                    {
                        clients[i] = new KeyObjectData { Name = kv.Key, Object = kv.Value.DataObject };
                        i++;
                    }
                }
                result.success = success.Objects != null;
                tcs.TrySetResult((result, clients));
            }, error =>
            {
                result.success = false;
                result.message = error.ErrorMessage;
                result.errorCode = (int)error.Error;
                tcs.TrySetResult((result, new KeyObjectData[0]));
            });

            return tcs.Task;
        }

        private static List<SetObject> ConvertToSetObjects(KeyObjectData[] clients)
        {
            List<SetObject> list = new List<SetObject>();
            foreach (var o in clients)
            {
                list.Add(new SetObject { ObjectName = o.Name, DataObject = o.Object });
            }

            return list;
        }
    }
}