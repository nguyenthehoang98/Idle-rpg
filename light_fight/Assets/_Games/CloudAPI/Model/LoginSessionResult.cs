using System;

namespace _Games.CloudAPI.Model
{
    [Serializable]
    public class LoginSessionResult
    {
        public string UserId;
        public bool IsNewPlayer;
        public object Context;
        public DateTime LoginTime;
    }
}