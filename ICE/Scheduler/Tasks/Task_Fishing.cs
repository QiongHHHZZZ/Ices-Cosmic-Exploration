using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ICE.Ui.DebugWindowTabs;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using System.Globalization;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Utilities.GatheringHelper.GatheringUtil;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_Fishing
    {
        // Something to note. 42, 43, 85 are the conditions that you get while you're fishing
        // 43 and 85 are active while you're fishing
        // 42 is active when reeling in a fish
        // Something to consider, start fishing... (that's condition 42 when you start)
        // Whenever all the conditions are cleared, check the inventory for the frame, see if you have enough/meet the score

        private static FishingDebug _fishingDebug = null;

        // Mission-entry fishing TP state (supports auto-accept + manual-accept).
        private static uint _preparedMissionIdForEntryTp = 0;
        private static uint _activeMissionIdForEntryTp = 0;
        private static bool _runtimeEntryTpHandled = false;

        private static bool IsAutoHookLoaded() => P.AutoHook.Installed;

        // CN-MAINT: MissFisher compatibility relies on plugin presence check by internal name.
        private static bool IsMissFisherLoaded() => Utils.HasPlugin("MissFisher");

        internal static bool ShouldEnableAutoHookRuntime()
        {
            return IsAutoHookLoaded() && !IsMissFisherLoaded();
        }

        internal static void StartFishingByAvailablePlugin(string handle)
        {
            var autoHookLoaded = IsAutoHookLoaded();
            var missFisherLoaded = IsMissFisherLoaded();

            // CN-MAINT: Conflict policy intentionally mirrors RedAsteroid behavior.
            // If both AutoHook and MissFisher are loaded, do NOT auto-start casting.
            if (autoHookLoaded && missFisherLoaded)
            {
                if (EzThrottler.Throttle("FishingPluginConflictWarning", 10_000))
                {
                    IceLogging.Warning("检测到 AutoHook 与 MissFisher 同时启用。为避免冲突，本次不自动抛竿，请停用其中一个。", handle);
                }
                return;
            }

            if (autoHookLoaded)
            {
                // CN-MAINT: AutoHook start command.
                Svc.Commands.ProcessCommand("/ahstart");
                return;
            }

            if (missFisherLoaded)
            {
                if (EzThrottler.Throttle("MissFisherStartCommand", 6000))
                {
                    // CN-MAINT: MissFisher start command.
                    Svc.Commands.ProcessCommand("/mf cosmic");
                }
                return;
            }

            if (EzThrottler.Throttle("FishingPluginMissingWarning", 10_000))
            {
                IceLogging.Warning("未检测到 AutoHook 或 MissFisher，无法自动抛竿。", handle);
            }
        }

        internal static bool TryDailyRoutinesTeleportToFishingSpot(Vector3 targetPosition, string handle)
        {
            if (!C.FishingUseDailyRoutinesTP)
                return false;

            if (!Utils.HasPlugin("DailyRoutines"))
            {
                if (EzThrottler.Throttle("FishingMissingDailyRoutines", 8000))
                {
                    IceLogging.Warning("未检测到 Daily Routines，已回退原有寻路。", handle);
                }
                return false;
            }

            if (EzThrottler.Throttle("FishingDailyRoutinesTeleport", 2500))
            {
                var command = string.Format(
                    CultureInfo.InvariantCulture,
                    "/pdrtp pos {0:F2} {1:F2} {2:F2}",
                    targetPosition.X,
                    targetPosition.Y,
                    targetPosition.Z);

                Svc.Commands.ProcessCommand(command);
                IceLogging.Debug($"已尝试 Daily Routines 传送：{command}", handle);
                return true;
            }

            return false;
        }

        internal static void MarkMissionEntryPrepared(uint missionId)
        {
            _preparedMissionIdForEntryTp = missionId;
        }

        internal static void UpdateMissionEntryTpState(uint currentMissionId)
        {
            if (currentMissionId == 0)
            {
                _activeMissionIdForEntryTp = 0;
                _preparedMissionIdForEntryTp = 0;
                _runtimeEntryTpHandled = false;
                return;
            }

            if (_activeMissionIdForEntryTp != currentMissionId)
            {
                _activeMissionIdForEntryTp = currentMissionId;
                _runtimeEntryTpHandled = false;
            }
        }

        internal static bool IsInsideMissionFishingCircle(CosmicHelper.CosmicInfo mission)
        {
            if (mission.Radius <= 0)
                return false;

            var playerPos = Player.Position;
            var player2D = new Vector2(playerPos.X, playerPos.Z);
            var flagPos = new Vector2(mission.MapPosition.X, mission.MapPosition.Y);

            return Vector2.Distance(player2D, flagPos) <= mission.Radius;
        }

        public static void Enqueue()
        {
            // think the process should be:
            // check score
            // if score not complete, check if can craft
            // if craft not required, fish
            // wait for fishing to be done

            P.TaskManager.EnqueueMulti
                (
                    new(() => Task_CheckScore.Fish(), "Checking Score: Fishing"),
                    new(() => Task_Gather.UseFood(), "Checking for food usage"),
                    new(() => FishingCheck(), "Checking Fishing State")
                );
        }

        private static int StartedFishing = 0;

        private static unsafe bool? FishCheckV2()
        {
            string handle = "Fishing Task: State Check";
            if (Svc.Condition[ConditionFlag.Fishing])
            {
                IceLogging.Info("We're currently in the middle of fishing, so we're going to wait for us to complete");
                StartedFishing = 0;
                P.TaskManager.Enqueue(() => FinishFishing(), "Waiting for fishing to complete");
                return true;
            }
            else
            {
                IceLogging.Verbose("We're not currently fishing. Checking to see what we should do", handle);
                if (Player.Mounted || Player.IsJumping)
                {
                    if (EzThrottler.Throttle("Log message: Jump/Dismount", 1000))
                        IceLogging.Verbose("We're in the middle of dismounting/jumping, waiting");

                    Utils.Dismount();
                    return false;
                }

                if (Svc.Condition[ConditionFlag.ExecutingGatheringAction])
                {
                    if (EzThrottler.Throttle("Gathering Action Execution", 2000))
                        IceLogging.Info("We're currently executing some gathering action, so waiting");

                    return false;
                }

                var hasBait = false;
                uint firstBait = 0;
                foreach (var bait in GatheringUtil.MoonBaits)
                {
                    foreach (var baitId in bait.Value)
                    {
                        if (PlayerHelper.GetItemCount(baitId, out var baitCount) && baitCount > 0)
                        {
                            hasBait = true;
                            firstBait = baitId;
                            break;
                        }
                    }
                    if (hasBait) break;
                }

                if (!hasBait)
                {
                    IceLogging.Info("We are reporting to be out of bait, proceeding to abandon/turnin mission");
                    SchedulerMain.State = IceState.AbandonMission;
                    return true;
                }
                if (CosmicHelper.CurrentBait == 0)
                {
                    if (EzThrottler.Throttle("Bait Message", 2000))
                        IceLogging.Debug($"We are reporting we didn't have a bait equipped, so we're going to equip the first bait that we found: [{firstBait}]", handle);
                    P.AutoHook.SwapBaitById(firstBait);
                    return false;
                }

                if (CosmicHelper.CurrentMissionInfo.Attributes.HasFlag(MissionAttributes.Collectables))
                {
                    if (!PlayerHelper.HasStatusId(805))
                    {
                        if (EzThrottler.Throttle("Log Throttle for fishing", 2000))
                            IceLogging.Debug("We need to apply collector's glove, so we're doing so", handle);

                        if (!Player.IsBusy)
                        {
                            if (EzThrottler.Throttle("Attempting to turn on collectability"))
                                ActionManager.Instance()->UseAction(ActionType.Action, 4101);
                        }
                        return false;
                    }
                }
                if (_fishingDebug == null)
                {
                    _fishingDebug = new FishingDebug();
                }

                if (_fishingDebug.IsFishable())
                {
                    var currentSpot = GetCurrentFishingSpot();
                    if (EzThrottler.Throttle("Fishable Location message"))
                    {
                        if (currentSpot != null)
                        {
                            IceLogging.Verbose("We were told we're in a fishable location, so going to report back we are suppose to be able to fish\n" +
                                               $"Current spot: {currentSpot.FishingSpot:N2}", handle);
                        }
                        else
                        {
                            IceLogging.Verbose($"We are currently in a fishable spot... but the data isn't loaded correctly? MissionID: {CosmicHelper.CurrentLunarMission}", handle);
                        }
                    }

                    if (EzThrottler.Throttle("Start Fishing: AH", 2000))
                    {
                        IceLogging.Verbose("We are telling fishing plugin to start via command...", handle);
                        if (ShouldEnableAutoHookRuntime())
                            P.AutoHook.SetPluginState(true);
                        StartFishingByAvailablePlugin(handle);
                    }

                    if (EzThrottler.Throttle("Started Fishing Throttle", 1000))
                    {
                        StartedFishing += 1;
                        IceLogging.Verbose($"+1 to waiting for fishing to actually start... {StartedFishing}", handle);
                    }
                    if (StartedFishing > 4)
                    {
                        if (EzThrottler.Throttle("Start fishing Error", 2000))
                        {
                            IceLogging.Error("We apperently... didn't start fishing. Which isn't good. Checking to see if we have bait", handle);
                            P.AutoHook.SwapBaitById(firstBait);
                        }

                        if (EzThrottler.Throttle("Attempting to turn on collectability"))
                            ActionManager.Instance()->UseAction(ActionType.Action, 4101);
                    }
                }
                else
                {
                    IceLogging.Verbose("We apperently aren't facing toward the fishing hole... or not close enough to one that we can actually start. So going to attempt to fix it", handle);
                    if (_fishingDebug.FindFishableLocation(out var fishablePos, searchSteps: 64))
                    {
                        IceLogging.Info("We're not in a fishable angle, so going to face one", handle);
                        P.TaskManager.Enqueue(() => FacePosition(fishablePos.Value));
                        return true;
                    }
                    else
                    {
                        IceLogging.Debug("Our current fishing position isn't viable. So going to move to the next fishing spot");
                        var mission = CosmicHelper.CurrentMissionInfo;
                        var flag = mission.MapPosition;
                        var territoryId = mission.TerritoryId;

                        var nextFishingSpot = GetNextFishingSpot(territoryId, flag, Player.Position);
                        if (nextFishingSpot != null)
                        {
                            IceLogging.Info($"We found another fishing spot to move to! {nextFishingSpot.FishingSpot} | moving to it");
                            P.TaskManager.Tasks.Clear();
                            P.TaskManager.Enqueue(() => InitiateMoving(nextFishingSpot.FishingSpot), "Vnav moving to fishing");
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private static int BaitCounter = 0;

        private static unsafe bool? FishingCheck()
        {
            if (_fishingDebug == null)
            {
                _fishingDebug = new FishingDebug();
            }

            if (Player.Mounted)
            {
                Utils.Dismount();
                return false;
            }

            string handle = "[Standard Fishing: Fishing Check]";
            if (EzThrottler.Throttle("Throttling intro message", 1000))
            {
                IceLogging.Debug("Checking to see where we need to be here", handle);
            }
            bool hasBait = false;

            if (CosmicHelper.CurrentBait == 0)
            {
                if (EzThrottler.Throttle("Equipping bait"))
                {
                    foreach (var bait in GatheringUtil.MoonBaits)
                    {
                        foreach (var baitId in bait.Value)
                        {
                            if (PlayerHelper.GetItemCount(baitId, out var count) && count > 0)
                            {
                                P.AutoHook.SwapBaitById(baitId);
                                IceLogging.Debug($"Telling it to equip bait ID: {baitId}", handle);
                                return false;
                            }
                        }
                    }

                    IceLogging.Info("If we've gotten here, that means we're out of bait. Proceeding to turnin/abandon the mission");
                    SchedulerMain.State = IceState.AbandonMission;
                    P.TaskManager.Tasks.Clear();
                    return true;
                }
                return false;
            }

            // little check here for seeing if we have any baits
            foreach (var bait in GatheringUtil.MoonBaits)
            {
                foreach (var baitId in bait.Value)
                {
                    if (PlayerHelper.GetItemCount(baitId, out var count) && count > 0)
                    {
                        if (EzThrottler.Throttle("Throttling bait message", 1000))
                            IceLogging.Debug("We have the bait! Continuing onwards");
                        hasBait = true;
                        break;
                    }
                }
            }

            if (!hasBait)
            {
                IceLogging.Info("If we've gotten here, that means we're out of bait. Proceeding to turnin/abandon the mission");
                SchedulerMain.State = IceState.AbandonMission;
                P.TaskManager.Tasks.Clear();
                return true;
            }
            else if (CosmicHelper.CurrentMissionInfo.Attributes.HasFlag(MissionAttributes.Collectables) && !PlayerHelper.HasStatusId(805))
            {
                if (EzThrottler.Throttle("Log Throttle for fishing"))
                    IceLogging.Debug("We need to apply collector's glove", "Task_Start Fishing");

                if (!Player.IsBusy)
                {
                    if (EzThrottler.Throttle("Attempting to turn on collectability"))
                        ActionManager.Instance()->UseAction(ActionType.Action, 4101);
                }
                return false;
            }
            else if (!Svc.Condition[ConditionFlag.Gathering])
            {
                if (!_fishingDebug.IsFishable())
                {
                    if (_fishingDebug.FindFishableLocation(out var fishablePos, searchSteps: 64))
                    {
                        IceLogging.Info("We're not in a fishable angle, so going to face one", handle);
                        P.TaskManager.Tasks.Clear();
                        P.TaskManager.Enqueue(() => FacePosition(fishablePos.Value));
                        return true;
                    }
                    else
                    {
                        IceLogging.Debug("Our current fishing position isn't viable. So going to move to the next fishing spot");
                        var mission = CosmicHelper.CurrentMissionInfo;
                        var flag = mission.MapPosition;
                        var territoryId = mission.TerritoryId;

                        var nextFishingSpot = GetNextFishingSpot(territoryId, flag, Player.Position);
                        if (nextFishingSpot != null)
                        {
                            IceLogging.Info($"We found another fishing spot to move to! {nextFishingSpot.FishingSpot} | moving to it");
                            P.TaskManager.Tasks.Clear();
                            P.TaskManager.Enqueue(() => InitiateMoving(nextFishingSpot.FishingSpot), "Vnav moving to fishing");
                            return true;
                        }
                    }
                }
                else if (EzThrottler.Throttle("Starting to fish", 1000))
                {
                    IceLogging.Debug("Telling it to start fishing", handle);
                    if (ShouldEnableAutoHookRuntime())
                        P.AutoHook.SetPluginState(true);
                    StartFishingByAvailablePlugin(handle);
                }
                else if (EzThrottler.Throttle("Adding counter for bait not equipped"))
                {
                    BaitCounter++;
                    IceLogging.Debug($"Adding 1 to the counter. Counter is at: {BaitCounter}");
                    if (BaitCounter >= 2)
                    {
                        foreach (var bait in GatheringUtil.MoonBaits)
                        {
                            foreach (var baitId in bait.Value)
                            {
                                if (PlayerHelper.GetItemCount(baitId, out var count) && count > 0)
                                {
                                    P.AutoHook.SwapBaitById(baitId);
                                    IceLogging.Debug($"Telling it to equip bait ID: {baitId}", handle);
                                    return false;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            else
            {
                // Means we are fishing, all we need to do is enable autohook then wait for us to get the amount of fish we need
                if (ShouldEnableAutoHookRuntime())
                    P.AutoHook.SetPluginState(true);
                IceLogging.Info("We're starting to fish. So kicking it over to checking the fish items", handle);
                P.TaskManager.Insert(() => FinishFishing(), "Waiting till we actually start fishing", Utils.TaskConfig);
                BaitCounter = 0;
                return true;
            }
        }

        private static int collectableCounter = 0;
        private static unsafe bool? FinishFishing()
        {
            if (!Svc.Condition[ConditionFlag.Fishing])
            {
                IceLogging.Info("We're done fishing, time to go back to the score check", "[Fishing: Finished]");
                return true;
            }
            else
            {
                if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var yesNo) && yesNo.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Adding +1 to counter", 250))
                    {
                        collectableCounter += 1;
                    }
                    if (collectableCounter >= 2)
                    {
                        if (EzThrottler.Throttle("Selecting yes to collectables"))
                        {
                            yesNo.Yes();
                        }
                        return false;
                    }
                    return false;
                }
            }
            if (collectableCounter != 0)
                collectableCounter = 0;

            return false;
        }
        public static unsafe bool? FacePosition(Vector3 pos, float tolerance = 0.1f)
        {
            float currentRotation = Player.Rotation;

            // If rotation is still changing, wait for it to stabilize
            if (Math.Abs(Player.Rotation - currentRotation) > tolerance)
            {
                return false;
            }

            Vector3 direction = pos - Player.Position;
            float targetRotation = (float)Math.Atan2(direction.X, direction.Z);

            float angleDifference = GetShortestAngleDifference(Player.Rotation, targetRotation);

            if (Math.Abs(angleDifference) < tolerance)
            {
                return true;
            }

            if (EzThrottler.Throttle("Facing toward the fishing hole"))
            {
                var fwk = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();

                var autoRotateConfig = fwk->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
                var autoRotateOriginal = autoRotateConfig->Value.UInt;

                autoRotateConfig->Value.UInt = 1;

                IceLogging.Debug($"Telling the game to face you to: {pos}");
                Vector3 temp = pos;
                ActionManager.Instance()->AutoFaceTargetPosition(&temp);

                autoRotateConfig->Value.UInt = autoRotateOriginal;
            }

            return false;
        }
        public static float GetShortestAngleDifference(float currentAngle, float targetAngle)
        {
            float difference = targetAngle - currentAngle;

            // Normalize to [-π, π] for shortest path
            while (difference > Math.PI) difference -= (float)(2 * Math.PI);
            while (difference < -Math.PI) difference += (float)(2 * Math.PI);

            return difference;
        }
        public static FisherSpotInfo? GetNextFishingSpot(uint zone, Vector2 flag, Vector3 playerPosition)
        {
            // Check if the zone and flag exist
            if (!MoonFishingLocations.TryGetValue(zone, out var zoneData) ||
                !zoneData.TryGetValue(flag, out var spots) ||
                spots.Count == 0)
            {
                return null;
            }

            // Find the index of the spot close to the player (within distance of 2)
            int currentIndex = -1;
            for (int i = 0; i < spots.Count; i++)
            {
                float distance = Vector3.Distance(playerPosition, spots[i].FacePosition);
                if (distance < 2f)
                {
                    currentIndex = i;
                    break;
                }
            }

            // If a close spot was found, return the next one (cycling back to 0 if at the end)
            if (currentIndex != -1)
            {
                int nextIndex = (currentIndex + 1) % spots.Count;
                return spots[nextIndex];
            }

            // If no close spot found, return the first entry
            return spots[0];
        }
        public static FisherSpotInfo? GetCurrentFishingSpot()
        {
            var zone = Player.Territory.RowId;
            var mission = CosmicHelper.CurrentMissionInfo;
            var flag = mission.MapPosition;

            // Check if the zone and flag exist
            if (!MoonFishingLocations.TryGetValue(zone, out var zoneData) || !zoneData.TryGetValue(flag, out var spots) || spots.Count == 0)
            {
                return null;
            }

            var closestSpot = spots.MinBy(x => Player.DistanceTo(x.FishingSpot));
            return closestSpot;
        }
        public static bool? InitiateMoving(Vector3 fishingPos)
        {
            string handle = "[Fishing: Initiate Move]";

            UpdateMissionEntryTpState(CosmicHelper.CurrentLunarMission);

            if (Player.DistanceTo(fishingPos) < 3f)
                return true;

            if (!_runtimeEntryTpHandled)
            {
                _runtimeEntryTpHandled = true;

                bool alreadyPreparedBeforeAccept =
                    _activeMissionIdForEntryTp != 0 &&
                    _preparedMissionIdForEntryTp == _activeMissionIdForEntryTp;

                bool insideMissionCircle = IsInsideMissionFishingCircle(CosmicHelper.CurrentMissionInfo);

                if (!alreadyPreparedBeforeAccept &&
                    !insideMissionCircle &&
                    TryDailyRoutinesTeleportToFishingSpot(fishingPos, handle))
                {
                    return false;
                }
            }

            if (!P.Navmesh.IsReady())
            {
                Utils.VnavBuildInfo();
                return false;
            }
            else if (P.Navmesh.IsRunning())
            {
                P.TaskManager.Enqueue(() => !P.Navmesh.IsRunning());
                return true;
            }
            else
            {
                if (EzThrottler.Throttle("Navmesh movement"))
                {
                    IceLogging.DestinationLogs.Log(fishingPos);
                    P.Navmesh.PathfindAndMoveTo(fishingPos, false);
                }
                return false;
            }
        }
    }
}
