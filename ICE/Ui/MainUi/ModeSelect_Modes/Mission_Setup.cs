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
        private static string newListName = "";

        public static Mission_Table? MissionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;

        public static void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            // Header at the top
            float scale = ImGuiHelpers.GlobalScale;

            using (var headerChild = ImRaii.Child("##modeSelect_StandardHeader", new Vector2(0, 45 * scale), true, ImGuiWindowFlags.NoScrollbar))
            {
                if (!headerChild.Success) return;

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
                    modeType = "Standard";
                else if (relicMode)
                {
                    modeType = "Relic Grind";
                    modeIcon = FontAwesomeIcon.ArrowUpRightDots;
                }
                else if (xpLeveling)
                {
                    modeType = "Leveling Grind";
                    modeIcon = FontAwesomeIcon.Leaf;
                }
                else if (goldMode)
                {
                    modeType = "Gold Completion Grind";
                    modeIcon = FontAwesomeIcon.Trophy;
                }
                else if (agendaMode)
                {
                    modeType = "Cosmic Agenda";
                    modeIcon = FontAwesomeIcon.ClipboardList;
                }

                ImGuiEx.IconWithText(modeIcon, T($"{modeType} Mode"));

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
                            || C.StopOnceRelicFinished;
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

                        ImGui.Text(T("So if you stop and you're unsure why... this might be why"));

                        ImGui.EndTooltip();
                    }
                }

                ImGui.SameLine(0, 10 * scale);
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);

                bool unsupportedArtisan = false; // xpLeveling && CosmicHelper.CrafterJobList.Contains((uint)Player.Job);
                bool unsupportedMoon = false; // PlayerHelper.IsInOizys() && xpLeveling;

                // TODO: Make sure to disable new moon for leveling / gathering. . . 
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
                else if (unsupportedMoon)
                {
                    ImGui.SameLine(0, 10 * scale);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + yOffset);
                    ImGuiEx.Icon(EColor.Red, FontAwesomeIcon.ExclamationTriangle);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Hey! This moon is currently not supported for leveling yet. (It's also worse than sinus or phaenna)"));
                        ImGui.Text(T("Please wait till I get the time to focus on this"));
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
                                         "1: This will check for your current CLASS [not menu class, actual current class] for relic turnin.\n" +
                                         "2: You must not have the tool eqipped for this to run full auto. \n" +
                                         "\t- This is due to the fact that I cba coding this in at this time. (might change my mind in the future *shrugs*)\n" +
                                         "3: This will take prio over \"Stop @ Relic Turnin\", in the sense that if you have both enabled, it will turnin vs stop. And continue about it's day\n" +
                                         "4: If you're on a crafting class, it will return you back to the stop you were crafting post turnin. \n" +
                                         "\t- This is optional, you can disable it at your own free will, I just like this so I can just go back to an isolated area of my choosing");
                    }

                    ImGui.Separator();
                    bool relic_AllowRedAlert = C.Relic_IncludeCriticals;
                    if (ImGui.Checkbox(T("Allow Red Alerts for Relic"), ref relic_AllowRedAlert))
                    {
                        C.Relic_IncludeCriticals = relic_AllowRedAlert;
                        C.Save();
                    }

                    bool OnlySelected = C.XPRelicOnlyEnabled;
                    if (ImGui.Checkbox(T("Only selected missions"), ref OnlySelected))
                    {
                        C.XPRelicOnlyEnabled = OnlySelected;
                        C.Save();
                    }


                    ImGui.EndPopup();
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

                    ImGui_Ice.DrawRankButton("Red Alert", MissionFilter.RedAlert, MissionTable);
                    ImGui_Ice.DrawRankButton("Sequence", MissionFilter.Sequence, MissionTable);
                    ImGui_Ice.DrawRankButton("Weather", MissionFilter.Weather, MissionTable);
                    ImGui_Ice.DrawRankButton("Timed", MissionFilter.Timed, MissionTable);
                    ImGui_Ice.DrawRankButton("A Rank", MissionFilter.ARank, MissionTable);
                    ImGui_Ice.DrawRankButton("B Rank", MissionFilter.BRank, MissionTable);
                    ImGui_Ice.DrawRankButton("C Rank", MissionFilter.CRank, MissionTable);
                    ImGui_Ice.DrawRankButton("D Rank", MissionFilter.DRank, MissionTable);

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
