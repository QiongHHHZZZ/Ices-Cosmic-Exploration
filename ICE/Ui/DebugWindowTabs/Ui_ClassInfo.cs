using static ICE.Localization.L10n;
﻿using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_ClassInfo
    {
        public static void Draw()
        {
            var classInfo = CosmicHelper.Cosmic_ClassInfo();

            using var tabBar = ImRaii.TabBar("Cosmic Class Info");
            if (!tabBar) return;

            foreach (var item in classInfo)
            {
                using var tabItem = ImRaii.TabItem($"Job {item.Key}");
                if (!tabItem) continue;

                ImGui.Text(T("Score: {0}", item.Value.Score));
                ImGui.Separator();

                using var table = ImRaii.Table($"ClassInfo_{item.Key}", 2,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
                if (!table) continue;

                // Set up columns
                ImGui.TableSetupColumn(T("Property"), ImGuiTableColumnFlags.WidthFixed, 150f);
                ImGui.TableSetupColumn(T("Value"), ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Current Stage
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(T("Current Stage"));
                ImGui.TableNextColumn();
                ImGui.Text($"{item.Value.Stage_Current}");

                // Next Stage
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(T("Next Stage"));
                ImGui.TableNextColumn();
                ImGui.Text($"{item.Value.Stage_Next}");

                // Experience entries
                foreach (var exp in item.Value.CurrentExp)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(exp.Value.Name);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{exp.Value.Current} / {exp.Value.Needed} ({T("Max: {0}", exp.Value.Max)})");
                }
            }
        }
    }
}
