using System;

namespace _Cloud.Model
{
    [Serializable]
    public struct CloudObjectData
    {
        public string Title;
        public ObjectData[] Objects;
    }

    [Serializable]
    public struct ObjectData
    {
        public string Name;
        public object Object;
    }
}