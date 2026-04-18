using Dalamud.Game.ClientState.Conditions;
using ECommons.Automation;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using Callback = ECommons.Automation.Callback;
using Time = (int start, int end);

namespace ICE.Scheduler.Handlers;

internal static unsafe class PlayerHandlers
{
    private static readonly uint stellarSprintID = 4398;

    public static unsafe bool IsMoving()
    {
        return AgentMap.Instance()->IsPlayerMoving;
    }
    public static bool PlayerFirstCosmicZone = false;

    internal static unsafe void Tick()
    {
        if (!P.overlayWindow.IsOpen && PlayerHelper.IsInCosmicZone() && C.ShowOverlay)
            P.overlayWindow.IsOpen = true;

        if (C.MoonSprint && PlayerHelper.IsInCosmicZone()
         && !PlayerHelper.HasStatusId(stellarSprintID) && Svc.Condition[ConditionFlag.NormalConditions]
         && IsMoving() && PlayerHelper.UsingSupportedJob())
            UseSprint();

        if ((!PlayerHelper.IsInCosmicZone()) && SchedulerMain.State != IceState.Idle)
        {
            DisablePlugin();
        }

        if (PlayerHelper.HasStatusId(4409) && C.RemoveStellarStatus)
        {
            if (EzThrottler.Throttle("Turning off Stellar Buff"))
                StatusManager.ExecuteStatusOff(4409);
        }

        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("WKSReward", out var addon) && GenericHelpers.IsAddonReady(addon))
        {
            if (EzThrottler.Throttle("Closing the reward popup"))
            {
                GenericHandlers.FireCallback("WKSReward", true, -1);
            }
        }

        if (C.StartUponEnterMoon)
        {
            if (PlayerHelper.IsInCosmicZone() && !PlayerFirstCosmicZone)
            {
                PlayerFirstCosmicZone = true;
                P.TaskManager.EnqueueDelay(1000);
                P.TaskManager.Enqueue(() => InitiateFirstCosmic(), "Waiting for player to be available");
            }
            if (PlayerFirstCosmicZone && !PlayerHelper.IsInCosmicZone())
                PlayerFirstCosmicZone = false;
        }

    }

    private static bool? InitiateFirstCosmic()
    {
        if (Player.Interactable && Player.Available)
        {
            SchedulerMain.EnablePlugin();
            return true;
        }

        return false;
    }

    internal static void DisablePlugin()
    {
        if (SchedulerMain.State != IceState.Idle)
        {
            P.TaskManager.Abort();
            SchedulerMain.DisablePlugin();
        }
        PlayerFirstCosmicZone = false;
    }

    private static void UseSprint()
    {
        var am = ActionManager.Instance();
        var isSprintReady = am->GetActionStatus(ActionType.GeneralAction, 4) == 0;

        if (isSprintReady) am->UseAction(ActionType.GeneralAction, 4);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Hours[long], Minutes[long]</returns>
    private static (long, long) GetEorzeaTime()
    {
        var eorzeaTime = Framework.Instance()->ClientTime.EorzeaTime;
        long hours = eorzeaTime / 3600 % 24;
        long minutes = eorzeaTime / 60 % 60;
        return (hours, minutes);
    }
}
