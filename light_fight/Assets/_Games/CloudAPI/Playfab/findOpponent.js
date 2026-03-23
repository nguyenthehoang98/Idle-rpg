handlers.findOpponent = function(args, context) {

    var playerId = currentPlayerId;

    log.info("=== findOpponent START ===", {
        playerId: playerId
    });

    // ===== STEP 1: GET MY SCORE =====
    var stats = server.GetPlayerStatistics({
        PlayFabId: playerId
    });

    var myScore = 0;

    if (stats.Statistics) {
        for (var i = 0; i < stats.Statistics.length; i++) {
            if (stats.Statistics[i].StatisticName === "Score") {
                myScore = stats.Statistics[i].Value;
                break;
            }
        }
    }

    log.info("My score", {
        score: myScore
    });

    // ===== STEP 2: LOAD LEADERBOARD =====
    var list = server.GetLeaderboard({
        StatisticName: "Score",
        StartPosition: 0,
        MaxResultsCount: 100
    });

    if (!list.Leaderboard || list.Leaderboard.length === 0) {
        log.error("No leaderboard data", {
            score: score
        });
        throw "No leaderboard data";
    }

    // ===== STEP 3: FIND MY POSITION (approx) =====
    var myIndex = -1;

    for (var i = 0; i < list.Leaderboard.length; i++) {
        if (list.Leaderboard[i].PlayFabId === playerId) {
            myIndex = i;
            break;
        }
    }

    // nếu không nằm trong top 100 → fallback theo score
    if (myIndex === -1) {
        var closestIndex = 0;
        var minDiff = Number.MAX_VALUE;

        for (var i = 0; i < list.Leaderboard.length; i++) {
            var diff = Math.abs(list.Leaderboard[i].StatValue - myScore);

            if (diff < minDiff) {
                minDiff = diff;
                closestIndex = i;
            }
        }

        myIndex = closestIndex;

        log.info("Fallback by score", {
            index: myIndex,
            score: list.Leaderboard[myIndex].StatValue
        });
    }

    log.info("My index (approx)", {
        index: myIndex
    });

    // ===== STEP 4: GET RANGE =====
    var RANGE = 5;

    var start = Math.max(0, myIndex - RANGE);
    var end = Math.min(list.Leaderboard.length, myIndex + RANGE + 1);

    var candidates = [];

    for (var i = start; i < end; i++) {
        var p = list.Leaderboard[i];

        if (p.PlayFabId !== playerId) {
            candidates.push(p);
        }
    }

    if (candidates.length === 0) {
        log.error("No opponent found", {
            score: score
        });
        throw "No opponent found";
    }

    // ===== RANDOM PICK =====
    var opponent = candidates[Math.floor(Math.random() * candidates.length)];

    log.info("Opponent selected", {
        opponentId: opponent.PlayFabId,
        score: opponent.StatValue
    });

    return opponent;
};