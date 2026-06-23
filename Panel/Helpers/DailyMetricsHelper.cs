using System;
using Panel.Models;

namespace Panel.Helpers;

public static class DailyMetricsHelper
{
    public static DateTime GetCurrentGameDay()
    {
        var now = DateTime.Now;
        return now.Hour < 4 ? now.Date.AddDays(-1) : now.Date;
    }

    public static bool CheckAndResetDailyMetrics(
        AccountSettingsRoot settings,
        int currentKilis,
        int currentMvbt,
        int currentMhbt,
        out int farmedKilis,
        out int farmedMvbt,
        out int farmedMhbt)
    {
        bool wasReset = false;
        var gameDay = GetCurrentGameDay();

        if (settings.Daily.LastResetDate < gameDay)
        {
            settings.Daily.LastResetDate = gameDay;
            settings.Daily.InitialKilisCount = currentKilis;
            settings.Daily.InitialMvbtCount = currentMvbt;
            settings.Daily.InitialMhbtCount = currentMhbt;
            settings.Daily.DailyQuestCompletedCount = 0;
            settings.Daily.DailyQuestCanceledCount = 0;
            settings.Daily.DailyQuestFinishedToday = false;
            settings.Daily.DailyQuestLastRunMode = string.Empty;
            wasReset = true;
        }

        farmedKilis = currentKilis - settings.Daily.InitialKilisCount;
        farmedMvbt = currentMvbt - settings.Daily.InitialMvbtCount;
        farmedMhbt = currentMhbt - settings.Daily.InitialMhbtCount;

        if (farmedKilis < 0) farmedKilis = 0;
        if (farmedMvbt < 0) farmedMvbt = 0;
        if (farmedMhbt < 0) farmedMhbt = 0;

        return wasReset;
    }

    public static void ForceReset(AccountSettingsRoot settings, int currentKilis, int currentMvbt, int currentMhbt)
    {
        settings.Daily.LastResetDate = GetCurrentGameDay();
        settings.Daily.InitialKilisCount = currentKilis;
        settings.Daily.InitialMvbtCount = currentMvbt;
        settings.Daily.InitialMhbtCount = currentMhbt;
        settings.Daily.DailyQuestCompletedCount = 0;
        settings.Daily.DailyQuestCanceledCount = 0;
        settings.Daily.DailyQuestFinishedToday = false;
        settings.Daily.DailyQuestLastRunMode = string.Empty;
    }
}
