using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_MissionSelect
    {
        private static List<uint> MissionList = new();

        // Field to store the slider position (0-11 range)
        private static int sliderIndex = 0;

        // Property that converts slider index to actual job value
        public static uint SelectedJob
        {
            get => sliderIndex == 0 ? 0u : (uint)(sliderIndex + 7);
            set
            {
                if (value == 0)
                    sliderIndex = 0;
                else if (value >= 8 && value <= 18)
                    sliderIndex = (int)value - 7;
            }
        }

        private static string[] jobLabels = { "All Jobs", "CRP", "BSM", "ARM", "GSM", 
                                              "LTW", "WVR", "ALC", "CUL", "MIN", "BTN", "FSH" };

        public static void Draw()
        {
            if (ImGui.Button(T("Copy Selected")))
            {
                var ordered = MissionList.OrderBy(x => x);
                var c = string.Join(", ", ordered);
                c += ",";
                ImGui.SetClipboardText(c);
            }

            ImGui.SameLine();
            if (ImGui.Button(T("Clear")))
            {
                MissionList.Clear();
            }

            ImGui.SetNextItemWidth(200);
            ImGui.SliderInt("Filter##JobFilter", ref sliderIndex, 0, 11, jobLabels[sliderIndex]);

            ImGui.Text($"1, 2, 3, 4, 5");
            ImGui.Text(T("2, 3, 5, 6, 7"));

            using (var missionTable = ImRaii.Child("Mission Selection Window", new Vector2(0, 0)))
            {
                if (!missionTable.Success)
                    return;

                if (ImGui.BeginTable("Quick Mission Add", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn(T("ID"));
                    ImGui.TableSetupColumn(T("Job"));
                    ImGui.TableSetupColumn(T("Added"));
                    ImGui.TableSetupColumn(T("Level"));
                    ImGui.TableSetupColumn(T("Name"));

                    ImGui.TableHeadersRow();

                    foreach (var mission in CosmicHelper.SheetMissionDict)
                    {
                        var id = mission.Key;
                        var value = mission.Value;

                        if (SelectedJob != 0)
                        {
                            if (!value.Jobs.Contains(SelectedJob))
                            {
                                continue;
                            }
                        }

                        ImGui.PushID($"{id}_{value.Name}");
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{id}");

                        ImGui.TableNextColumn();
                        int jobCount = 0;
                        foreach (var job in value.Jobs)
                        {
                            var jobIcon = CosmicHelper.ClassInfoDict[job];
                            if (jobCount > 0)
                                ImGui.SameLine();
                            ImGui.Image(jobIcon.JobIcon.GetWrapOrEmpty().Handle, new(24, 24));
                            jobCount += 1;
                        }

                        ImGui.TableNextColumn();
                        bool added = MissionList.Contains(id);
                        ImGui.Checkbox($"##{id}_{value.Name}_checkbox", ref added);
                        if (ImGui.IsItemClicked())
                        {
                            if (MissionList.Contains(id))
                                MissionList.Remove(id);
                            else
                                MissionList.Add(id);
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{value.Level}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{value.Name}");


                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
