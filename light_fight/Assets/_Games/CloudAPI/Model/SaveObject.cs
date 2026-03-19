using System;

namespace CloudAPI.Model
{
    [Serializable]
    public struct CloudObjectData
    {
        public string Title;
        public ObjectData[] Objects;
    }
}