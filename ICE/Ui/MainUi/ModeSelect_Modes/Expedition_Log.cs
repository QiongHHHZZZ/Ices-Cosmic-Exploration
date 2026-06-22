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
using System.Linq;
using System.Reflection;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.ModeSelect_Modes
{
    internal class Expedition_Log
    {

        private static uint SelectedJob = 8;
        private const uint ProgressTabId = 0;
        private static uint selectedTabId = ProgressTabId;
        private static bool HideCompleted = false;

        private static Completion_Table? CompletionTable;
        private static List<CosmicHelper.MissionInfo> TableItems = [];
        private static int ItemCount = 0;
        private static string newListName = string.Empty;

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
                if (ImGui_Ice.SliderButton("Hide Completed", "Hide Completed", ref HideCompleted))
                {
                    ResetCompletionTable();
                }

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
                    string displayName = T(name);
                    ImGui.AlignTextToFramePadding();
                    if (ImGui.Selectable($"{displayName}##{name}_{jobInfo.Key}", SelectedJob == jobInfo.Key, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        SelectedJob = jobInfo.Key;
                        ResetCompletionTable();
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
                    ResetCompletionTable();
                }

                ImGui.EndTable();
            }
        }
        private static Dictionary<uint, List<uint>> MissionList = CosmicMoonRegistry.All.ToDictionary(m => m.TerritoryId, _ => new List<uint>());

        private static uint TabIdForMoon(CosmicMoonDefinition moon) => moon.TerritoryId;
        private static void ClassDetails()
        {
            float scale = ImGuiHelpers.GlobalScale;

            float scrollbarSize = ImGui.GetStyle().ScrollbarSize;
            float buttonRowHeight = (ImGui.GetTextLineHeight() + 8 * scale + 4 * scale) + scrollbarSize;

            if (ImGui.BeginChild("##Expedition_TabScroll", new(0, buttonRowHeight), false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (SelectedJob != 0)
                {
                    var classIcon = CosmicHelper.ClassInfoDict[SelectedJob];
                    DrawImageTabButton(T("Class Progress"), ProgressTabId, ref selectedTabId, classIcon.JobIcon.GetWrapOrEmpty());
                }
                else
                {
                    var allClassTexture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.CosmicClassTracker.png").GetWrapOrEmpty();
                    DrawImageTabButton(T("All Class progresses"), ProgressTabId, ref selectedTabId, allClassTexture);
                }

                foreach (var moon in CosmicMoonRegistry.All.OrderBy(m => m.ExpeditionTabIndex))
                {
                    var tabId = TabIdForMoon(moon);
                    if (tabId == ProgressTabId)
                        continue;

                    List<uint> missions = new();
                    uint completed = 0;

                    if (SelectedJob != 0)
                    {
                        foreach (var mission in CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Contains(SelectedJob)).Where(x => x.Value.TerritoryId == moon.TerritoryId))
                        {
                            missions.Add(mission.Key);
                            if (mission.Value.CompletionStatus is CosmicHelper.Status.Gold)
                                completed += 1;
                        }
                    }
                    else
                    {
                        foreach (var mission in CosmicHelper.SheetMissionDict.Where(x => x.Value.TerritoryId == moon.TerritoryId))
                        {
                            missions.Add(mission.Key);
                            if (mission.Value.CompletionStatus is CosmicHelper.Status.Gold)
                                completed += 1;
                        }
                    }

                    var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), moon.IconResource).GetWrapOrEmpty();
                    ImGui.SameLine();
                    if (DrawImageTabButton($"{T(moon.DisplayName)} [{completed} / {missions.Count()}]", tabId, ref selectedTabId, texture))
                    {
                        ResetCompletionTable();
                    }
                }
                ImGui_Ice.EndCategoryButtonRow();
            }
            ImGui.EndChild();

            var bottomSpace = ImGui.GetTextLineHeight() + 6f;
            bottomSpace += 12f; // prevent the tabs from creating a scrollbar

            Vector2 size = new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - bottomSpace);
            if (ImGui.BeginChild("###CompletionTableV2", size, false))
            {
                if (selectedTabId == ProgressTabId)
                {
                    if (ImGui.BeginChild(T("Class Progress")))
                    {
                        ClassProgress();
                    }
                    ImGui.EndChild();
                }
                else
                {
                    try
                    {
                        if (CompletionTable == null && CosmicHelper.SheetMissionDict.Count > 0)
                        {
                            foreach (var mission in CosmicHelper.SheetMissionDict)
                            {
                                if (mission.Value.TerritoryId != selectedTabId)
                                    continue;

                                if (HideCompleted && mission.Value.CompletionStatus == CosmicHelper.Status.Gold)
                                    continue;

                                if (SelectedJob != 0 && !mission.Value.Jobs.Contains(SelectedJob))
                                    continue;

                                CosmicHelper.MissionInfo missionDetails = new() { Id = mission.Key };
                                TableItems.Add(missionDetails);

                                if (mission.Value.SequenceMissions_Previous.Count() != 0)
                                {
                                    foreach (var prevMission in mission.Value.SequenceMissions_Previous)
                                    {
                                        if (!TableItems.Any(x => x.Id == prevMission))
                                        {
                                            CosmicHelper.MissionInfo prevMissionDetails = new() { Id = prevMission };
                                            TableItems.Add(prevMissionDetails);
                                        }
                                    }
                                }
                            }
                            ItemCount = TableItems.Count();
                            CompletionTable = new(TableItems);
                        }
                        var filterActive = CompletionTable.FilteredItems.Count != 0 && CompletionTable.FilteredItems.Count != ItemCount;
                        var filterCount = filterActive ? $" (of {ItemCount})" : "";
                        var height = ImGui.GetFrameHeight();
                        CompletionTable.Draw(height + 4f);
                    }
                    catch (Exception ex)
                    {
                        IceLogging.Error(ex.Message, "Drawing Completion Table");
                    }
                }
            }
            ImGui.EndChild();
        }
        public static bool DrawImageTabButton(string label, uint tabId, ref uint selectedTabId, IDalamudTextureWrap? image = null, float spacingAfter = 5, bool disabled = false, Vector2? uv0 = null, Vector2? uv1 = null)
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

            bool isSelected = selectedTabId == tabId;

            var buttonRect = new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight);
            bool isHovered = !disabled && ImGui.IsMouseHoveringRect(cursorPos, buttonRect)
                          && ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup | ImGuiHoveredFlags.ChildWindows);
            bool isClicked = isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

            if (isClicked && !disabled)
            {
                selectedTabId = tabId;
            }

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
            ImGui.InvisibleButton($"##{tabId}_btn", new Vector2(contentWidth, contentHeight));
            ImGui.SameLine(0, spacingAfter * scale);

            return isSelected;
        }
        public static void ClassProgress()
        {
            var expInfo = CosmicHelper.Cosmic_ClassInfo();

            if (SelectedJob == 0)
            {
                var researchTypes = CosmicMoonRegistry.All
                    .Where(m => m.ResearchIconResource != null)
                    .Select(m => (Name: T(m.DisplayName), Asset: m.ResearchIconResource!, MaxRelicStage: m.MaxRelicStage))
                    .ToArray();

                if (ImGui.BeginTable("Class Progress: Icon Preview", researchTypes.Length, ImGuiTableFlags.SizingFixedFit))
                {
                    for (int i = 0; i < researchTypes.Length; i++)
                        ImGui.TableSetupColumn(researchTypes[i].Name);

                    ImGui.TableNextRow();
                    for (int i = 0; i < researchTypes.Length; i++)
                    {
                        var type = researchTypes[i];
                        var icon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), type.Asset).GetWrapOrEmpty();
                        ImGui.TableSetColumnIndex(i);
                        ImGui.Image(icon.Handle, new(24, 24));
                    }

                    ImGui.TableNextRow();
                    for (int i = 0; i < researchTypes.Length; i++)
                    {
                        var type = researchTypes[i];
                        ImGui.TableSetColumnIndex(i);
                        var count = expInfo.Where(x => x.Value.Stage_Current >= type.MaxRelicStage).Count();
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
                                // Bar max is highest relic stage across all moons (20 on Auxesia; was hardcoded 17).
                                currentExpStage = CosmicMoonRegistry.HighestMaxRelicStage + (completedSubStages / 10f);
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

                            ImGui_Ice.Draw_XPBar(currentExpStage, CosmicMoonRegistry.HighestMaxRelicStage, CosmicHelper.MaxRelicExpStatus, size: new Vector2(200, barHeight));
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
                var maxRelicStage = CosmicMoonRegistry.GetMaxRelicStage((uint)Svc.ClientState.TerritoryType);
                var jobStatus = expInfo[SelectedJob];
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
                    ImGui_Ice.Table_FullCenterText($" {jobStatus.Stage_Current} / {maxRelicStage}");

                    ImGui.TableNextColumn();
                    var LvCellMin = ImGui.GetCursorScreenPos();
                    ImGui.SetCursorScreenPos(new Vector2(LvCellMin.X, LvCellMin.Y + offsetY));
                    ImGui_Ice.Draw_XPBar(jobStatus.Stage_Current, maxRelicStage, maxRelicStage);

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
        private static void ResetCompletionTable()
        {
            CompletionTable = null;
            TableItems.Clear();
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

