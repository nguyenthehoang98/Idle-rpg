using System;

namespace _KIT.Utils
{
    public static class TimeUtils
    {
        #region ONLINE TIME / SERVER TIME

        /*public static IEnumerator GetUnixTimeRequest()
        {
            string uri = "https://worldtimeapi.org/api/ip";
            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    JObject jObject = JObject.Parse(json);
                    string id = "unixtime";
                    if (jObject.TryGetValue(id, out JToken value))
                    {
                        string unix = value.ToString();
                        dateTimeCurrent = DateTimeOffset.FromUnixTimeSeconds(long.Parse(unix)).LocalDateTime;
                        Debug.Log("Unix Time Request: " + dateTimeCurrent);
                    }
                }
            }

            timeEngineCurrent = (long)Time.time;
        }*/

        #endregion
        
        #region LOCAL TIME / UTC TIME

        public static DateTime Now
        {
            get
            {
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                DateTime now = RealNow;
                DateTime localTime = TimeZoneInfo.ConvertTime(now, timeZone);
                return localTime;
            }
        }

        public static DateTime RealNow
        {
            get
            {
                TimeZoneInfo timeZone = TimeZoneInfo.Local;
                DateTime localTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZone);
                return localTime;
            }
        }

        public static long NowUnixTime => GetUnixTime(Now);

        public static long GetUnixTime(DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static int GetSecondsUntilNextDay()
        {
            DateTime currentTime = Now;
            DateTime nextDay = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day).AddDays(1).Date;
            TimeSpan timeUntilNextDay = nextDay - currentTime;
            double secondsUntilNextDay = timeUntilNextDay.TotalSeconds;
            return (int)secondsUntilNextDay;
        }

        public static bool IsNewDay(long unixTime)
        {
            DateTime dateTime = GetDateTime(unixTime);
            DateTime currentTime = Now;
#if GAME_TESTER
            currentTime = currentTime.AddSeconds(1);
#endif
            return dateTime.Date != currentTime.Date && dateTime < currentTime;
        }

        public static DateTime GetDateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
        }

        public static string FormatSecondAsTime_HHMMSS(this int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        public static string FormatSecondAsTime_MMSS(this int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"mm\:ss");
        }

        public static string FormatSecondAsTime_MMSS(this long seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"mm\:ss");
        }

        public static string FormatSecondAsTime_HHMM(this int seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:d}H {1:d2}M ", (int)timeSpan.TotalHours, timeSpan.Minutes);
        }

        #endregion
    }
}