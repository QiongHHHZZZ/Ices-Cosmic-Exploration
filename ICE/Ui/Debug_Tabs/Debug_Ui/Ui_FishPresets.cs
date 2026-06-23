using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Lua;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;
using static ICE.Localization.L10n;

namespace ICE.Ui.Debug_Tabs.Debug_Ui
{
    internal class Ui_FishPresets
    {
        private static string search_MissionId = string.Empty;
        private static string search_MissionName = "";

        private static int _selectedPlanetIndex = 0;

        private static uint selectedMission = 0;

        public static void Draw()
        {
            if (ImGui.BeginTable("Fish Editor | Window Selector", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit, ImGui.GetContentRegionAvail()))
            {
                ImGui.TableSetupColumn(T("Mission Selector"));
                ImGui.TableSetupColumn(T("Mission Details"), ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.InputText(T("Search Name"), ref search_MissionName, 100);
                ImGui.InputText(T("Search ID"), ref search_MissionId, 100);
                using (var missionSelection = ImRaii.Child("Mission Selection Child", new(300, ImGui.GetContentRegionAvail().Y)))
                {
                    ImGui.Separator();

                    MissionSelector();
                }

                ImGui.TableNextColumn();
                using (var missionDetails = ImRaii.Child("Mission Selection Info", ImGui.GetContentRegionAvail()))
                {
                    Mission_DetailedView();
                }

                ImGui.EndTable();
            }
        }

        private static void MissionSelector()
        {
            var list = CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Contains(18));

            if (search_MissionId != string.Empty)
            {
                list = list.Where(x => x.Key.ToString().Contains(search_MissionId));
            }

            if (!string.IsNullOrEmpty(search_MissionName))
            {
                var searchTerm = search_MissionName.Trim().ToLowerInvariant();
                list = list.Where(x => x.Value.Name.ToLowerInvariant().Contains(searchTerm));
            }

            foreach (var mission in list)
            {
                bool isSelected = mission.Key == selectedMission;
                if (mission.Value.Fish_Presets.Count == 0)
                {
                    ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    ImGui.SameLine();
                }

                if (ImGui.Selectable($"[{mission.Key}] - {mission.Value.Name}##{mission.Key}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                {
                    selectedMission = mission.Key;
                }
            }
        }

        private static void Mission_DetailedView()
        {
            if (CosmicHelper.SheetMissionDict.TryGetValue(selectedMission, out var missionInfo))
            {
                for (int i = 0; i < CosmicMoonRegistry.All.Length; i++)
                {
                    var moon = CosmicMoonRegistry.All[i];
                    ImGui.RadioButton(moon.DisplayName, ref _selectedPlanetIndex, i);

                    if (i < CosmicMoonRegistry.All.Length - 1)
                        ImGui.SameLine();
                }

                if (ImGui.Button(T("Export All Presets")))
                {
                    var clipboard = ExportAllMissions();
                    ImGui.SetClipboardText(clipboard);
                }

                ImGui.SameLine();

                if (ImGui.Button(T("Export Selected Mission")))
                {
                    var clipboard = ExportSelected();
                    ImGui.SetClipboardText(clipboard);
                }

                ImGui.SameLine();
                if (ImGui.Button(T("Export for wiki")))
                {
                    var clipboard = ExportWiki();
                    ImGui.SetClipboardText(clipboard);
                }

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"[{selectedMission}] {missionInfo.Name}");
                ImGui.SameLine();
                if (ImGui.Button(T("Copy Mission Name")))
                {
                    ImGui.SetClipboardText($"[{selectedMission}] {missionInfo.Name}");
                }
                if (ImGui.Button(T("Import New Preset")))
                {
                    var clipboard = ImGui.GetClipboardText();
                    if (clipboard.StartsWith("AH"))
                    {
                        missionInfo.Fish_Presets.Add(clipboard);
                    }
                    else
                    {
                        IceLogging.Error("Not a valid autohook preset.\n" +
                                         "Expected to start with: AH4_\n" +
                                         $"Text: {clipboard}");
                    }
                }
                ImGui.SameLine(0, 10);
                if (ImGui.Button(T("Temp Set Presets")))
                {
                    P.AutoHook.DeleteAllAnonymousPresets();
                    var preset = missionInfo.Fish_Presets[0];
                    if (preset.StartsWith("AHFOLDER"))
                    {
                        IceLogging.Verbose("We found a folder! We're going to import that", "AH Import");
                        P.AutoHook.CreateAndSelectAnonymousFolder(preset);
                    }
                    else
                    {
                        IceLogging.Verbose("Basic Fishing preset (bless) single import it is", "AH Import");
                        P.AutoHook.CreateAndSelectAnonymousPreset(preset);
                    }
                }
                if (ImGui.BeginTable("Autohook Presets", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    for (int i = 0; i < missionInfo.Fish_Presets.Count; i++)
                    {
                        var preset = missionInfo.Fish_Presets[i];
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{i}");

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(200);
                        ImGui.InputText($"##{i}_{preset}", ref preset, 1000);

                        ImGui.TableNextColumn();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"##{i}"))
                        {
                            missionInfo.Fish_Presets.RemoveAt(i);
                        }
                    }

                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text(T("No mission selected currently. Woops [{0}]", selectedMission));
            }
        }

        private static string ExportAllMissions()
        {
            var sb = new StringBuilder();
            var selectedMoon = CosmicMoonRegistry.All[_selectedPlanetIndex];
            foreach (var mission in CosmicHelper.SheetMissionDict
                .Where(x => x.Value.Jobs.Contains(18))
                .Where(x => x.Value.TerritoryId == selectedMoon.TerritoryId))
            {

                sb.AppendLine($"\t\tFishingPreset[{mission.Key}] = new()");
                sb.AppendLine("\t\t{");

                foreach (var preset in mission.Value.Fish_Presets)
                {
                    sb.AppendLine($"\t\t\t\"{preset}\",");
                }

                sb.AppendLine("\t\t};");
            }

            return sb.ToString();
        }

        private static string ExportSelected()
        {
            var sb = new StringBuilder();
            if (CosmicHelper.SheetMissionDict.TryGetValue(selectedMission, out var mission))
            {
                sb.AppendLine($"\t\tFishingPreset[{selectedMission}] = new()");
                sb.AppendLine("\t\t{");

                foreach (var preset in mission.Fish_Presets)
                {
                    sb.AppendLine($"\t\t\t\"{preset}\",");
                }

                sb.AppendLine("\t\t};");
            }

            return sb.ToString();
        }

        private static string ExportWiki()
        {
            var sb = new StringBuilder();
            if (CosmicHelper.SheetMissionDict.TryGetValue(selectedMission, out var mission))
            {
                sb.AppendLine($"### [{selectedMission}] {mission.Name}");
                foreach (var preset in mission.Fish_Presets)
                {
                    sb.AppendLine("```");
                    sb.AppendLine($"{preset}");
                    sb.AppendLine("```");
                }
            }

            return sb.ToString();
        }
    }
}
