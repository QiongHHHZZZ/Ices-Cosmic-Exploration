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

        public static Mission_Table? MissionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;

        // Search bar state: free text search against mission ID and name.
        private static string _searchText = string.Empty;
        private static Mission_Table.TableViewMode _tableViewMode = Mission_Table.TableViewMode.Compact;
        private static bool _openCustomColumnPopup;
        private static Vector2 _tableViewPopupPos;

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


                        ImGui.EndPopup();
                    }
                }
            }

            using (var bodyChild = ImRaii.Child("##modeSelect_Body", new Vector2(0, -1), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!bodyChild.Success) return;

                EnsureMissionTable();
                DrawMissionToolbar(scale);

                var bottomSpace = ImGui.GetTextLineHeight() + 18f; // prevent the tabs from creating a scrollbar
                var available = ImGui.GetContentRegionAvail();
                var tableHeight = available.Y - bottomSpace;

                if (MissionTable == null || available.X <= 2f || tableHeight <= ImGui.GetFrameHeight() * 3f)
                    return;

                using (var tableChild = ImRaii.Child("###MissionTableV3", new Vector2(available.X, tableHeight), false))
                {
                    if (!tableChild.Success) return;

                    try
                    {
                        var height = ImGui.GetFrameHeight();
                        MissionTable.ViewMode = _tableViewMode;
                        MissionTable.Draw(height + 2f);
                    }
                    catch (Exception ex)
                    {
                        IceLogging.Error(ex.Message, "Drawing Mission Table");
                    }
                }
            }
        }

        private static void EnsureMissionTable()
        {
            if (MissionTable != null || CosmicHelper.SheetMissionDict.Count == 0)
                return;

            TableItems.Clear();
            foreach (var mission in CosmicHelper.SheetMissionDict)
                TableItems.Add(new CosmicHelper.MissionInfo { Id = mission.Key });

            ItemCount = TableItems.Count;
            MissionTable = new(TableItems)
            {
                SearchText = _searchText,
            };
        }

        private static void DrawMissionToolbar(float scale)
        {
            var style = ImGui.GetStyle();
            float cardHeight = ImGui.GetTextLineHeight() + ImGui.GetFrameHeight() + 24 * scale;
            float groupRowHeight = cardHeight + style.ScrollbarSize + 4 * scale;
            float toolbarHeight = groupRowHeight + ImGui.GetFrameHeight() + style.ItemSpacing.Y + 16 * scale;

            using var toolbarStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(8 * scale, 7 * scale))
                .Push(ImGuiStyleVar.ItemSpacing, new Vector2(8 * scale, 6 * scale));

            using (var toolbar = ImRaii.Child("##missionToolbar", new Vector2(0, toolbarHeight), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (!toolbar.Success)
                    return;

                using (var filterStrip = ImRaii.Child("##missionFilterStrip", new Vector2(0, groupRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
                {
                    if (filterStrip.Success)
                    {
                        DrawFilterCard("Tasks", GetFilterCardWidth(scale, T("Tasks"), T("Red Alert"), T("Sequence"), T("Weather"), T("Timed"), T("Master"), T("A Rank"), T("B Rank"), T("C Rank"), T("D Rank")), cardHeight, scale, () =>
                        {
                            ImGui_Ice.DrawRankButton(T("Red Alert"), MissionFilter.RedAlert, MissionTable);
                            ImGui_Ice.DrawRankButton(T("Sequence"), MissionFilter.Sequence, MissionTable);
                            ImGui_Ice.DrawRankButton(T("Weather"), MissionFilter.Weather, MissionTable);
                            ImGui_Ice.DrawRankButton(T("Timed"), MissionFilter.Timed, MissionTable);
                            ImGui_Ice.DrawRankButton(T("Master"), MissionFilter.Master, MissionTable);
                            ImGui_Ice.DrawRankButton(T("A Rank"), MissionFilter.ARank, MissionTable);
                            ImGui_Ice.DrawRankButton(T("B Rank"), MissionFilter.BRank, MissionTable);
                            ImGui_Ice.DrawRankButton(T("C Rank"), MissionFilter.CRank, MissionTable);
                            ImGui_Ice.DrawRankButton(T("D Rank"), MissionFilter.DRank, MissionTable, spacingAfter: -1);
                        });
                        ImGui.SameLine(0, 8 * scale);

                        DrawFilterCard("Experience", GetFilterCardWidth(scale, T("Experience"), "I", "II", "III", "IV", "V", "VI", "VII"), cardHeight, scale, () =>
                        {
                            ImGui_Ice.DrawItemFilterButton("I", ItemFilter.HasI, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("II", ItemFilter.HasII, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("III", ItemFilter.HasIII, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("IV", ItemFilter.HasIV, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("V", ItemFilter.HasV, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("VI", ItemFilter.HasVI, MissionTable);
                            ImGui_Ice.DrawItemFilterButton("VII", ItemFilter.HasVII, MissionTable, spacingAfter: -1);
                        });
                        ImGui.SameLine(0, 8 * scale);

                        DrawFilterCard("State", GetFilterCardWidth(scale, T("State"), T("Enabled"), T("Disabled")), cardHeight, scale, () =>
                        {
                            ImGui_Ice.DrawItemFilterButton(T("Enabled"), ItemFilter.Enabled, MissionTable);
                            ImGui_Ice.DrawItemFilterButton(T("Disabled"), ItemFilter.Disabled, MissionTable, spacingAfter: -1);
                        });
                        ImGui.SameLine(0, 8 * scale);

                        DrawFilterCard("Tokens", GetFilterCardWidth(scale, T("Tokens"), T("Has Tokens"), T("No Tokens")), cardHeight, scale, () =>
                        {
                            ImGui_Ice.DrawItemFilterButton(T("Has Tokens"), ItemFilter.HasTokens, MissionTable);
                            ImGui_Ice.DrawItemFilterButton(T("No Tokens"), ItemFilter.NoTokens, MissionTable, spacingAfter: -1);
                        });
                        ImGui.SameLine(0, 8 * scale);

                        DrawFilterCard("Completion", GetFilterCardWidth(scale, T("Completion"), T("Not Completed"), T("Completed"), T("Gold")), cardHeight, scale, () =>
                        {
                            ImGui_Ice.DrawItemFilterButton(T("Not Completed"), ItemFilter.NotCompleted, MissionTable);
                            ImGui_Ice.DrawItemFilterButton(T("Completed"), ItemFilter.Completed, MissionTable);
                            ImGui_Ice.DrawItemFilterButton(T("Gold"), ItemFilter.Gold, MissionTable, spacingAfter: -1);
                        });
                    }
                }

                DrawSearchAndViewRow(scale);
            }
        }

        private static float GetFilterCardWidth(float scale, string title, params string[] chipLabels)
        {
            var cardPaddingX = 8f * scale;
            var chipPaddingX = 8f * scale;
            var chipSpacing = 5f * scale;
            var contentWidth = 0f;

            for (var i = 0; i < chipLabels.Length; i++)
            {
                contentWidth += ImGui.CalcTextSize(chipLabels[i]).X + chipPaddingX * 2f;
                if (i + 1 < chipLabels.Length)
                    contentWidth += chipSpacing;
            }

            contentWidth = MathF.Max(contentWidth, ImGui.CalcTextSize(title).X);
            return contentWidth + cardPaddingX * 2f + 2f * scale;
        }

        private static void DrawFilterCard(string label, float width, float height, float scale, Action drawContent)
        {
            using var cardStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(8 * scale, 7 * scale))
                .Push(ImGuiStyleVar.ItemSpacing, new Vector2(5 * scale, 5 * scale));
            using var card = ImRaii.Child($"##missionFilterCard_{label}", new Vector2(width, height), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            if (!card.Success)
                return;

            ImGui.TextColored(new Vector4(0.58f, 0.78f, 1.00f, 0.92f), T(label));
            drawContent();
        }

        private static void DrawSearchAndViewRow(float scale)
        {
            if (MissionTable == null)
                return;

            float viewButtonWidth = 142 * scale;
            float searchViewGap = 20 * scale;
            float searchGroupWidth = Math.Min(430 * scale, Math.Max(220 * scale, ImGui.GetContentRegionAvail().X - viewButtonWidth - searchViewGap - 8 * scale));

            DrawUnifiedSearch(searchGroupWidth, scale);
            ImGui.SameLine(0, searchViewGap);

            if (ImGui.Button($"{T("View")}: {GetTableViewLabel(_tableViewMode)}", new Vector2(viewButtonWidth, 0)))
            {
                _tableViewPopupPos = ImGui.GetItemRectMin() + new Vector2(0, ImGui.GetItemRectSize().Y + 4 * scale);
                ImGui.OpenPopup("##missionTableViewPopup");
            }

            DrawViewPopups(scale);
        }

        private static void DrawUnifiedSearch(float width, float scale)
        {
            var style = ImGui.GetStyle();
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();
            var height = ImGui.GetFrameHeight();
            var size = new Vector2(width, height);
            var rounding = 6f * scale;
            var inputPaddingX = 12f * scale;

            drawList.AddRectFilled(pos, pos + size, ImGui.GetColorU32(ImGuiCol.FrameBg), rounding);
            drawList.AddRect(pos, pos + size, ImGui.GetColorU32(ImGuiCol.Border), rounding, ImDrawFlags.RoundCornersAll, 1f * scale);

            ImGui.SetCursorScreenPos(new Vector2(pos.X + inputPaddingX, pos.Y));
            ImGui.SetNextItemWidth(MathF.Max(1f, width - inputPaddingX * 2f));
            using (ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 0)
                   .Push(ImGuiStyleVar.FrameRounding, 0)
                   .Push(ImGuiStyleVar.FramePadding, new Vector2(0, style.FramePadding.Y)))
            using (ImRaii.PushColor(ImGuiCol.FrameBg, Vector4.Zero)
                   .Push(ImGuiCol.FrameBgHovered, Vector4.Zero)
                   .Push(ImGuiCol.FrameBgActive, Vector4.Zero))
            {
                if (ImGui.InputTextWithHint("##searchInput", T("Search..."), ref _searchText, 256))
                {
                    if (MissionTable != null)
                        MissionTable.SearchText = _searchText;
                    MissionTable?.SetFilterDirty();
                }
            }

            ImGui.SetCursorScreenPos(pos + new Vector2(width, 0));
        }

        private static void DrawViewPopups(float scale)
        {
            ImGui.SetNextWindowPos(_tableViewPopupPos, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(new Vector2(148 * scale, 0), ImGuiCond.Appearing);
            PushMissionPopupStyle(scale);
            if (ImGui.BeginPopup("##missionTableViewPopup"))
            {
                DrawTableViewOption(Mission_Table.TableViewMode.Compact);
                DrawTableViewOption(Mission_Table.TableViewMode.Full);
                DrawTableViewOption(Mission_Table.TableViewMode.Custom);
                ImGui.EndPopup();
            }
            PopMissionPopupStyle();

            if (_openCustomColumnPopup)
            {
                ImGui.OpenPopup("##missionCustomColumnPopup");
                _openCustomColumnPopup = false;
            }

            ImGui.SetNextWindowPos(_tableViewPopupPos, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(new Vector2(260 * scale, 0), ImGuiCond.Appearing);
            PushMissionPopupStyle(scale);
            if (ImGui.BeginPopup("##missionCustomColumnPopup"))
            {
                MissionTable?.DrawCustomColumnSelector();
                ImGui.EndPopup();
            }
            PopMissionPopupStyle();
        }

        private static void PushMissionPopupStyle(float scale)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12 * scale, 10 * scale));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8 * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2 * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(6 * scale, 6 * scale));
            ImGui.PushStyleColor(ImGuiCol.PopupBg, ImGui.GetColorU32(new Vector4(0.045f, 0.060f, 0.095f, 1.00f)));
            ImGui.PushStyleColor(ImGuiCol.Border, ImGui.GetColorU32(new Vector4(0.42f, 0.64f, 0.95f, 1.00f)));
            ImGui.PushStyleColor(ImGuiCol.Header, ImGui.GetColorU32(new Vector4(0.14f, 0.30f, 0.55f, 0.92f)));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, ImGui.GetColorU32(new Vector4(0.20f, 0.42f, 0.75f, 1.00f)));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, ImGui.GetColorU32(new Vector4(0.25f, 0.52f, 0.92f, 1.00f)));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(new Vector4(0.92f, 0.96f, 1.00f, 1.00f)));
        }

        private static void PopMissionPopupStyle()
        {
            ImGui.PopStyleColor(6);
            ImGui.PopStyleVar(4);
        }

        private static void DrawTableViewOption(Mission_Table.TableViewMode mode)
        {
            bool selected = _tableViewMode == mode;
            var label = GetTableViewLabel(mode);
            if (ImGui.Selectable(label, selected))
            {
                _tableViewMode = mode;
                if (mode == Mission_Table.TableViewMode.Custom)
                    _openCustomColumnPopup = true;
            }
            if (selected)
                ImGui.SetItemDefaultFocus();
        }

        private static string GetTableViewLabel(Mission_Table.TableViewMode mode)
        {
            return mode switch
            {
                Mission_Table.TableViewMode.Compact => T("Compact"),
                Mission_Table.TableViewMode.Full => T("Full"),
                Mission_Table.TableViewMode.Custom => T("Custom"),
                _ => T("Unknown"),
            };
        }
    }
}
