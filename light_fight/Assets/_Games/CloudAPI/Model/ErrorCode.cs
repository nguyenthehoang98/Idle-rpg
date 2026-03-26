namespace _Games.CloudAPI.Model
{
    // Đợi làm xong rồi sửa.
    public enum ErrorCode
    {
        Unknown = 100,
        // Client error code
        CLIENT_USER_DATA_ERROR_LOAD_DATA = 1000,
        CLIENT_FIND_OPPONENT_ERROR_LOAD_DATA = 1010,
        CLIENT_FIND_OPPONENT_EMPTY_DATA = 1011,
        CLIENT_GET_LEADERBOARD_EMPTY_DATA = 1012,
    }
}