using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ICE.Utilities.Cosmic_Helper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_Repair
    {
        public static void Enqueue()
        {
            if (PlayerHelper.AnyNeedsRepair(Char_Info.RepairPercent) && Char_Info.RepairAllGear)
            {
                P.TaskManager.EnqueueMulti
                (
                    new(OpenSelfRepair, "Opening the self repair window"),
                    new(SelfRepair_All, "Executing the self repair"),
                    new(CloseRepair, "Closing Self Repair"),
                    new(() => SchedulerMain.State = IceState.GrabMission)
                );
            }
            else if (PlayerHelper.NeedsRepair(Char_Info.RepairPercent))
            {
                var currentJob = (uint)Player.Job;

                P.TaskManager.EnqueueMulti
                (
                    new(OpenSelfRepair, "Opening the self repair window"),
                    new(SelfRepair, "Executing the self repair"),
                    new(CloseRepair, "Closing Self Repair"),
                    new(() =>  SchedulerMain.State = IceState.GrabMission)
                );
            }
        }
        public static unsafe bool? HubCheck()
        {
            string tag = "[Hub Return]";

            if (!C.UseHubReturn)
            {
                IceLogging.Info("We were told we didn't wanna hub return, so we gonna respec this");
                return true;
            }

            if (C.AvoidStellarReturn && !C.AvoidStellarReturnExceptHub)
            {
                IceLogging.Info("Stellar Return is fully disabled, walking to hub instead", tag);
                return true;
            }

            if (CosmicMoonRegistry.TryGetHubCenter(Player.Territory.RowId, out var HubCenter))
            {
                Vector3 PlayerPos = Player.Position;

                if (Player.DistanceTo(HubCenter) < C.HubReturn_Distance)
                {
                    if (PlayerHelper.IsScreenReady())
                    {
                        IceLogging.Info("Player is in the range of the main hub area right now", tag);
                        return true;
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Waiting for screen to be ready", 2000))
                            IceLogging.Verbose("Waiting for screen to be ready", tag);
                    }
                }
                else
                {
                    //Not within the vicinity of the hub area, time to return
                    if (!Player.IsBusy)
                    {
                        if (EzThrottler.Throttle("Returning back to the moon base"))
                            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 26);
                    }
                }

                return false;
            }
            else
            {
                IceLogging.Error($"HEY. WE'RE MISSING THE HUB. THIS ISN'T GOOD. PLEASE ICE FIX THIS <3\n" +
                    $"From: Past Ice.", tag);
                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                return true;
            }
        }
        public static bool? Repair_PathTo()
        {
            string handle = "[Task_Repair: PathTo]";

            var zoneId = Player.Territory;

            if (NpcData.TryGetNpc(Player.Territory.RowId, NpcData.NpcType.Repair, out var npcEntry))
            {
                Vector3 randomPos = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, 0.5f);
                if (!Task_NavmeshMove.Task_NavTo(randomPos, distance: 5, npcLoc: npcEntry.Location_Npc).Value)
                {
                    if (EzThrottler.Throttle("Repair move message", 1000))
                        IceLogging.Verbose($"Pathing to repair NPC. Current distance: {Player.DistanceTo(npcEntry.Location_Npc)}", handle);
                }
                else
                {
                    IceLogging.Debug("We're close enough to the repair npc! Continuing on", handle);
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("Error message: NPC", 5000))
                    IceLogging.Error("Hey! We don't have this npc coded yet, which means I forgot bout it, could you let me know\n" +
                                     $"Planet Territory ID: {Player.Territory.RowId}", handle);
            }

            return false;
        }
        public static unsafe bool? RepairAtNpc()
        {
            var zoneId = Player.Territory;
            IGameObject? gameObject = null;

            if (NpcData.TryGetNpc(Player.Territory.RowId, NpcData.NpcType.Repair, out var npcEntry))
            {
                Utils.TryGetObjectByDataId(npcEntry.NpcId, out gameObject);
            }

            var currentTarget = Svc.Targets.Target;
            var repairAmount = Char_Info.RepairPercent;

            IceLogging.Debug($"{!PlayerHelper.NeedsRepair(99.9f)} | {!(PlayerHelper.AnyNeedsRepair(99.9f) && Char_Info.RepairAllGear)}");

            if (!PlayerHelper.NeedsRepair(99.9f) && !(PlayerHelper.AnyNeedsRepair(99.9f) && Char_Info.RepairAllGear))
            {
                IceLogging.Debug("Repair Complete! Finishing task and closing window");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Repair>("Repair", out var repair) && repair.IsAddonReady)
            {
                if (PlayerHelper.NeedsRepair(repairAmount) || (PlayerHelper.AnyNeedsRepair(99.9f) && Char_Info.RepairAllGear))
                {
                    if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var Yesno) && Yesno.IsAddonReady)
                    {
                        if (FrameThrottler.Throttle("Saying yes to the gil"))
                            Yesno.Yes();
                    }
                    else if (EzThrottler.Throttle("Sending Repair Request", 2000))
                    {
                        if (C.RepairAllGear)
                        {
                            IceLogging.Debug("Firing off callbacl to repair all", "Self Repair Task: All");
                            GenericHandlers.FireCallback("Repair", true, 1);
                        }
                        else
                        {
                            IceLogging.Debug("Repair Callback", "[Self Repair Task]");
                            repair.RepairAll();
                        }
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectIconString", out var iconString) && GenericHelpers.IsAddonReady(iconString))
            {
                if (FrameThrottler.Throttle("Firing off repair string"))
                {
                    IceLogging.Debug("Selecting repair from vendor", "[Self Repair Task]");
                    ECommons.Automation.Callback.Fire(iconString, true, 6);
                }
            }
            else
            {
                if (EzThrottler.Throttle("Attempting to target the repair NPC + Interact"))
                {
                    Utils.TargetgameObject(gameObject);
                    Utils.InteractWithObject(gameObject);
                }
            }

            return false;
        }
        public unsafe static bool? OpenSelfRepair()
        {
            if (C.Stop_DarkMatter && PlayerHelper.GetItemCount(Utils.DarkMatter_8Id, out var dmCount) && dmCount < Char_Info.Minimum_DarkMatter)
            {
                IceLogging.ChatInfo(T("Dark matter is below the minimum we want to keep, so self repair is stopping."), "[I.C.E.] Task: Self Repair");
                SchedulerMain.State = IceState.Idle;
                P.TaskManager.Tasks.Clear();
                return true;
            }

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var x) && GenericHelpers.IsAddonReady(x))
            {
                return true;
            }

            if (EzThrottler.Throttle("Opening Self Repair", 1000))
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 6);
            return false;
        }
        public unsafe static bool? SelfRepair()
        {
            if (!PlayerHelper.NeedsRepair(Char_Info.RepairPercent))
            {
                return true;
            }
            else if (Svc.Condition[ConditionFlag.Mounted])
            {
                if (EzThrottler.Throttle("Attempting to dismount for repairing"))
                {
                    IceLogging.Debug("Dismounting for self repair", "[Self Repair Task]");
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesno", out var addon) && GenericHelpers.IsAddonReady(addon))
            {
                if (FrameThrottler.Throttle("SelectYesnoThrottle", 300))
                {
                    IceLogging.Debug("SelectYesno Callback", "Self Repair Task");
                    ECommons.Automation.Callback.Fire(addon, true, 0);
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var addon2) && GenericHelpers.IsAddonReady(addon2))
            {
                if (FrameThrottler.Throttle("Firing off repair request", 300))
                {
                    IceLogging.Debug("Repair Callback", "[Self Repair Task]");
                    ECommons.Automation.Callback.Fire(addon2, true, 0);
                }
            }
            return false;
        }
        public unsafe static bool? SelfRepair_All()
        {
            string tag = "Self Repair: All";

            if (!PlayerHelper.AnyNeedsRepair(Char_Info.RepairPercent))
            {
                IceLogging.Debug("All gear has been repaired, continuing", tag);
                return true;
            }
            else if (Svc.Condition[ConditionFlag.Mounted])
            {
                if (EzThrottler.Throttle("Attempting to dismount for repairing"))
                {
                    IceLogging.Debug("Dismounting for self repair", tag);
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesno", out var addon) && GenericHelpers.IsAddonReady(addon))
            {
                if (FrameThrottler.Throttle("SelectYesnoThrottle", 300))
                {
                    IceLogging.Debug("SelectYesno Callback", tag);
                    ECommons.Automation.Callback.Fire(addon, true, 0);
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var addon2) && GenericHelpers.IsAddonReady(addon2))
            {
                if (FrameThrottler.Throttle("Firing off repair request", 300))
                {
                    IceLogging.Debug("Repair Callback", tag);
                    ECommons.Automation.Callback.Fire(addon2, true, 1);
                }
            }
            return false;
        }
        public unsafe static bool? CloseRepair()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var Yesno) && Yesno.IsAddonReady)
            {
                if (FrameThrottler.Throttle("Closing surprise repair window"))
                {
                    ECommons.Automation.Callback.Fire(Yesno.Base, true, -1);
                }
            }
            else if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var repairWindow))
            {
                if (GenericHelpers.IsAddonReady(repairWindow))
                {
                    if (EzThrottler.Throttle("Attempting to close out the repair window", 300))
                    {
                        IceLogging.Debug("Closing the repair window", "[Repair Task]");
                        ECommons.Automation.Callback.Fire(repairWindow, true, -1);
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }


    }
}
