using ECommons.Automation.LegacyTaskManager;
using ECommons.GameHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

namespace ICE.Scheduler.Handlers
{
    internal static unsafe class GenericManager
    {
        internal static TaskManager taskManager = new();
        static TaskManager TaskManager => taskManager;
        private static bool? ConfirmOrAbort(AddonRequest* addon)
        {
            if (addon->HandOverButton != null && addon->HandOverButton->IsEnabled)
            {
                new AddonMaster.Request((IntPtr)addon).HandOver();
                return true;
            }
            return false;
        }

        internal static void Tick()
        {
            if (EzThrottler.Throttle("DelayedTick"))
            {
                if (AddonHelper.IsAddonActive("WKSLottery") && C.GambaEnabled && SchedulerMain.State == IceState.Idle)
                    SchedulerMain.EnablePlugin();
            }
        }

        private static bool PandoraGatherState = false;
        private static bool PandoraInteractState = false;
        private static bool PandoraCordialState = false;

        public static void StorePandoraStates()
        {
            // This is done this way becuase pandora tends to start locking up peoples machines after a long period of time
            PandoraGatherState = P.Pandora.GetFeatureEnabled("Pandora Quick Gather") ?? false;
            PandoraInteractState = P.Pandora.GetFeatureEnabled("Auto-interact with Gathering Nodes") ?? false;
            PandoraCordialState = P.Pandora.GetFeatureEnabled("Auto-Cordial") ?? false;

            if (PandoraGatherState)
                P.Pandora.SetFeatureEnabled("Pandora Quick Gather", false);
            if (PandoraInteractState)
                P.Pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", false);
            if (PandoraCordialState && C.AutoCordial)
                P.Pandora.SetFeatureEnabled("Auto-Cordial", false);
        }

        public static void RestorePandoraStates()
        {
            if (PandoraGatherState)
                P.Pandora.SetFeatureEnabled("Pandora Quick Gather", true);
            if (PandoraInteractState)
                P.Pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", true);
            if (PandoraCordialState)
                P.Pandora.SetFeatureEnabled("Auto-Cordial", true);
        }
    }
}
