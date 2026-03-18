using _Cloud.Playfab;

namespace _Cloud.Model
{
    public static class CloudUtils
    {
        static ICloudLogin cloudLogin = new PlayFabLogin();

        public static ICloudLogin GetCloudLogin()
        {
            return cloudLogin;
        }
    }
}