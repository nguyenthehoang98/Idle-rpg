namespace _Games.CloudAPI
{
    public static class CloudConfig
    {
        // Khai báo các Key save data ở đây. => check sameple để xem cách sử dụng
        public const string USER_DATA_EQUIPMENT = "UserData_Equipment";
        
        // config xác định số lần save/load -> cloud
        
        /// <summary>
        /// Đặt ngưỡng số lần save ở client, sau đó sẽ save lên server.
        /// </summary>
        public const int CACHE_USER_DATA_SAVE_THRESHOLD = 10;
        
        /// <summary>
        /// Đặt ngưỡng số lần get ở client, sau đó sẽ save lên server.
        /// </summary>
        public const int CACHE_USER_DATA_GET_THRESHOLD = 10;
        
        /// <summary>
        /// Đặt ngưỡng số lần submit ở client, sau đó sẽ save lên server.
        /// </summary>
        public const int CACHE_LEADERBOARD_SUBMIT_THRESHOLD = 5;
        
        /// <summary>
        /// Đặt ngưỡng cho số lần request lấy leaderboard data. ví dụ sau 10 lần submit thì sẽ get rank
        /// </summary>
        public const int CACHE_LEADERBOARD_REFRESH_RANK = 10;
    }
}