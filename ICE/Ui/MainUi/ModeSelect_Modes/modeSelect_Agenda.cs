using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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
            PlaylistOptions.GoldClassMissions,
        };

        public static uint SelectedJob = 8;
        public static PlaylistOptions SelectedOption = PlaylistOptions.None;

        public static AgendaProfileInfo SelectedAgenda = new();
        public static string profileName = "";
        public static string profileDescription = "";
        private static string _importBuffer = "";

        public static void Draw()
        {
            if (ImGui.BeginTabBar("Agenda Mode: Tabs"))
            {
                if (ImGui.BeginTabItem(T("Current Agenda")))
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

                    ImGui.SameLine();
                    var validAgenda = C.Cosmic_Agenda.Count() > 0;
                    using (ImRaii.Disabled(!validAgenda))
                    {
                        if (ImGui.Button(T("Save to Favorites")))
                        {
                            ImGui.OpenPopup("Agenda Info: Profile Save");
                        }
                    }
                    if (ImGui.BeginPopup("Agenda Info: Profile Save"))
                    {
                        ImGui.InputText(T("Name"), ref profileName);
                        ImGui.InputTextMultiline(T("Description"), ref profileDescription);
                        using (ImRaii.Disabled(profileName == string.Empty))
                        {
                            if (ImGui.Button(T("Save")))
                            {
                                AgendaProfileInfo newProfile = new()
                                {
                                    Name = profileName,
                                    Description = profileDescription,
                                    MissionList = C.Cosmic_Agenda.Select(a => a.Clone()).ToList()
                                };

                                C.Agenda_Profiles.Add(newProfile);
                                C.Save();

                                profileName = "";
                                profileDescription = "";
                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndPopup();
                    }

                    CosmicAgendaTable();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem(T("Saved Agenda's")))
                {
                    List<AgendaProfileInfo> listToRemove = new();

                    // Export button — copies to clipboard
                    if (ImGui.Button(T("Export to Clipboard")))
                    {
                        ImGui.SetClipboardText(ExportProfile(SelectedAgenda));
                    }

                    ImGui.SameLine();

                    // Import
                    ImGui.SetNextItemWidth(300);
                    ImGui.InputText("##ImportBox", ref _importBuffer, 5028);
                    ImGui.SameLine();
                    if (ImGui.Button(T("Import")))
                    {
                        if (TryImportProfile(_importBuffer, out var imported))
                        {
                            // Avoid duplicate names
                            imported.MissionList = new List<AgendaInfo>(imported.MissionList);
                            C.Agenda_Profiles.Add(imported);
                            C.Save();
                            _importBuffer = "";
                        }
                        else
                        {
                            // Optional: show an error notification
                            Notify.Error(T("Invalid import string."));
                        }
                    }

                    if (C.Agenda_Profiles.Count > 0)
                    {
                        float spacing = 10f;
                        float leftPanelWidth = 200f;
                        float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
                        float childHeight = ImGui.GetContentRegionAvail().Y;

                        if (ImGui.BeginChild("Agenda List Viewer", new Vector2(leftPanelWidth, childHeight), true))
                        {
                            for (int i = 0; i < C.Agenda_Profiles.Count; i++)
                            {
                                var agenda = C.Agenda_Profiles[i];

                                ImGui.PushID($"{agenda.Name}_{i}");

                                bool isSeleced = SelectedAgenda == agenda;
                                string label = isSeleced ? $"→ {agenda.Name}" : $"{agenda.Name}";

                                if (ImGui.Selectable(label, isSeleced))
                                {
                                    SelectedAgenda = agenda;
                                }

                                ImGui.PopID();
                            }
                        }
                        ImGui.EndChild();

                        ImGui.SameLine(0, spacing);
                        if (ImGui.BeginChild("Agenda Viewer: Details", new(rightPanelWidth, childHeight), true))
                        {
                            var agenda = SelectedAgenda;
                            ImGui.Text(T("Profile Name: {0}", agenda.Name));
                            ImGui.TextWrapped(T("Description: {0}", agenda.Description));

                            bool held = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
                            using (ImRaii.Disabled(!held))
                            {
                                if (ImGui.Button(T("Apply to agenda")))
                                {
                                    C.Cosmic_Agenda = agenda.MissionList.Select(a => a.Clone()).ToList();
                                    C.Save();
                                }
                            }
                            if (!held && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.SetTooltip(T("Hold shift to allow applying"));
                            }

                            ImGui.SameLine();
                            bool cntrlHeld = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
                            using (ImRaii.Disabled(!cntrlHeld))
                            {
                                if (ImGui.Button(T("Delete Profile")))
                                    listToRemove.Add(SelectedAgenda);
                            }
                            if (!cntrlHeld && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.SetTooltip(T("Hold Control to delete profile"));
                            }

                            if (ImGui.BeginTable("Agenda Missions Table: Favorites Info", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                            {
                                ImGui.TableSetupColumn(T("Job"));
                                ImGui.TableSetupColumn(T("Agenda"));
                                ImGui.TableSetupColumn(T("Run Until.."));
                                ImGui.TableSetupColumn(T("Mode Select"));

                                for (int i = 0; i < agenda.MissionList.Count; i++)
                                {
                                    var agendaInfo = agenda.MissionList[i];
                                    var selectedOption = agendaInfo.SelectedOption;

                                    ImGui.TableNextRow();

                                    ImGui.TableSetColumnIndex(0);
                                    var jobImage = CosmicHelper.JobIconDict[agendaInfo.SelectedJob];
                                    float zoom = 0.15f;

                                    ImGui.Image(jobImage.GetWrapOrEmpty().Handle, new Vector2(20, 20), new Vector2(zoom, zoom), new Vector2(1 - zoom, 1 - zoom));

                                    ImGui.TableNextColumn();
                                    ImGui.SetNextItemWidth(200);
                                    var optionName = CosmicHelper.PlaylistOptionString(selectedOption);
                                    ImGui.Text(optionName);

                                    ImGui.TableNextColumn();
                                    string optionText = selectedOption switch
                                    {
                                        PlaylistOptions.SelectedRelicLv => $"{agendaInfo.SelectedRelicLevel}",
                                        PlaylistOptions.CreditAmount => $"{agendaInfo.CreditAmount}",
                                        PlaylistOptions.PlanetAmount => $"{agendaInfo.PlanetAmount}",
                                        PlaylistOptions.DronebitAmount => $"{agendaInfo.DronebitAmount}",
                                        PlaylistOptions.ClassLevel => $"{agendaInfo.ClassLevel}",
                                        PlaylistOptions.ClassScore => $"{agendaInfo.ClassScore}",
                                        _ => ""
                                    };
                                    ImGui.Text(optionText);

                                    ImGui.TableNextColumn();
                                    ImGui.Text($"{ModeSelectString(agendaInfo.SelectedMode)}");
                                }

                                ImGui.EndTable();
                            }

                            if (listToRemove.Count > 0)
                            {
                                C.Agenda_Profiles.Remove(SelectedAgenda);
                                SelectedAgenda = new();
                                C.Save();
                            }
                        }
                        ImGui.EndChild();
                    }
                    else
                    {
                        ImGui.TextWrapped(T("You currently don't have any profiles saved! Please either make one and save, or import if you would like to populate this listing"));
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static string ModeSelectString(ModeSelect mode)
        {
            return mode switch
            {
                ModeSelect.Standard => T("Standard"),
                ModeSelect.RelicMode => T("Relic Grind Mode"),
                ModeSelect.LevelMode => T("Leveling Mode"),
                ModeSelect.MissionGoldMode => T("Gold Completion Mode"),
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

            using (var PlaylistTable = ImRaii.Table("Cosmic Agenda Table", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
            {
                if (PlaylistTable)
                {
                    ImGui.TableSetupColumn("##Reorder");
                    ImGui.TableSetupColumn(T("Job"));
                    ImGui.TableSetupColumn(T("Agenda"));
                    ImGui.TableSetupColumn(T("Run Until.."));
                    ImGui.TableSetupColumn(T("Mode Select"));
                    ImGui.TableSetupColumn(T("Remove"));
                    ImGui.TableSetupColumn(T("Progress"), ImGuiTableColumnFlags.WidthStretch);

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

                        if (ImGui.ImageButton(jobImage.GetWrapOrEmpty().Handle,new Vector2(20, 20), new Vector2(zoom, zoom), new Vector2(1 - zoom, 1 - zoom)))
                        {
                            ImGui.OpenPopup("Job Selection");
                        }
                        if (ImGui.BeginPopup("Job Selection"))
                        {
                            if (ImGui.BeginTable("JobTable", 2, ImGuiTableFlags.BordersInnerV))
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

                                ImGui.EndTable();
                            }

                            ImGui.EndPopup();
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
                                var classScore = CosmicHelper.Cosmic_ClassInfo;
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
                        if (currentMode == ModeSelect.Standard && PlayerHelper.IsInCosmicZone())
                        {
                            var SinusStandard = CosmicHelper.SheetMissionDict.Where(x => x.Value.TerritoryId == 1237)
                                .Where(x => C.MissionConfig.ContainsKey(x.Key))
                                .Where(x => C.MissionConfig[x.Key].Enabled)
                                .Where(x => x.Value.Jobs.Contains(agendaInfo.SelectedJob))
                                .Where(x => x.Value.Rank < 6)
                                .Count();

                            var PhaennaStandard = CosmicHelper.SheetMissionDict.Where(x => x.Value.TerritoryId == 1291)
                                .Where(x => C.MissionConfig.ContainsKey(x.Key))
                                .Where(x => C.MissionConfig[x.Key].Enabled)
                                .Where(x => x.Value.Jobs.Contains(agendaInfo.SelectedJob))
                                .Where(x => x.Value.Rank < 6)
                                .Count();

                            var OizysStandard = CosmicHelper.SheetMissionDict.Where(x => x.Value.TerritoryId == 1310)
                                .Where(x => C.MissionConfig.ContainsKey(x.Key))
                                .Where(x => C.MissionConfig[x.Key].Enabled)
                                .Where(x => x.Value.Jobs.Contains(agendaInfo.SelectedJob))
                                .Where(x => x.Value.Rank < 6)
                                .Count();

                            bool sinusWarning = PlayerHelper.IsInSinusArdorum() && SinusStandard == 0;
                            bool phaennaWarning = PlayerHelper.IsInPhaenna() && PhaennaStandard == 0;
                            bool oizysWarning = PlayerHelper.IsInOizys() && OizysStandard == 0;

                            if (sinusWarning || phaennaWarning || oizysWarning)
                            {
                                string tooltip = T("No standard missions are enabled for your current planet/moon and selected job.\nPlease enable some so the agenda does not stall when there are no timed or weather missions.\nCurrently enabled on this planet/moon:");
                                     

                                if (PlayerHelper.IsInSinusArdorum())
                                    tooltip += T("\nSinus = {0}", SinusStandard);
                                else if (PlayerHelper.IsInPhaenna())
                                    tooltip += T("\nPhaenna = {0}", PhaennaStandard);
                                else if (PlayerHelper.IsInOizys())
                                    tooltip += T("\nOizys = {0}", OizysStandard);

                                ImGui.SameLine();
                                ImGui.AlignTextToFramePadding();
                                ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle, tooltip, false);
                            }
                        }

                        ImGui.TableNextColumn();
                        if (ImGuiEx.IconButton(Dalamud.Interface.FontAwesomeIcon.Trash))
                        {
                            C.Cosmic_Agenda.Remove(agendaInfo);
                            C.Save();
                        }

                        ImGui.TableNextColumn();
                        if (PlayerHelper.IsInCosmicZone() && Player.Available)
                        {
                            int current = 0;
                            int goal = 0;

                            var job = agendaInfo.SelectedJob;
                            var territory = Player.Territory.RowId;

                            if (selectedOption is PlaylistOptions.SinusMax 
                                               or PlaylistOptions.PhaennaMax 
                                               or PlaylistOptions.OizysMax 
                                               or PlaylistOptions.SelectedRelicLv 
                                               or PlaylistOptions.ToolMaxExp)
                            {
                                var ScoreInfo = CosmicHelper.Cosmic_ClassInfo;

                                var jobInfo = ScoreInfo[job];
                                current = MaxToolProgress(job);
                                goal = selectedOption switch
                                {
                                    PlaylistOptions.SinusMax => 9,
                                    PlaylistOptions.PhaennaMax => 14,
                                    PlaylistOptions.OizysMax => 17,
                                    PlaylistOptions.ToolMaxExp => MaxToolProgress(job, false),
                                    PlaylistOptions.SelectedRelicLv => agendaInfo.SelectedRelicLevel,
                                    _ => 20
                                };
                            }
                            else if (selectedOption is PlaylistOptions.ClassLevel)
                            {
                                current = Player.GetLevel((Job)job);
                                goal = agendaInfo.ClassLevel;
                            }
                            else if (selectedOption is PlaylistOptions.CreditAmount)
                            {
                                uint cosmoCreditId = 45690;
                                if (PlayerHelper.GetItemCount(cosmoCreditId, out var creditAmount))
                                {
                                    current = creditAmount;
                                    goal = agendaInfo.CreditAmount;
                                }
                            }
                            else if (selectedOption is PlaylistOptions.PlanetAmount)
                            {
                                if (CosmicHelper.PlanetCreditInfo.TryGetValue(territory, out var gambaCredits) && PlayerHelper.GetItemCount(gambaCredits, out var gambaAmount))
                                {
                                    current = gambaAmount;
                                    goal = agendaInfo.PlanetAmount;
                                }
                            }
                            else if (selectedOption is PlaylistOptions.DronebitAmount)
                            {
                                if (CosmicHelper.DronebitInfo.TryGetValue(territory, out var dronebitAmount))
                                {
                                    PlayerHelper.GetItemCount(dronebitAmount.creditId, out var count);

                                    current = count;
                                    goal = agendaInfo.DronebitAmount;
                                }
                            }
                            else if (selectedOption is PlaylistOptions.ClassScore)
                            {
                                var ScoreInfo = CosmicHelper.Cosmic_ClassInfo;
                                current = ScoreInfo[job].Score;
                                goal = agendaInfo.ClassScore;
                            }
                            else if (selectedOption is PlaylistOptions.GoldClassMissions)
                            {
                                var totalCompleted = CosmicHelper.Cosmic_ClassInfo;
                                var planet = Player.Territory.RowId;
                                /*
                                if (totalCompleted[job].MissionCompleted.TryGetValue(planet, out var completionRate))
                                {
                                    current = completionRate.TotalCompleted;
                                    goal = completionRate.AllMissions.Count();
                                }
                                */
                            }

                            var rowY = ImGui.GetCursorScreenPos().Y;
                            var rowHeight = ImGui.GetTextLineHeightWithSpacing();
                            var barHeight = ImGui.GetTextLineHeight();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (rowHeight - barHeight) / 2f);

                            ImGui_Ice.Draw_XPBar(current, goal, goal, size: new Vector2(ImGui.GetContentRegionAvail().X, barHeight));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Current: {0:N0}", current));
                                ImGui.Text(T("Goal: {0:N0}", goal));
                                ImGui.EndTooltip();
                            }
                        }

                        ImGui.PopID();
                    }
                }
            }

            _dragDrop.End(); // Step 4: Process drag/drop outside the table
        }
        private static int MaxToolProgress(uint job, bool getCurrent = true)
        {
            var max = 17;

            var ScoreInfo = CosmicHelper.Cosmic_ClassInfo;
            var jobInfo = ScoreInfo[job];
            if (jobInfo.Stage_Current != jobInfo.Stage_Next && getCurrent)
                return jobInfo.Stage_Current;

            if (getCurrent)
            {
                foreach (var exp in jobInfo.CurrentExp)
                {
                    if (exp.Value.Current == exp.Value.Max)
                        max += 1;
                }
                return max;
            }
            else
            {
                foreach (var exp in CosmicHelper.ExpDictionary)
                    max += 1;

                return max;
            }
        }
        public static string ExportProfile(AgendaProfileInfo profile)
        {
            var json = JsonConvert.SerializeObject(profile);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
        public static bool TryImportProfile(string base64, out AgendaProfileInfo profile)
        {
            profile = new();
            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                profile = JsonConvert.DeserializeObject<AgendaProfileInfo>(json) ?? new();
                return profile.Name != string.Empty;
            }
            catch
            {
                return false;
            }
        }
    }
}
