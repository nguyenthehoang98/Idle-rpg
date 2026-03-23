handlers.initPlayer = function(args, context) {
    var playerId = currentPlayerId;
    var titleData = server.GetTitleData({
        Keys: ["CURRENT_SEASON"]
    });
    var CURRENT_SEASON = titleData.Data["CURRENT_SEASON"];
    var userData = server.GetUserInternalData({
        PlayFabId: playerId,
        Keys: ["Rank", "Season", "Tier"]
    });

    var data = userData.Data || {};
    var rank = data["Rank"] ? data["Rank"].Value : null;
    var season = data["Season"] ? data["Season"].Value : null;
    var tier = data["Tier"] ? data["Tier"].Value : null;
    if (!rank) {
        rank = "Bronze";
        tier = "V";
        server.UpdateUserInternalData({
            PlayFabId: playerId,
            Data: {
                Rank: rank,
                Tier: tier,
                Season: CURRENT_SEASON,
                Score: "0"
            }
        });

        return {
            IsNew: true,
            Rank: rank,
            Tier: tier,
            Season: CURRENT_SEASON
        };
    }

    // Sẽ phải kiểm tra rank mới ở đây rồi check logic update rank
    //if (season !== CURRENT_SEASON) {

    return {
        Rank: rank,
        Tier: tier,
        Season: CURRENT_SEASON
    };
};