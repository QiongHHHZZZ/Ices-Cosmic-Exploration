using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.STD;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentWKSMission;

namespace ICE.Ui.DebugWindowTabs
{
    internal class CS_Missions
    {
        public static void Draw()
        {
            var missionList = CosmicHandler.All_AvailableMissions();
            var tabInfo = HudInfo();

            ImGui.Text($"Selected Job Index {tabInfo.SelectedJobIndex}");
            ImGui.Text($"Selected Tab Index {tabInfo.SelectedTabIndex}");
            ImGui.Text($"Selected Filter Index {tabInfo.SelectedFilterIndex}");

            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var x) && x.IsAddonReady)
            {
                for (int i = 0; i < x.SelectClass.Length; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();

                    if (ImGui.Button($"[{i}]"))
                    {
                        x.SelectClass[i].Select();
                    }
                }
            }

            if (ImGui.BeginTable("CS: Missions Avaialble", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Id");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Flag");

                ImGui.TableHeadersRow();

                foreach (var mission in missionList)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission, out var sheetInfo))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        foreach (var job in sheetInfo.Jobs)
                        {
                            ImGui.Image(CosmicHelper.ClassInfoDict[job].JobIcon.GetWrapOrEmpty().Handle, new(24, 24));
                            ImGui.SameLine();
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{sheetInfo.Name}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"Rank {sheetInfo.Rank}");
                    }
                }

                ImGui.EndTable();
            }
        }

        public unsafe static List<MissionEntry> Basic_ProvisionalMissions()
        {
            List<MissionEntry> allMissions = new();

            var wks = AgentWKSMission.Instance();
            if (wks is null)
                return allMissions;

            if (!wks->IsAgentActive())
                return allMissions;

            StdVector<MissionEntry> basicList = default;
            if (wks->GetBasicMissions(&basicList))
            {
                foreach (var mission in basicList)
                    allMissions.Add(mission);
            }

            StdVector<MissionEntry> provisionalList = default;
            if (wks->GetProvisionalMissions(&provisionalList))
            {
                foreach (var mission in provisionalList)
                    allMissions.Add(mission);
            }

            return allMissions;
        }

        public unsafe static List<MissionEntry> VisibleMissions()
        {
            List<MissionEntry> allMissions = new();

            var wks = AgentWKSMission.Instance();
            if (wks is null)
                return allMissions;

            if (!wks->IsAgentActive())
                return allMissions;

            if (wks->Data == null)
                return allMissions;

            foreach (var mission in wks->Data->MissionList.ToList())
                allMissions.Add(mission);

            return allMissions;
        }

        public class UiInfo
        {
            public int SelectedJobIndex { get; set; } = -1;
            public int SelectedTabIndex { get; set; } = -1;
            public int SelectedFilterIndex { get; set; } = -1;
        }

        public static unsafe UiInfo HudInfo()
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
    }
}
