using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using Lumina.Excel.Sheets;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_FishInfo
    {
        public static void Draw()
        {
            var fishMissions = CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Contains(18))
                .OrderBy(x => x.Key)
                .ToDictionary();

            if (ImGui.BeginTable(T("Fishing Info"), 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn(T("MissionID"));
                ImGui.TableSetupColumn(T("Mission Name"));
                ImGui.TableSetupColumn(T("Attribute"));
                ImGui.TableSetupColumn(T("Specific"));
                ImGui.TableSetupColumn(T("Total Req"));
                ImGui.TableSetupColumn(T("Variety Req"), ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableHeadersRow();

                foreach (var mission in fishMissions)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("{0}", mission.Key));

                    ImGui.TableNextColumn();
                    ImGui.Text(T("{0}", mission.Value.Name));

                    ImGui.TableNextColumn();
                    ImGui.Text(T("{0}", mission.Value.Attributes));

                    ImGui.TableNextColumn();
                    if (mission.Value.Gathering_Min.Count > 0)
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.Fish);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();

                            if (ImGui.BeginTable(T("Fish Item Info"), 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                            {
                                foreach (var fishItem in mission.Value.Gathering_Min)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    if (Svc.Data.GetExcelSheet<Item>().TryGetRow(fishItem.Key, out var fishInfo))
                                    {
                                        ImGui.Text(T("{0}", fishInfo.Name.ToString()));
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.Text(T("{0}", fishItem.Value));

                                    ImGui.TableNextColumn();
                                    ImGui.Text(T("{0}", fishItem.Key));
                                }

                                ImGui.EndTable();
                            };

                            ImGui.EndTooltip();
                        }
                    }
                    else
                    {
                        ImGui.Text("-");
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text(T("{0}", mission.Value.Fish_AmountRequired));

                    ImGui.TableNextColumn();
                    ImGui.Text(T("{0}", mission.Value.Fish_VarietyAmount));
                }

                ImGui.EndTable();
            }
        }
    }
}
