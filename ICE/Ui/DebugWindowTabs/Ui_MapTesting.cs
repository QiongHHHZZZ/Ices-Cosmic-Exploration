using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities;
using ICE.Utilities.Cosmic_Helper;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_MapTesting
    {
        private static int TableRow = 1;
        private static int posX = 0;
        private static int posY = 0;
        private static int posRadius = 0;

        public static unsafe void Draw()
        {
            ImGui.InputInt(T("TableId"), ref TableRow);

            var MapInfo = ExcelHelper.MarkerSheet;

            if (ImGui.Button($"Test Radius"))
            {
                var agent = AgentMap.Instance();

                int _x = MapInfo.GetRow((uint)TableRow).X.ToInt() - 1024;
                int _y = MapInfo.GetRow((uint)TableRow).Y.ToInt() - 1024;
                int _radius = MapInfo.GetRow((uint)TableRow).Radius.ToInt();
                IceLogging.Debug($"X: {_x} Y: {_y} Radius: {_radius}");

                // In cosmic zone use where you are; otherwise Sinus so debug tools still work out of hub.
                var territoryId = PlayerHelper.IsInCosmicZone()
                    ? Player.Territory.RowId
                    : CosmicMoonRegistry.Sinus.TerritoryId;
                Utils.SetGatheringRing(territoryId, _x, _y, _radius);
            }
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("Map X (Sheet)"), ref posX);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("Map Y (Sheet)"), ref posY);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("Map Radius"), ref posRadius);
            if (ImGui.Button($"Test Map Marker from coords"))
            {
                var agent = AgentMap.Instance();
                int _x = posX - 1024;
                int _y = posY - 1024;
                IceLogging.Debug($"X: {_x} Y: {_y}");

                Utils.SetGatheringRing(agent->CurrentTerritoryId, _x, _y, posRadius);
            }
        }
    }
}
