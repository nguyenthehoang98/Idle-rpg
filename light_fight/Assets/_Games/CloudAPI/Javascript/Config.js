const USER_DATA_INTERNAL_CURRENT_MATCH = "current_match";
const USER_DATA_INTERNAL_SEASON_DATA = "season_data";
const USER_DATA_INTERNAL_HISTORY_MATCH = "histoties_match";
const TITLE_DATA_INTERNAL_CURRENT_SEASON = "CURRENT_SEASON";
const SCORE_BASE = 1e7;
const TIER_BASE = 1e9;
const USER_DATA_INTERNAL_MAXIMUM_HISTORY = 10;

function GetStatisticsKey(seasion){
    return seasion + "_Score"; 
}

function DecodeScore(matchScore){
    const rank = Math.floor(matchScore / TIER_BASE);
    const tier = Math.floor((matchScore % TIER_BASE) / SCORE_BASE);
    const currentScore = matchScore % SCORE_BASE;
    return { rank, tier, currentScore };
}

function EncodeScore(rank, tier, score){
    if (rank < 0 || tier < 0 || score < 0) return 0;
    return rank * TIER_BASE + tier * SCORE_BASE + score;
}