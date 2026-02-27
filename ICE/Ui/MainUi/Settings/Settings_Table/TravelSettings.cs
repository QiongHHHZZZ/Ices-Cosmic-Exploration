using Dalamud.Interface;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Pictomancy;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class TravelSettings
    {
        private static bool visualizeRadius = false;
        private static bool visualizeDismountRadius = false;
        private static Dictionary<uint, string> availableMounts = new();

        private static string mountSearchText = "";
        private static int mountDisplayOffset = 0;
        private static int mountItemsPerPage = 10;

        public static unsafe void Draw()
        {
            MountSelection();
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
            PathfindingSettings();
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
            StuckSettings();
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
            CraftingLocations();
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
            DailyRoutinesExtensions();
        }

        private static unsafe void MountSelection()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Feather, T("Mount Settings"));
            ImGui.Dummy(new Vector2(0, 5));

            bool mountOutsideMission = C.UseMountOutsideMission;
            bool mountInMission = C.UseMountInMission;
            float minMountRange = C.MountRadius;
            float dismountRange = C.DismountRadius;

if (ImGui.Button(T("Select Mounting Option")))
            {
                availableMounts.Clear();
                availableMounts[0] = T("Mount Roulette");

                var mountSheet = Svc.Data.GetExcelSheet<Mount>();

                foreach (var mountItem in mountSheet)
                {
                    if (!PlayerState.Instance()->IsMountUnlocked(mountItem.RowId)) continue;

                    string mountName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mountItem.Singular.ToString().ToLower());
                    uint id = mountItem.RowId;

                    availableMounts[id] = mountName;
                }

                mountSearchText = "";
                mountDisplayOffset = 0;

                ImGui.OpenPopup("Mount Options");
            }
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            string mountDisplayName = C.MountId == 0 ? T("Mount Roulette") : C.MountName;
            ImGui.Text(T("Mount: {0}", mountDisplayName));

            if (ImGui.BeginPopup("Mount Options"))
            {
ImGui.InputText(T("Search"), ref mountSearchText, 100);

                var filteredMounts = availableMounts
                    .Where(kvp => string.IsNullOrEmpty(mountSearchText) ||
                                  kvp.Value.Contains(mountSearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                int totalItems = filteredMounts.Count;
                int maxOffset = Math.Max(0, totalItems - mountItemsPerPage);
                mountDisplayOffset = Math.Min(mountDisplayOffset, maxOffset);

                var displayMounts = filteredMounts
                    .Skip(mountDisplayOffset)
                    .Take(mountItemsPerPage);

                foreach (var mount in displayMounts)
                {
                    if (ImGui.Selectable($"{mount.Value}##{mount.Key}"))
                    {
                        C.MountId = mount.Key;
                        C.MountName = mount.Value;
                        C.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.Separator();

if (ImGui.Button(T("Previous")) && mountDisplayOffset > 0)
                {
                    mountDisplayOffset = Math.Max(0, mountDisplayOffset - mountItemsPerPage);
                }

                ImGui.SameLine();
                ImGui.Text(T("{0}-{1} of {2}", mountDisplayOffset + 1, Math.Min(mountDisplayOffset + mountItemsPerPage, totalItems), totalItems));

                ImGui.SameLine();
if (ImGui.Button(T("Next")) && mountDisplayOffset < maxOffset)
                {
                    mountDisplayOffset = Math.Min(maxOffset, mountDisplayOffset + mountItemsPerPage);
                }

                ImGui.EndPopup();
            }

if (ImGui.Checkbox(T("Use mount outside mission"), ref mountOutsideMission))
            {
                C.UseMountOutsideMission = mountOutsideMission;
                C.Save();
            }

if (ImGui.Checkbox(T("Use mount in mission"), ref mountInMission))
            {
                C.UseMountInMission = mountInMission;
                C.Save();
            }

            ImGui.SetNextItemWidth(100);
            if (ImGui.DragFloat(T("Minimum Mounting Range"), ref minMountRange, 1))
            {
                C.MountRadius = minMountRange;
                C.Save();
            }
            ImGui.SameLine();
ImGui.Checkbox(T("Visualize radius"), ref visualizeRadius);
            ImGui.SetNextItemWidth(100);
            if (ImGui.DragFloat(T("Dismount Target Range"), ref dismountRange, 1))
            {
                C.DismountRadius = dismountRange;
                C.Save();
            }
            ImGui.SameLine();
ImGui.Checkbox(T("Visualize Dismount Radius"), ref visualizeDismountRadius);

            using (var drawList = PictoService.Draw(hints: Utils.GetPictoHints()))
            {
                if (drawList == null)
                    return;

                var playerPos = Player.Position;

                if (visualizeRadius)
                    PictoService.VfxRenderer.AddCircle("Mount_Radius Circle", playerPos, C.MountRadius, Utils.FromUintABGR(2616716297));
                if (visualizeDismountRadius)
                    PictoService.VfxRenderer.AddCircle("Dismount_Radius Circle", playerPos, C.DismountRadius, Utils.FromUintABGR(2601121571));
            }
        }

        private static void PathfindingSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Route, T("Pathfinding"));
            ImGui.Dummy(new Vector2(0, 5));

            bool stellarSprint = C.MoonSprint;
if (ImGui.Checkbox(T("Auto-Use Stellar Sprint"), ref stellarSprint))
            {
                C.MoonSprint = stellarSprint;
                C.Save();
            }

            bool closestNode = C.ClosestNodeSelection;
if (ImGui.Checkbox(T("Prioritize closest gathering node"), ref closestNode))
            {
                C.ClosestNodeSelection = closestNode;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
ImGui.SetTooltip(T("Always navigate to the closest targetable node instead of following the fixed route order.\nUseful for timed EX+ missions where speed matters."));
            }

            bool randomize = C.RandomizeWaypoints;
if (ImGui.Checkbox(T("Randomize waypoint positions"), ref randomize))
            {
                C.RandomizeWaypoints = randomize;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
ImGui.SetTooltip(T("Adds a small random offset to navigation destinations so the character doesn't always follow the exact same path"));
            }
            if (randomize)
            {
                ImGui.SameLine();
                float radius = C.RandomizeWaypointsRadius;
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderFloat(T("Randomize radius (yalms)"), ref radius, 0.5f, 1.0f, "%.1f"))
                {
                    C.RandomizeWaypointsRadius = radius;
                    C.SaveDebounced();
                }
                bool showDebug = C.RandomizeWaypointsDebug;
if (ImGui.Checkbox(T("Show random location debug target"), ref showDebug))
                {
                    C.RandomizeWaypointsDebug = showDebug;
                    C.Save();
                }
            }

            bool useHubReturn = C.UseHubReturn;
if (ImGui.Checkbox(T("Use Hub Return"), ref useHubReturn))
            {
                C.UseHubReturn = useHubReturn;
                C.Save();
            }
            ImGui.SameLine();
            bool useAethernet = C.UseAethernet;
if (ImGui.Checkbox(T("Use Aethernet"), ref useAethernet))
            {
                C.UseAethernet = useAethernet;
                C.Save();
            }

            bool avoidStellarReturn = C.AvoidStellarReturn;
if (ImGui.Checkbox(T("Avoid Stellar Return for pathing"), ref avoidStellarReturn))
            {
                C.AvoidStellarReturn = avoidStellarReturn;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
ImGui.SetTooltip(T("When enabled, the pathfinder will not use Stellar Return to travel to gathering nodes.\nThis applies to both Hub Return and Hub + Aethernet travel methods."));
            }
            if (C.AvoidStellarReturn)
            {
                ImGui.SameLine();
                bool exceptHub = C.AvoidStellarReturnExceptHub;
if (ImGui.Checkbox(T("Except for hub activities"), ref exceptHub))
                {
                    C.AvoidStellarReturnExceptHub = exceptHub;
                    C.Save();
                }
                if (ImGui.IsItemHovered())
                {
ImGui.SetTooltip(T("When enabled, Stellar Return will still be used to return to the hub\nfor activities like credit purchases, gambling, drone bits, and repairs."));
                }
            }

            var minHubReturnDistance = C.HubReturn_Distance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.DragFloat(T("Distance before hub return is used (yalms)"), ref minHubReturnDistance))
            {
                C.HubReturn_Distance = minHubReturnDistance;
                C.SaveDebounced();
            }

            bool DisableRedAlertPathing = C.DisablePathfindingToRedAlert;
