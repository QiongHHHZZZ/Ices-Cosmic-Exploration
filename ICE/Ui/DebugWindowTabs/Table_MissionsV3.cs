using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;
using ICE.Utilities.ImGuiTools;
using static ICE.Localization.L10n;
using static ICE.Utilities.Cosmic_Helper.CosmicHelper;

namespace ICE.Ui.DebugWindowTabs;

internal class Table_MissionsV3
{
    private static Mission_Table? MissionTable;
    private static List<MissionInfo> TableItems = [];
    private static int ItemCount = 0;

    public static void Draw()
    {
        var bottomSpace = ImGui.GetTextLineHeight() + 6f;
        bottomSpace += 12f; // prevent the tabs from creating a scrollbar

        var available = ImGui_Ice.GetVisibleContentRegionAvail();
        Vector2 size = new(MathF.Max(1f, available.X), MathF.Max(1f, available.Y - bottomSpace));

        ImGui.Text(T("Item Count: {0}", ItemCount));
        if (ImGui.BeginChild("###MissionTableV3", size, false))
        {
            var showRedAlert = C.MissionFilter.HasFlag(MissionFilter.RedAlert);
            if (ImGui.Checkbox(T("Red Alert"), ref showRedAlert))
            {
                C.MissionFilter = showRedAlert
                    ? C.MissionFilter | MissionFilter.RedAlert
                    : C.MissionFilter & ~MissionFilter.RedAlert;
                C.SaveDebounced();
                MissionTable.SetFilterDirty();
            }

            try
            {
                if (MissionTable == null && CosmicHelper.SheetMissionDict.Count > 0)
                {
                    foreach (var mission in CosmicHelper.SheetMissionDict)
                    {
                        MissionInfo missionDetails = new() { Id = mission.Key };
                        TableItems.Add(missionDetails);
                    }
                    ItemCount = TableItems.Count();
                    MissionTable = new(TableItems);
                }
                var filterActive = MissionTable.FilteredItems.Count != 0 && MissionTable.FilteredItems.Count != ItemCount;
                var filterCount = filterActive ? $" (of {ItemCount})" : "";
                var height = ImGui.GetFrameHeight();
                MissionTable.Draw(height + 2f);
            }
            catch (Exception ex)
            {
                IceLogging.Error(ex.Message, "Drawing Mission Table");
            }
        }
        ImGui.EndChild();
    }
}
