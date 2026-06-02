using System.Collections.Generic;
using System.Diagnostics;
using static ICE.ConfigFiles.Config.MissionSettings;

public class MissionTimer
{
    private Stopwatch stopwatch;
    private uint currentMission;
    private bool isRunning;

    public int TimeHistoryLimit => C.TimeHistoryLimit;

    public MissionTimer()
    {
        stopwatch = new Stopwatch();
    }

    public void StartMission(uint missionId)
    {
        currentMission = missionId;
        stopwatch.Restart();
        isRunning = true;
    }

    public TimeSpan CompleteMission()
    {
        if (!isRunning)
        {
            return TimeSpan.Zero;
        }

        stopwatch.Stop();
        var duration = stopwatch.Elapsed;
        isRunning = false;

        UpdateMissionStats(currentMission, duration);
        currentMission = 0;

        return duration;
    }

    private void UpdateMissionStats(uint missionId, TimeSpan duration)
    {
        if (!C.MissionConfig.ContainsKey(missionId))
        {
            C.MissionConfig[missionId] = new();
        }

        var stats = C.MissionConfig[missionId];

        // Adding the new time entry here
        stats.TurninRecords.Add(new TurninData
        {
            Time = duration.TotalSeconds,
            State = Mission_Settings.TurninState,
        });

        if (Mission_Settings.TurninState == TurninState.Bronze)
            stats.BronzeCompletion++;
        else if (Mission_Settings.TurninState == TurninState.Silver)
            stats.SilverCompletions++;
        else if (Mission_Settings.TurninState == TurninState.Gold)
            stats.GoldCompletions++;
        else if (Mission_Settings.TurninState == TurninState.Critical)
            stats.CriticalCompletions++;

        // Increment total completions (always tracks full history)
        stats.TotalCompletions++;

        // Apply time history limit per state if set
        if (TimeHistoryLimit > 0)
        {
            // Group records by state
            var groupedByState = stats.TurninRecords
                .GroupBy(t => t.State)
                .ToList();

            // Keep only the most recent TimeHistoryLimit records for each state
            var trimmedRecords = new List<TurninData>();
            foreach (var group in groupedByState)
            {
                trimmedRecords.AddRange(group.TakeLast(TimeHistoryLimit));
            }

            stats.TurninRecords = trimmedRecords;
        }

        // Calculate stats based on the (possibly limited) time history
        if (stats.TurninRecords.Any())
        {
            stats.BestTime = stats.TurninRecords.Min(t => t.Time);
            stats.AverageTime = stats.TurninRecords.Average(t => t.Time);

            // Calculate per-state averages
            var bronzeRecords = stats.TurninRecords.Where(t => t.State == TurninState.Bronze).ToList();
            var silverRecords = stats.TurninRecords.Where(t => t.State == TurninState.Silver).ToList();
            var goldRecords = stats.TurninRecords.Where(t => t.State == TurninState.Gold).ToList();
            var criticalRecords = stats.TurninRecords.Where(t => t.State == TurninState.Critical).ToList();

            stats.AverageBronzeTime = bronzeRecords.Any() ? bronzeRecords.Average(t => t.Time) : 0;
            stats.AverageSilverTime = silverRecords.Any() ? silverRecords.Average(t => t.Time) : 0;
            stats.AverageGoldTime = goldRecords.Any() ? goldRecords.Average(t => t.Time) : 0;
            stats.AverageCriticalTime = criticalRecords.Any() ? criticalRecords.Average(t => t.Time) : 0;
        }

        stats.TotalAttempts += 1;

        C.Save();
    }

    public void ResetTimers(uint missionId)
    {
        if (!C.MissionConfig.ContainsKey(missionId))
        {
            C.MissionConfig[missionId] = new();
        }

        var stats = C.MissionConfig[missionId];
        stats.TurninRecords.Clear();
        stats.BestTime = double.MaxValue;

        stats.AverageTime = 0;
        stats.AverageBronzeTime = 0;
        stats.AverageSilverTime = 0;
        stats.AverageGoldTime = 0;
        stats.AverageCriticalTime = 0;

        stats.TotalCompletions = 0;
        stats.BronzeCompletion = 0;
        stats.SilverCompletions = 0;
        stats.GoldCompletions = 0;
        stats.CriticalCompletions = 0;
        stats.TotalAttempts = 0;

        C.Save();
    }

    public void AbandonMission()
    {
        stopwatch.Stop();
        stopwatch.Reset();
        isRunning = false;

        if (!C.MissionConfig.ContainsKey(currentMission))
        {
            C.MissionConfig[currentMission] = new();
        }

        var stats = C.MissionConfig[currentMission];
        stats.TotalAttempts++;
        C.Save();
        currentMission = 0;
    }
}