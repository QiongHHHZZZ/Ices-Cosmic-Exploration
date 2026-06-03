using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Reflection;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_LevelingMissions
    {
        public static void Draw()
        {
            if (ImGui.BeginTable(T("Leveling Table"), 14, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableSetupColumn(T("Planet"));
                ImGui.TableSetupColumn(T("Lv"));
                for (int i = 1; i < 12; i++)
                {
                    ImGui.TableSetupColumn($"##icon_{i}", ImGuiTableColumnFlags.WidthStretch);
                }

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                // Column 0 — "Planet" with proper header styling
                ImGui.TableSetColumnIndex(0);
                ImGui.TableHeader(T("Planet"));

                // Column 1 — "Lv"
                ImGui.TableNextColumn();
                ImGui.TableHeader(T("Lv"));

                // Icon columns
                for (uint i = 8; i < 19; i++)
                {
                    ImGui.TableNextColumn();
                    ImGui.TableHeader($"##icon_header_{i}");
                    ImGui.SameLine(0, 0);

                    float colWidth = ImGui.GetColumnWidth();
                    float cellHeight = ImGui.GetFrameHeight();
                    float imgSize = 20f;

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (colWidth - imgSize) * 0.5f);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (cellHeight - imgSize) * 0.5f);

                    var image = CosmicHelper.ClassInfoDict[i];
                    ImGui.Image(image.JobIcon.GetWrapOrEmpty().Handle, new Vector2(imgSize));
                }

                List<uint> levels = new() { 10, 50, 90 };

                // One row block per hub — Auxesia included when QuickLevelList has entries for territory 1319
                foreach (var moon in CosmicMoonRegistry.All)
                {
                    var levelingMissions = CosmicHelper.QuickLevelList
                        .Where(x => CosmicHelper.SheetMissionDict[x].TerritoryId == moon.TerritoryId)
                        .ToList();

                    DrawPlanetLevelRows(moon.IconResource, levelingMissions, levels);
                }

                ImGui.EndTable();
            }
        }

        private static void DrawPlanetLevelRows(string assetPath, List<uint> missionIds, List<uint> levels)
        {
            var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), assetPath).GetWrapOrEmpty();

            foreach (var lv in levels)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Image(texture.Handle, new Vector2(24));

                ImGui.TableNextColumn();
                ImGui.Text($"{lv}");

                for (uint i = 8; i < 19; i++)
                {
                    var mission = missionIds
                        .Select(id => (id, CosmicHelper.SheetMissionDict[id]))
                        .Where(x => x.Item2.Level == lv && x.Item2.Jobs.Contains(i))
                        .FirstOrDefault();

                    ImGui.TableNextColumn();
                    if (mission.id != 0)
                        ImGui.Text($"{mission.id}");
                    else
                        ImGui.TextDisabled("-");
                }
            }
        }
    }
}
