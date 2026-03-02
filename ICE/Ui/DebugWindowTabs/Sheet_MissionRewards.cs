using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Sheet_MissionRewards
    {
        public static void Draw()
        {
            List<uint> SheetMissionIds = new() { 1, 22, 566, 625 };

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.Reorderable |
                            ImGuiTableFlags.Hideable |
                            ImGuiTableFlags.SizingFixedFit;

            if (ImGui.BeginTable("Mission Reward Sheet", 18, tableFlags))
            {
                // Setup columns - these names won't be directly visible
                ImGui.TableSetupColumn(T("Mission ID"));
                ImGui.TableSetupColumn(T("Column 0"));
                ImGui.TableSetupColumn(T("Column 1"));
                ImGui.TableSetupColumn(T("Column 2"));
                ImGui.TableSetupColumn(T("Column 3"));
                ImGui.TableSetupColumn(T("Column 4"));
                ImGui.TableSetupColumn(T("Column 5"));
                ImGui.TableSetupColumn(T("Column 6"));
                ImGui.TableSetupColumn(T("Column 7"));
                ImGui.TableSetupColumn(T("Column 8"));
                ImGui.TableSetupColumn(T("Column 9"));
                ImGui.TableSetupColumn(T("Column 10"));
                ImGui.TableSetupColumn(T("Column 11"));
                ImGui.TableSetupColumn(T("Column 12"));
                ImGui.TableSetupColumn(T("Column 13"));
                ImGui.TableSetupColumn(T("Column 14"));
                ImGui.TableSetupColumn(T("Column 15"));
                ImGui.TableSetupColumn(T("Column 16"));
                ImGui.TableSetupColumn(T("Column 17"));

                // Draw custom header row with tooltips
                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                // Column 0: Mission ID
                ImGui.TableSetColumnIndex(0);
                ImGui.TableHeader("Mission ID");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Row ID"));
                    ImGui.EndTooltip();
                }

                // Column 1
                ImGui.TableSetColumnIndex(1);
                ImGui.TableHeader("CosmoCredits");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown0"));
                    ImGui.EndTooltip();
                }

                // Column 2
                ImGui.TableSetColumnIndex(2);
                ImGui.TableHeader("PlanetCredits");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown1"));
                    ImGui.EndTooltip();
                }

                // Column 3
                ImGui.TableSetColumnIndex(3);
                ImGui.TableHeader("Reward [0]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown2"));
                    ImGui.EndTooltip();
                }

                // Column 4
                ImGui.TableSetColumnIndex(4);
                ImGui.TableHeader("Reward [1]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown3"));
                    ImGui.EndTooltip();
                }

                // Column 5
                ImGui.TableSetColumnIndex(5);
                ImGui.TableHeader("Reward [2]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown4"));
                    ImGui.EndTooltip();
                }

                // Column 6
                ImGui.TableSetColumnIndex(6);
                ImGui.TableHeader("Reward Amount");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown8"));
                    ImGui.EndTooltip();
                }

                // Column 7
                ImGui.TableSetColumnIndex(7);
                ImGui.TableHeader("Tool [0]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown9"));
                    ImGui.EndTooltip();
                }

                // Column 8
                ImGui.TableSetColumnIndex(8);
                ImGui.TableHeader("Tool [1]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown10"));
                    ImGui.EndTooltip();
                }

                // Column 9
                ImGui.TableSetColumnIndex(9);
                ImGui.TableHeader("Tool [2]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown11"));
                    ImGui.EndTooltip();
                }

                // Column 10
                ImGui.TableSetColumnIndex(10);
                ImGui.TableHeader("Exp Type [0]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown12"));
                    ImGui.EndTooltip();
                }

                // Column 11
                ImGui.TableSetColumnIndex(11);
                ImGui.TableHeader("Exp Type [1]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown13"));
                    ImGui.EndTooltip();
                }

                // Column 12
                ImGui.TableSetColumnIndex(12);
                ImGui.TableHeader("Exp Type [2]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown14"));
                    ImGui.EndTooltip();
                }

                // Column 13
                ImGui.TableSetColumnIndex(13);
                ImGui.TableHeader("Reward ItemID");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown15"));
                    ImGui.EndTooltip();
                }

                // Column 14
                ImGui.TableSetColumnIndex(14);
                ImGui.TableHeader("ExpModifier [0]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown16"));
                    ImGui.EndTooltip();
                }

                // Column 15
                ImGui.TableSetColumnIndex(15);
                ImGui.TableHeader("ExpModifier [1]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown17"));
                    ImGui.EndTooltip();
                }

                // Column 16
                ImGui.TableSetColumnIndex(16);
                ImGui.TableHeader("ExpModifier [2]");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown18"));
                    ImGui.EndTooltip();
                }

                // Column 17
                ImGui.TableSetColumnIndex(17);
                ImGui.TableHeader("? ? ?");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Unknown19"));
                    ImGui.EndTooltip();
                }

                foreach (var id in SheetMissionIds)
                {
                    if (Svc.Data.GetExcelSheet<WKSMissionReward>().TryGetRow(id, out var rewardSheet))
                    {
                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{rewardSheet.RowId}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.CosmoCredits}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.PlanetCredits}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[0]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[1]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[2]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ItemCount}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.Tool[0].Value.RowId}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.Tool[1].Value.RowId}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.Tool[2].Value.RowId}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[0]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[1]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ResearchReward[2]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.Item.RowId}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ExpModifier[0]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ExpModifier[1]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.ExpModifier[2]}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{rewardSheet.Unknown19}");
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
