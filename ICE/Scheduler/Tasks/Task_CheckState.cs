using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ICE.Sounds;
using ICE.Utilities.Cosmic_Helper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_CheckState
    {
        public static void Enqueue()
        {
            P.TaskManager.Enqueue(() => CheckStateV2(), "Checking to see what state we should be in");
        }

        private static void UpdateMissionState(uint missionId)
        {
            // Clearing the current mission modifiers.
            SchedulerMain.MissionState = MissionAttributes.None;

            // Grabbing the mission info from the dictionary entry
            var missionDictInfo = CosmicHelper.SheetMissionDict[missionId];

            // Updating the Mission state to be the same as the current mission that's fired.
            SchedulerMain.MissionState = missionDictInfo.Attributes;
        }
        private static bool? CheckStateV2()
        {
            string tag = "[Task: Check State]";

            var currentMode = C.SelectedMode;
            var currentMissionId = CosmicHelper.CurrentLunarMission;
            Task_Gather.UpdateMissionEntryTpState(currentMissionId);
            Task_Fishing.UpdateMissionEntryTpState(currentMissionId);
            if (CosmicHelper.CurrentLunarMission != 0)
                UpdateMissionState(currentMissionId);

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var lottery) && lottery.IsAddonReady)
            {
                IceLogging.Info("We are currently gambling at the wheel, so going to continue on with that and wait for it to finish", tag);
                SchedulerMain.State = IceState.Gambling;
                return true;
            }
            else if (currentMissionId != 0)
            {
                IceLogging.Verbose($"Currently in the middle of a mission: {CosmicHelper.CurrentLunarMission}. Checking the state of what we should do");
                if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
                {
                    IceLogging.Debug($"Mission Infomation was active, checking if a mission is timed out.");
                    if (CosmicHandler.IsMissionTimedOut())
                    {
                        // Mission time has reached 0, checking the score/aborting if necessary
                        IceLogging.Info("Mission is currently timed out. Going to abandon the mission state", "[Task: Check State]");
                        SchedulerMain.State = IceState.AbandonMission;
                        P.TaskManager.Tasks.Clear();
                        return true;
                    }
                    else
                    {
                        IceLogging.Debug($"Mission isn't timed out... checking other states");
                        UpdateMissionState(currentMissionId);
                        C.MissionConfig.TryGetValue(currentMissionId, out var config);

                        var s = SchedulerMain.MissionState;
                        bool dualMission = (s.HasFlag(MissionAttributes.Craft) && (s.HasFlag(MissionAttributes.Gather) || s.HasFlag(MissionAttributes.Fish)));
                        // In the middle of a dual mission. 
                        // First, checking to see if you're in the middle of a gathering or crafting action
                        if (C.OnlyGrabMission_Debug || config.ManualMode || UnsupportedMissions.Ids.Contains(currentMissionId))
                        {
                            // TODO: Remove this once properly coded
                            if (s.HasFlag(MissionAttributes.Fish))
                            {
                                IceLogging.Info("Currently not built in/supported yet. Swapping to manual mode");
                            }
                            else
                            {
                                IceLogging.Info($"You have either manual mode enabled, or you have OnlyGrabMission enabled. Swapping to manual mode state");
                            }
                            SchedulerMain.State = IceState.ManualMode;
                        }
                        else if (dualMission)
                        {
                            IceLogging.Info("We're in a dual craft mission, going to kick it over there", "[Task: Check State]");
                            Mission_Settings.ResetNodeCounter();
                            SchedulerMain.State = IceState.DualClass;
                        }
                        else if (Svc.Condition[ConditionFlag.Crafting] || P.Artisan.IsBusy())
                        {
                            IceLogging.Info("We are on a crafter, and either in the middle of crafting or need to start.", "[Task: Check State]");
                            SchedulerMain.State = IceState.Craft;
                        }
                        else if (Svc.Condition[ConditionFlag.Gathering])
                        {
                            Mission_Settings.ResetNodeCounter();
                            IceLogging.Info("On a gathering class, kicking over to the gathering action", "[Task: Check State]");
                            SchedulerMain.State = IceState.Gather;
                        }
                        else if (s.HasFlag(MissionAttributes.Fish))
                        {
                            IceLogging.Debug("We seem to be in the middle of a fishing mission. Going to reset/import all the presets");
                            Task_ExecuteMission.FishingTask(currentMissionId);
                            SchedulerMain.State = IceState.ScoreCheck;
                        }
                        else
                        {
                            // Not currently in the middle of an action, so time to check score and go from there.
                            IceLogging.Debug("Not in the middle of an action, swapping to score checking", "[Task_CheckState]");
                            SchedulerMain.State = IceState.ScoreCheck;
                        }

                        return true;
                    }
                }
                else
                {
                    // The mission info (the one that contains the timer + current score while a mission is active) isn't loaded. Going to fix that.
                    if (EzThrottler.Throttle("Attempting to open the mission information window"))
                    {
                        IceLogging.Info("Opening the mission information window, you're in the middle of one!", "[Check State]");
                        CosmicHelper.OpenStellarMission();
                    }
                    return false;
                }
            }
            else if (currentMode == ModeSelect.AgendaMode)
            {
                if (C.StopOnceHitLunarCredits)
                {
                    var territory = Player.Territory.RowId;
                    var itemId = CosmicHelper.PlanetCreditInfo[territory];

                    PlayerHelper.GetItemCount(itemId, out var credits);
                    if (credits >= C.LunarCreditsCap)
                    {
                        IceLogging.ChatInfo(T("You've either hit the Lunar Credit threshold, or gone above it.\nStopping I.C.E."), "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitCosmoCredits && !C.BuyItems)
                {
                    if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var hud) && hud.IsAddonReady && (hud.CosmoCredit >= C.CosmoCreditsCap))
                    {
                        IceLogging.ChatInfo(T("Stopping the plugin as you have {0} Cosmocredits.", hud.CosmoCredit), "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }

                IceLogging.Verbose("We're currently in agenda mode. We need to check to see if we have anything even in the agenda before we continue", tag);
                if (C.Cosmic_Agenda.Count > 0)
                {
                    IceLogging.Verbose($"We have a task list that we need to complete! Going to swap over to check and see what goal we need to complete");
                    P.TaskManager.Enqueue(() => AgendaCheck(), "Start Mode: Agenda Check");
                    return true;
                }
                else
                {
                    IceLogging.Error($"We're currently set in agenda mode, and it says we have none... if you believe this is an error, and you can prove that" +
                        $"you have setup your agenda to do as you want, please let me know <3", tag);
                    SchedulerMain.State = IceState.Idle;
                    P.TaskManager.Tasks.Clear();
                    return true;
                }
            }
            else
            {
                Mission_Settings.Mode = currentMode;
                var jobId = Mission_Settings.SelectedJob;

                IceLogging.Info("We have a pre-selected mode enabled. So we're just going to run that down till we're told to stop\n" +
                    $"Selected Mode: {currentMode}\n" +
                    $"Main job for basic missions: {jobId}", tag);
                IceLogging.Info("We're going to do our standard check of [If we need to stop] and [What we need to do before a mission]", tag);

                var cosmicClassInfo = CosmicHelper.Cosmic_ClassInfo;
                if (C.StopWhenLevel)
                {
                    var level = Player.GetLevel((Job)jobId);
                    if (level >= C.TargetLevel)
                    {
                        SchedulerMain.State = IceState.Idle;
                        IceLogging.ChatInfo(T("Stop At Player Level is enabled.\nYour current level is: {0} and Goal: {1}", Player.Level, C.TargetLevel), "[I.C.E.]");
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }

                        return true;
                    }
                }
                if (C.StopOnceHitCosmicScore)
                {
                    var currentScore = cosmicClassInfo[(uint)jobId].Score;
                    if (currentScore >= C.CosmicScoreCap)
                    {
                        SchedulerMain.State = IceState.Idle;
                        IceLogging.ChatInfo(T("Stop At Cosmic Score is enabled.\nYour current level is: {0} and Goal: {1}", currentScore, C.CosmicScoreCap), "[I.C.E.]");
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitLunarCredits)
                {
                    var territory = Player.Territory.RowId;
                    var itemId = CosmicHelper.PlanetCreditInfo[territory];

                    PlayerHelper.GetItemCount(itemId, out var credits);
                    if (credits >= C.LunarCreditsCap)
                    {
                        IceLogging.ChatInfo(T("You've either hit the Lunar Credit threshold, or gone above it.\nStopping I.C.E."), "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceHitCosmoCredits && !C.BuyItems)
                {
                    if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var hud) && hud.IsAddonReady && (hud.CosmoCredit >= C.CosmoCreditsCap))
                    {
                        IceLogging.ChatInfo(T("Stopping the plugin as you have {0} Cosmocredits.", hud.CosmoCredit), "[I.C.E.]");
                        SchedulerMain.State = IceState.Idle;
                        if (C.PlaySoundAlert)
                        {
                            _ = SoundPlayer.PlaySoundAsync();
                        }
                        return true;
                    }
                }
                if (C.StopOnceRelicFinished)
                {
                    var relicInfo = cosmicClassInfo[(uint)jobId];
                    bool potentionalTurnin = relicInfo.Stage_Current != relicInfo.Stage_Next;
                    bool canTurnin = true;

                    if (potentionalTurnin)
                    {
                        foreach (var exp in relicInfo.CurrentExp)
                        {
                            if (exp.Value.Current < exp.Value.Needed)
                            {
                                IceLogging.Verbose($"We're missing the following exp: {exp.Value.Name}.\n" +
                                    $"Need: {exp.Value.Needed}.\n" +
                                    $"Have: {exp.Value.Current}");
                                canTurnin = false;
                                break;
                            }
                        }
                        if (canTurnin)
                        {
                            if (C.TurninRelic)
                            {
                                IceLogging.Debug("Relic is at a point to be able to turnin! Going to do so later", tag);
                            }
                            else
                            {
                                IceLogging.ChatInfo(T("We're at the point we can turn in the relic! Please do so, or disable stop when at relic turnin"), tag);
                                SchedulerMain.State = IceState.Idle;
                                if (C.PlaySoundAlert)
                                {
                                    _ = SoundPlayer.PlaySoundAsync();
                                }
                                return true;
                            }
                        }
                    }
                    else
                    {
                        bool needToCap = false;
                        foreach (var exp in relicInfo.CurrentExp)
                        {
                            if (exp.Value.Current != exp.Value.Max)
                            {
                                IceLogging.Verbose($"Found an exp that we still need:\n" +
                                    $"[Current] = {exp.Value.Current}\n" +
                                    $"[Max] = {exp.Value.Max}\n" +
                                    $"[Kind] = {exp.Key}");
                                needToCap = true;
                                break;
                            }
                        }

                        if (!needToCap)
                        {
                            IceLogging.Info("We've reached the completed relic level wooo! Stopping for now", tag);
                            SchedulerMain.State = IceState.Idle;
                            if (C.PlaySoundAlert)
                            {
                                _ = SoundPlayer.PlaySoundAsync();
                            }
                            P.TaskManager.Tasks.Clear();
                            return true;
                        }
                    }
                }

                IceLogging.Info("We have passed all stop when checks. So going to just do a general check on what we need to do", tag);
                P.TaskManager.Enqueue(() => HubActivityCheck(), "Checking for reasons to go to hub");
            }

            return true;
        }
        private static bool? AgendaCheck()
        {
            string tag = "[Agenda Check]";

            var agenda = C.Cosmic_Agenda;
            var relicProgress = CosmicHelper.Cosmic_ClassInfo;
            PlayerHelper.GetItemCount(45690, out var creditAmount);
            int planetCreditAmount = 10000;
            var territory = Player.Territory.RowId;
            if (PlayerHelper.IsInCosmicZone())
            {
                var planetCreditId = CosmicHelper.PlanetCreditInfo[territory];
                PlayerHelper.GetItemCount(planetCreditId, out planetCreditAmount);
            }

            int dronebitAmount = 5000;
            if (PlayerHelper.IsInOizys())
            {
                var dronebitId = CosmicHelper.DronebitInfo[territory].creditId;
                PlayerHelper.GetItemCount(dronebitId, out dronebitAmount);
            }

            IceLogging.Verbose("Checking to see which one we're going to start (if any)", tag);

            foreach (var entry in agenda)
            {
                IceLogging.Verbose($"Checking:\t" +
                    $"Job: {entry.SelectedJob}\n" +
                    $"Agenda: {entry.SelectedMode}");

                var job = entry.SelectedJob;
                var relicInfo = relicProgress[job];

                var relicLevel = relicInfo.Stage_Current;
                var classScore = relicInfo.Score;
                var level = Player.GetLevel((Job)job);

                bool MaxLevelExp = true;
                foreach (var exp in relicInfo.CurrentExp)
                {
                    if (relicInfo.Stage_Current != relicInfo.Stage_Next)
                    {
                        MaxLevelExp = false;
                        break;
                    }

                    if (exp.Value.Current != exp.Value.Max)
                    {
                        MaxLevelExp = false;
                        break;
                    }
                }

                var goal = entry.SelectedOption;
                bool achieved = false;

                achieved = goal switch
                {
                    PlaylistOptions.SinusMax => relicLevel >= 9,
                    PlaylistOptions.PhaennaMax => relicLevel >= 14,
                    PlaylistOptions.OizysMax => relicLevel >= 17,
                    PlaylistOptions.SelectedRelicLv => relicLevel >= entry.SelectedRelicLevel,
                    PlaylistOptions.CreditAmount => creditAmount >= entry.CreditAmount,
                    PlaylistOptions.PlanetAmount => planetCreditAmount >= entry.PlanetAmount,
                    PlaylistOptions.DronebitAmount => dronebitAmount >= entry.DronebitAmount,
                    PlaylistOptions.ClassLevel => level >= entry.ClassLevel,
                    PlaylistOptions.ClassScore => classScore >= entry.ClassScore,
                    PlaylistOptions.ToolMaxExp => MaxLevelExp,
                    _ => true
                };

                if (!achieved)
                {
                    IceLogging.Info($"Priority has been found to achieve: {goal}. Going to aim to complete this goal", tag);
                    Mission_Settings.Mode = entry.SelectedMode;
                    Mission_Settings.SelectedJob = entry.SelectedJob;
                    P.TaskManager.Enqueue(() => HubActivityCheck(), "Checking for reason to go to hub");
                    return true;
                }
                else
                {
                    IceLogging.Info($"The following goal is complete, ignoring it: Goal: {goal} Job: {job}", tag);
                }
            }

            IceLogging.Info("We've actually finished our agenda! Congrats. Stopping the process", tag);
            P.TaskManager.Tasks.Clear();
            SchedulerMain.State = IceState.Idle;

            return true;
        }
        private static bool? HubActivityCheck()
        {
            string tag = "Task Check State: Hub Activity Check";

            var territoryId = Player.Territory.RowId;
            var relicProgress = CosmicHelper.Cosmic_ClassInfo;

            bool BuyDrones = false;
            bool GambaWheel = false;
            bool BuyItems = false;
            bool RepairVendor = false;
            bool TurninRelic = false;

            bool repairSelfGear = PlayerHelper.NeedsRepair(Char_Info.RepairPercent);
            bool repairAllGear = PlayerHelper.AnyNeedsRepair(Char_Info.RepairPercent) && Char_Info.RepairAllGear;

            var eventInfo = CosmicHandler.EventInfo();
            var worldState = eventInfo.Value.wksEvent;

            if (C.DisableHub_Critical && worldState is CosmicHandler.WKSEvents.RedAlert_Progressing)
            {
                IceLogging.Info("We currently have a red alert up, and we were told NOT to go to the hub for hub related activities, so we're not going to do so\n" +
                    "Progressing to grabbing missions", tag);
                SchedulerMain.State = IceState.GrabMission;

                return true;
            }

            IceLogging.Verbose($"Repair Class: {repairSelfGear} | Repair All: {repairAllGear}", tag);

            bool selfRepairCraft = Char_Info.SelfRepairCrafter && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
            bool selfRepairGathering = Char_Info.SelfRepairGather && CosmicHelper.GatheringJobList.Contains((uint)Player.Job);

            bool spiritbonded = C.SelfSpiritbondGather 
                && CosmicHelper.GatheringJobList.Contains((uint)Player.Job)
                && Task_Spiritbond.IsSpiritbondReadyAny();

            if (repairSelfGear || repairAllGear)
            {
                IceLogging.Verbose($"We were told we needed repairs, one of these should be true...", tag);
                if (Char_Info.RepairAtVendor)
                {
                    IceLogging.Debug("We were told to repair at the vendor, so we'll add that to the list of hub activities", tag);
                    RepairVendor = true;
                }
                else
                {
                    IceLogging.Verbose($"Self Repair Crafter: {selfRepairCraft} | Self Repair Gathering: {selfRepairGathering}");
                    if (selfRepairCraft || selfRepairGathering)
                    {
                        IceLogging.Debug("We were told that we can repair at ONE of these. So going to exit -> self repair", tag);
                        SchedulerMain.State = IceState.Repair;
                        return true;
                    }
                }
            }

            if (spiritbonded)
            {
                SchedulerMain.State = IceState.Spiritbond;
                return true;
            }

            if (CosmicHelper.DronebitInfo.TryGetValue(territoryId, out var dronebitAmount))
            {
                BuyDrones = C.Cosmodrone_Buy && Task_ArtifactSearch.CanBuyDroneBoxes();
                IceLogging.Verbose($"Buying drones? {BuyDrones}", tag);
            }
            if (CosmicHelper.PlanetCreditInfo.TryGetValue(territoryId, out var gambaCredits) && PlayerHelper.GetItemCount(gambaCredits, out var gambaAmount))
            {
                IceLogging.Verbose($"{C.GambaAtAmount} >= {gambaAmount} && Gamba between runs {C.GambaBetweenRuns}");
                GambaWheel = C.GambaAtAmount <= gambaAmount && C.GambaBetweenRuns;
            }
            if (C.BuyItems)
            {
                uint cosmoCreditId = 45690;
                if (PlayerHelper.GetItemCount(cosmoCreditId, out var creditAmount))
                {
                    BuyItems = creditAmount >= C.CosmoBuyAtAmount && Task_BuyCosmoItems.CanPurchaseAnyItem();
                }
            }
            if (C.TurninRelic)
            {
                var jobId = Mission_Settings.SelectedJob;
                var jobInfo = relicProgress[jobId];

                bool isUpgradable = jobInfo.Stage_Current != jobInfo.Stage_Next;
                var canUpgrade = true;
                foreach (var exp in jobInfo.CurrentExp)
                {
                    if (exp.Value.Current < exp.Value.Needed)
                    {
                        IceLogging.Verbose($"Missing {exp.Value.Needed} to turn in relic", tag);
                        canUpgrade = false;
                        break;
                    }
                }

                TurninRelic = isUpgradable && canUpgrade;
            }

            Task_HubActivities.ApplyPostHubReentryGuard(ref BuyDrones, ref GambaWheel);

            if (BuyDrones || GambaWheel || BuyItems || RepairVendor || TurninRelic)
            {
                IceLogging.Info("We have some reason to return back to the base so... we're doing so.\n" +
                                  $"Can Buy Drones: {BuyDrones}\n" +
                                  $"Gamba Wheel: {GambaWheel}\n" +
                                  $"Buying Cosmocredit Items: {BuyItems}\n" +
                                  $"Repair At Vendor: {RepairVendor}\n" +
                                  $"Turnin Relic: {TurninRelic}", tag);
                Task_HubActivities.CanBuyDrones = BuyDrones;
                Task_HubActivities.CanGamba = GambaWheel;
                Task_HubActivities.CosmoBuy = BuyItems;
                Task_HubActivities.RepairNpc = RepairVendor;
                Task_HubActivities.RelicTurnin = TurninRelic;
                SchedulerMain.State = IceState.HubReturn;
            }
            else
            {
                IceLogging.Info("We have no reason to go to the hub. So going to just proceed to see about grabbing missions");
                SchedulerMain.State = IceState.GrabMission;
            }

            return true;
        }
    }
}
