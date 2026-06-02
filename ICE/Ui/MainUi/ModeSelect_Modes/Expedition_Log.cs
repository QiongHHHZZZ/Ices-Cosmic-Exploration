using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using System.Reflection;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class Expedition_Log
    {

        private static uint SelectedJob = 8;

        public enum ExpeditionTabs
        {
            Progress,
            Sinus,
            Phaenna,
            Oizys
        }

        private static ExpeditionTabs selectedTab = ExpeditionTabs.Progress;
        private static bool HideCompleted = false;

        public static void Draw()
        {
            float spacing = 10f;
            float leftPanelWidth = 200f;
            float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
            float childHeight = ImGui.GetContentRegionAvail().Y;

            if (ImGui.BeginChild("Expedition: Class Selection", new(leftPanelWidth, childHeight), true))
            {
                ClassSelection();
            }
            ImGui.EndChild();

            ImGui.SameLine();

            if (ImGui.BeginChild("Detailed Class View", new(rightPanelWidth, childHeight), true))
            {
                ClassDetails();
            }
            ImGui.EndChild();
        }
        private static void ClassSelection()
        {
            if (ImGui.BeginTable("Class Selection Table", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn(T("Icon"));
                ImGui.TableSetupColumn(T("Name"));

                foreach (var jobInfo in CosmicHelper.ClassInfoDict.OrderBy(x => x.Key))
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Image(jobInfo.Value.JobIcon.GetWrapOrEmpty().Handle, new(24, 24));

                    ImGui.TableNextColumn();
                    string name = jobInfo.Value.JobName;
                    ImGui.AlignTextToFramePadding();
                    if (ImGui.Selectable(name, SelectedJob == jobInfo.Key, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedJob = jobInfo.Key;
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                var allClassTexture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.CosmicClassTracker.png").GetWrapOrEmpty();

                ImGui.Image(allClassTexture.Handle, new Vector2(20, 20));

                ImGui.TableNextColumn();
                if (ImGui.Selectable(T("All Classes"), SelectedJob == 0, ImGuiSelectableFlags.SpanAllColumns))
                {
                    SelectedJob = 0;
                }

                ImGui.EndTable();
            }
        }
        private static Dictionary<ExpeditionTabs, List<uint>> MissionList = new()
        {
            [ExpeditionTabs.Sinus] = new(),
            [ExpeditionTabs.Phaenna] = new(),
            [ExpeditionTabs.Oizys] = new(),
        };
        private static void ClassDetails()
        {
            float scale = ImGuiHelpers.GlobalScale;

            float scrollbarSize = ImGui.GetStyle().ScrollbarSize;
            float buttonRowHeight = (ImGui.GetTextLineHeight() + 8 * scale + 4 * scale) + scrollbarSize;

            if (ImGui.BeginChild("##Expedition_TabScroll", new(0, buttonRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                var moons = new (string Name, string Asset, ExpeditionTabs type, uint Territory)[]
                {
                    ("Sinus Ardorum", "ICE.Resources.Sinus_Ardorum.png", ExpeditionTabs.Sinus, 1237),
                    ("Phaenna", "ICE.Resources.Phaenna.png", ExpeditionTabs.Phaenna, 1291),
                    ("Oizys", "ICE.Resources.Oizys.png", ExpeditionTabs.Oizys, 1310),
                };

                if (SelectedJob != 0)
                {
                    var classIcon = CosmicHelper.ClassInfoDict[SelectedJob];
                    DrawImageTabButton(T("Class Progress"), ExpeditionTabs.Progress, ref selectedTab, classIcon.JobIcon.GetWrapOrEmpty());
                }
                else
                {
                    var allClassTexture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.CosmicClassTracker.png").GetWrapOrEmpty();
                    DrawImageTabButton(T("All Class progresses"), ExpeditionTabs.Progress, ref selectedTab, allClassTexture);
                }
                foreach (var moon in moons)
                {
                    List<uint> missions = new();
                    uint completed = 0;

                    if (SelectedJob != 0)
                    {
                        foreach (var mission in CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Contains(SelectedJob)).Where(x => x.Value.TerritoryId == moon.Territory))
                        {
                            missions.Add(mission.Key);
                            if (mission.Value.CompletionStatus is CosmicHelper.Status.Gold)
                                completed += 1;
                        }
                    }
                    else
                    {
                        foreach (var mission in CosmicHelper.SheetMissionDict.Where(x => x.Value.TerritoryId == moon.Territory))
                        {
                            missions.Add(mission.Key);
                            if (mission.Value.CompletionStatus is CosmicHelper.Status.Gold)
                                completed += 1;
                        }
                    }

                    MissionList[moon.type] = missions;

                    var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), moon.Asset).GetWrapOrEmpty();
                    ImGui.SameLine();
                    DrawImageTabButton(T("{0} [{1} / {2}]", T(moon.Name), completed, missions.Count()), moon.type, ref selectedTab, texture);
                }
                ImGui_Ice.EndCategoryButtonRow();
            }
            ImGui.EndChild();

            if (selectedTab is ExpeditionTabs.Progress)
            {
                if (ImGui.BeginChild(T("Class Progress")))
                {
                    ClassProgress();
                }
                ImGui.EndChild();
            }
            else if (selectedTab is ExpeditionTabs.Sinus or ExpeditionTabs.Phaenna or ExpeditionTabs.Oizys)
            {
                ImGui.Checkbox(T("Hide Completed"), ref HideCompleted);
                if (ImGui.BeginChild("Mission Completion Status Window", ImGui.GetContentRegionAvail()))
                {
                    MissionTable(MissionList[selectedTab]);
                }
                ImGui.EndChild();
            }
        }
        public static bool DrawImageTabButton(string label, ExpeditionTabs tab, ref ExpeditionTabs selectedTab, IDalamudTextureWrap? image = null, float spacingAfter = 5, bool disabled = false, Vector2? uv0 = null, Vector2? uv1 = null)
        {
            float scale = ImGuiHelpers.GlobalScale;

            float horizontalPadding = 8 * scale;
            float verticalPadding = 4 * scale;
            float imageTextSpacing = 4 * scale;
            float imageSize = ImGui.GetTextLineHeight(); // Square image, matched to text height

            var drawList = ImGui.GetWindowDrawList();
            var cursorPos = ImGui.GetCursorScreenPos();

            var textSize = ImGui.CalcTextSize(label);

            float imageWidth = image != null ? imageSize + imageTextSpacing : 0;
            float contentWidth = horizontalPadding * 2 + imageWidth + textSize.X;
            float contentHeight = verticalPadding * 2 + textSize.Y;

            bool isSelected = selectedTab == tab;

            var buttonRect = new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight);
            bool isHovered = !disabled && ImGui.IsMouseHoveringRect(cursorPos, buttonRect)
                          && ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup | ImGuiHoveredFlags.ChildWindows);
            bool isClicked = isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

            if (isClicked && !disabled)
                selectedTab = tab;

            var bgColor = ImGui_Ice.GetButtonColor(isSelected, isHovered, disabled);
            var textColor = disabled
                ? ImGui.GetColorU32(ImGuiCol.TextDisabled)
                : ImGui.GetColorU32(ImGuiCol.Text);

            drawList.AddRectFilled(cursorPos, buttonRect, bgColor, 5.0f * scale);

            // Draw image if provided
            if (image != null)
            {
                var imageMin = new Vector2(cursorPos.X + horizontalPadding, cursorPos.Y + verticalPadding);
                var imageMax = imageMin + new Vector2(imageSize, imageSize);

                drawList.AddImage(image.Handle, imageMin, imageMax, uv0 ?? Vector2.Zero, uv1 ?? Vector2.One, 0xFFFFFFFF);
            }

            // Draw label
            ImGui.SetCursorScreenPos(new Vector2(
                cursorPos.X + horizontalPadding + imageWidth,
                cursorPos.Y + verticalPadding));

            ImGui.PushStyleColor(ImGuiCol.Text, textColor);
            ImGui.Text(label);
            ImGui.PopStyleColor();

            ImGui.SetCursorScreenPos(cursorPos);
            ImGui.InvisibleButton($"##{tab}_btn", new Vector2(contentWidth, contentHeight));
            ImGui.SameLine(0, spacingAfter * scale);

            return isSelected;
        }
        public static void MissionTable(List<uint> missions)
        {
            int GetMissionPriority(CosmicHelper.CosmicInfo info) => info.Attributes switch
            {
                var a when a.HasFlag(MissionAttributes.Critical) => 0,
                var a when a.HasFlag(MissionAttributes.ProvisionalWeather) => 1,
                var a when a.HasFlag(MissionAttributes.ProvisionalTimed) => 2,
                var a when a.HasFlag(MissionAttributes.ProvisionalSequential) => 3,
                _ => info.Rank switch
                {
                    5 => 4,
                    4 => 5,
                    3 => 6,
                    2 => 7,
                    1 => 8,
                    _ => 9
                }
            };

            void TableDetails(List<uint> missionList)
            {
                ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders |
                                             ImGuiTableFlags.Hideable | ImGuiTableFlags.SizingFixedFit;

                if (ImGui.BeginTable($"ExpeditionLog_Table", 9, tableFlags))
                {
                    ImGui.TableSetupColumn(T("Enabled"));
                    ImGui.TableSetupColumn(T("Job"));
                    ImGui.TableSetupColumn(T("Kind"));
                    ImGui.TableSetupColumn(T("ID"));
                    ImGui.TableSetupColumn("完成");
                    ImGui.TableSetupColumn(T("Mission Name"));
                    ImGui.TableSetupColumn(T("Turnin Mode"));
                    ImGui.TableSetupColumn(T("Profile Setting"));
                    ImGui.TableSetupColumn(T("Notes"));

                    // TODO: Make it to where type/rank is shown here instead of notes

                    ImGui.TableHeadersRow();

                    foreach (var entry in missionList)
                    {
                        var Id = entry;
                        var missionConfig = C.MissionConfig[Id];
                        var missionInfo = CosmicHelper.SheetMissionDict[Id];

                        if (HideCompleted && missionInfo.CompletionStatus is CosmicHelper.Status.Gold)
                            continue;

                        ImGui.TableNextRow();
                        ImGui.PushID(Id);
                        if (C.HighlightVisibleMissions && CosmicHelper.CurrentLunarMission == Id)
                        {
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                        }

                        ImGui.TableSetColumnIndex(0);
                        bool enabled = missionConfig.Enabled;
                        if (ImGui_Ice.Table_CenterCheckbox("##EnableMission", ref enabled))
                        {
                            missionConfig.Enabled = enabled;
                            if (missionConfig.Enabled == true)
                            {
                                foreach (var prevMission in CosmicHelper.SheetMissionDict[Id].SequenceMissions_Previous)
                                {
                                    C.MissionConfig[prevMission].Enabled = true;
                                }
                            }

                            C.Save();
                        }

                        ImGui.TableNextColumn();
                        if (missionInfo.Jobs.Count > 1)
                        {
                            ISharedImmediateTexture? job1Icon = CosmicHelper.ClassInfoDict[missionInfo.Jobs.First()].JobIcon;
                            ISharedImmediateTexture? job2Icon = CosmicHelper.ClassInfoDict[missionInfo.Jobs.Last()].JobIcon;
                            Vector2 imageSize = new Vector2(23, 23);

                            ImGui.Image(job1Icon.GetWrapOrEmpty().Handle, imageSize);
                            ImGui.SameLine(0, 2);
                            ImGui.Image(job2Icon.GetWrapOrEmpty().Handle, imageSize);
                        }
                        else
                        {
                            ISharedImmediateTexture? job1Icon = CosmicHelper.ClassInfoDict[missionInfo.Jobs.First()].JobIcon;
                            Vector2 imageSize = new Vector2(23, 23);
                            ImGui.Image(job1Icon.GetWrapOrEmpty().Handle, imageSize);
                        }

                        ImGui.TableNextColumn();
                        MissionTypeButton(missionInfo);

                        ImGui.TableNextColumn();
                        if (C.HighlightVisibleMissions)
                        {
                            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var wksMission) && wksMission.IsAddonReady)
                            {
                                if (wksMission.StellerMissions.Any(x => x.MissionId == Id))
                                {
                                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(new Vector4(1.0f, 0.0f, 0.0f, 0.3f))); // Red with 30% alpha
                                }
                            }
                        }

                        ImGui_Ice.Table_FullCenterText(Id.ToString());

                        ImGui.TableNextColumn();
                        ImGui_Ice.CompletionStatusButton(missionInfo);

                        ImGui.TableNextColumn();
                        ImGui.TextColored(ImGuiColors.DalamudWhite2, missionInfo.Name);
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                            ImGui.SetTooltip(T("Open mission details"));
                        }
                        if (missionInfo.MarkerId != 0)
                        {
                            ImGui.SameLine();
                            ImGui.PushFont(UiBuilder.IconFont);
                            ImGui.Text(FontAwesomeIcon.Flag.ToIconString());
                            ImGui.PopFont();
                            if (ImGui.IsItemClicked())
                            {
                                Utils.SetGatheringRing(missionInfo.TerritoryId, (int)missionInfo.MapPosition.X, (int)missionInfo.MapPosition.Y, missionInfo.Radius, missionInfo.Name);
                            }
#if DEBUG
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("X: {0} Y: {1}", missionInfo.MapPosition.X, missionInfo.MapPosition.Y));
                            }
