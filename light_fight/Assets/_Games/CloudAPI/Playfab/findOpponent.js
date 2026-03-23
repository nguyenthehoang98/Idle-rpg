handlers.findOpponent = function(args, context) {
    var STAT_NAME = "Score";
    var RANGE = 5;

    var playerId = currentPlayerId;
    var myScore = 0;

    var statsResult = server.GetPlayerStatistics({
        PlayFabId: playerId
    });

    if (statsResult.Statistics) {
        for (var i = 0; i < statsResult.Statistics.length; i++) {
            var stat = statsResult.Statistics[i];

            if (stat.StatisticName === STAT_NAME) {
                myScore = stat.Value;
                break;
            }
        }
    }

    var leaderboardResult = server.GetLeaderboard({
        StatisticName: STAT_NAME,
        StartPosition: 0,
        MaxResultsCount: 100
    });

    var leaderboard = leaderboardResult.Leaderboard;

    if (!leaderboard || leaderboard.length === 0) {
        return error(1004, "Leaderboard is empty");
    }

    var myIndex = -1;
    for (var i = 0; i < leaderboard.length; i++) {
        if (leaderboard[i].PlayFabId === playerId) {
            myIndex = i;
            break;
        }
    }

    if (myIndex === -1) {
        var closestIndex = 0;
        var minDiff = Number.MAX_VALUE;

        for (var i = 0; i < leaderboard.length; i++) {
            var diff = Math.abs(leaderboard[i].StatValue - myScore);

            if (diff < minDiff) {
                minDiff = diff;
                closestIndex = i;
            }
        }

        myIndex = closestIndex;
    }

    var start = Math.max(0, myIndex - RANGE);
    var end = Math.min(leaderboard.length, myIndex + RANGE + 1);

    var candidates = [];

    for (var i = start; i < end; i++) {
        var entry = leaderboard[i];

        if (entry.PlayFabId !== playerId) {
            candidates.push(entry);
        }
    }

    if (candidates.length === 0) {
        return error(1005, "No suitable opponent");
    }

    // ===== STEP 6: RANDOM OPPONENT =====
    var randomIndex = Math.floor(Math.random() * candidates.length);
    return candidates[randomIndex];
};