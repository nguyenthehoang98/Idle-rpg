handlers.submitResult = function(args, context) {
    var matchId = args.MatchId;
    var isWin = args.Result;

    // Kiểm tra xem input có hợp lệ không
    if (!matchId) return error(1000, "MatchId is required");
    if (typeof isWin !== "boolean") return error(1001, "Result must be boolean");

    // Session info
    var playerId = currentPlayerId;
    var currentSeason = getCurrentSeason()
    var userData = server.GetUserInternalData({
        PlayFabId: playerId,
        Keys: [USER_DATA_INTERNAL_HISTORY_MATCH,
            USER_DATA_INTERNAL_CURRENT_MATCH,
            USER_DATA_INTERNAL_SEASON_DATA
        ]
    });

    var match = userData.Data?.current_match ?
        JSON.parse(userData.Data.current_match.Value) :
        null;

    // Kiểm tra xem match có hợp lệ không
    if (!match) return error(1002, "Match not found");
    if (match.matchId !== matchId) return error(1003, "Invalid matchId");
    if (match.playerId !== playerId) return error(1003, "Invalid player");
    if (match.status == true) return error(1003, "Match already completed");
    if (Date.now() - match.createdAt > 15 * 60 * 1000) return error(1003, "Match expired");

    // Tìm chỉ số của player hiện tại
    var stats = server.GetPlayerStatistics({
        PlayFabId: playerId
    });
    var currentMatchScore = 0;
    if (stats.Statistics) {
        for (var i = 0; i < stats.Statistics.length; i++) {
            var s = stats.Statistics[i];
            if (s.StatisticName === getStatisticsKey(currentSeason)) {
                currentMatchScore = s.Value;
                break;
            }
        }
    }

    // tính thông tin hiện tại của player
    const {
        rank,
        tier,
        currentScore
    } = ParseScore(currentMatchScore);

    // tính điểm dựa trên thắng thua, rank...
    var delta;
    if (isWin) delta = 10;
    else delta = -5;

    // tính lại công thức
    var newScore = Math.max(0, currentScore + delta);
    var newMatchScore = encodeScore(rank, tier, newScore);

    // Cập nhật leaderboard
    server.UpdatePlayerStatistics({
        PlayFabId: playerId,
        Statistics: [{
            StatisticName: getStatisticsKey(currentSeason),
            Value: newMatchScore
        }]
    });

    // Cập nhật lại UserData
    match.status = true;
    match.delta = delta;

    var histories = userData.Data?.[USER_DATA_INTERNAL_HISTORY_MATCH] ?
        JSON.parse(userData.Data[USER_DATA_INTERNAL_HISTORY_MATCH].Value) :
        [];

    histories.push(match);
    if (histories.length > USER_DATA_INTERNAL_MAXIMUM_HISTORY) {
        histories.shift();
    }

    var season = userData.Data?.season_data ?
        JSON.parse(userData.Data.season_data.Value) :
        {
            win: 0,
            lose: 0
        };
    if (isWin) season.win += 1;
    else season.lose += 1;
    season.rank = rank;
    season.tier = tier;
    season.score = newScore;

    server.UpdateUserInternalData({
        PlayFabId: playerId,
        Data: {
            [USER_DATA_INTERNAL_CURRENT_MATCH]: "",
            [USER_DATA_INTERNAL_HISTORY_MATCH]: JSON.stringify(histories),
            [USER_DATA_INTERNAL_SEASON_DATA]: JSON.stringify(season)
        }
    });

    return {
        success: true,
        delta: delta,
        score: newScore,
        rank: rank,
        tier: tier
    }
}