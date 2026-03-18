using System.Collections.Generic;
using _Cloud.Model;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.Json;
using UnityEngine;
using EntityKey = PlayFab.ClientModels.EntityKey;

namespace _Cloud.Playfab
{
    public class PlayFabSaveLoad : ICloudSaveLoad
    {
        /// <summary>
        /// Với PlayFab thì saveObject có thể gửi từng đối tượng được -> nhẹ hơn, nhanh hơn, giảm chi phí
        /// Khi override thì cấu trúc bắt buộc giống nhau -> tránh mất dữ liệu
        /// </summary>
        public UniTask<RequestResult> SaveObjectAsync(LoginSessionData session, CloudObjectData objectData)
        {
            var tcs = new UniTaskCompletionSource<RequestResult>();
            var result = new RequestResult { Method = "PlayFab" };

            string entityId = "";
            if (session.AuthenticationContext is PlayFabAuthenticationContext context)
            {
                entityId = context.EntityId;
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Invalid AuthenticationContext";
                tcs.TrySetResult(result);
                return tcs.Task;
            }

            SetObjectsRequest request = new SetObjectsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey
                {
                    Id = entityId, Type = objectData.Title
                },
                Objects = ConvertToSetObjects(objectData),
                AuthenticationContext = context
            };

            PlayFabDataAPI.SetObjects(request, success =>
            {
                result.Success = true;
                tcs.TrySetResult(result);
            }, error =>
            {
                result.Success = false;
                result.ErrorMessage = error.ErrorMessage;
                result.ErrorCode = (int)error.Error;
                tcs.TrySetResult(result);
            });

            return tcs.Task;
        }
        
        /// <summary>
        /// Với PlayFab thì getObject băt́ buộc lấy cả 1 cục. Có thể tối ưu bằng cách load -> cache
        /// </summary>
        public UniTask<(RequestResult, CloudObjectData)> GetObjectsAsync(LoginSessionData session, string title)
        {
            var tcs = new UniTaskCompletionSource<(RequestResult, CloudObjectData)>();
            var result = new RequestResult { Method = "PlayFab" };

            string entityId = "";
            if (session.AuthenticationContext is PlayFabAuthenticationContext context)
            {
                entityId = context.EntityId;
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Invalid AuthenticationContext";
                tcs.TrySetResult((result, default));
                return tcs.Task;
            }

            GetObjectsRequest request = new GetObjectsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = title },
                AuthenticationContext = context
            };
            PlayFabDataAPI.GetObjects(request, success =>
            {
                CloudObjectData objectData = new CloudObjectData
                {
                    Title = title,
                    Objects = new ObjectData[0]
                };
                if (success.Objects != null)
                {
                    ObjectData[] objects = new ObjectData[success.Objects.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, ObjectResult> kv in success.Objects)
                    {
                        objects[i] = new ObjectData { Name = kv.Key, Object = kv.Value.DataObject };
                        i++;
                    }
                    objectData.Objects = objects;
                }
                result.Success = true;
                tcs.TrySetResult((result, objectData));
            }, error =>
            {
                result.Success = false;
                result.ErrorMessage = error.ErrorMessage;
                result.ErrorCode = (int)error.Error;
                tcs.TrySetResult((result, default));
            });

            return tcs.Task;
        }

        List<SetObject> ConvertToSetObjects(CloudObjectData objectData)
        {
            List<SetObject> list = new List<SetObject>();
            foreach (var o in objectData.Objects)
            {
                list.Add(new SetObject
                {
                    ObjectName = o.Name,
                    DataObject = o.Object
                });
            }

            return list;
        }
    }
}