using ECommons.GameHelpers;
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

                StdVector<MissionEntry> basicList = default;
                if (wks->GetBasicMissions(&basicList))
                {
                    foreach (var mission in basicList)
                        allMissions.Add(mission.MissionUnitId);
                }

                StdVector<MissionEntry> provisionalList = default;
                if (wks->GetProvisionalMissions(&provisionalList))
                {
                    foreach (var mission in provisionalList)
                        allMissions.Add(mission.MissionUnitId);
                }

                foreach (var mission in wks->Data->MissionList.ToList())
                {
                    if (!allMissions.Contains(mission.MissionUnitId))
                        allMissions.Add(mission.MissionUnitId);
                }
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

                StdVector<MissionEntry> basicList = default;
                if (wks->GetBasicMissions(&basicList))
                {
                    foreach (var mission in basicList)
                        allMissions.Add(mission.MissionUnitId);
                }

                StdVector<MissionEntry> provisionalList = default;
                if (wks->GetProvisionalMissions(&provisionalList))
                {
                    foreach (var mission in provisionalList)
                        allMissions.Add(mission.MissionUnitId);
                }
            }

            return allMissions;
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
        internal unsafe static List<uint> AllMissions()
        {
            List<uint> allMissions = new();
            foreach (var mission in Basic_AvailableMissions())
                allMissions.Add(mission);

            foreach (var mission in Provisional_AvailableMissions())
                allMissions.Add(mission);

            foreach (var mission in VisibleMissions())
                if (!allMissions.Contains(mission))
                    allMissions.Add(mission);

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
    }
}
