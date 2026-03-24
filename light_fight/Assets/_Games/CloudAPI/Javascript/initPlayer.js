handlers.initPlayer = function(args, context) {
    var playerId = currentPlayerId;
    var currentSeason = getCurrentSeason();

    var userData = server.GetUserInternalData({
        PlayFabId: playerId,
        Keys: [USER_DATA_INTERNAL_SEASON_DATA]
    });

    var isNewPlayer = userData.Data?.[USER_DATA_INTERNAL_SEASON_DATA];
    if (isNewPlayer) {
        var season = {
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

    var season = JSON.parse(isNewPlayer.Value);
    return {
        IsNew: false,
        Rank: season.rank,
        Tier: season.tier,
        Score: season.score,
        Season: currentSeason
    };
};