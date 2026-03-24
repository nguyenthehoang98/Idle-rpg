handlers.updateLeaderboard = function(args, context) {
    var SCORE_BASE = 10000000; // 1e7
    var TIER_BASE = 1000000000; // 1e9
    
    var playerId = currentPlayerId;
    var matchScore = args.Score;
    if (matchScore == null) return error(1000, "Score is required");
    if (!Number.isFinite(matchScore)) return error(1001, "Score must be a number");
    if (matchScore < 0) return error(1002, "Score must be non-negative");

    var titleData = server.GetTitleData({
        Keys: ["CURRENT_SEASON"]
    });
    var CURRENT_SEASON = titleData.Data["CURRENT_SEASON"];
    if (!CURRENT_SEASON) {
        return error(2000, "CURRENT_SEASON is not configured");
    }
   
    var rank = Math.floor(matchScore / TIER_BASE);
    var tier = Math.floor((matchScore % TIER_BASE) / SCORE_BASE);
    var score = matchScore % SCORE_BASE;
    
    var statName = CURRENT_SEASON + "_Score";
    var result = server.UpdatePlayerStatistics({
        PlayFabId: playerId,
        Statistics: [{
            StatisticName: statName,
            Value: score
        }]
    });

    return {
        success: true,
        result: result
    };
};