using System;
using System.Collections.Generic;
using _Games.CloudAPI.Model;

namespace _Games.CloudAPI.Samples
{
    [Serializable]
    public struct PlayerEquipment : IObjectData
    {
        public List<int> equipments;
       
        public string Name() => "PlayerEquipment";

        public object Value()
        {
            return this;
        }
    }

    [Serializable]
    public struct PlayerProgress : IObjectData
    {
        public string name;
        public int level;
        public string Name() => "PlayerProgress";

        public object Value()
        {
            return this;
        }
    }
}