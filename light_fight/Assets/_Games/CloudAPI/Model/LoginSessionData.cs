namespace CloudAPI.Model
{
    public struct LoginSessionData
    {
        public string UserId;
        public bool IsNewPlayer;
        public object AuthenticationContext;
        public System.DateTime LoginTime;
    }
}
