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
    }

    [Serializable]
    public struct PlayerProgress : IObjectData
    {
        public string name;
        public int level;
        public string Name() => "PlayerProgress";
    }

    [Serializable]
    public struct PlayerScore : IObjectData
    {
        public int score;

        public PlayerScore(int score)
        {
            this.score = score;
        }

        public string Name() => "Score";

        public override string ToString()
        {
            return score.ToString();
        }
    }
}