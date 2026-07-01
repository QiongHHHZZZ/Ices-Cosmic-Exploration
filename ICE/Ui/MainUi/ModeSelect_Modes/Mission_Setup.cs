using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Ui.MainUi.Settings;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class Mission_Setup
    {
        private static readonly Dictionary<string, uint> BattleJobs = new()
        {
            // Tanks
            { "Paladin", 19 },
            { "Warrior", 21 },
            { "Dark Knight", 32 },
            { "Gunbreaker", 37 },
    
            // Healers
            { "White Mage", 24 },
            { "Scholar", 28 },
            { "Astrologian", 33 },
            { "Sage", 40 },
    
            // Melee DPS
            { "Monk", 20 },
            { "Dragoon", 22 },
            { "Ninja", 30 },
            { "Samurai", 34 },
            { "Reaper", 39 },
            { "Viper", 41 },
    
            // Physical Ranged DPS
            { "Bard", 23 },
            { "Machinist", 31 },
            { "Dancer", 38 },
    
            // Magical Ranged DPS
            { "Black Mage", 25 },
            { "Summoner", 27 },
            { "Red Mage", 35 },
            { "Pictomancer", 42 }
        };

        public static CosmicTables.Mission_Table? MissionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;
        private static string newListName = string.Empty;

        public static void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            // Header at the top
            float scale = ImGuiHelpers.GlobalScale;

            using (var headerChild = ImRaii.Child("##modeSelect_StandardHeader", new Vector2(0, 45 * scale), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (headerChild.Success)
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10 * scale);
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5 * scale);

                string modeType = string.Empty;
                FontAwesomeIcon modeIcon = FontAwesomeIcon.List;

                bool standard = C.SelectedMode == ModeSelect.Standard;
                bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
                bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
                bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
                bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;


                if (standard)
                    modeType = T("Standard Mode");
                else if (relicMode)
                {
                    modeType = T("Relic Grind Mode");
                    modeIcon = FontAwesomeIcon.ArrowUpRightDots;
                }
                else if (xpLeveling)
                {
                    modeType = T("Leveling Grind Mode");
                    modeIcon = FontAwesomeIcon.Leaf;
                }
                else if (goldMode)
                {
                    modeType = T("Gold Completion Grind Mode");
                    modeIcon = FontAwesomeIcon.Trophy;
                }
                else if (agendaMode)
                {
                    modeType = T("Cosmic Agenda Mode");
                    modeIcon = FontAwesomeIcon.ClipboardList;
                }

                ImGuiEx.IconWithText(modeIcon, modeType);

                ImGui.SameLine(0, 10 * scale);

                // Adjust the Y position to center the button vertically with the text
                float textHeight = ImGui.GetTextLineHeight();
                float buttonHeight = ImGui.GetFrameHeight();
                float yOffset = (textHeight - buttonHeight) / 2f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, T("Mode Selection")))
                {
                    ImGui.OpenPopup("Mode Select | Select Mode Window");
                }
                if (ImGui.BeginPopup("Mode Select | Select Mode Window"))
                {
                    MainWindow.ModeSelection();

                    ImGui.EndPopup();
                }

                uint currentJobId = (uint)Player.Job;
                bool usingSupportedJob = CosmicHelper.CrafterJobList.Contains(currentJobId) || CosmicHelper.GatheringJobList.Contains(currentJobId);

                bool AnyStop = C.StopOnceHitCosmicScore
                             | C.StopWhenLevel
                            || C.StopOnceHitCosmoCredits
                            || C.StopOnceHitLunarCredits
                            || C.StopOnceRelicFinished
                            || C.StopOnceStandardMissionsGolded;
                if (AnyStop)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();

                        ImGui.Text(T("It appears that you have on of the following enabled"));
                        if (C.StopOnceHitCosmicScore)
                            ImGui.BulletText(T("Stop at Cosmic Score [{0:N0}]", C.CosmicScoreCap));
                        if (C.StopWhenLevel)
                            ImGui.BulletText(T("Stop When Level [{0:N0}]", C.TargetLevel));
                        if (C.StopOnceHitCosmoCredits)
                            ImGui.BulletText(T("Stop once cosmo credit hit [{0:N0}]", C.CosmoCreditsCap));
                        if (C.StopOnceHitLunarCredits)
                            ImGui.BulletText(T("Stop once planetary credit hit [{0:N0}]", C.LunarCreditsCap));
                        if (C.StopOnceRelicFinished)
                            ImGui.BulletText(T("Stop once relic completed"));
                        if (C.StopOnceStandardMissionsGolded)
                            ImGui.BulletText(T("Stop when all standard missions are golded"));

                        ImGui.Text(T("So if you stop and you're unsure why... this might be why"));

                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                bool unsupportedArtisan = false; // xpLeveling && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
                bool unsupportedMoon = xpLeveling
                    && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var currentMoon)
                    && !CosmicMoonRegistry.HasLevelingContent(currentMoon);

                // Leveling on a hub requires QuickLevelList entries; gathering still needs route YAML per territory
                using (ImRaii.Disabled(SchedulerMain.State != IceState.Idle || !usingSupportedJob || unsupportedMoon))
                {
                    if (ImGui.Button(T("Start"), new Vector2(150 * scale, 0)))
                    {
                        SchedulerMain.EnablePlugin();
                    }
                }

                if (unsupportedArtisan)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Hey! You need to update artisan to use this mode, please update to at minimum:"));
                        ImGui.Text(T("4.0.4.29"));
                        ImGui.EndTooltip();
                    }
                }
                else if (unsupportedMoon && CosmicMoonRegistry.TryGetMoon(Player.Territory.RowId, out var unsupportedHub))
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Hey! {0} is not supported for leveling yet.", T(unsupportedHub.DisplayName)));
                        var missing = new List<string>();
                        if (!CosmicMoonRegistry.HasLevelingContent(unsupportedHub))
                            missing.Add(T("QuickLevelList missions"));
                        if (!CosmicMoonContent.HasGatheringRoutes(unsupportedHub.TerritoryId))
                            missing.Add(T("gathering routes"));
                        if (missing.Count > 0)
                            ImGui.Text(T("Still needed: {0}.", string.Join(", ", missing)));
                        ImGui.EndTooltip();
                    }
                }
                if (P.AutoHook.Installed && !Utils.HasPlugin("MissFisher") && !P.AutoHook.UpdatedPlugin() && CosmicMoonRegistry.Auxesia.TerritoryId == Player.Territory.RowId)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Hey! Your version of autohook is not currently supported on this planet"));
                        ImGui.Text(T("You need to (currently) be on the testing version to be able fish automated here"));
                        ImGui.Text(T("There will be another warning to pop up if you try and run this still and it selects a fishing mission..."));
                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                using (ImRaii.Disabled(SchedulerMain.State == IceState.Idle))
                {
                    using (ImRaii.PushColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1.0f)))
                    using (ImRaii.PushColor(ImGuiCol.ButtonActive, new Vector4(0.7f, 0.1f, 0.1f, 1.0f)))
                    {
                        if (ImGui.Button(T("Stop"), new Vector2(150 * scale, 0)))
                        {
                            SchedulerMain.DisablePlugin();
                        }
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                if (ImGui.Button(T("Mission Settings")))
                {
                    ImGui.OpenPopup("Mission Settings: Popup");
                }
                if (ImGui.BeginPopup("Mission Settings: Popup"))
                {
                    // TODO: Mission Settings
                    bool grindAllProvisionals = C.GrindAllProvisionals;
                    if (ImGui.Checkbox(T("Provisional: Allow All Classes"), ref grindAllProvisionals))
                    {
                        C.GrindAllProvisionals = grindAllProvisionals;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker(T("Enabling this will show you all weather/timed/sequence missions that you can grind,\nON TOP OF doing the normal missions for whichever class you start on.\nIf you just want to focus one specific class, set this to false"));

                    bool allowCriticalsAllClass = C.GrindOffClassRedAlert;
                    if (ImGui.Checkbox(T("Critical: Allow All Classes"), ref allowCriticalsAllClass))
                    {
                        C.GrindOffClassRedAlert = allowCriticalsAllClass;
                        C.Save();
                    }
                    ImGuiEx.HelpMarker(T("This will allow you to grind other classes for criticals/red alerts. (So if you're on crp, but a bsm red alert pops up)"));

                    bool removeGold = C.RemoveAfterGold;
                    if (ImGui.Checkbox(T("Remove Mission Upon Gold Completion"), ref removeGold))
                    {
                        C.RemoveAfterGold = removeGold;
                        C.Save();
                    }
                    using (ImRaii.Disabled(!removeGold))
                    {
                        bool keepARanks = C.KeepARanks;
                        if (ImGui.Checkbox(T("Keep \"A Rank\" missions and below"), ref keepARanks))
                        {
                            C.KeepARanks = keepARanks;
                            C.Save();
                        }
                    }

                    ImGui.Checkbox(T("Stop after current mission"), ref Mission_Settings.StopAfterCurrent);
                    bool relicTurnin = C.TurninRelic;
                    if (ImGui.Checkbox(T("Turnin if relic is complete") + "##RelicTurnin_GeneralSetting", ref relicTurnin))
                    {
                        C.TurninRelic = relicTurnin;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.TextDisabled("?");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(T("THIS IS YOUR HEADS UP ON HOW THIS WORKS. If I change this in the future, this tooltip will also change.\n") +
                                         T("1: This will check for your current CLASS [not menu class, actual current class] for relic turnin.\n") +
                                         T("2: You must not have the tool eqipped for this to run full auto. \n") +
                                         T("\t- This is due to the fact that I cba coding this in at this time. (might change my mind in the future *shrugs*)\n") +
                                         T("3: This will take prio over \"Stop @ Relic Turnin\", in the sense that if you have both enabled, it will turnin vs stop. And continue about it's day\n") +
                                         T("4: If you're on a crafting class, it will return you back to the stop you were crafting post turnin. \n") +
                                         T("\t- This is optional, you can disable it at your own free will, I just like this so I can just go back to an isolated area of my choosing"));
                    }

                    ImGui.Separator();
                    bool relic_AllowRedAlert = C.Relic_IncludeCriticals;
                    if (ImGui.Checkbox(T("Relic Mode: Allow Red Alerts"), ref relic_AllowRedAlert))
                    {
                        C.Relic_IncludeCriticals = relic_AllowRedAlert;
                        C.Save();
                    }

                    bool OnlySelected = C.XPRelicOnlyEnabled;
                    if (ImGui.Checkbox(T("Relic Mode: Only Enabled"), ref OnlySelected))
                    {
                        C.XPRelicOnlyEnabled = OnlySelected;
                        C.Save();
                    }
                    if (ImGui.Button(T("Open Job Swap Settings")))
                    {
                        C.SelectedTab = WindowSelection.CharacterSettings;
                    }

                    if (ImGui.Button(T("Save Current Mission Preset")))
                    {
                        ImGui.OpenPopup("Preset Save Editor");
                    }

                    if (ImGui.BeginPopup("Preset Save Editor"))
                    {
                        ImGui.InputText(T("Playlist Name"), ref newListName);
                        using (ImRaii.Disabled(string.IsNullOrEmpty(newListName)))
                        {
                            if (ImGui.Button(T("Save New List")))
                            {
                                List<uint> new_Playlist = new();
                                foreach (var mission in C.MissionConfig.Where(x => x.Value.Enabled))
                                {
                                    new_Playlist.Add(mission.Key);
                                }
                                if (C.Mission_Playlist.ContainsKey(newListName))
                                {
                                    C.Mission_Playlist[newListName] = new_Playlist;
                                }
                                else
                                {
                                    C.Mission_Playlist.Add(newListName, new_Playlist);
                                }
                                C.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndPopup();
                    }

                    if (C.Mission_Playlist.Count > 0)
                    {
                        if (ImGui.Button(T("View All Presets")))
                        {
                            ImGui.OpenPopup("Preset: List Viewer");
                        }

                        if (ImGui.BeginPopup("Preset: List Viewer"))
                        {
                            ImGui.Text(T("Load Mission Preset"));

                            if (ImGui.BeginTable($"Preset: TableViewer", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                            {
                                ImGui.TableSetupColumn(T("Name"));
                                ImGui.TableSetupColumn(T("Amount Enabled"));

                                ImGui.TableHeadersRow();

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.AlignTextToFramePadding();
                                ImGui.Text(T("Clear All"));
                                ImGui.SameLine();
                                if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"FreshPreset_Button"))
                                {
                                    foreach (var mission in C.MissionConfig)
                                    {
                                        mission.Value.Enabled = false;
                                    }
                                    C.Save();
                                    ImGui.CloseCurrentPopup();
                                }

                                foreach (var item in C.Mission_Playlist)
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Key}");
                                    ImGui.SameLine();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.ArrowUpRightFromSquare, $"{item.Key}_Button"))
                                    {
                                        foreach (var mission in C.MissionConfig)
                                        {
                                            if (item.Value.Contains(mission.Key))
                                                mission.Value.Enabled = true;
                                            else
                                                mission.Value.Enabled = false;
                                        }
                                        C.Save();
                                        ImGui.CloseCurrentPopup();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip(T("Import Missions"));
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{item.Value.Count}");

                                    ImGui.TableNextColumn();
                                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"{item.Key}_Remove"))
                                    {
                                        C.Mission_Playlist.Remove(item);
                                        C.Save();
                                    }
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip(T("Remove from list"));
                                    }
                                }

                                ImGui.EndTable();
                            }

                            ImGui.EndPopup();
                        }
                    }


                ImGui.EndPopup();
                }
            }
            }

            using (var bodyChild = ImRaii.Child("##modeSelect_Body", new Vector2(0, -1), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!bodyChild.Success) return;

                float scrollbarSize = ImGui.GetStyle().ScrollbarSize;
                float buttonRowHeight = (ImGui.GetTextLineHeight() + 8 * scale + 4 * scale) + scrollbarSize;

                using (var missionButtons = ImRaii.Child("##tab_scroll", new Vector2(0, buttonRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    if (!missionButtons.Success)
                        return;

                    ImGui_Ice.DrawRankButton(T("Red Alert"), MissionFilter.RedAlert, MissionTable);
                    ImGui_Ice.DrawRankButton(T("Sequence"), MissionFilter.Sequence, MissionTable);
                    ImGui_Ice.DrawRankButton(T("Weather"), MissionFilter.Weather, MissionTable);
                    ImGui_Ice.DrawRankButton(T("Timed"), MissionFilter.Timed, MissionTable);
                    ImGui_Ice.DrawRankButton(T("Master"), MissionFilter.Master, MissionTable);
                    ImGui_Ice.DrawRankButton(T("A Rank"), MissionFilter.ARank, MissionTable);
                    ImGui_Ice.DrawRankButton(T("B Rank"), MissionFilter.BRank, MissionTable);
                    ImGui_Ice.DrawRankButton(T("C Rank"), MissionFilter.CRank, MissionTable);
                    ImGui_Ice.DrawRankButton(T("D Rank"), MissionFilter.DRank, MissionTable);

                    ImGui_Ice.EndCategoryButtonRow();
                }

                var bottomSpace = ImGui.GetTextLineHeight() + 6f;
                bottomSpace += 12f; // prevent the tabs from creating a scrollbar

                Vector2 size = new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - bottomSpace);
                if (ImGui.BeginChild("###MissionTableV3", size, false))
                {
                    try
                    {
                        if (MissionTable == null && CosmicHelper.SheetMissionDict.Count > 0)
                        {
                            foreach (var mission in CosmicHelper.SheetMissionDict)
                            {
                                CosmicHelper.MissionInfo missionDetails = new() { Id = mission.Key };
                                TableItems.Add(missionDetails);
                            }
                            ItemCount = TableItems.Count();
                            MissionTable = new(TableItems);
                        }
                        var filterActive = MissionTable.FilteredItems.Count != 0 && MissionTable.FilteredItems.Count != ItemCount;
                        var filterCount = filterActive ? $" (of {ItemCount})" : "";
                        var height = ImGui.GetFrameHeight();
                        MissionTable.Draw(height + 4f);
                    }
                    catch (Exception ex)
                    {
                        IceLogging.Error(ex.Message, "Drawing Mission Table");
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}
