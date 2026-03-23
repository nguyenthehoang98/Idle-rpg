using System;

namespace _Games.CloudAPI.Model
{
    [Serializable]
    public struct RequestResult
    {
        public bool success;
        public string message;
        public int errorCode;
    }
}