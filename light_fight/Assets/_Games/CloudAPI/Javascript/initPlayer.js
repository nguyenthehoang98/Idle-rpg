handlers.initPlayer = function(args, context) {
    var playerId = currentPlayerId;
    var currentSeason = getCurrentSeason();
    var userData = server.GetUserInternalData({
        PlayFabId: playerId,
        Keys: [USER_DATA_INTERNAL_SEASON_DATA]
    });

    var raw = userData.Data?.[USER_DATA_INTERNAL_SEASON_DATA];

    var needReset = false;
    var season;

    // Chưa có data => tạo
    if (!raw || !raw.Value) {
        needReset = true;
    } else {
        try {
            season = JSON.parse(raw.Value);
            // Sang season mới
            if (season.season !== currentSeason) {
                needReset = true;
            }
        } catch (e) {
            needReset = true;
        }
    }

    if (needReset) {
        season = {
            season: currentSeason, 
            win: 0,
            lose: 0,
            rank: 0,
            tier: 0,
            score: 0
        };

        server.UpdateUserInternalData({
            PlayFabId: playerId,
            Data: {
                [USER_DATA_INTERNAL_SEASON_DATA]: JSON.stringify(season)
            }
        });

        return {
            IsNew: true,
            Rank: season.rank,
            Tier: season.tier,
            Score: season.score,
            Season: currentSeason
        };
    }

    return {
        IsNew: false,
        Rank: season.rank,
        Tier: season.tier,
        Score: season.score,
        Season: currentSeason
    };
};