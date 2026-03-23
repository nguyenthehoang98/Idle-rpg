handlers.updateLeaderboard = function(args, context) {
    var playerId = currentPlayerId;
    var score = args.Score;

    if (score == null) return error(1000, "Score is required");
    if (typeof score !== "number") return error(1001, "Score must be a number");
    if (score < 0) return error(1002, "Score must be non-negative");
    if (score > 1000000) return error(1003, "Score exceeds allowed limit");

    server.UpdatePlayerStatistics({
        PlayFabId: playerId,
        Statistics: [{
            StatisticName: "Score",
            Value: score
        }]
    });

    return {
        success: true,
        newScore: score
    };
};