if (ImGui.Checkbox(T("Disable Pathfinding to Red Alerts"), ref DisableRedAlertPathing))
            {
                C.DisablePathfindingToRedAlert = DisableRedAlertPathing;
                C.Save();
            }
        }

        private static void StuckSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.ExclamationTriangle, T("Stuck Detection"));
            ImGui.Dummy(new Vector2(0, 5));

            bool unstuckEnabled = C.JumpIfStuck_V2 || C.RetargetIfStuck;
if (ImGui.Checkbox(T("If stuck during nav movement:"), ref unstuckEnabled))
            {
                if (unstuckEnabled)
                    C.JumpIfStuck_V2 = true;
                else
                {
                    C.JumpIfStuck_V2 = false;
                    C.RetargetIfStuck = false;
                }
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.HelpMarker(T(
                "When stuck during navmesh movement for the configured delay:\n" +
                "- Jump: attempts to jump over the obstacle\n" +
                "- Retarget: stops and re-pathfinds to the destination (re-randomizes if enabled)"));
            if (!unstuckEnabled) ImGui.BeginDisabled();
if (ImGui.RadioButton(T("Jump"), C.JumpIfStuck_V2 && !C.RetargetIfStuck))
            {
                C.JumpIfStuck_V2 = true;
                C.RetargetIfStuck = false;
                C.Save();
            }
            ImGui.SameLine();
if (ImGui.RadioButton(T("Retarget"), C.RetargetIfStuck))
            {
                C.RetargetIfStuck = true;
                C.JumpIfStuck_V2 = false;
                C.Save();
            }

            ImGui.Dummy(new Vector2(0, 4));
            ImGui.Text(T("after"));
            ImGui.SameLine();
            int stuckDelay = C.StuckDelayMs;
            ImGui.SetNextItemWidth(120);
            if (ImGui.SliderInt("##StuckDelay", ref stuckDelay, 500, 3000))
            {
                if (C.StuckDelayMs != stuckDelay)
                {
                    C.StuckDelayMs = stuckDelay;
                    C.SaveDebounced();
                }
            }
            ImGui.SameLine();
            ImGui.Text(T("ms stuck"));
            if (!unstuckEnabled) ImGui.EndDisabled();
        }

        private static void CraftingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.MapPin, T("Crafting Return Spot"));
            ImGui.Dummy(new Vector2(0, 5));

            bool usePersonalLocations = C.PersonalReturnSpot;
