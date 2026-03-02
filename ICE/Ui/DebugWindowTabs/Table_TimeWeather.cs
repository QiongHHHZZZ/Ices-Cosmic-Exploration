using static ICE.Localization.L10n;
﻿using Lumina.Excel.Sheets;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_TimeWeather
    {
        public static unsafe void Draw()
        {
            var timeSheet = Svc.Data.GetExcelSheet<WKSMissionLotterySpecialCond>();

            if (ImGui.BeginTable($"WKSMission Time Sheet", 4, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn(T("Key"));
                ImGui.TableSetupColumn(T("Weather Required"));
                ImGui.TableSetupColumn(T("Start Hour"));
                ImGui.TableSetupColumn(T("End Hour"));

                ImGui.TableHeadersRow();

                foreach (var entry in timeSheet)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{entry.RowId}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.WeatherRequired.Value.Name}"); // Unknown 0

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.StartTimeHour}"); // Unknown 1

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.EndTimeHour}"); // Unknown 2

                }

                ImGui.EndTable();
            }
        }
    }
}
