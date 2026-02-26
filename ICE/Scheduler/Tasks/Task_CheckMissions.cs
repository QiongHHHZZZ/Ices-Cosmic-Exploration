using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Utilities.Cosmic;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using YamlDotNet.Core.Tokens;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Ui.MainUi.ModeSelect_Modes.modeSelect_TableInfo;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_CheckMissions
    {
        private static Dictionary<string, List<uint>> MissionLibrary = new()
        {
            ["Critical"] = new(),
            ["Weather"] = new(),
            ["Timed"] = new(),
            ["Sequence"] = new(),
            ["Ex"] = new(),
            ["A"] = new(),
            ["B"] = new(),
            ["C"] = new(),
            ["D"] = new(),
            ["???"] = new(),
        };

        private static readonly Random _random = new Random();

        public static void Enqueue()
        {
            P.TaskManager.EnqueueMulti
                (
                    new(() => RefreshMissionLibrary(), "Refreshing the mission library"),
                    new(() => OpenMissionUi(), "Opening Mission Ui"),
                    new(() => CheckTabs(), "Checking tabs for valid missions")
                );
        }
        private static int GrabMission_Counter = 0;
        private static bool? GrabMissionDelay()
        {
            if (C.DelayGrabMission)
            {
                if (EzThrottler.Throttle("Adding delay to grab mission", C.DelayIncrease))
                {
                    GrabMission_Counter += 1;
                }

                if (GrabMission_Counter > 1)
                {
                    GrabMission_Counter = 0;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                GrabMission_Counter = 0;
                return true;
            }
        }
        private static void ReOpenMissionUi(string tag)
        {
            if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud) && moonHud.IsAddonReady)
            {
                if (EzThrottler.Throttle("Opening the mission ui"))
                {
                    IceLogging.Info("Opening the moon mission selection hud", tag);
                    moonHud.Mission();
                }
            }
        }
        private static string LibraryInfo(KeyValuePair<uint, CosmicHelper.CosmicInfo> mission)
        {
            string entry = string.Empty;
            var attribute = mission.Value.Attributes;
            var rank = mission.Value.Rank;

            if (attribute.HasFlag(MissionAttributes.ProvisionalWeather))
                entry = "Weather";
            else if (attribute.HasFlag(MissionAttributes.ProvisionalTimed))
                entry = "Timed";
            else if (attribute.HasFlag(MissionAttributes.ProvisionalSequential))
                entry = "Sequence";
            else if (attribute.HasFlag(MissionAttributes.Critical))
                entry = "Critical";
            else if (rank != 0)
            {
                entry = rank switch
                {
                    5 => "Ex",
                    4 => "A",
                    3 => "B",
                    2 => "C",
                    1 => "D",
                    _ => "???",
                };
            }

            return entry;
        }
        public static bool? RefreshMissionLibrary()
        {
            string tag = "Task Check Mission: Refresh Mission Library";

            foreach (var entry in MissionLibrary)
            {
                entry.Value.Clear();
            }

            var modeSelected = Mission_Settings.Mode;
            foreach (var mission in CosmicHelper.SheetMissionDict)
            {
                if (mission.Value.TerritoryId != Player.Territory.RowId)
                    continue;

                bool provisional = mission.Value.Attributes.HasFlag(MissionAttributes.ProvisionalSequential)
                                || mission.Value.Attributes.HasFlag(MissionAttributes.ProvisionalTimed)
                                || mission.Value.Attributes.HasFlag(MissionAttributes.ProvisionalWeather);

                var missionId = mission.Key;

                if (C.MissionConfig.TryGetValue(missionId, out var config))
                {
                    if (modeSelected == ModeSelect.LevelMode && CosmicHelper.QuickLevelList.Contains(mission.Key))
                    {
                        var job = Mission_Settings.SelectedJob;
                        var jobLevel = Player.GetLevel((Job)job);
                        var missionLevel = mission.Value.Level;

                        if (!mission.Value.Jobs.Contains(job))
                            continue;

                        // Short end of it all, making sure to see what tier the player should be doing
                        // Taking the players level and making sure it matches to the tier
                        // 90+ = 90
                        // 50-89 = 50
                        // 10-49 = 10
                        int playerTier = jobLevel >= 90 ? 90 : jobLevel >= 50 ? 50 : 10;

                        if (missionLevel != playerTier)
                            continue;
                        else
                        {
                            MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                    else if (modeSelected == ModeSelect.RelicMode)
                    {
                        if (provisional)
                            continue;

                        var jobLevel = Math.Min(Player.GetLevel((Job)mission.Value.Jobs.First()), Player.GetLevel((Job)mission.Value.Jobs.Last()));
                        if (jobLevel < mission.Value.Level)
                            continue;

                        if (C.XPRelicOnlyEnabled)
                        {
                            if (config.Enabled)
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                        else
                        {
                            if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                    else if (modeSelected == ModeSelect.Standard)
                    {
                        if (!config.Enabled)
                            continue;

                        var jobLevel = Math.Min(Player.GetLevel((Job)mission.Value.Jobs.First()), Player.GetLevel((Job)mission.Value.Jobs.Last()));
                        if (jobLevel < mission.Value.Level)
                            continue;

                        if (provisional)
                        {
                            if (C.GrindAllProvisionals)
                            {
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            }
                            else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                            {
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            }
                        }
                        else if (mission.Value.Attributes.HasFlag(MissionAttributes.Critical))
                        {
                            if (C.GrindOffClassRedAlert)
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                            else if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                        else
                        {
                            if (mission.Value.Jobs.Contains(Mission_Settings.SelectedJob))
                                MissionLibrary[LibraryInfo(mission)].Add(missionId);
                        }
                    }
                }
                else
                {
                    IceLogging.Error("We're missing a mission from the config, please report back so I can fix this.\n" +
                        $"MissionID: {mission.Key} | Job (First) {mission.Value.Jobs.First()} | Rank: {mission.Value.Rank}");
                }
            }

            IceLogging.Debug("All viable missions have been loaded, reporting back all counts");
            foreach (var entry in MissionLibrary)
            {
                IceLogging.Debug($"[{entry.Key}] = {entry.Value.Count}", tag);
            }

            if (MissionLibrary.All(x => x.Value.Count == 0))
            {
                IceLogging.Verbose("We currently have no viable missions... which is odd. Please make sure you have some enabled, or report back if this is incorrect\n" +
                    $"Config Mode: {C.SelectedMode} | Mode going into this: {Mission_Settings.Mode}", tag);

                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                return true;
            }
            else
            {
                IceLogging.Verbose($"Mission finder says we have a valid mission list. So we gonna go find one", tag);
                IceLogging.Verbose($"Stardard tab missions job: {Mission_Settings.SelectedJob}");
                return true;
            }
        }
        public static bool? OpenMissionUi()
        {
            string tag = "[Task Check Mission: Open Mission UI]";

            if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talkUi) && talkUi.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing the talk"))
                {
                    IceLogging.Info("Talk ui was visible, clicking through", tag);
                    talkUi.Click();
                }

                return false;
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var hud) && hud.IsAddonReady)
            {
                IceLogging.Info("The Mission Selection Ui is visible! Continuing on", tag);
                return true;
            }
            else
            {
                ReOpenMissionUi(tag);
            }

            return false;
        }
        public static bool? CheckTabs()
        {
            static bool? OpenSpecificTab(MissionTypes type)
            {
                string tag = "Opening Tab";

                if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var hud) && hud.IsAddonReady)
                {
                    switch (type)
                    {
                        case MissionTypes.RedAlert:
                            hud.CriticalMissions(); break;
                        case MissionTypes.Provisional:
                            hud.ProvisionalMissions(); break;
                        case MissionTypes.Standard:
                            hud.BasicMissions(); break;
                    }
                    return true;
                }
                else
                {
                    ReOpenMissionUi(tag);
                }

                return false;
            }

            string tag = "Check Missions: Check Tabs";
            var priority = C.MissionTypePrio;

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var x) && x.IsAddonReady)
            {
                foreach (var type in C.MissionTypePrio)
                {
                    switch (type)
                    {
                        case MissionTypes.RedAlert:
                            {
                                if (MissionLibrary["Critical"].Count > 0)
                                {
                                    P.TaskManager.EnqueueMulti
                                    (
                                        new(() => OpenSpecificTab(type), $"Opening {type}"),
                                        new(() => FrameDelay(8), "Delaying 8 frames for tab"),
                                        new(() => CheckMissions(MissionLibrary["Critical"], type), "Checking Critical tab for missions")
                                    );
                                }
                                break;
                            }
                        case MissionTypes.Provisional:
                            {
                                List<uint> provisionals = new();
                                foreach (var ProvisionalPrio in C.MissionPrio)
                                {
                                    string key = ProvisionalPrio switch
                                    {
                                        ProvisionalTypes.ProvisionalWeather => "Weather",
                                        ProvisionalTypes.ProvisionalSequential => "Sequence",
                                        ProvisionalTypes.ProvisionalTimed => "Timed",
                                        _ => ""
                                    };

                                    if (MissionLibrary.TryGetValue(key, out var missionList))
                                    {
                                        foreach (var jobId in C.JobPrio)
                                        {
                                            // Add missions that match this provisional type AND this job
                                            foreach (var missionId in missionList)
                                            {
                                                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var missionInfo) && missionInfo.Jobs.Contains(jobId) && !provisionals.Contains(missionId))
                                                {
                                                    provisionals.Add(missionId);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (provisionals.Count > 0)
                                {
                                    P.TaskManager.EnqueueMulti
                                    (
                                        new(() => OpenSpecificTab(type), $"Opening {type}"),
                                        new(() => FrameDelay(8), "Delaying 8 frames for tab"),
                                        new(() => CheckMissions(provisionals, type), "Checking Provisional tab for missions")
                                    );
                                }
                                break;
                            }
                        case MissionTypes.Standard:
                            {
                                List<uint> basicMissions = new();
                                List<string> MissionRanks = new() { "Ex", "A", "B", "C", "D" };
                                foreach (var rank in MissionRanks)
                                {
                                    if (MissionLibrary.TryGetValue(rank, out var missionList))
                                    {
                                        foreach (var mission in missionList)
                                        {
                                            if (!basicMissions.Contains(mission))
                                                basicMissions.Add(mission);
                                        }
                                    }
                                }
                                P.TaskManager.EnqueueMulti
                                (
                                    new(() => OpenSpecificTab(type), $"Opening {type}"),
                                    new(() => FrameDelay(8), "Delaying 8 frames for tab"),
                                    new(() => CheckMissions(basicMissions, type), "Checking Basic Mission tab for missions")
                                );
                                break;
                            }
                        case MissionTypes.DroneSearch:
                            {
                                if (C.Cosmodrone_Run)
                                {
                                    P.TaskManager.Enqueue(() => Task_ArtifactSearch.RefreshMapInfo(), "Inserting Drone Task");
                                }
                                break;
                            }
                    }
                }

                P.TaskManager.Enqueue(() => FindReroll(), "Find mission to reroll for");
            }
            else
            {
                ReOpenMissionUi(tag);
            }
            return true;
        }
        private static bool? CheckMissions(List<uint> missionList, MissionTypes type)
        {
            string tag = "[Check Missions: Queue]";
            void LogInfo(uint missionId)
            {
                var sheetInfo = CosmicHelper.SheetMissionDict[missionId];
                bool provisional = sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalSequential)
                    || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalTimed)
                    || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalWeather);
                var redAlert = sheetInfo.Attributes.HasFlag(MissionAttributes.Critical);
                string jobs = string.Join(", ", sheetInfo.Jobs);

                IceLogging.Info($"We found a mission! We're going to exit out of this task and grab the following\n: " +
                    $"[Id] = {missionId} " +
                    $"[Selected Job] = {Mission_Settings.SelectedJob}\n" +
                    $"[Mission Job] = {jobs}" +
                    $"Red Alert: {redAlert}" +
                    $"Provisional: {provisional}", tag);
            }

            int JobTab(uint job)
            {
                int jobUnlocked = 0;
                Dictionary<uint, int> jobTab = new();
                for (int i = 0; i < CosmicHelper.SupportedJobs.Count(); i++)
                {
                    var currentJob = CosmicHelper.SupportedJobs[i];
                    var level = Player.GetLevel((Job)currentJob);
                    if (level != 0)
                    {
                        IceLogging.Verbose($"{currentJob} - tab: {jobUnlocked}");
                        jobTab[currentJob] = jobUnlocked;
                        jobUnlocked++;
                    }
                    else
                    {
                        jobTab[currentJob] = 0;
                    }
                }

                return jobTab[job];
            }

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
            {
                if (type == MissionTypes.Standard)
                {
                    IceLogging.Verbose($"Checking Standard missions. Mission Count: {missionList.Count()}", tag);
                    bool OnCorrectTab()
                    {
                        foreach (var mission in missionInfo.StellerMissions)
                        {
                            var sheetInfo = CosmicHelper.SheetMissionDict[mission.MissionId];
                            bool isCritical = sheetInfo.Attributes.HasFlag(MissionAttributes.Critical);
                            bool isProvisional = sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalSequential)
                                || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalTimed)
                                || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalWeather);


                            if (isCritical || isProvisional)
                            {
                                if (EzThrottler.Throttle("We're on the wrong tab... changing"))
                                {
                                    IceLogging.Verbose("Current Tab (Atleast what we think it is):\n" +
                                        $"Critical? {isCritical}\n" +
                                        $"Provisional? {isProvisional}\n" +
                                        $"MissionID: {mission.MissionId}");
                                    IceLogging.Info("Selecting basic tab", tag);
                                    missionInfo.BasicMissions();
                                }

                                return false;
                            }
                            if (!sheetInfo.Jobs.Contains(Mission_Settings.SelectedJob))
                            {
                                var jobTab = JobTab(Mission_Settings.SelectedJob);

                                if (EzThrottler.Throttle("Selecting job tab"))
                                {
                                    IceLogging.Verbose($"Selecting tab: {jobTab}");
                                    missionInfo.SelectClass[jobTab].Select();
                                }
                                return false;
                            }
                        }

                        return true;
                    }

                    if (OnCorrectTab())
                    {
                        var mode = Mission_Settings.Mode;
                        if (mode == ModeSelect.Standard)
                        {
                            foreach (var missionId in missionList)
                            {
                                var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                                if (mission != null)
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }

                            IceLogging.Info("No missions were found for basic missions. Continuing on", tag);
                            return true;
                        }
                        else if (mode == ModeSelect.LevelMode)
                        {
                            foreach (var missionId in missionList)
                            {
                                // Ideally... there's only 1 mission here if it filters out properly. 
                                var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                                if (mission != null)
                                {
                                    LogInfo(missionId);
                                    Insert_GrabMissionTask(missionId);
                                    return true;
                                }
                            }
                            IceLogging.Verbose($"We seem to have not found the mission. Going to double check to make sure we have the tab unlocked", tag);
                            var firstMission = missionInfo.StellerMissions.FirstOrDefault();
                            if (firstMission != null)
                            {
                                var TestMissionLevel = CosmicHelper.SheetMissionDict[firstMission.MissionId].Level;
                                var ListMissionLevel = CosmicHelper.SheetMissionDict[missionList.First()].Level;

                                if (EzThrottler.Throttle("Level message info"))
                                {
                                    IceLogging.Verbose($"The very first available mission is visible. Checking the level\n" +
                                        $"Test level: {TestMissionLevel}\n" +
                                        $"List Mission level: {ListMissionLevel}", tag);
                                }

                                if (TestMissionLevel < ListMissionLevel)
                                {
                                    IceLogging.Verbose("We need to actually grab a mission that hasn't been completed for the purpose of getting the next rank unlocked", tag);
                                    if (ListMissionLevel == 90)
                                    {
                                        if (TestMissionLevel == 50)
                                        {
                                            var mission = missionInfo.StellerMissions
                                                .Where(m => CosmicHelper.Unlock_MissionList.Contains(m.MissionId))
                                                .Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 50)
                                                .Where(m => !MissionCompleted(m.MissionId))
                                                .FirstOrDefault();

                                            if (mission != null)
                                            {
                                                mission.Select();
                                                Insert_GrabMissionTask(mission.MissionId);
                                                IceLogging.Verbose($"Mission was found to unlock lv. 90 tab {mission.MissionId}", tag);
                                                return true;
                                            }
                                        }
                                        else if (TestMissionLevel == 10)
                                        {
                                            var mission = missionInfo.StellerMissions
                                                .Where(m => CosmicHelper.Unlock_MissionList.Contains(m.MissionId))
                                                .Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10)
                                                .Where(m => !MissionCompleted(m.MissionId))
                                                .FirstOrDefault();

                                            if (mission != null)
                                            {
                                                mission.Select();
                                                Insert_GrabMissionTask(mission.MissionId);
                                                IceLogging.Verbose($"Mission was found to unlock lv. 90 tab {mission.MissionId}", tag);
                                                return true;
                                            }
                                        }
                                    }
                                    else if (ListMissionLevel == 50)
                                    {
                                        if (TestMissionLevel == 10)
                                        {
                                            var mission = missionInfo.StellerMissions
                                                .Where(m => CosmicHelper.Unlock_MissionList.Contains(m.MissionId))
                                                .Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10)
                                                .Where(m => !MissionCompleted(m.MissionId))
                                                .FirstOrDefault();

                                            if (mission != null)
                                            {
                                                mission.Select();
                                                Insert_GrabMissionTask(mission.MissionId);
                                                IceLogging.Verbose($"Mission was found to unlock lv. 90 tab {mission.MissionId}", tag);
                                                return true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }
                        else if (mode == ModeSelect.RelicMode)
                        {
                            var job = Mission_Settings.SelectedJob;
                            var relicInfo = CosmicHelper.Cosmic_ClassInfo();
                            var classInfo = relicInfo[job];

                            var urgency = new Dictionary<int, float>();
                            foreach (var exp in classInfo.CurrentExp)
                            {
                                if (classInfo.Stage_Current != classInfo.Stage_Next)
                                    urgency[exp.Key] = exp.Value.Needed > 0 ? 1f - (float)exp.Value.Current / exp.Value.Needed : 0f;
                                else
                                    urgency[exp.Key] = 1f - (float)exp.Value.Current / exp.Value.Max;
                            }
                            IceLogging.Verbose($"Urgency Exp Values");
                            foreach (var exp in urgency)
                            {
                                IceLogging.Verbose($"{exp.Key} : Value: {exp.Value:N2}");
                            }

                            uint? bestMissionId = null;
                            float bestScore = float.NegativeInfinity;
                            foreach (var missionId in missionList)
                            {
                                var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                                if (mission != null)
                                {
                                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.MissionId, out var sheetInfo))
                                    {
                                        float score = 0;
                                        foreach (var reward in sheetInfo.RelicXpInfo)
                                        {
                                            if (urgency.TryGetValue(reward.Key, out var info))
                                            {
                                                float contribution = info * reward.Value;
                                                if (contribution > 0)
                                                {
                                                    score += contribution;
                                                }
                                            }
                                        }
                                        if (score > bestScore)
                                        {
                                            bestScore = score;
                                            bestMissionId = missionId;
                                        }
                                    }
                                }
                            }

                            if (bestMissionId != null)
                            {
                                LogInfo(bestMissionId.Value);
                                Insert_GrabMissionTask(bestMissionId.Value);
                                return true;
                            }
                            else
                            {
                                IceLogging.Info("Enabled only mode was set to on in the Relic grind mode. So we have to reroll WOOO");
                                return true;
                            }
                        }
                    }
                }
                else if (type == MissionTypes.Provisional)
                {
                    if (missionInfo.StellerMissions.Count() > 0)
                    {
                        foreach (var missionId in missionList)
                        {
                            var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                            if (mission != null)
                            {
                                LogInfo(missionId);
                                Insert_GrabMissionTask(missionId);
                                return true;
                            }
                        }
                    }

                    IceLogging.Info("No missions were found in the provisional tab, continuing on", tag);
                    return true;
                }
                else if (type == MissionTypes.RedAlert)
                {
                    if (missionInfo.StellerMissions.Count() > 0)
                    {
                        foreach (var missionId in missionList)
                        {
                            var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                            if (mission != null)
                            {
                                LogInfo(missionId);
                                Insert_GrabMissionTask(missionId);
                                return true;
                            }
                        }
                    }
                    IceLogging.Info("No missions were found in the red alert tab, continuing on", tag);
                    return true;
                }
            }
            else
            {
                ReOpenMissionUi(tag);
            }

            return false;
        }
        private static void Insert_GrabMissionTask(uint missionId)
        {
            P.TaskManager.Tasks.Clear();

            // Extract materia between missions if spiritbond is ready and next mission is not EX+
            if (C.SelfSpiritbondGather && Task_Spiritbond.IsSpiritbondReadyAny())
            {
                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var nextMission) && nextMission.Rank < 6)
                {
                    IceLogging.Info($"Next mission rank {nextMission.Rank} is below EX+, extracting materia first");
                    P.TaskManager.Enqueue(() => Task_Spiritbond.ExtractMateria(), "Extracting materia before next mission");
                }
            }

            P.TaskManager.EnqueueMulti
                (
                    new(() => CheckForMovementRequired(missionId), "Checking to see if we need to move to mission"),
                    new(() => Mission_ChangeJob(missionId), "Changing to correct job for mission"),
                    new(() => GrabMission(missionId), "Grabbing mission to initate")
                );
        }
        private static bool? Mission_ChangeJob(uint missionId)
        {
            IceLogging.Verbose("Starting to change job");

            var mission = CosmicHelper.SheetMissionDict[missionId];
            var jobId = mission.Jobs.First();

            if ((uint)Player.Job == jobId)
                return true;
            else
            {
                if (EzThrottler.Throttle("Swapping to job for mission"))
                {
                    GearsetHandler.TaskClassChange((Job)jobId);
                    IceLogging.Debug($"Swapping to job: {jobId}");
                }
                return false;
            }
        }
        private static Vector3 randomFishingHole = Vector3.Zero;
        private static bool? CheckForMovementRequired(uint missionId)
        {
            string tag = "[Check Missions: Movement Check]";

            var sheetInfo = CosmicHelper.SheetMissionDict[missionId];
            var missionConfig = C.MissionConfig[missionId];

            if (missionConfig.ManualMode || UnsupportedMissions.Ids.Contains(missionId))
            {
                IceLogging.Info("Mission is currently in manual mode, or not supported. So not going to pathfind to it.", tag);
                return true;
            }
            else if (!P.Navmesh.Installed)
            {
                IceLogging.Error("HEY. YOU DIDN'T READ THE HELP ME PAGE. AND NOW YOU'RE MISSING NAVMESH. So... yeah... if things break this is why");
                return true;
            }
            else if (sheetInfo.Attributes.HasFlag(MissionAttributes.Gather))
            {
                var missionTerritory = sheetInfo.TerritoryId;
                var mapId = sheetInfo.MapPosition;
                var gatherInfo = GatheringRouteLoader.GetRoute(missionTerritory, mapId);

                if (gatherInfo.Count == 0)
                {
                    IceLogging.Error("Hey, so this is actually missing the information for it. So going to just actually add it to the unsupported mission list", tag);
                    UnsupportedMissions.Ids.Add(missionId);
                    return true;
                }
                else
                {
                    var startNode = gatherInfo[0];

                    foreach (var node in gatherInfo)
                    {
                        if (Player.DistanceTo(node.Position) < 5)
                        {
                            IceLogging.Info("We're close enough to the node! So continuing onto grabbing the mission", tag);
                            return true;
                        }
                    }

                    IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now", tag);

                    // CN-MAINT: gather Daily Routines TP is mission-entry only.
                    if (Task_Gather.TryDailyRoutinesTeleportToGatherLandZone(startNode.LandZone, tag))
                    {
                        return false;
                    }

                    Task_NavmeshMove.Enqueue_NavmeshTask(startNode.LandZone);
                    return true;
                }
            }
            else if (sheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
            {
                var location = sheetInfo.MapPosition;
                var territory = sheetInfo.TerritoryId;
                var fishingHole = GatheringUtil.MoonFishingLocations[territory][location];

                if (fishingHole == null || fishingHole.Count == 0)
                {
                    IceLogging.Error("We've seemed to have ran into a problem with the fishing hole... either it's missing spots, or it doesn't exist. Please report back to me on this with logs leading up to this\n" +
                        $"Mission ID: {missionId} | Map Position: {location} | Moon Territory: {territory}\n" +
                        $"Adding to the unsupported list so it's marked on your side for now", tag);
                    UnsupportedMissions.Ids.Add(missionId);
                }

                foreach (var fishingSpot in fishingHole)
                {
                    if (Player.DistanceTo(fishingSpot.FishingSpot) < 3)
                    {
                        IceLogging.Info($"We've reached our fishing spot! We are current at: {fishingSpot.FishingSpot}", tag);
                        randomFishingHole = Vector3.Zero;
                        return true;
                    }
                }

                if (randomFishingHole == Vector3.Zero)
                {
                    var _random = new Random();
                    var randomIndex = _random.Next(fishingHole.Count);
                    if (EzThrottler.Throttle("Setting fishing hole destination"))
                    {
                        IceLogging.Debug($"Random number spot said we're going to the following fishing hole #: {randomIndex}");
                        randomFishingHole = fishingHole[randomIndex].FishingSpot;
                    }
                }
                else
                {
                    if (Task_Fishing.TryDailyRoutinesTeleportToFishingSpot(randomFishingHole, tag))
                    {
                        return false;
                    }

                    IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now");
                    Task_NavmeshMove.Enqueue_NavmeshTask(randomFishingHole);
                    randomFishingHole = Vector3.Zero;
                    return true;
                }
            }
            else if (C.PersonalReturnSpot)
            {
                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Critical))
                {
                    IceLogging.Info($"We are currently aimed to do a critical mission, and we're on a crafter(?) so we're not going to move from our spot", tag);
                    return true;
                }
                else
                {
                    var territory = Player.Territory.RowId;
                    if (C.CrafterLocations.TryGetValue(territory, out var location))
                    {
                        IceLogging.Verbose("If we've gotten this far, that means we need to figure out a path to go to the node. Doing so now");
                        Task_NavmeshMove.Enqueue_NavmeshTask(location);
                        return true;
                    }
                    else
                    {
                        IceLogging.Debug("No location is set for this place, so continuing on", tag);
                        return true;
                    }
                }
            }
            else
            {
                IceLogging.Info("Mission was not a gathering or critical mission. Navmesh moving was not necessary. Moving onto next step", tag);
                return true;
            }

            return false;
        }
        private static int retryCheck = 0;
        private static bool? GrabMission(uint missionId, bool reroll = false)
        {
            string tag = "[Check Missions: Grab Mission]";

            if (CosmicHelper.CurrentLunarMission != 0)
            {
                retryCheck = 0;
                Mission_Settings.ResetNodeCounter();

                if (reroll)
                {
                    SchedulerMain.State = IceState.AbandonMission;
                    Task_AbandonMission.ForceAbandon = true;
                }
                else
                {
                    SchedulerMain.State = IceState.ExecutingMission;
                    Task_AbandonMission.ForceAbandon = false;
                }
                Mission_Settings.nodeTotal = 0;
                P.TaskManager.Tasks.Clear();
                IceLogging.Debug($"State upon exiting: {SchedulerMain.State}");
                return true;
            }
            else
            {
                if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
                {
                    var selectMission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                    if (selectMission != null)
                    {
                        if (EzThrottler.Throttle("Selecting mission", 500))
                            InitiateMission(missionId);
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Selecting tab info correctly", 500))
                        {
                            if (EzThrottler.Throttle("Selecting the proper tab", 2000))
                            {
                                var sheetInfo = CosmicHelper.SheetMissionDict[missionId];
                                if (sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalSequential) || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalWeather) 
                                    || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalTimed))
                                {
                                    missionInfo.ProvisionalMissions();
                                }
                                else if (sheetInfo.Attributes.HasFlag(MissionAttributes.Critical))
                                {
                                    missionInfo.CriticalMissions();
                                }
                                else
                                {
                                    missionInfo.BasicMissions();
                                }
                            }
                            if (EzThrottler.Throttle("Counter Adder"))
                            {
                                retryCheck += 1;
                            }
                        }

                        if (retryCheck >= 4)
                        {
                            IceLogging.Verbose($"Mission could no longer be found: {missionId}, retrying the process", tag);
                            retryCheck = 0;
                            P.TaskManager.Tasks.Clear();
                            return true;
                        }
                    }
                }
                else
                {
                    ReOpenMissionUi(tag);
                }
            }

            return false;
        }
        private static unsafe void InitiateMission(uint missionId)
        {
            var WKSInstance = WKSManager.Instance();
            if (WKSInstance != null)
            {
                WKSInstance->MissionModule->InitiateMission((ushort)missionId);
            }
        }
        private static bool? FindReroll()
        {
            string tag = "[Check Missions: Find Reroll]";

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var testMission = missionInfo.StellerMissions.FirstOrDefault();
                uint missionToAbandon = 0;
                if (testMission != null)
                {
                    var attribute = CosmicHelper.SheetMissionDict[testMission.MissionId].Attributes;
                    bool nonStandard = attribute.HasFlag(MissionAttributes.ProvisionalSequential) || attribute.HasFlag(MissionAttributes.ProvisionalTimed) 
                                    || attribute.HasFlag(MissionAttributes.ProvisionalWeather) || attribute.HasFlag(MissionAttributes.Critical);
             
                    if (nonStandard)
                    {
                        if (FrameThrottler.Throttle("Selecting proper tab", 8))
                        {
                            missionInfo.BasicMissions();
                        }
                        return false;
                    }
                    else
                    {
                        List<uint> AExRank = new List<uint>();
                        List<uint> ARank = new List<uint>();
                        List<uint> BRank = new List<uint>();
                        List<uint> CRank = new List<uint>();
                        List<uint> DRank = new List<uint>();

                        IceLogging.Info($"We're abandoning mission... so this should be the right tab for this: Rank: {CosmicHelper.SheetMissionDict[testMission.MissionId].Rank}");

                        // Track mission appearance counts
                        foreach (var mission in missionInfo.StellerMissions)
                        {
                            var missionId = mission.MissionId;

                            // Increment appearance count
                            if (!Mission_Settings.missionApperenceCount.ContainsKey(missionId))
                                Mission_Settings.missionApperenceCount[missionId] = 0;
                            Mission_Settings.missionApperenceCount[missionId]++;

                            var rank = CosmicHelper.SheetMissionDict[missionId].Rank;
                            IceLogging.Verbose($"Checking: {missionId} | Rank: {rank}");

                            switch (rank)
                            {
                                case 5: AExRank.Add(missionId); break;
                                case 4: ARank.Add(missionId); break;
                                case 3: BRank.Add(missionId); break;
                                case 2: CRank.Add(missionId); break;
                                case 1:
                                default: DRank.Add(missionId); break;
                            }
                        }

                        bool CheckARanks = (MissionLibrary["Ex"].Count > 0 || MissionLibrary["A"].Count > 0) && (AExRank.Count > 0 || ARank.Count > 0);
                        bool CheckBRanks = (MissionLibrary["B"].Count > 0 && BRank.Count > 0);
                        bool CheckCRanks = (MissionLibrary["C"].Count > 0 && CRank.Count > 0);
                        bool CheckDRanks = (MissionLibrary["D"].Count > 0 && DRank.Count > 0);

                        IceLogging.Verbose($"[Ex] = {AExRank.Count()}\n" +
                            $"[A] = {ARank.Count()}\n" +
                            $"[B] = {BRank.Count()}\n" +
                            $"[C] = {CRank.Count()}\n" +
                            $"[D] = {DRank.Count()}", tag);

                        List<string> ranks = new() { "Ex", "A", "B", "C", "D" };
                        var enabledCount = 0;
                        foreach (var rank in ranks)
                        {
                            enabledCount += MissionLibrary[rank].Count();
                        }

                        if (enabledCount == 0)
                        {
                            IceLogging.Info("We don't have any basic missions enabled under the following class\n" +
                                $"{Mission_Settings.SelectedJob}. So we're just going to clear -> Reset (Assuming we're checking for timed and such)");
                            P.TaskManager.Tasks.Clear();
                            return true;
                        }

                        var random = new Random();
                        void ShuffleList<T>(List<T> list, Random rnd)
                        {
                            for (int i = list.Count - 1; i > 0; i--)
                            {
                                int j = rnd.Next(i + 1);
                                (list[i], list[j]) = (list[j], list[i]);
                            }
                        }

                        ShuffleList(AExRank, random);
                        ShuffleList(ARank, random);
                        ShuffleList(BRank, random);
                        ShuffleList(CRank, random);
                        ShuffleList(DRank, random);

                        // small function to find a frequent mission that might be locking us
                        uint FindFrequentMission(List<uint> missionList, int threshold = 3)
                        {
                            foreach (var missionId in missionList)
                            {
                                if (CosmicHelper.SheetMissionDict.TryGetValue(missionId, out var mission))
                                {
                                    if (mission.Jobs.Count == 2)
                                    {
                                        if (!(Player.GetLevel((Job)mission.Jobs[0]) >= 100 && Player.GetLevel((Job)mission.Jobs[1]) >= 100))
                                            continue;
                                    }
                                }

                                if (Mission_Settings.missionApperenceCount.TryGetValue(missionId, out int count) && count >= threshold)
                                {
                                    return missionId;
                                }
                            }
                            return 0;
                        }

                        if (CheckARanks)
                        {
                            // Check for frequent missions in A/AEx ranks first
                            uint frequentAEx = FindFrequentMission(AExRank, Mission_Settings.rerollThreshold);
                            uint frequentA = FindFrequentMission(ARank, Mission_Settings.rerollThreshold);

                            if (AExRank.Count > 2)
                            {
                                if (frequentAEx != 0)
                                {
                                    missionToAbandon = frequentAEx;
                                    IceLogging.Debug($"Abandoning frequently appearing AEX mission (appeared {Mission_Settings.missionApperenceCount[frequentAEx]} times)", tag);
                                    Mission_Settings.previousAbandonRank = 5;
                                }
                                else
                                {
                                    IceLogging.Debug($"Only AEX Rank missions are available. Forcing an AEX rank to be accepted");
                                    missionToAbandon = AExRank.First();
                                    Mission_Settings.previousAbandonRank = 5;
                                }
                            }
                            else if (ARank.Count > 2)
                            {
                                if (frequentA != 0)
                                {
                                    missionToAbandon = frequentA;
                                    IceLogging.Debug($"Abandoning frequently appearing A mission (appeared {Mission_Settings.missionApperenceCount[frequentA]} times)", tag);
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                                else
                                {
                                    IceLogging.Debug($"Only A Rank missions are available. Forcing an A rank to be accepted", tag);
                                    missionToAbandon = ARank.First();
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                            }
                            else
                            {
                                if (Mission_Settings.previousAbandonRank == 5)
                                {
                                    if (frequentA != 0)
                                    {
                                        missionToAbandon = frequentA;
                                        IceLogging.Debug($"Abandoning frequently appearing A mission (appeared {Mission_Settings.missionApperenceCount[frequentA]} times)", tag);
                                        Mission_Settings.previousAbandonRank = 4;
                                    }
                                    else
                                    {
                                        missionToAbandon = ARank.First();
                                        IceLogging.Debug($"Abandoning Rank 4 Mission.");
                                        Mission_Settings.previousAbandonRank = 4;
                                    }
                                }
                                else if (Mission_Settings.previousAbandonRank == 4)
                                {
                                    if (frequentAEx != 0)
                                    {
                                        missionToAbandon = frequentAEx;
                                        IceLogging.Debug($"Abandoning frequently appearing AEX mission (appeared {Mission_Settings.missionApperenceCount[frequentAEx]} times)", tag);
                                        Mission_Settings.previousAbandonRank = 5;
                                    }
                                    else
                                    {
                                        missionToAbandon = AExRank.First();
                                        IceLogging.Debug($"Abandoning Rank 5 Mission", tag);
                                        Mission_Settings.previousAbandonRank = 5;
                                    }
                                }
                                else
                                {
                                    missionToAbandon = ARank.First();
                                    IceLogging.Debug($"Starting off w/ abandoning an A rank", tag);
                                    Mission_Settings.previousAbandonRank = 4;
                                }
                            }
                        }
                        else if (CheckBRanks)
                        {
                            uint frequentB = FindFrequentMission(BRank, Mission_Settings.rerollThreshold);
                            if (frequentB != 0)
                            {
                                missionToAbandon = frequentB;
                                IceLogging.Debug($"Abandoning frequently appearing B mission (appeared {Mission_Settings.missionApperenceCount[frequentB]} times)", tag);
                            }
                            else
                            {
                                missionToAbandon = BRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 3;
                        }
                        else if (CheckCRanks)
                        {
                            uint frequentC = FindFrequentMission(CRank, Mission_Settings.rerollThreshold);
                            if (frequentC != 0)
                            {
                                missionToAbandon = frequentC;
                                IceLogging.Debug($"Abandoning frequently appearing C mission (appeared {Mission_Settings.missionApperenceCount[frequentC]} times)", tag);
                            }
                            else
                            {
                                missionToAbandon = CRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 2;
                        }
                        else if (CheckDRanks)
                        {
                            uint frequentD = FindFrequentMission(DRank, Mission_Settings.rerollThreshold);
                            if (frequentD != 0)
                            {
                                missionToAbandon = frequentD;
                                IceLogging.Debug($"Abandoning frequently appearing D mission (appeared {Mission_Settings.missionApperenceCount[frequentD]} times)", tag);
                            }
                            else
                            {
                                missionToAbandon = DRank.First();
                            }
                            Mission_Settings.previousAbandonRank = 1;
                        }
                        else if (Mission_Settings.Mode == ModeSelect.LevelMode)
                        {
                            if (MissionLibrary["B"].Count > 0)
                            {
                                IceLogging.Debug("Leveling mode is active. Need to find a valid C or D Rank mission", tag);
                                var mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 50).FirstOrDefault();

                                if (mission != null)
                                {
                                    missionToAbandon = mission.MissionId;
                                }
                                else
                                {
                                    mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10).FirstOrDefault();

                                    if (mission != null)
                                        missionToAbandon = mission.MissionId;
                                }
                            }
                            else if (MissionLibrary["C"].Count > 0)
                            {
                                var mission = missionInfo.StellerMissions.Where(m => CosmicHelper.SheetMissionDict[m.MissionId].Level == 10).FirstOrDefault();
                                if (mission != null)
                                    missionToAbandon = mission.MissionId;
                            }
                        }

                        if (missionToAbandon != 0)
                        {
                            P.TaskManager.EnqueueMulti
                                (
                                    new(() => Mission_ChangeJob(missionToAbandon)),
                                    new(() => GrabMission(missionToAbandon, true))
                                );
                            return true;
                        }
                    }
                }
                else
                {
                    if (FrameThrottler.Throttle("Selecting proper tab", 8))
                    {
                        missionInfo.BasicMissions();
                    }
                    return false;
                }
            }
            else
            {
                ReOpenMissionUi(tag);
            }

            return false;
        }
        public static bool? FrameDelay(int amount)
        {
            P.TaskManager.InsertDelay(amount, true);
            return true;

        }
        private static unsafe bool MissionCompleted(uint id)
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return true;

            var manager = (WKSManagerCustom*)managerPtr;
            var isCompleted = manager->IsMissionCompleted(id);

            return isCompleted;
        }
        private static void Notes()
        {
            /*
             * This is kind of my place to just... figure out how tf the logic is going to work. 
             * Right now, the logic is 
             * 1: Store all the missions in the dictionary.
             *   - This doesn't matter if what kind of mode, we're just storing it. It should... allow for re-rolling of missions even when in relic mode on weird edge cases (aka, only selected missions for some reason)
             * 2: Added in logic for checking each tab, and adding drone checking somewhere in there. 
             *   - The way this works should be: Check each tab for a mission. If one exist in that place, we're just going to clear the queue -> just proceed to the grab mission task 
             *   - If not, then it continues onto the next kind
             *   - Drone mode is in there as a general "Hey, we gonna check to see if we can open a drone/have a drone running -> find it between missions (this is nice cause it allows users to dictate when they're going to go looking for a box in case of weather. red alert...)
             * 3: If we get to this point in the queue and we STILL haven't grabbed a mission, it means that we need to reroll for one. 
             *   - Logic will be the same here as before. Check to see what ones need to be rerolled if possible
             *
            */
        }
    }
}