if (ImGui.Checkbox(T("Use personal return spots"), ref usePersonalLocations))
            {
                C.PersonalReturnSpot = usePersonalLocations;
                C.Save();
            }
            if (usePersonalLocations)
            {
                var territory = Player.Territory.RowId;
                var location = Player.Position;
                ImGui.SameLine();
                if (C.CrafterLocations.TryGetValue(territory, out var moonLoc))
                {
if (ImGui.Button(T("Set to current location")))
                    {
                        C.CrafterLocations[territory] = location;
                        C.Save();
                    }
                    ImGui.SameLine();
                    ImGui.Text($"({moonLoc.X:N1}, {moonLoc.Y:N1}, {moonLoc.Z:N1})");
                }
                else
                {
if (ImGui.Button(T("Add Location")))
                    {
                        C.CrafterLocations[territory] = Player.Position;
                        C.Save();
                    }
                    ImGui.SameLine();
ImGui.Text(T("No location set"));
                }
            }
        }

        private static void DailyRoutinesExtensions()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Plug, T("Daily Routines扩展"));
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextDisabled(T("使用Daily Routines传送"));
            ImGui.Dummy(new Vector2(0, 2));

            // CN-MAINT: Daily Routines TP toggles rendered in two compact rows.
            bool useFishingTp = C.FishingUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("钓鱼任务"), ref useFishingTp))
            {
                C.FishingUseDailyRoutinesTP = useFishingTp;
                C.Save();
            }

            ImGui.SameLine();
            bool useGatherTp = C.GatherUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("采集任务"), ref useGatherTp))
            {
                C.GatherUseDailyRoutinesTP = useGatherTp;
                C.Save();
            }

            ImGui.SameLine();
            bool usePersonalReturnTp = C.PersonalReturnUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("个人返回点"), ref usePersonalReturnTp))
            {
                C.PersonalReturnUseDailyRoutinesTP = usePersonalReturnTp;
                C.Save();
            }

            bool useHubReturnTp = C.HubReturnUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("购买物品后返回"), ref useHubReturnTp))
            {
                C.HubReturnUseDailyRoutinesTP = useHubReturnTp;
                C.Save();
            }

            ImGui.SameLine();
            bool useDroneTp = C.Cosmodrone_UseDailyRoutinesTP;
            if (ImGui.Checkbox(T("无人机任务"), ref useDroneTp))
            {
                C.Cosmodrone_UseDailyRoutinesTP = useDroneTp;
                C.Save();
            }

            if (C.FishingUseDailyRoutinesTP || C.GatherUseDailyRoutinesTP || C.PersonalReturnUseDailyRoutinesTP || C.HubReturnUseDailyRoutinesTP || C.Cosmodrone_UseDailyRoutinesTP)
            {
                ImGui.TextWrapped(T("提示：请确认 Daily Routines 的“快捷传送面板”模块已开启。传送失败会自动回退原有寻路。"));
            }
        }
    }
}
