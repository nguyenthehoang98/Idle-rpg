handlers.findOpponent = function (args, context) {
    var playerId = currentPlayerId;
    var currentSeason = getCurrentSeason();
    var userData = server.GetUserInternalData({
        PlayFabId: playerId,
        Keys: [USER_DATA_INTERNAL_LAST_OPPONENTS]
    });

    // Tìm chỉ số của player hiện tại
    var stats = server.GetPlayerStatistics({ PlayFabId: playerId });
    var matchScore = 0;
    if (stats.Statistics) {
        for (var i = 0; i < stats.Statistics.length; i++) {
            if (stats.Statistics[i].StatisticName === getStatisticsKey(currentSeason)) {
                matchScore = stats.Statistics[i].Value;
                break;
            }
        }
    }

    // tính thông tin hiện tại của player
    const {
        rank,
        tier,
        currentScore
    } = decodeScore(matchScore);

    // dánh sách các đối thủ trước đó
     var lastOpponents = userData.Data?.last_opponents
        ? JSON.parse(userData.Data.last_opponents.Value)
        : [];

    var lb = server.GetLeaderboard({
        StatisticName: getStatisticsKey(currentSeason),
        PlayFabId: playerId,
        MaxResultsCount: FIND_OPPONENT_MAXIMUM_QUERY
    });

    var list = lb.Leaderboard || [];
    if (list.length === 0) {
        return error(1003, "Leaderboard empty");
    }

    var candidates = [];
    for (var i = 0; i < list.length; i++) {
        var entry = list[i];
        if (entry.PlayFabId === playerId) continue;
        if (lastOpponents.includes(entry.PlayFabId)) continue;
        candidates.push(entry);
    }

    // fallback nếu filter hết
    if (candidates.length === 0) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PlayFabId !== playerId) {
                candidates.push(list[i]);
            }
        }
    }

    if (candidates.length === 0) {
        return error(1003, "No opponent found");
    }

    var opponent = candidates[Math.floor(Math.random() * candidates.length)];
    lastOpponents.push(opponent.PlayFabId);
    if (lastOpponents.length > USER_DATA_INTERNAL_MAXIMUM_CHECK_LAST_OPPONENT) {
        lastOpponents.shift();
    }

    function generateMatchId() {
        return Math.random().toString(36).substr(2, 9);
    }

    // Tạo matchId
    var match = {
        matchId: generateMatchId(),
        playerId: playerId,
        opponentId: opponent.PlayFabId,
        createdAt: Date.now(),
        status: false
    };

    server.UpdateUserInternalData({
        PlayFabId: playerId,
        Data: {
            [USER_DATA_INTERNAL_CURRENT_MATCH]: JSON.stringify(match),
            [USER_DATA_INTERNAL_LAST_OPPONENTS]: JSON.stringify(lastOpponents)
        }
    });

    // nếu cần trường thông tin thì lấy ở đây luôn, đỡ phải query từ client
    return {
        MatchId: match.matchId,
        OpponentId: opponent.PlayFabId
    };
}