handlers.updateLeaderboard = function(args, context) {
    var playerId = currentPlayerId;
    var score = parseInt(args.Score);
    // mở rộng ra thêm League để đáp ứng các bảng
    if (score == null) return error(1000, "Score is required");
    if (typeof score !== "number") return error(1001, "Score must be a number");
    if (score < 0) return error(1002, "Score must be non-negative");
    if (score > 1000000) return error(1003, "Score exceeds allowed limit");

    var result = server.UpdatePlayerStatistics({
        PlayFabId: playerId,
        Statistics: [{
            StatisticName: "Score",
            Value: score
        }]
    });

    return {
        success: true,
        result: result
    };
};