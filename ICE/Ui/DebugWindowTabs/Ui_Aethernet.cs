using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Utilities.Cosmic_Helper;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_Aethernet
    {
        private static unsafe bool AethernetUnlocked(uint id)
        {
            var teleportAgent = Telepo.Instance();
            if (teleportAgent == null) return false;

            foreach (var info in teleportAgent->TeleportList)
            {
                if (info.AetheryteId == id)
                    return true;
            }
            return false;
        }

        private static unsafe void TeleportList()
        {
            var sheet = Svc.Data.GetExcelSheet<Aetheryte>()!;

            var teleportAgent = Telepo.Instance();
            if (teleportAgent != null)
            {
                foreach (var info in teleportAgent->TeleportList)
                {
                    var row = sheet.GetRow(info.AetheryteId);
                    var territory = row.Territory.Value.PlaceName.Value.Name.ToString() ?? "unknown";
                    var name = row.PlaceName.Value.Name.ToString() ?? "unknown";
                    IceLogging.Info($"ID: {info.AetheryteId}, Name: {name}, Territory: {territory}, IsAetheryte: {row.IsAetheryte}", "[Aethernet Debug]");
                }
            }

            var uiState = UIState.Instance();
            if (uiState != null)
            {
                var aethernetSheet = Svc.Data.GetExcelSheet<Aetheryte>()!;
                foreach (var row in aethernetSheet)
                {
                    if (row.IsAetheryte) continue; // skip main aetherytes
                    if (uiState->IsAetheryteUnlocked(row.RowId))
                    {
                        IceLogging.Info($"ID: {row.RowId}, Name: {row.PlaceName.Value.Name}, Territory: {row.Territory.Value.PlaceName.Value.Name}", "[Aethernet Debug]");
                    }
                }
            }
        }

        public static void Draw()
        {
            if (ImGui.Button("Get Active List"))
            {
                TeleportList();
            }

            if (ImGui.BeginTable("Aethershard Unlocked", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                foreach (var zone in Task_NavmeshMove.PlanetAethernet)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("ID:");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{zone.Key}");

                    foreach (var entry in zone.Value)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{entry.AethernetId}");

                        ImGui.TableNextColumn();
                        var unlocked = AethernetUnlocked(entry.AethernetId) ? FontAwesomeIcon.Check : FontAwesomeIcon.XmarksLines;
                        ImGuiEx.Icon(unlocked);
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
