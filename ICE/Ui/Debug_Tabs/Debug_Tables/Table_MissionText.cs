using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.Debug_Tabs.Debug_Tables
{
    internal class Table_MissionText
    {
        private static Dictionary<string, HashSet<uint>> MissionText = new();

        private static string searchFilter = "";

        public static void Draw()
        {
            // Search filter input
            ImGui.Text(T("Search missions:"));
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##searchFilter", ref searchFilter, 256);

            if (ImGui.Button(T("Update all mission text")))
            {
                UpdateText();
            }

            ImGui.SameLine();
            if (ImGui.Button(T("Clear")))
            {
                searchFilter = "";
            }

            ImGui.Separator();

            // Filter and display results in a child window
            var filteredMissions = MissionText.Where(kvp =>
                kvp.Value.Count > 0 &&
                (string.IsNullOrEmpty(searchFilter) ||
                 kvp.Key.Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
            );

            // Show count of filtered results
            int totalCount = MissionText.Count(kvp => kvp.Value.Count > 0);
            int filteredCount = filteredMissions.Count();

            if (!string.IsNullOrEmpty(searchFilter))
            {
                ImGui.Text(T("Showing {0} of {1} missions", filteredCount, totalCount));
            }

            // Create a child window for the scrollable results
            if (ImGui.BeginChild("MissionResults", new System.Numerics.Vector2(0, 0), true))
            {
                foreach (var text in filteredMissions)
                {
                    // Create unique button ID using the text key
                    if (ImGui.Button($"Copy Ids##{text.Key}"))
                    {
                        // Convert HashSet to comma-separated string
                        string idsString = string.Join(", ", text.Value);
                        ImGui.SetClipboardText(idsString);
                    }
                    ImGui.SameLine();

                    // Show the HashSet values before the text
                    string idsDisplay = string.Join(", ", text.Value);
                    ImGui.Text($"[{idsDisplay}] {text.Key}");
                }
            }
            ImGui.EndChild();
        }

        private static void UpdateText()
        {
            foreach (var t in Svc.Data.GetExcelSheet<WKSMissionText>())
            {
                var rowId = t.RowId;
                string text = t.Text.ToString();

                if (string.IsNullOrEmpty(text))
                    continue;
                else
                {
                    if (MissionText.TryGetValue(text, out var missionText))
                    {
                        missionText.Add(rowId);
                    }
                    else
                    {
                        MissionText[text] = new HashSet<uint>() { rowId };
                    }
                }
            }
        }
    }
}
