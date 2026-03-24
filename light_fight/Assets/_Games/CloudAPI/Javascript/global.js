const USER_DATA_INTERNAL_CURRENT_MATCH = "MATCH_DATA";
const USER_DATA_INTERNAL_SEASON_DATA = "SEASON_DATA";
const USER_DATA_INTERNAL_HISTORY_MATCH = "HISTORY_DATA";
const USER_DATA_INTERNAL_LAST_OPPONENTS = "LAST_OPPONENT";
const TITLE_DATA_INTERNAL_CURRENT_SEASON = "CURRENT_SEASON";
const TITLE_DATA_INTERNAL_LEADERBOARD_SEASON = "LEADERBOARD_SEASON";
const SCORE_BASE = 1e7;
const TIER_BASE = 1e9;
const USER_DATA_INTERNAL_MAXIMUM_HISTORY = 10;
const USER_DATA_INTERNAL_MAXIMUM_CHECK_LAST_OPPONENT = 5;
const FIND_OPPONENT_MAXIMUM_QUERY = 10;

var cachedSeason = null;
function getCurrentSeason() {
    if (cachedSeason) return cachedSeason;
    var titleData = server.GetTitleData({
        Keys: [TITLE_DATA_INTERNAL_CURRENT_SEASON]
    });
    var season = titleData.Data[TITLE_DATA_INTERNAL_CURRENT_SEASON];
    if (!season) {
       error(1002, "Season not found");
    }
    cachedSeason = season;
    return season;
}

function getStatisticsKey(seasion){
    return seasion + "_Score"; 
}

function decodeScore(matchScore){
    const rank = Math.floor(matchScore / TIER_BASE);
    const tier = Math.floor((matchScore % TIER_BASE) / SCORE_BASE);
    const currentScore = matchScore % SCORE_BASE;
    return { rank, tier, currentScore };
}

function encodeScore(rank, tier, score){
    if (rank < 0 || tier < 0 || score < 0) return 0;
    return rank * TIER_BASE + tier * SCORE_BASE + score;
}

function error(code, message) {
    return {
        success: false,
        error: {
            code: code,
            message: message,
        }
    };
}