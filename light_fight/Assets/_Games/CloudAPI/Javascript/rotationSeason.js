handlers.rotationSeason = function () {
    var titleData = server.GetTitleData({
        Keys: [TITLE_DATA_INTERNAL_LEADERBOARD_SEASON]
    });

    var season = JSON.parse(titleData.Data[TITLE_DATA_INTERNAL_LEADERBOARD_SEASON]);

    var now = new Date();
    var utcHour = now.getUTCHours(); // dùng UTC cho đồng bộ

    var maintenanceStart = season.maintenance_hours.start;
    var maintenanceEnd = season.maintenance_hours.end;

    // 👉 check đang trong maintenance window
    var inMaintenance = utcHour >= maintenanceStart && utcHour < maintenanceEnd;

    if (!inMaintenance) {
        return { rotated: false, reason: "not in maintenance window" };
    }

    var start = new Date(season.start_time);
    var durationMs = season.duration_days * 5 * 60 * 1000;
    var endTime = new Date(start.getTime() + durationMs);

    // 👉 chưa hết season
    if (now < endTime) {
        return { rotated: false, reason: "season not ended" };
    }

    // 👉 rotate (đơn giản, vì đã có maintenance bảo kê)
    season.season_id += 1;
    season.start_time = now.toISOString();

    server.SetTitleData({
        Key: TITLE_DATA_INTERNAL_LEADERBOARD_SEASON,
        Value: JSON.stringify(season)
    });

    return {
        rotated: true,
        newSeasonId: season.season_id
    };
};