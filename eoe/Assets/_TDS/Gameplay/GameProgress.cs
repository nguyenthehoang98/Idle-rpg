using System;
using System.Collections.Generic;
using _TDS.GameConfig;
using UnityEngine;

namespace _TDS.Gameplay
{
    [Serializable]
    public sealed class GameProgressState
    {
        public int version = 1;
        public int campaignLevel = RunSelection.DefaultLevel;
        public int playerLevel = 1;
        public int totalExperience;
        public int totalGold;
        public int[] selectedHeroIds = Array.Empty<int>();
        public List<int> selectedUpgradeIds = new List<int>();
        public long lastSavedUtcTicks;
    }

    public readonly struct OfflineReward
    {
        public OfflineReward(int minutes, int experience, int gold)
        {
            Minutes = minutes;
            Experience = experience;
            Gold = gold;
        }

        public int Minutes { get; }
        public int Experience { get; }
        public int Gold { get; }
        public bool HasReward => Experience > 0 || Gold > 0;
    }

    public static class GameProgress
    {
        private const string SaveKey = "idle-rpg.progress.v1";
        private const int MaxOfflineMinutes = 8 * 60;
        private const int OfflineExperiencePerMinute = 1;
        private const int OfflineGoldPerMinute = 1;

        private static GameProgressState state;
        private static OfflineReward lastOfflineReward;

        public static GameProgressState State
        {
            get
            {
                EnsureLoaded();
                return state;
            }
        }

        public static OfflineReward LastOfflineReward
        {
            get
            {
                EnsureLoaded();
                return lastOfflineReward;
            }
        }

        public static void Load()
        {
            if (state != null) return;

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            state = string.IsNullOrEmpty(json)
                ? NewState()
                : JsonUtility.FromJson<GameProgressState>(json);

            if (state == null || state.version != 1)
            {
                state = NewState();
            }

            state.campaignLevel = Math.Max(RunSelection.DefaultLevel, state.campaignLevel);
            state.playerLevel = Math.Max(1, state.playerLevel);
            state.selectedHeroIds ??= Array.Empty<int>();
            state.selectedUpgradeIds ??= new List<int>();
            lastOfflineReward = CollectOffline(DateTime.UtcNow.Ticks);
            Save();
        }

        public static void SaveRun(
            int level,
            int[] heroIds,
            BattleRunRewards rewards,
            bool victory,
            ExpConfig expConfig = null)
        {
            EnsureLoaded();
            if (rewards == null) return;

            int nextLevel = victory ? level + 1 : level;
            state.campaignLevel = Math.Max(state.campaignLevel, Math.Max(RunSelection.DefaultLevel, nextLevel));
            state.totalExperience += Math.Max(0, rewards.Experience);
            state.totalGold += Math.Max(0, rewards.Gold);
            if (expConfig != null)
            {
                state.playerLevel = expConfig.GetLevelForExperience(state.totalExperience);
            }
            state.selectedHeroIds = heroIds == null ? Array.Empty<int>() : (int[])heroIds.Clone();
            state.selectedUpgradeIds.AddRange(rewards.SelectedUpgradeIds);
            Save();
        }

        public static void Save()
        {
            EnsureLoadedWithoutSave();
            state.lastSavedUtcTicks = DateTime.UtcNow.Ticks;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            state = NewState();
            lastOfflineReward = default;
            PlayerPrefs.DeleteKey(SaveKey);
            Save();
        }

        public static int CalculateOfflineMinutes(long lastSavedUtcTicks, long nowUtcTicks)
        {
            if (lastSavedUtcTicks <= 0 || nowUtcTicks <= lastSavedUtcTicks) return 0;
            long elapsedSeconds = new TimeSpan(nowUtcTicks - lastSavedUtcTicks).Ticks / TimeSpan.TicksPerSecond;
            return Math.Min(MaxOfflineMinutes, Math.Max(0, (int)(elapsedSeconds / 60)));
        }

        private static OfflineReward CollectOffline(long nowUtcTicks)
        {
            int minutes = CalculateOfflineMinutes(state.lastSavedUtcTicks, nowUtcTicks);
            OfflineReward reward = new OfflineReward(
                minutes,
                minutes * OfflineExperiencePerMinute,
                minutes * OfflineGoldPerMinute);

            state.totalExperience += reward.Experience;
            state.totalGold += reward.Gold;
            state.lastSavedUtcTicks = nowUtcTicks;
            return reward;
        }

        private static GameProgressState NewState()
        {
            return new GameProgressState
            {
                lastSavedUtcTicks = DateTime.UtcNow.Ticks,
            };
        }

        private static void EnsureLoaded()
        {
            if (state == null) Load();
        }

        private static void EnsureLoadedWithoutSave()
        {
            if (state == null) state = NewState();
        }
    }
}
