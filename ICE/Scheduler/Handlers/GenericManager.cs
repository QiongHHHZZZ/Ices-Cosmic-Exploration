using ECommons.Automation.LegacyTaskManager;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

namespace ICE.Scheduler.Handlers
{
    internal static unsafe class GenericManager
    {
        internal static TaskManager taskManager = new();
        static TaskManager TaskManager => taskManager;
        private static List<int> SlotsFilled { get; set; } = new();
        private static bool? ConfirmOrAbort(AddonRequest* addon)
        {
            if (addon->HandOverButton != null && addon->HandOverButton->IsEnabled)
            {
                new AddonMaster.Request((IntPtr)addon).HandOver();
                return true;
            }
            return false;
        }
        private static bool? TryClickItem(AddonRequest* addon, int i)
        {
            if (SlotsFilled.Contains(i)) return true;

            var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1).Address;

            if (contextMenu is null || !contextMenu->IsVisible)
            {
                var slot = i - 1;
                var unk = (44 * i) + (i - 1);

                ECommons.Automation.Callback.Fire(&addon->AtkUnitBase, false, 2, slot, 0, 0);

                return false;
            }
            else
            {
                ECommons.Automation.Callback.Fire(contextMenu, false, 0, 0, 1021003, 0, 0);
                Svc.Log.Debug($"Filled slot {i}");
                SlotsFilled.Add(i);
                return true;
            }
        }

        internal static void Tick()
        {
            if (EzThrottler.Throttle("DelayedTick"))
            {
                if (AddonHelper.IsAddonActive("WKSLottery") && C.GambaEnabled && SchedulerMain.State == IceState.Idle)
                    SchedulerMain.EnablePlugin();
            }
            //by Taurenkey https://github.com/PunishXIV/PandorasBox/blob/24a4352f5b01751767c7ca7f1d4b48369be98711/PandorasBox/Features/UI/AutoSelectTurnin.cs
            if (SchedulerMain.State != IceState.Idle)
            {
                if (GenericHelpers.TryGetAddonByName<AddonRequest>("Request", out var addon3))
                {
                    for (var i = 1; i <= addon3->EntryCount; i++)
                    {
                        if (SlotsFilled.Contains(addon3->EntryCount)) ConfirmOrAbort(addon3);
                        if (SlotsFilled.Contains(i)) return;
                        var val = i;
                        TaskManager.DelayNext($"ClickTurnin{val}", 10);
                        TaskManager.Enqueue(() => TryClickItem(addon3, val));
                    }
                }
                else
                {
                    SlotsFilled.Clear();
                    TaskManager.Abort();
                }
            }
        }

        private static bool PandoraGatherState = false;
        private static bool PandoraInteractState = false;
        private static bool PandoraCordialState = false;
        private static bool PandoraAutoTurnin = false;

        public static void StorePandoraStates()
        {
            // This is done this way becuase pandora tends to start locking up peoples machines after a long period of time
            PandoraGatherState = P.Pandora.GetFeatureEnabled("Pandora Quick Gather") ?? false;
            PandoraInteractState = P.Pandora.GetFeatureEnabled("Auto-interact with Gathering Nodes") ?? false;
            PandoraCordialState = P.Pandora.GetFeatureEnabled("Auto-Cordial") ?? false;
            PandoraAutoTurnin = P.Pandora.GetFeatureEnabled("Auto-select Turn-ins") ?? false;

            if (PandoraGatherState)
                P.Pandora.SetFeatureEnabled("Pandora Quick Gather", false);
            if (PandoraInteractState)
                P.Pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", false);
            if (PandoraCordialState && C.AutoCordial)
                P.Pandora.SetFeatureEnabled("Auto-Cordial", false);
            if (PandoraAutoTurnin)
                P.Pandora.SetFeatureEnabled("Auto-select Turn-ins", false);

            Pandora_WasRestored = false;
        }

        public static bool Pandora_WasRestored = false;

        public static void RestorePandoraStates()
        {
            if (PandoraGatherState)
                P.Pandora.SetFeatureEnabled("Pandora Quick Gather", true);
            if (PandoraInteractState)
                P.Pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", true);
            if (PandoraCordialState)
                P.Pandora.SetFeatureEnabled("Auto-Cordial", true);
            if (PandoraAutoTurnin)
                P.Pandora.SetFeatureEnabled("Auto-select Turn-ins", true);

            if (!Pandora_WasRestored)
                Pandora_WasRestored = true;
        }
    }
}
