using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using System.Reflection.Metadata;
using TerraFX.Interop.Windows;
using YamlDotNet.Core.Tokens;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Ui.MainUi.ModeSelect_Modes.modeSelect_TableInfo;
using static ICE.Utilities.WKSManagerCustom;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_CheckScore
    {
        public static void Enqueue()
        {
            var Id = CosmicHelper.CurrentLunarMission;
            var mission = CosmicHelper.SheetMissionDict[Id];

            var jobs = mission.Jobs;

            if (CosmicHelper.CrafterJobList.Any(x => jobs.Contains(x)) && CosmicHelper.GatheringJobList.Any(x => jobs.Contains(x)))
            {
                P.TaskManager.Enqueue(() => DualClass(), "Checking dual class score");
                P.TaskManager.Enqueue(() => SchedulerMain.State = IceState.DualClass, "Setting state to dual class");
            }
            else if (CosmicHelper.CrafterJobList.Any(x => jobs.Contains(x)))
            {
                IceLogging.Info("Currently on a crafting job, checking for crafting scoring", "Task: Score Check");
                P.TaskManager.Enqueue(() => SchedulerMain.State = IceState.Craft);
                P.TaskManager.Enqueue(() => Craft_V2(), "Checking for crafting score mission");
            }
            else if (CosmicHelper.GatheringJobList.Any(x => jobs.Contains(x)))
            {
                var jobId = Player.Job;

                IceLogging.Info($"Currently on a gathering job {jobId}");
                if (jobId == (Job)18)
                {
                    P.TaskManager.Enqueue(() => SchedulerMain.State = IceState.Fish);
                    P.TaskManager.Enqueue(() => Fish(), "Checking fishing missions for score");
                }
                else
                {
                    P.TaskManager.Enqueue(() => SchedulerMain.State = IceState.Gather);
                    P.TaskManager.Enqueue(() => Gather_V2(), "Checking the gathering score");
                }
            }
        }

        public static unsafe bool? Fish()
        {
            string tag = "[Task_Check Score: Fish]";
            var currentMission = CosmicHelper.CurrentLunarMission;

            if (EzThrottler.Throttle("Fish Score Check Throttle", 1000))
                IceLogging.Verbose($"Score check for fish was initialized. Checking for minimum requirements: [{currentMission}]", tag);

            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
            {
                if (CosmicHelper.SheetMissionDict.TryGetValue(currentMission, out var sheetInfo))
                {
                    var rank = CurrentRank();

                    if (rank == MissionRank.Depleted)
                    {
                        IceLogging.Debug("Mission is either timed out, or out of resources. So going to force a turnin", tag);
                        SchedulerMain.State = IceState.AbandonMission;
                        P.TaskManager.Tasks.Clear();
                        return true;
                    }

                    IceLogging.Verbose("Mission info was valid, searching what we should be checking for", tag);

                    if (sheetInfo.Gathering_Min.Count > 0)
                    {
                        IceLogging.Verbose($"Fishing mission has a minumum amount of fish needed. Checking the specifics for each", tag);
                        foreach (var fishItem in sheetInfo.Gathering_Min)
                        {
                            if (PlayerHelper.GetItemCount(fishItem.Key, out var amount))
                            {
                                if (amount < fishItem.Value)
                                {
                                    IceLogging.Debug("We've found a fish that we're still missing!\n" +
                                        $"ItemID: {fishItem.Key}. We need: {fishItem.Value}. We have: {amount}", tag);

                                    return true;
                                }
                            }
                        }
                    }
                    else if (sheetInfo.Fish_AmountRequired > 0)
                    {
                        IceLogging.Verbose("We're in a mission where we need a certain amount of fish overall. So checking that", tag);
                        var amount = CurrentCollectedTotal();

                        if (amount < sheetInfo.Fish_AmountRequired)
                        {
                            IceLogging.Debug($"We're not at the total amount needed for the mission. Need: {sheetInfo.Fish_AmountRequired} | Have: {amount}", tag);
                            return true;
                        }
                    }
                    else if (sheetInfo.Fish_VarietyAmount > 0)
                    {
                        var amount = CurrentIndividualTotal();
                        if (amount < sheetInfo.Fish_VarietyAmount)
                        {
                            IceLogging.Debug($"Need a variety of fish, and we're missing some. Need: {sheetInfo.Fish_VarietyAmount} | Have: {amount}", tag);
                            return true;
                        }
                    }
                    else
                    {
                        var currentScore = CurrentScore();
                        var bronzeScore = sheetInfo.BronzeScore;

                        if (currentScore < bronzeScore && sheetInfo.BronzeScore != 0)
                        {
                            IceLogging.Debug("We need to still hit the score threshold for bronze. So we're still gonna fish", tag);
                            return true;
                        }
                    }

                    if (rank != MissionRank.None || sheetInfo.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        if (sheetInfo.Attributes.HasFlag(MissionAttributes.ScoreTimeRemaining))
                        {
                            IceLogging.Debug("We're in a mission where we're just meeting the minimum score. Turning in", tag);
                            SchedulerMain.State = IceState.TurninMission;
                            P.TaskManager.Tasks.Clear();

                            Mission_Settings.TurninState = DetermineTurninState();
                            return true;
                        }
                        else
                        {
                            bool shouldTurnin = false;

                            if (sheetInfo.Attributes.HasFlag(MissionAttributes.Critical))
                            {
                                IceLogging.Verbose("We're in a critical mission, this needs to just be turned in", tag);
                                shouldTurnin = true;
                            }
                            else if (CosmicHandler.IsMissionTimedOut())
                            {
                                IceLogging.Verbose("We seem to be timed out of our current mission. But we've hit a minimum threshold of score, so we're just going to turnin", tag);
                                shouldTurnin = true;
                            }
                            else if (Mission_Settings.Mode == ModeSelect.LevelMode && rank >= MissionRank.Bronze)
                            {
                                IceLogging.Debug("We're in Leveling Mode, and we only need a bronze. So we're setting turnin to true");
                                shouldTurnin = true;
                            }
                            else
                            {
                                var config = C.MissionConfig[currentMission];

                                shouldTurnin = ((config.TurninGold || config.AutoTurnin) && rank >= MissionRank.Gold) ||
                                               (config.TurninSilver && rank >= MissionRank.Silver) ||
                                               (config.TurninBronze && rank >= MissionRank.Bronze);
                            }

                            if (shouldTurnin)
                            {
                                MedalChecker(rank);
                                IceLogging.Info("The threshold for scoring was met. Time to turnin", tag);
                                SchedulerMain.State = IceState.TurninMission;
                                P.TaskManager.Tasks.Clear();

                                return true;
                            }
                            else
                            {
                                var config = C.MissionConfig[currentMission];

                                IceLogging.Debug("We're still going for a score/not met threshold.\n" +
                                    $"Rank: {rank.ToString()}\n" +
                                    $"Any Turnin: {config.AutoTurnin}" +
                                    $"Gold Turnin: {config.TurninGold}\n" +
                                    $"Silver Turnin: {config.TurninSilver}\n" +
                                    $"Bronze Turnin: {config.TurninBronze}", tag);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        IceLogging.Error($"Hey, it seems something slipped through the cracks. If you could report to me this it would be great\n" +
                            $"ID: {currentMission}\n" +
                            $"Name: {sheetInfo.Name}\n" +
                            $"Missed the score ranking but slipped through", tag);

                        return true;
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud) && moonHud.IsAddonReady)
            {
                if (EzThrottler.Throttle("Opening the moon hud", 1000))
                {
                    moonHud.Mission();
                    IceLogging.Info("Hud wasn't visible. Opening it", "[Score Check]");
                }
            }

            return false;
        }

        public static bool? Craft_V2()
        {
            string tag = "[Check Score: Craft]";

            var currentScore = CurrentScore();
            var rank = CurrentRank();

            if (rank == MissionRank.Depleted)
            {
                IceLogging.Debug("Mission is either timed out, or out of resources. So going to force a turnin", tag);
                SchedulerMain.State = IceState.AbandonMission;
                P.TaskManager.Tasks.Clear();
                return true;
            }

            if (Svc.Condition[ConditionFlag.ExecutingGatheringAction])
            {
                return false;
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var id = CosmicHelper.CurrentLunarMission;
                if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var sheet))
                {

                    if (sheet.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        if (currentScore < sheet.BronzeScore)
                        {
                            IceLogging.Info("We still need score for the critical mission, so going to craft some more", tag);
                            SchedulerMain.State = IceState.Craft;
                            return true;
                        }
                        else
                        {
                            IceLogging.Info("Minimum score for criticals has been hit WOOO", tag);
                        }
                    }
                    else if (rank == MissionRank.None)
                    {
                        IceLogging.Info("We haven't completed the minimum crafts required for the turnin. Going to craft more", tag);
                        return true;
                    }

                    if (rank != MissionRank.None || sheet.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        bool shouldTurnin = false;

                        if (sheet.Attributes.HasFlag(MissionAttributes.Critical))
                        {
                            IceLogging.Verbose("We're in a critical mission, this needs to just be turned in", tag);
                            shouldTurnin = true;
                        }
                        else if (CosmicHandler.IsMissionTimedOut())
                        {
                            IceLogging.Verbose("We seem to be timed out of our current mission. But we've hit a minimum threshold of score, so we're just going to turnin", tag);
                            shouldTurnin = true;
                        }
                        else if (Mission_Settings.Mode == ModeSelect.LevelMode && rank >= MissionRank.Bronze)
                        {
                            IceLogging.Debug("We're in Leveling Mode, and we only need a bronze. So we're setting turnin to true");
                            shouldTurnin = true;
                        }
                        else
                        {
                            var config = C.MissionConfig[id];

                            shouldTurnin = ((config.TurninGold || config.AutoTurnin) && rank >= MissionRank.Gold) ||
                                           (config.TurninSilver && rank >= MissionRank.Silver) ||
                                           (config.TurninBronze && rank >= MissionRank.Bronze);
                        }

                        if (shouldTurnin)
                        {
                            MedalChecker(rank);
                            IceLogging.Info("The threshold for scoring was met. Time to turnin", tag);
                            SchedulerMain.State = IceState.TurninMission;
                            P.TaskManager.Tasks.Clear();

                            return true;
                        }
                        else
                        {
                            var config = C.MissionConfig[id];

                            IceLogging.Debug("We're still going for a score/not met threshold.\n" +
                                $"Rank: {rank.ToString()}\n" +
                                $"Any Turnin: {config.AutoTurnin}" +
                                $"Gold Turnin: {config.TurninGold}\n" +
                                $"Silver Turnin: {config.TurninSilver}\n" +
                                $"Bronze Turnin: {config.TurninBronze}", tag);
                            return true;
                        }
                    }
                    else
                    {
                        IceLogging.Error($"Hey, it seems something slipped through the cracks. If you could report to me this it would be great\n" +
                            $"ID: {id}\n" +
                            $"Name: {sheet.Name}\n" +
                            $"Missed the score ranking but slipped through", tag);

                        return true;
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud) && moonHud.IsAddonReady)
            {
                if (EzThrottler.Throttle("Opening the moon hud", 1000))
                {
                    moonHud.Mission();
                    IceLogging.Info("Hud wasn't visible. Opening it", "[Score Check]");
                }
            }

            return false;
        }
        public static bool? Gather_V2()
        {
            string tag = "[Check Score: Gather]";

            var currentScore = CurrentScore();
            var rank = CurrentRank();

            if (rank == MissionRank.Depleted)
            {
                IceLogging.Debug("Mission is either timed out, or out of resources. So going to force a turnin", tag);
                SchedulerMain.State = IceState.AbandonMission;
                P.TaskManager.Tasks.Clear();
                return true;
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var id = CosmicHelper.CurrentLunarMission;
                if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var sheet))
                {
                    if (sheet.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        foreach (var item in sheet.Gathering_Min)
                        {
                            if (PlayerHelper.GetItemCount(item.Key, out var count) && count < item.Value)
                            {
                                IceLogging.Info("We're still missing items for the critical mission, so continuing on", tag);
                                return true;
                            }
                        }
                    }
                    else if (rank == MissionRank.None)
                    {
                        IceLogging.Info("We still haven't achieved atleast bronze scoring for the missions. So we're going to continue on", tag);
                        return true;
                    }

                    if (rank != MissionRank.None || sheet.Attributes.HasFlag(MissionAttributes.Critical))
                    {
                        bool shouldTurnin = false;

                        if (sheet.Attributes.HasFlag(MissionAttributes.Critical))
                        {
                            IceLogging.Verbose("We're in a critical mission, this needs to just be turned in", tag);
                            shouldTurnin = true;
                        }
                        else if (CosmicHandler.IsMissionTimedOut())
                        {
                            IceLogging.Verbose("We seem to be timed out of our current mission. But we've hit a minimum threshold of score, so we're just going to turnin", tag);
                            shouldTurnin = true;
                        }
                        else if (Mission_Settings.Mode == ModeSelect.LevelMode && rank >= MissionRank.Bronze)
                        {
                            IceLogging.Debug("We're in Leveling Mode, and we only need a bronze. So we're setting turnin to true", tag);
                            shouldTurnin = true;
                        }
                        else if (sheet.Attributes.HasFlag(MissionAttributes.ScoreTimeRemaining))
                        {
                            IceLogging.Debug("Score is based on time remaining, and we have some sort of rank. Turning in", tag);
                            shouldTurnin = true;
                        }
                        else if (sheet.Attributes.HasFlag(MissionAttributes.Limited) && Mission_Settings.nodeTotal == 8)
                        {
                            if (!Svc.Condition[ConditionFlag.Gathering])
                            {
                                shouldTurnin = true;
                                IceLogging.Debug("We might of not reached our turnin point, but we've ran out of nodes to gather at. So we're just going to just turnin", tag);
                            }
                        }
                        else
                        {
                            var config = C.MissionConfig[id];

                            shouldTurnin = ((config.TurninGold || config.AutoTurnin) && rank >= MissionRank.Gold) ||
                                           (config.TurninSilver && rank >= MissionRank.Silver) ||
                                           (config.TurninBronze && rank >= MissionRank.Bronze);
                        }

                        if (shouldTurnin)
                        {
                            MedalChecker(rank);
                            IceLogging.Info("The threshold for scoring was met. Time to turnin", tag);
                            SchedulerMain.State = IceState.TurninMission;
                            P.TaskManager.Tasks.Clear();

                            return true;
                        }
                        else
                        {
                            var config = C.MissionConfig[id];

                            IceLogging.Debug("We're still going for a score/not met threshold.\n" +
                                $"Rank: {rank.ToString()}\n" +
                                $"Any Turnin: {config.AutoTurnin}" +
                                $"Gold Turnin: {config.TurninGold}\n" +
                                $"Silver Turnin: {config.TurninSilver}\n" +
                                $"Bronze Turnin: {config.TurninBronze}", tag);
                            return true;
                        }
                    }
                    else
                    {
                        IceLogging.Error($"Hey, it seems something slipped through the cracks. If you could report to me this it would be great\n" +
                            $"ID: {id}\n" +
                            $"Name: {sheet.Name}\n" +
                            $"Missed the score ranking but slipped through", tag);

                        return true;
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud) && moonHud.IsAddonReady)
            {
                if (EzThrottler.Throttle("Opening the moon hud", 1000))
                {
                    moonHud.Mission();
                    IceLogging.Info("Hud wasn't visible. Opening it", "[Score Check]");
                }
            }

            return false;
        }
        public static unsafe bool? DualClass()
        {
            string tag = "Score Check: Dual Class";

            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var currentScore = CurrentScore();
                var rank = CurrentRank();
                var Id = CosmicHelper.CurrentLunarMission;

                if (CosmicHandler.IsMissionTimedOut())
                {
                    SchedulerMain.State = IceState.AbandonMission;
                    P.TaskManager.Tasks.Clear();
                    return true;
                }
                else if (rank == MissionRank.Depleted)
                {
                    SchedulerMain.State = IceState.AbandonMission;
                    P.TaskManager.Tasks.Clear();
                    return true;
                }

                var config = C.MissionConfig[Id];
                bool shouldTurnin = ((config.TurninGold || config.AutoTurnin) && rank >= MissionRank.Gold) ||
                                    (config.TurninSilver && rank >= MissionRank.Silver) ||
                                    (config.TurninBronze && rank >= MissionRank.Bronze);

                if (shouldTurnin)
                {
                    MedalChecker(rank);
                    IceLogging.Info("The threshold for scoring was met. Time to turnin", tag);
                    SchedulerMain.State = IceState.TurninMission;
                    P.TaskManager.Tasks.Clear();

                    return true;
                }
                else
                {
                    IceLogging.Debug("We're still going for a score/not met threshold.\n" +
                        $"Rank: {rank.ToString()}\n" +
                        $"Any Turnin: {config.AutoTurnin}" +
                        $"Gold Turnin: {config.TurninGold}\n" +
                        $"Silver Turnin: {config.TurninSilver}\n" +
                        $"Bronze Turnin: {config.TurninBronze}", tag);
                    return true;
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud))
            {
                if (EzThrottler.Throttle("Opening the moon hud", 1000))
                {
                    moonHud.Mission();
                    IceLogging.Info("Hud wasn't visible. Opening it", tag);
                }
            }

            return false;
        }

        private static unsafe uint CurrentCollectedTotal()
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return 0;

            var manager = (WKSManagerCustom*)managerPtr;
            return manager->CollectedTotal;
        }

        private static unsafe uint CurrentIndividualTotal()
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return 0;

            var manager = (WKSManagerCustom*)managerPtr;
            return manager->CollectedIndividual;
        }

        private static unsafe uint CurrentScore()
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return 0;

            var manager = (WKSManagerCustom*)managerPtr;
            return manager->CurrentScore;
        }

        public static unsafe MissionRank CurrentRank()
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return MissionRank.None;

            var manager = (WKSManagerCustom*)managerPtr;
            return manager->CurrentRank;
        }

        public static TurninState DetermineTurninState()
        {
            string timerString = ActiveTimerAddon();
            TimeSpan silverRequirement = ParseRequirementTime(SilverTimerAddon());
            TimeSpan goldRequirement = ParseRequirementTime(GoldTimerAddon());

            // Parse the timer string to get remaining time (left side of /)
            var remainingTime = ParseCurrentTime(timerString);

            IceLogging.Info($"Timer Info:\n" +
                $"Current Timer: {remainingTime}\n" +
                $"Silver Requirement: {silverRequirement}\n" +
                $"Gold Requirement: {goldRequirement}");

            if (remainingTime >= goldRequirement)
                return TurninState.Gold;
            else if (remainingTime >= silverRequirement)
                return TurninState.Silver;
            else
                return TurninState.Bronze;
        }

        private static TimeSpan ParseCurrentTime(string timerString)
        {
            IceLogging.Verbose($"Raw timer string: '{timerString}'");

            // Trim to remove the clock icon and any whitespace
            var currentTimeStr = timerString.Trim();

            // Remove any non-numeric characters except ':' (like the clock icon)
            currentTimeStr = new string(currentTimeStr.Where(c => char.IsDigit(c) || c == ':').ToArray());

            IceLogging.Verbose($"Cleaned time string: '{currentTimeStr}'");

            // Parse the time (format: M:SS or MM:SS)
            var timeParts = currentTimeStr.Split(':');
            IceLogging.Verbose($"Time parts count: {timeParts.Length}");

            if (timeParts.Length != 2)
            {
                IceLogging.Verbose($"Time split failed - got {timeParts.Length} parts");
                return new TimeSpan(0, 0, 0);
            }

            if (!int.TryParse(timeParts[0], out var minutes) ||
                !int.TryParse(timeParts[1], out var seconds))
            {
                IceLogging.Verbose($"Failed to parse time values");
                return new TimeSpan(0, 0, 0);
            }

            IceLogging.Verbose($"Successfully parsed - Minutes: {minutes}, Seconds: {seconds}");
            return new TimeSpan(0, minutes, seconds);
        }

        private static TimeSpan ParseRequirementTime(string requirementString)
        {
            if (string.IsNullOrWhiteSpace(requirementString))
                return TimeSpan.Zero;

            // Split on whitespace and take the first part (the time)
            var parts = requirementString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return TimeSpan.Zero;

            var timeStr = parts[0];

            // Parse the time (format: M:SS or MM:SS)
            var timeParts = timeStr.Split(':');
            if (timeParts.Length != 2)
                return TimeSpan.Zero;

            if (!int.TryParse(timeParts[0], out var minutes) ||
                !int.TryParse(timeParts[1], out var seconds))
                return TimeSpan.Zero;

            return new TimeSpan(0, minutes, seconds);
        }

        private static unsafe string ActiveTimerAddon()
        {
            return AddonHelper.GetNodeText("WKSMissionInfomation", 24);
        }

        private static string SilverTimerAddon()
        {
            return AddonHelper.GetNodeText("WKSMissionInfomation", 15);
        }

        private static string GoldTimerAddon()
        {
            return AddonHelper.GetNodeText("WKSMissionInfomation", 11);
        }

        private static void MedalChecker(uint current, uint silver, uint gold)
        {
            if (current >= gold)
                Mission_Settings.TurninState = TurninState.Gold;
            else if (current >= silver)
                Mission_Settings.TurninState = TurninState.Silver;
            else
                Mission_Settings.TurninState = TurninState.Bronze;
        }

        private static void MedalChecker(MissionRank rank)
        {
            if (rank == MissionRank.Gold)
                Mission_Settings.TurninState = TurninState.Gold;
            else if (rank == MissionRank.Silver)
                Mission_Settings.TurninState = TurninState.Silver;
            else if (rank == MissionRank.Bronze)
                Mission_Settings.TurninState = TurninState.Bronze;
        }
    }
}
