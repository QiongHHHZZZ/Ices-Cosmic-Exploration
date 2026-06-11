using ECommons.Automation.UIInput;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.STD;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentWKSMission;

namespace ICE.Utilities
{
    internal class CosmicHandler
    {
        // WKSMission category tab index for "Tool Mastery Missions" (0 = Basic, 3 = Tool Mastery).
        internal const byte ToolMasteryTab = 3;

        internal unsafe static bool IsMissionTimedOut()
        {
            var c = UIState.Instance()->MassivePcContentTodo.Director;
            if (c != null)
            {
                var todo = c->MassivePcContentTodos[1];
                if (todo[1].Enabled)
                {
                    var t = todo[1];
                    var timeRemaining = t.EndTimestamp - Framework.GetServerTime();
                    if (timeRemaining > 0)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public enum WKSEvents
        {
            Mechops_Commenced = 0,
            RedAlert_Incoming = 1,
            RedAlert_Progressing = 2,
            MechOps_Issues = 5,
            MechOps_Deploying = 6,
            WaitingforDevStage = 8,
        }

        internal unsafe static (WKSEvents wksEvent, uint timer)? EventInfo()
        {
            var agent = AgentWKSAnnounce.Instance();
            if (agent == null || agent->Data == null)
                return null;

            var data = agent->Data;
            return ((WKSEvents)data->State, data->EndTime);
        }

        internal unsafe static List<uint> All_AvailableMissions()
        {
            List<uint> allMissions = new();

            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                foreach (var mission in Basic_AvailableMissions())
                    allMissions.Add(mission);

                foreach (var mission in Provisional_AvailableMissions())
                    allMissions.Add(mission);

                foreach (var mission in Critical_AvailableMissions())
                    allMissions.Add(mission);

                foreach (var mission in Mastery_AvailableMissions())
                    allMissions.Add(mission);
            }

            return allMissions;
        }
        internal unsafe static List<uint> Basic_AvailableMissions()
        {
            List<uint> allMissions = new();

            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                StdVector<MissionEntry> basicList = default;
                if (wks->GetBasicMissions(&basicList))
                {
                    foreach (var mission in basicList)
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
        }
        internal unsafe static List<uint> Provisional_AvailableMissions()
        {
            List<uint> allMissions = new();

            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                StdVector<MissionEntry> provisionalList = default;
                if (wks->GetProvisionalMissions(&provisionalList))
                {
                    foreach (var mission in provisionalList)
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
        }
        internal unsafe static List<uint> Critical_AvailableMissions()
        {
            List<uint> allMissions = new();

            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                StdVector<MissionEntry> criticalList = default;
                if (wks->GetCriticalMissions(&criticalList))
                {
                    foreach (var mission in criticalList)
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
        }
        internal unsafe static List<uint> Mastery_AvailableMissions()
        {
            List<uint> allMissions = new();
            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                if (Player.Territory.RowId != CosmicMoonRegistry.Auxesia.TerritoryId)
                    return allMissions;

                StdVector<MissionEntry> masterList = default;
                if (AgentWKSMissionEx.GetMasterMissions(wks, &masterList))
                {
                    foreach (var mission in masterList)
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
        }

        // Tool Mastery (tab 3) has no getter, so we must actually switch the UI to it (by clicking the
        // tab button - the agent SelectedTab field does not move the UI). Tab buttons are sequential:
        // Basic=17, Provisional=18, Critical=19, Tool Mastery=20 (17 + tab index). CurrentTab = AtkValues[27].
        // Returns true once we're on the requested tab.
        internal unsafe static bool EnsureCategoryTab(byte tab)
        {
            try
            {
                var addonPtr = Svc.GameGui.GetAddonByName("WKSMission");
                if (addonPtr.Address == nint.Zero)
                    return false;

                var addon = (AtkUnitBase*)addonPtr.Address;
                if (!addon->IsVisible)
                    return false;

                if (addon->AtkValues[27].UInt == tab)
                    return true;

                var btn = addon->GetComponentButtonById((uint)(17 + tab));
                if (btn != null && btn->IsEnabled && btn->AtkResNode->IsVisible())
                {
                    if (EzThrottler.Throttle("WKS Switch Category Tab", 250))
                        btn->ClickAddonButton(addon);
                }
                return false;
            }
            catch (Exception ex)
            {
                IceLogging.Error($"EnsureCategoryTab threw: {ex.Message}");
                return false;
            }
        }
        internal unsafe static List<uint> VisibleMissions()
        {
            List<uint> allMissions = new();

            if (GenericHelpers.TryGetAddonMaster<WKSMission>(out var wksMission) && wksMission.IsAddonReady)
            {
                var wks = AgentWKSMission.Instance();
                if (wks is null)
                    return allMissions;

                if (!wks->IsAgentActive())
                    return allMissions;

                foreach (var mission in wks->Data->MissionList.ToList())
                {
                    if (!allMissions.Contains(mission.MissionUnitId))
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
        }
        public class UiInfo
        {
            public int SelectedJobIndex { get; set; } = -1;
            public int SelectedTabIndex { get; set; } = -1;
            public int SelectedFilterIndex { get; set; } = -1;
        }
        internal static unsafe UiInfo HudInfo()
        {
            UiInfo selectedInfo = new();

            var wks = AgentWKSMission.Instance();
            if (wks is null)
                return selectedInfo;

            if (!wks->IsAgentActive())
                return selectedInfo;

            var data = wks->Data;
            if (data is null)
                return selectedInfo;

            selectedInfo.SelectedJobIndex = data->SelectedJobIndex;
            selectedInfo.SelectedTabIndex = data->SelectedTabIndex;
            selectedInfo.SelectedFilterIndex = data->SelectedFilterIndex;

            return selectedInfo;
        }
        internal static unsafe bool IsMissionGold(uint missionId)
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return false;

            var isGold = managerPtr->IsMissionGolded(missionId);

            return isGold;
        }
        internal static unsafe CosmicHelper.Status MissionStatus(uint missionId)
        {
            var managerPtr = WKSManager.Instance();
            if (managerPtr == null) return CosmicHelper.Status.None;

            bool isGold = managerPtr->IsMissionGolded(missionId);
            bool isCompleted = managerPtr->IsMissionCompleted(missionId);

            if (isGold)
                return CosmicHelper.Status.Gold;
            else if (isCompleted)
                return CosmicHelper.Status.Completed;
            else
                return CosmicHelper.Status.None;
        }
        internal static unsafe uint GetScore()
        {
            var manager = WKSManager.Instance();
            if (manager == null) return 0;

            var missionManager = manager->MissionModule;
            if (missionManager == null) return 0;

            var mission = manager->State.CurrentMission;
            return mission.Score;
        }

    }
}
