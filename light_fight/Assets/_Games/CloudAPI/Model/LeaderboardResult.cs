using System;
using _Games.CloudAPI.Samples;

namespace _Games.CloudAPI.Model
{
    [Serializable]
    public struct LeaderBoardRequest
    {
        public string StatisticName;
        public int StartIndex;
        public int MaxResultsCount;
    }
    
    [Serializable]
    public class LeaderboardResult
    {
        public LeaderboardEntry[] Entries;
        public DateTime? NextTimeReset;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string DisplayName;
        public string UserId;
        public int Index;
        public int Stat;
        public object Profile;
    }

    [Serializable]
    public class FindOpponentResult
    {
        public string MatchId;
        public string OpponentId;
    }

    [Serializable]
    public class PlayerRankResult
    {
        public bool IsNew;
        public RankName Rank;
        public TierName Tier;
        public int Score;
        public string Season;
    }

    [Serializable]
    public class MatchingSubmitRequest
    {
        public string MatchId;
        public bool IsWin;
    }
}