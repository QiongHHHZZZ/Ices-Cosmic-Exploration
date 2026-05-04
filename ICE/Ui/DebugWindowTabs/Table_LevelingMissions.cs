using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Reflection;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_LevelingMissions
    {
        public static void Draw()
        {
            if (ImGui.BeginTable("Leveling Table", 14, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableSetupColumn("Planet");
                ImGui.TableSetupColumn("Lv");
                for (int i = 1; i < 12; i++)
                {
                    ImGui.TableSetupColumn($"##icon_{i}", ImGuiTableColumnFlags.WidthStretch);
                }

                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                // Column 0 — "Planet" with proper header styling
                ImGui.TableSetColumnIndex(0);
                ImGui.TableHeader("Planet");

                // Column 1 — "Lv"
                ImGui.TableNextColumn();
                ImGui.TableHeader("Lv");

                // Icon columns
                for (uint i = 8; i < 19; i++)
                {
                    ImGui.TableNextColumn();
                    ImGui.TableHeader("##icon_header_" + i);
                    ImGui.SameLine(0, 0);

                    float colWidth = ImGui.GetColumnWidth();
                    float cellHeight = ImGui.GetFrameHeight();
                    float imgSize = 20f;

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (colWidth - imgSize) * 0.5f);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (cellHeight - imgSize) * 0.5f);

                    var image = CosmicHelper.JobIconDict[i];
                    ImGui.Image(image.GetWrapOrEmpty().Handle, new Vector2(imgSize));
                }

                List<uint> sinusLeveling = CosmicHelper.QuickLevelList.Where(x => CosmicHelper.SheetMissionDict[x].TerritoryId == 1237).ToList();
                List<uint> phaennaLeveling = CosmicHelper.QuickLevelList.Where(x => CosmicHelper.SheetMissionDict[x].TerritoryId == 1291).ToList();
                List<uint> oizysLeveling = CosmicHelper.QuickLevelList.Where(x => CosmicHelper.SheetMissionDict[x].TerritoryId == 1310).ToList();

                List<uint> levels = new() { 10, 50, 90 };

                DrawPlanetLevelRows("ICE.Resources.Sinus_Ardorum.png", sinusLeveling, levels);
                DrawPlanetLevelRows("ICE.Resources.Phaenna.png", phaennaLeveling, levels);
                DrawPlanetLevelRows("ICE.Resources.Oizys.png", oizysLeveling, levels);

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
