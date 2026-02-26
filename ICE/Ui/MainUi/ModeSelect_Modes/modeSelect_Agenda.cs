using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;
using TerraFX.Interop.Windows;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class modeSelect_Agenda
    {
        public static List<uint> JobOptions = new() { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

        public static List<PlaylistOptions> PlaylistOptionsOrder = new()
        {
            PlaylistOptions.None,
            PlaylistOptions.SinusMax,
            PlaylistOptions.PhaennaMax,
            PlaylistOptions.OizysMax,
            PlaylistOptions.ToolMaxExp,
            PlaylistOptions.SelectedRelicLv,

            PlaylistOptions.CreditAmount,
            PlaylistOptions.PlanetAmount,
            PlaylistOptions.DronebitAmount,

            PlaylistOptions.ClassLevel,
        };

        public static uint SelectedJob = 8;
        public static PlaylistOptions SelectedOption = PlaylistOptions.None;

        public static void Draw()
        {
            var selectedJobIcon = CosmicHelper.JobIconDict[SelectedJob];
            var selectedJobName = CosmicHelper.GetJobName(SelectedJob);

            ImGui.Image(selectedJobIcon.GetWrapOrEmpty().Handle, new Vector2(20, 20));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);

            if (ImGui.BeginCombo("##JobCombo", selectedJobName))
            {
                using (var table = ImRaii.Table("JobSelectionTable", 2, ImGuiTableFlags.BordersInnerV))
                {
                    if (table)
                    {
                        ImGui.TableSetupColumn(T("Icon"), ImGuiTableColumnFlags.WidthFixed, 24);
                        ImGui.TableSetupColumn(T("Name"), ImGuiTableColumnFlags.WidthStretch);

                        foreach (var jobId in JobOptions)
                        {
                            var jobIcon = CosmicHelper.JobIconDict[jobId];
                            var jobName = CosmicHelper.GetJobName(jobId);
                            bool isSelected = jobId == SelectedJob;

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();

                            // Icon column
                            ImGui.Image(jobIcon.GetWrapOrEmpty().Handle, new Vector2(20, 20));

                            ImGui.TableNextColumn();

                            // Name column with selectable
                            if (ImGui.Selectable($"{jobName}##{jobName}_{jobId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                SelectedJob = jobId;
                            }

                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            var optionName = CosmicHelper.PlaylistOptionString(SelectedOption);

            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo("##Playlist Options", optionName))
            {
                foreach (PlaylistOptions option in Enum.GetValues<PlaylistOptions>())
                {
                    var displayName = CosmicHelper.PlaylistOptionString(option);
                    bool isSelected = SelectedOption == option;

                    if (ImGui.Selectable($"{displayName}##{option}", isSelected))
                    {
                        SelectedOption = option;
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            using (ImRaii.Disabled(SelectedOption == PlaylistOptions.None))
            {
                if (ImGui.Button(T("Add to Cosmic Agenda")))
                {
                    var mode = ModeSelect.Standard;
                    if (SelectedOption is PlaylistOptions.SinusMax or PlaylistOptions.PhaennaMax or PlaylistOptions.OizysMax or PlaylistOptions.SelectedRelicLv)
                    {
                        mode = ModeSelect.RelicMode;
                    }
                    else if (SelectedOption is PlaylistOptions.ClassLevel)
                    {
                        mode = ModeSelect.LevelMode;
                    }

                    var newAgenda = new AgendaInfo()
                    {
                        SelectedOption = SelectedOption,
                        SelectedJob = SelectedJob,
                        SelectedMode = mode
                    };

                    C.Cosmic_Agenda.Add(newAgenda);
                    C.SaveDebounced();
                }
            }

            CosmicAgendaTable();
        }

        private static string ModeSelectString(ModeSelect mode)
        {
            return mode switch
            {
                ModeSelect.Standard => T("Standard"),
                ModeSelect.RelicMode => T("Relic Grind Mode"),
                ModeSelect.LevelMode => T("Leveling Mode"),
                // ModeSelect.ScoreMode => "Scoring Mode",
                ModeSelect.AgendaMode => T("Cosmic Agenda Mode"),
                _ => T("??? {0}", mode)
            };
        }

        private static ImGuiEx.RealtimeDragDrop<AgendaInfo>? _dragDrop;

        private static void CosmicAgendaTable()
        {
            // Initialize drag/drop if it doesn't exist
            _dragDrop ??= new ImGuiEx.RealtimeDragDrop<AgendaInfo>(
                "CosmicAgendaDragDrop",
                (info) => $"{info.SelectedJob}_{info.SelectedOption}_{info.GetHashCode()}", // Unique ID generator
                smallButton: false
            );

            _dragDrop.Begin(); // Step 1: Begin drag/drop tracking

            using (var PlaylistTable = ImRaii.Table("Cosmic Agenda Table", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
            {
                if (PlaylistTable)
                {
                    ImGui.TableSetupColumn("##Reorder");
                    ImGui.TableSetupColumn(T("Job"));
                    ImGui.TableSetupColumn(T("Agenda"));
                    ImGui.TableSetupColumn(T("Run Until.."));
                    ImGui.TableSetupColumn(T("Mode Select"));
                    ImGui.TableSetupColumn(T("Remove"));

                    ImGui.TableHeadersRow();

                    for (int i = 0; i < C.Cosmic_Agenda.Count; i++)
                    {
                        ImGui.PushID(i);

                        var agendaInfo = C.Cosmic_Agenda[i];
                        var selectedOption = agendaInfo.SelectedOption;

                        ImGui.TableNextRow();
                        _dragDrop.NextRow(); // Step 2: Mark new row
                        _dragDrop.SetRowColor(agendaInfo); // Optional: Highlight dragged row

                        ImGui.TableSetColumnIndex(0);
                        // Step 3: Draw the drag/drop button
                        _dragDrop.DrawButtonDummy(agendaInfo, C.Cosmic_Agenda, i);

                        ImGui.TableNextColumn();
                        var jobImage = CosmicHelper.JobIconDict[agendaInfo.SelectedJob];
                        float zoom = 0.15f;

                        if (ImGui.ImageButton(
                            jobImage.GetWrapOrEmpty().Handle,
                            new Vector2(20, 20),
                            new Vector2(zoom, zoom),
                            new Vector2(1 - zoom, 1 - zoom)
                        ))
                        {
                            ImGui.OpenPopup("Job Selection");
                        }
                        using (var popup = ImRaii.Popup("Job Selection"))
                        {
                            if (popup)
                            {
                                using (var table = ImRaii.Table("JobTable", 2, ImGuiTableFlags.BordersInnerV))
                                {
                                    if (table)
                                    {
                                        ImGui.TableSetupColumn(T("Icon"), ImGuiTableColumnFlags.WidthFixed, 24);
                                        ImGui.TableSetupColumn(T("Name"), ImGuiTableColumnFlags.WidthStretch);

                                        foreach (var jobId in JobOptions)
                                        {
                                            var jobIcon = CosmicHelper.JobIconDict[jobId];
                                            var jobName = CosmicHelper.GetJobName(jobId);
                                            bool isSelected = jobId == SelectedJob;

                                            ImGui.TableNextRow();
                                            ImGui.TableNextColumn();

                                            ImGui.Image(jobIcon.GetWrapOrEmpty().Handle, new Vector2(20, 20));

                                            ImGui.TableNextColumn();

                                            if (ImGui.Selectable($"{jobName}##{jobName}_{jobId}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                                            {
                                                agendaInfo.SelectedJob = jobId;
                                                C.Save();
                                            }

                                            if (isSelected)
                                            {
                                                ImGui.SetItemDefaultFocus();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(200);
                        var optionName = CosmicHelper.PlaylistOptionString(selectedOption);
                        if (ImGui.BeginCombo("##Playlist Options", optionName))
                        {
                            foreach (PlaylistOptions option in Enum.GetValues<PlaylistOptions>())
                            {
                                var displayName = CosmicHelper.PlaylistOptionString(option);
                                bool isSelected = agendaInfo.SelectedOption == option;

                                if (ImGui.Selectable($"{displayName}##{option}", isSelected))
                                {
                                    agendaInfo.SelectedOption = option;
                                    C.Save();
                                }

                                if (isSelected)
                                {
                                    ImGui.SetItemDefaultFocus();
                                }
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(150);
                        if (selectedOption == PlaylistOptions.SelectedRelicLv)
                        {
                            var level = agendaInfo.SelectedRelicLevel;
                            if (ImGui.InputInt("##Relic Level", ref level))
                            {
                                agendaInfo.SelectedRelicLevel = level;
                                C.SaveDebounced();
                            }
                        }
                        else if (selectedOption == PlaylistOptions.CreditAmount)
                        {
                            var creditAmount = agendaInfo.CreditAmount;
                            if (ImGui.DragInt("##Credit Amount", ref creditAmount, 200f, 0, 30_000))
                            {
                                agendaInfo.CreditAmount = creditAmount;
                                C.SaveDebounced();
                            }
                        }
                        else if (selectedOption == PlaylistOptions.PlanetAmount)
                        {
                            var planetCredit = agendaInfo.PlanetAmount;
                            if (ImGui.DragInt("##Planet Amount", ref planetCredit, 500f, 0, 10_000))
                            {
                                agendaInfo.PlanetAmount = planetCredit;
                                C.SaveDebounced();
                            }
                        }
                        else if (selectedOption == PlaylistOptions.DronebitAmount)
                        {
                            var dronebitAmount = agendaInfo.DronebitAmount;
                            if (ImGui.DragInt("##Dronebit Amount", ref dronebitAmount, 200, 0, 5_000))
                            {
                                agendaInfo.DronebitAmount = dronebitAmount;
                                C.SaveDebounced();
                            }
                        }
                        else if (selectedOption == PlaylistOptions.ClassLevel)
                        {
                            var classLevel = agendaInfo.ClassLevel;
                            if (ImGui.InputInt("##ClassLevel", ref classLevel))
                            {
                                agendaInfo.ClassLevel = classLevel;
                                C.SaveDebounced();
                            }
                        }
                        else if (selectedOption == PlaylistOptions.ClassScore)
                        {
                            var score = agendaInfo.ClassScore;
                            if (ImGui.SliderInt("##ClassScore", ref score, 0, 500_000))
                            {
                                agendaInfo.ClassScore = score;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                var classScore = CosmicHelper.Cosmic_ClassInfo();
                                if (classScore.TryGetValue(agendaInfo.SelectedJob, out var job))
                                {
                                    ImGui.SetTooltip(T("Current Score: {0:N0}", job.Score));
                                }
                                else
                                {
                                    ImGui.SetTooltip(T("No score can be loaded"));
                                }
                            }
                        }

                        ImGui.TableNextColumn();
                        var currentMode = agendaInfo.SelectedMode;
                        ImGui.SetNextItemWidth(150);
                        if (ImGui.BeginCombo("##Mode Selection", ModeSelectString(currentMode)))
                        {
                            foreach (ModeSelect option in Enum.GetValues(typeof(ModeSelect)))
                            {
                                if (option == ModeSelect.AgendaMode)
                                    continue;
                                else
                                {
                                    var displayName = ModeSelectString(option);
                                    bool isSelected = agendaInfo.SelectedMode == option;

                                    if (ImGui.Selectable($"{displayName}##{option}", isSelected))
                                    {
                                        agendaInfo.SelectedMode = option;
                                        C.Save();
                                    }

                                    if (isSelected)
                                    {
                                        ImGui.SetItemDefaultFocus();
                                    }
                                }
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.TableNextColumn();
                        if (ImGuiEx.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        {
                            C.Cosmic_Agenda.Remove(agendaInfo);
                            C.Save();
                        }

                        ImGui.PopID();
                    }
                }
            }

            _dragDrop.End(); // Step 4: Process drag/drop outside the table
        }

        private static void Option_RelicLv()
        {

        }
    }
}