#endif
                        }
                        if (CosmicHelper.CriticalLocations.TryGetValue(Id, out var criticalLoc))
                        {
                            ImGui.SameLine();
                            ImGuiEx.Icon(FontAwesomeIcon.FlagCheckered);
                            if (ImGui.IsItemClicked())
                            {
                                Utils.SetFlagForNPC(missionInfo.TerritoryId, criticalLoc.MapInfo.X, criticalLoc.MapInfo.Y);
                            }
                        }

                        ImGui.TableNextColumn();
                        if (missionInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining))
                        {
                            ImGui_Ice.Table_FullCenterText(T("Auto"));
                            if (missionConfig.TurninGoal != TurninState.Gold)
                            {
                                missionConfig.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                        }
                        else
                        {
                            Vector4 BronzeColor = new Vector4(0.804f, 0.498f, 0.196f, 1.0f);
                            Vector4 SilverColor = new Vector4(0.753f, 0.753f, 0.753f, 1.0f);
                            Vector4 GoldColor = new Vector4(1.0f, 0.843f, 0.0f, 1.0f);
                            Vector4 DisabledColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

                            var fontSize = ImGui.GetFontSize();
                            var framePadding = ImGui.GetStyle().FramePadding;
                            var buttonSize = new Vector2(fontSize + framePadding.X * 2, fontSize + framePadding.Y * 2);
                            var spacing = ImGui.GetStyle().ItemSpacing.X;
                            var totalWidth = (buttonSize.X * 3) + (spacing * 2);

                            // Center the group
                            var cursorPosX = ImGui.GetCursorPosX();
                            var availWidth = ImGui.GetContentRegionAvail().X;
                            ImGui.SetCursorPosX(cursorPosX + (availWidth - totalWidth) * 0.5f);

                            // Gold
                            ImGui.PushStyleColor(ImGuiCol.Text, missionConfig.TurninGoal == TurninState.Gold ? GoldColor : DisabledColor);
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Gold", buttonSize))
                            {
                                // If AutoTurnin is on, we're enabling individual controls
                                if (missionConfig.AutoTurnin)
                                {
                                    missionConfig.AutoTurnin = false;
                                    missionConfig.TurninGold = false;  // Turn off gold
                                    missionConfig.TurninSilver = true; // Keep others on
                                    missionConfig.TurninBronze = true;
                                }
                                else
                                {
                                    // Toggle the button
                                    missionConfig.TurninGold = !missionConfig.TurninGold;

                                    // Check the new state
                                    if (missionConfig.TurninGold && missionConfig.TurninSilver && missionConfig.TurninBronze)
                                    {
                                        // All three enabled -> AutoTurnin mode
                                        missionConfig.AutoTurnin = true;
                                        missionConfig.TurninGold = false;
                                        missionConfig.TurninSilver = false;
                                        missionConfig.TurninBronze = false;
                                    }
                                    else if (!missionConfig.TurninGold && !missionConfig.TurninSilver && !missionConfig.TurninBronze)
                                    {
                                        // All three disabled -> AutoTurnin mode (don't disable any)
                                        missionConfig.AutoTurnin = true;
                                    }
                                }

                                C.SaveDebounced();
                            }
                            // Right-click to enable only this one
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                missionConfig.AutoTurnin = false;
                                missionConfig.TurninGold = true;
                                missionConfig.TurninSilver = false;
                                missionConfig.TurninBronze = false;
                                C.SaveDebounced();
                            }
                            ImGui.PopStyleColor();
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();

                                if (missionConfig.AutoTurnin)
                                {
                                    ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Gold Enabled"));

                                    ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Silver Enabled"));

                                    ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Bronze Enabled"));
                                }
                                else
                                {
                                    if (missionConfig.TurninGold)
                                    {
                                        ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Gold Enabled"));
                                    }
                                    if (missionConfig.TurninSilver)
                                    {
                                        ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Silver Enabled"));
                                    }
                                    if (missionConfig.TurninBronze)
                                    {
                                        ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Bronze Enabled"));
                                    }
                                }

                                ImGui.Text(T("Right click to only enable gold"));

                                ImGui.EndTooltip();
                            }

                            ImGui.SameLine();

                            // Silver
                            ImGui.PushStyleColor(ImGuiCol.Text, missionConfig.TurninSilver || missionConfig.AutoTurnin ? SilverColor : DisabledColor);
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Silver", buttonSize))
                            {
                                // If AutoTurnin is on, we're enabling individual controls
                                if (missionConfig.AutoTurnin)
                                {
                                    missionConfig.AutoTurnin = false;
                                    missionConfig.TurninGold = true;
                                    missionConfig.TurninSilver = false;  // Turn off silver
                                    missionConfig.TurninBronze = true;
                                }
                                else
                                {
                                    // Toggle the button
                                    missionConfig.TurninSilver = !missionConfig.TurninSilver;

                                    // Check the new state
                                    if (missionConfig.TurninGold && missionConfig.TurninSilver && missionConfig.TurninBronze)
                                    {
                                        // All three enabled -> AutoTurnin mode
                                        missionConfig.AutoTurnin = true;
                                        missionConfig.TurninGold = false;
                                        missionConfig.TurninSilver = false;
                                        missionConfig.TurninBronze = false;
                                    }
                                    else if (!missionConfig.TurninGold && !missionConfig.TurninSilver && !missionConfig.TurninBronze)
                                    {
                                        // All three disabled -> AutoTurnin mode (don't disable any)
                                        missionConfig.AutoTurnin = true;
                                    }
                                }

                                C.SaveDebounced();
                            }
                            // Right-click to enable only this one
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                missionConfig.AutoTurnin = false;
                                missionConfig.TurninGold = false;
                                missionConfig.TurninSilver = true;
                                missionConfig.TurninBronze = false;
                                C.SaveDebounced();
                            }
                            ImGui.PopStyleColor();
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();

                                if (missionConfig.AutoTurnin)
                                {
                                    ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Gold Enabled"));

                                    ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Silver Enabled"));

                                    ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Bronze Enabled"));
                                }
                                else
                                {
                                    if (missionConfig.TurninGold)
                                    {
                                        ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Gold Enabled"));
                                    }
                                    if (missionConfig.TurninSilver)
                                    {
                                        ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Silver Enabled"));
                                    }
                                    if (missionConfig.TurninBronze)
                                    {
                                        ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Bronze Enabled"));
                                    }
                                }

                                ImGui.Text(T("Right click to only enable silver"));

                                ImGui.EndTooltip();
                            }

                            ImGui.SameLine();

                            // Bronze
                            ImGui.PushStyleColor(ImGuiCol.Text, missionConfig.TurninBronze || missionConfig.AutoTurnin ? BronzeColor : DisabledColor);
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Bronze", buttonSize))
                            {
                                // If AutoTurnin is on, we're enabling individual controls
                                if (missionConfig.AutoTurnin)
                                {
                                    missionConfig.AutoTurnin = false;
                                    missionConfig.TurninGold = true;
                                    missionConfig.TurninSilver = true;
                                    missionConfig.TurninBronze = false;  // Turn off bronze
                                }
                                else
                                {
                                    // Toggle the button
                                    missionConfig.TurninBronze = !missionConfig.TurninBronze;

                                    // Check the new state
                                    if (missionConfig.TurninGold && missionConfig.TurninSilver && missionConfig.TurninBronze)
                                    {
                                        // All three enabled -> AutoTurnin mode
                                        missionConfig.AutoTurnin = true;
                                        missionConfig.TurninGold = false;
                                        missionConfig.TurninSilver = false;
                                        missionConfig.TurninBronze = false;
                                    }
                                    else if (!missionConfig.TurninGold && !missionConfig.TurninSilver && !missionConfig.TurninBronze)
                                    {
                                        // All three disabled -> AutoTurnin mode (don't disable any)
                                        missionConfig.AutoTurnin = true;
                                    }
                                }

                                C.SaveDebounced();
                            }
                            // Right-click to enable only this one
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                missionConfig.AutoTurnin = false;
                                missionConfig.TurninGold = false;
                                missionConfig.TurninSilver = false;
                                missionConfig.TurninBronze = true;
                                C.SaveDebounced();
                            }
                            ImGui.PopStyleColor();
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();

                                if (missionConfig.AutoTurnin)
                                {
                                    ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Gold Enabled"));

                                    ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Silver Enabled"));

                                    ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                    ImGui.SameLine();
                                    ImGui.Text(T("Bronze Enabled"));
                                }
                                else
                                {
                                    if (missionConfig.TurninGold)
                                    {
                                        ImGuiEx.Icon(GoldColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Gold Enabled"));
                                    }
                                    if (missionConfig.TurninSilver)
                                    {
                                        ImGuiEx.Icon(SilverColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Silver Enabled"));
                                    }
                                    if (missionConfig.TurninBronze)
                                    {
                                        ImGuiEx.Icon(BronzeColor, FontAwesomeIcon.Trophy);
                                        ImGui.SameLine();
                                        ImGui.Text(T("Bronze Enabled"));
                                    }
                                }

                                ImGui.Text(T("Right click to only enable bronze"));

                                ImGui.EndTooltip();
                            }
                        }

                        ImGui.TableNextColumn();
                        bool gatherProfile = missionInfo.Attributes.HasFlag(MissionAttributes.Gather);
                        bool collectable = missionInfo.Attributes.HasFlag(MissionAttributes.Collectables) || missionInfo.Attributes.HasFlag(MissionAttributes.ReducedItems);

                        if (gatherProfile && !collectable)
                        {
                            string profileName = "???";
                            if (C.GatherProfiles.TryGetValue(missionConfig.GProfileId, out var profileSetting))
                            {
                                profileName = profileSetting.Name;
                            }
                            else
                            {
                                profileName = "???";
                            }

                            if (ImGui_Ice.Table_CenteredButton(T(profileName)))
                            {
                                ImGui.OpenPopup("Selecting Gathering Profile");
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Select profile to use"));
                                ImGui.EndTooltip();
                            }
                            if (ImGui.BeginPopup("Selecting Gathering Profile"))
                            {
                                ImGui.Text(T("Currently Selected: {0}", T(profileName)));
                                ImGui.Separator();

                                foreach (var profile in C.GatherProfiles)
                                {
                                    var id = profile.Key;
                                    bool profileSelected = missionConfig.GProfileId == id;
                                    ImGui.PushID($"{id}_{profile.Value.Name}");
                                    if (ImGui.RadioButton($"{T(profile.Value.Name)}##{profile.Value.Name}", profileSelected))
                                    {
                                        missionConfig.GProfileId = id;
                                        C.Save();
                                    }
                                    ImGui.PopID();
                                }

                                ImGui.EndPopup();
                            }
                        }
                        else if (gatherProfile && collectable)
                        {
                            ImGui_Ice.Table_FullCenterText(T("Auto"));
                        }
                        else if (missionInfo.Attributes.HasFlag(MissionAttributes.Fish))
                        {
                            if (ImGui_Ice.Table_CenteredButton(T("Select Profile")))
                            {
                                ImGui.OpenPopup("Select Fishing Profile");
                            }
                            if (ImGui.BeginPopup("Select Fishing Profile"))
                            {
                                ImGui.Text(T("Fishing profile: {0}", missionInfo.Name));
                                ImGui.Separator();
                                bool builtInPreset = missionConfig.Use_BuildinPreset;
                                if (ImGui.Checkbox(T("Use Built In Preset"), ref builtInPreset))
                                {
                                    missionConfig.Use_BuildinPreset = builtInPreset;
                                    C.Save();
                                }
                                ImGuiEx.HelpMarker(T("Having this enabled means it will use the default preset that is included with the plugin for autohook.\nIf you would like to use one that you already have in autohook, you can un-checkmark this and type the name of it below"));
                                using (ImRaii.Disabled(builtInPreset))
                                {
                                    string presetName = missionConfig.AutoHookPresetName;
                                    ImGui.SetNextItemWidth(200);
                                    if (ImGui.InputText(T("Preset Name"), ref presetName))
                                    {
                                        missionConfig.AutoHookPresetName = presetName;
                                        C.Save();
                                    }
                                }

                                ImGui.EndPopup();
                            }
                        }
                        if (missionInfo.Attributes.HasFlag(MissionAttributes.Craft))
                        {
                            if (ImGui.Button(T("Open Craft Settings")))
                            {
                                ImGui.OpenPopup("Craft Settings: Recipies");
                            }

                            if (ImGui.BeginPopup("Craft Settings: Recipies"))
                            {
                                ImGui.TextDisabled($"{entry}");
                                ImGui.SameLine();
                                ImGui.Text(T("Mission: {0}", missionInfo.Name));

                                Mission_Table.CrafterManagement(missionInfo, entry);

                                ImGui.EndPopup();
                            }
                        }

                        #region Notes

                        ImGui.TableNextColumn();
                        int notesCount = 0;

                        if (missionInfo.Attributes.HasFlag(MissionAttributes.ProvisionalTimed))
                        {
                            ImGui_Ice.Table_FontCenter(FontAwesomeIcon.Clock);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"{missionInfo.StartTime}:00 - {missionInfo.EndTime - 1}:59");
                                ImGui.EndTooltip();
                            }
                            notesCount++;
                        }
                        if (missionInfo.SequenceMissions_Previous.Count() != 0 || missionInfo.SequenceMissions_Next.Count() != 0)
                        {
                            if (notesCount != 0)
                                ImGui.SameLine(0, 5);

                            ImGui_Ice.Table_FontCenter(FontAwesomeIcon.ListOl);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Sequence Missions"));
                                if (missionInfo.SequenceMissions_Previous.Count() != 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Previous Missions"));
                                    foreach (var prevMission in missionInfo.SequenceMissions_Previous)
                                    {
                                        ImGui.Text($"[{prevMission}] - {CosmicHelper.SheetMissionDict[prevMission].Name}");
                                    }
                                }

                                if (missionInfo.SequenceMissions_Next.Count() != 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Next Missions"));
                                    foreach (var nextMission in missionInfo.SequenceMissions_Next)
                                    {
                                        ImGui.Text($"[{nextMission}] - {CosmicHelper.SheetMissionDict[nextMission].Name}");
                                    }
                                }
                                ImGui.EndTooltip();
                            }
                            notesCount++;
                        }
                        if (missionInfo.Attributes.HasFlag(MissionAttributes.ProvisionalWeather))
                        {
                            if (notesCount > 0)
                                ImGui.SameLine(0, 2);

                            if (CosmicHelper.WeatherIds.ContainsKey(missionInfo.Weather))
                            {
                                ISharedImmediateTexture? weatherIcon = CosmicHelper.WeatherIconDict[missionInfo.Weather];
                                Vector2 ImageSize = new Vector2(23, 23);
                                ImGui.Image(weatherIcon.GetWrapOrEmpty().Handle, ImageSize);
                            }
                            else
                            {
                                ImGui_Ice.Table_FontCenter(FontAwesomeIcon.Cloud);
                            }

                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Weather: {0}", T(CosmicHelper.GetCosmicWeatherName(missionInfo.Weather))));
                                ImGui.EndTooltip();
                            }
                            notesCount++;
                        }
                        if (CosmicHelper.MissionUnlock.TryGetValue(Id, out var unlock))
                        {
                            if (notesCount > 0)
                                ImGui.SameLine(0, 2);

                            if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                            {
                                if (tex.TryGetWrap(out var wrap, out var exc))
                                {
                                    ImGui.Image(wrap.Handle, new Vector2(23, 23), new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                                }
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("The following missions are required to have gold before you can do this one"));
                                foreach (var mission in unlock)
                                {
                                    ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[mission]);
                                    ImGui.SameLine();
                                    ImGui.Text($"[{mission}] - {CosmicHelper.SheetMissionDict[mission].Name}");
                                }
                                ImGui.EndTooltip();
                            }
                            notesCount++;

                        }
                        if (missionInfo.Jobs.Count > 1)
                        {
                            if (notesCount > 0)
                                ImGui.SameLine(0, 2);

                            ISharedImmediateTexture? job1Icon = CosmicHelper.ClassInfoDict[missionInfo.Jobs.First()].JobIcon;
                            ISharedImmediateTexture? job2Icon = CosmicHelper.ClassInfoDict[missionInfo.Jobs.Last()].JobIcon;
                            Vector2 imageSize = new Vector2(23, 23);

                            ImGui.Image(job1Icon.GetWrapOrEmpty().Handle, imageSize);
                            ImGui.SameLine(0, 2);
                            ImGui.Image(job2Icon.GetWrapOrEmpty().Handle, imageSize);
                            notesCount++;
                        }
                        /*
                        if (CosmicHelper.CustomMissionNotes.TryGetValue(Id, out var notes))
                        {
                            if (notesCount > 0)
                                ImGui.SameLine();
                            ImGuiEx.Icon(FontAwesomeIcon.Trophy);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T(notes.NoteInfo));
                                ImGui.Text(T("Average Score Per Minute: {0:N2}", notes.SPM));

                                ImGui.EndTooltip();
                            }
                        }
                        */

                        #endregion
                    }

                    ImGui.EndTable();
                }
            }

            var sortedMissions = missions.OrderBy(m => GetMissionPriority(CosmicHelper.SheetMissionDict[m])).ToList();
            TableDetails(sortedMissions);
        }
        public static void ClassProgress()
        {
            var expInfo = CosmicHelper.Cosmic_ClassInfo();

            if (SelectedJob == 0)
            {
                var researchTypes = new (string Name, string Asset, int MaxLv)[]
                {
                    ("Sinus", "ICE.Resources.ResearchIcons.novice.png", 9),
                    ("Phaenna", "ICE.Resources.ResearchIcons.intermediate.png", 14),
                    ("Oizys", "ICE.Resources.ResearchIcons.advance.png", 17),
                };

                if (ImGui.BeginTable("Class Progress: Icon Preview", 3, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn(T("Sinus"));
                    ImGui.TableSetupColumn(T("Phaenna"));
                    ImGui.TableSetupColumn(T("Oizys"));

                    ImGui.TableNextRow();
                    for (int i = 0; i < researchTypes.Count(); i++)
                    {
                        var type = researchTypes[i];
                        var icon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), type.Asset).GetWrapOrEmpty();
                        ImGui.TableSetColumnIndex(i);
                        ImGui.Image(icon.Handle, new(24, 24));
                    }

                    ImGui.TableNextRow();
                    for (int i = 0; i < researchTypes.Count(); i++)
                    {
                        var type = researchTypes[i];
                        ImGui.TableSetColumnIndex(i);
                        var count = expInfo.Where(x => x.Value.Stage_Current >= type.MaxLv).Count();
                        ImGui_Ice.Table_FullCenterText($"{count}/11");
                    }

                    ImGui.EndTable();
                }

                if (ImGui.BeginTable("Class Progress: All", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn(T("Job"));
                    ImGui.TableSetupColumn(T("Relic"));
                    ImGui.TableSetupColumn("##Relic_XPBar");
                    ImGui.TableSetupColumn(T("Score"));
                    ImGui.TableSetupColumn("##Score_XPBar");

                    ImGui.TableHeadersRow();

                    foreach (var job in CosmicHelper.ClassInfoDict.OrderBy(x => x.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        var icon = job.Value;
                        ImGui.Image(icon.JobIcon.GetWrapOrEmpty().Handle, new(24, 24));

                        if (expInfo.TryGetValue(job.Key, out var jobInfo))
                        {
                            float currentExpStage = 0;
                            if (jobInfo.Stage_Current != jobInfo.Stage_Next)
                            {
                                currentExpStage = jobInfo.Stage_Current;
                            }
                            else
                            {
                                int completedSubStages = 0;
                                foreach (var exp in jobInfo.CurrentExp)
                                {
                                    if (exp.Value.Current == exp.Value.Max)
                                        completedSubStages++;
                                }
                                // Use integer math, then convert once at the end
                                currentExpStage = CosmicHelper.MaxRelicLevel + (completedSubStages / 10f);
                            }

                            ImGui.TableNextColumn();
                            ImGui_Ice.Table_FullCenterText($"{currentExpStage}");

                            ImGui.TableNextColumn();

                            // Get the column's available width and current cursor position
                            var colWidth = ImGui.GetColumnWidth();
                            var cellMin = ImGui.GetCursorScreenPos();

                            float barHeight = 10f;
                            float rowHeight = ImGui.GetFrameHeight(); // matches text/icon row height

                            // Vertically center the bar
                            float offsetY = (rowHeight - barHeight) / 2f;
                            ImGui.SetCursorScreenPos(new Vector2(cellMin.X, cellMin.Y + offsetY));

                            ImGui_Ice.Draw_XPBar(currentExpStage, CosmicHelper.MaxRelicLevel, CosmicHelper.MaxRelicExpStatus, size: new Vector2(200, barHeight));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui_Ice.Draw_ExpTable(job.Key);
                                ImGui.EndTooltip();
                            }

                            ImGui.TableNextColumn();
                            ImGui_Ice.Table_FullCenterText($"{jobInfo.Score:N0}");

                            ImGui.TableNextColumn();
                            var scoreColWidth = ImGui.GetColumnWidth();
                            var scoreCellMin = ImGui.GetCursorScreenPos();

                            ImGui.SetCursorScreenPos(new Vector2(scoreCellMin.X, scoreCellMin.Y + offsetY));
                            ImGui_Ice.Draw_XPBar(jobInfo.Score, 500_000, 500_000, size: new Vector2(200, barHeight));
                        }
                    }

                    ImGui.EndTable();
                }
            }
            else
            {
                if (!expInfo.TryGetValue(SelectedJob, out var jobStatus))
                {
                    ImGui.TextWrapped(T("No expedition data is available for this class yet."));
                    return;
                }

                if (ImGui.BeginTable("Specific Class Details", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn(T("Info"));
                    ImGui.TableSetupColumn(T("ExpBar"));

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.Table_FullCenterText(T("Class Score"));

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.Table_FullCenterText($"{jobStatus.Score:N0}");

                    ImGui.TableNextColumn();
                    // Get the column's available width and current cursor position
                    var colWidth = ImGui.GetColumnWidth();
                    var cellMin = ImGui.GetCursorScreenPos();

                    float barHeight = 10f;
                    float rowHeight = ImGui.GetFrameHeight(); // matches text/icon row height

                    // Vertically center the bar
                    float offsetY = (rowHeight - barHeight) / 2f;
                    ImGui.SetCursorScreenPos(new Vector2(cellMin.X, cellMin.Y + offsetY));

                    ImGui_Ice.Draw_XPBar(jobStatus.Score, 500_000, 500_000, size: new(200, barHeight));

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Stage"));

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.Table_FullCenterText($" {jobStatus.Stage_Current} / {CosmicHelper.MaxRelicLevel}");

                    ImGui.TableNextColumn();
                    var LvCellMin = ImGui.GetCursorScreenPos();
                    ImGui.SetCursorScreenPos(new Vector2(LvCellMin.X, LvCellMin.Y + offsetY));
                    ImGui_Ice.Draw_XPBar(jobStatus.Stage_Current, CosmicHelper.MaxRelicLevel, CosmicHelper.MaxRelicLevel);

                    foreach (var exp in jobStatus.CurrentExp)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui_Ice.Table_FullCenterText($"{exp.Value.Name} | {exp.Value.Current:N0} / {exp.Value.Needed:N0}");

                        ImGui.TableNextColumn();
                        var scoreCellMin = ImGui.GetCursorScreenPos();
                        ImGui.SetCursorScreenPos(new Vector2(scoreCellMin.X, scoreCellMin.Y + offsetY));
                        ImGui_Ice.Draw_XPBar(exp.Value.Current, exp.Value.Needed, exp.Value.Max, size: new(200, barHeight));
                    }


                    ImGui.EndTable();
                }
            }
        }
        private static void MissionTypeButton(CosmicHelper.CosmicInfo missionInfo)
        {
            if (missionInfo.IsCritical)
            {
                var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.Red_Alert.png").GetWrapOrEmpty();
                DrawCenteredImage(texture, 23f);
            }
            else if (missionInfo.IsWeather)
            {
                if (CosmicHelper.WeatherIds.ContainsKey(missionInfo.Weather))
                {
                    var weatherIcon = CosmicHelper.WeatherIconDict[missionInfo.Weather].GetWrapOrEmpty();
                    DrawCenteredImage(weatherIcon, 23f);
                }
                else
                {
                    ImGui_Ice.Table_FullCenterFont(FontAwesomeIcon.Cloud);
                }

                if (ImGui.IsItemHovered())
                    DrawTooltip(() => ImGui.Text(T("Weather: {0}", T(CosmicHelper.GetCosmicWeatherName(missionInfo.Weather)))));
            }
            else if (missionInfo.IsTimed)
            {
                ImGui_Ice.Table_FullCenterFont(FontAwesomeIcon.Clock);

                if (ImGui.IsItemHovered())
                    DrawTooltip(() => ImGui.Text($"{missionInfo.StartTime}:00 - {missionInfo.EndTime - 1}:59"));
            }
            else if (missionInfo.IsSequence)
            {
                ImGui_Ice.Table_FullCenterFont(FontAwesomeIcon.ListOl);

                if (ImGui.IsItemHovered())
                {
                    DrawTooltip(() =>
                    {
                        ImGui.Text(T("Sequence Missions"));

                        if (missionInfo.SequenceMissions_Previous.Any())
                        {
                            ImGui.Separator();
                            ImGui.Text(T("Previous Missions"));
                            foreach (var prev in missionInfo.SequenceMissions_Previous)
                                ImGui.Text($"[{prev}] - {CosmicHelper.SheetMissionDict[prev].Name}");
                        }

                        if (missionInfo.SequenceMissions_Next.Any())
                        {
                            ImGui.Separator();
                            ImGui.Text(T("Next Missions"));
                            foreach (var next in missionInfo.SequenceMissions_Next)
                                ImGui.Text($"[{next}] - {CosmicHelper.SheetMissionDict[next].Name}");
                        }
                    });
                }
            }
            else
            {
                var rank = missionInfo switch
                {
                    { ARank: true } => "A",
                    { BRank: true } => "B",
                    { CRank: true } => "C",
                    { Drank: true } => "D",
                    _ => ""
                };

                if (rank.Length > 0)
                    ImGui_Ice.Table_FullCenterText(rank);
            }
        }
        private static void DrawCenteredImage(IDalamudTextureWrap image, float size)
        {
            var cellWidth = ImGui.GetContentRegionAvail().X;
            var cellHeight = ImGui.GetFrameHeight();
            var cursor = ImGui.GetCursorPos();

            ImGui.SetCursorPosX(cursor.X + (cellWidth - size) * 0.5f);
            ImGui.SetCursorPosY(cursor.Y + (cellHeight - size) * 0.5f);
            ImGui.Image(image.Handle, new Vector2(size));
        }
        private static void DrawTooltip(Action content)
        {
            ImGui.BeginTooltip();
            content();
            ImGui.EndTooltip();
        }
    }
}

