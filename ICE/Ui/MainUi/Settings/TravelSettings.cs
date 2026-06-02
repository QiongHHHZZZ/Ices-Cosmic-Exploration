using Dalamud.Interface;
using ECommons.GameHelpers;
using ICE.Ui.DebugWindowTabs;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class TravelSettings
    {
        private static FishingDebug _fishingDebug = null;

        public static unsafe void Draw()
        {
            if (_fishingDebug == null)
            {
                _fishingDebug = new FishingDebug();
            }

            PathfindingSettings();

            Separator();
            StuckSettings();

            Separator();
            CraftingLocations();

            Separator();
            FishingLocations();

            Separator();
            DailyRoutinesExtensions();
        }

        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
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

            bool useRedAlertNpc = C.UseRedAlertNpc;
            if (ImGui.Checkbox(T("Use Red Alert NPC for travel"), ref useRedAlertNpc))
            {
                C.UseRedAlertNpc = useRedAlertNpc;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled(T("Beta, might not work"));

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

            bool DisableHubActivies_RE = C.DisableHub_Critical;
            if (ImGui.Checkbox(T("Don't do hub activities when a red alert is active"), ref DisableHubActivies_RE))
            {
                C.DisableHub_Critical = DisableHubActivies_RE;
                C.Save();
            }

            bool delayAether = C.Delay_Aethernet;
            if (ImGui.Checkbox("Add delay to athernet / npc travel", ref delayAether))
            {
                C.Delay_Aethernet = delayAether;
                C.Save();
            }
            ImGuiEx.HelpMarker("Adds a random delay before interacting with the aethershard / red alert npc travel.\n" +
                "The delays will be before, and a little bit inbetween interacting with menus");
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
            ImGuiEx.IconWithText(FontAwesomeIcon.Plug, T("Daily Routines Extensions"));
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextDisabled(T("Use Daily Routines teleport"));
            ImGui.Dummy(new Vector2(0, 2));

            // CN-MAINT: Daily Routines TP toggles rendered in two compact rows.
            bool useFishingTp = C.FishingUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("Fishing Missions"), ref useFishingTp))
            {
                C.FishingUseDailyRoutinesTP = useFishingTp;
                C.Save();
            }

            ImGui.SameLine();
            bool useGatherTp = C.GatherUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("Gathering Missions"), ref useGatherTp))
            {
                C.GatherUseDailyRoutinesTP = useGatherTp;
                C.Save();
            }

            ImGui.SameLine();
            bool usePersonalReturnTp = C.PersonalReturnUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("Personal Return Spots"), ref usePersonalReturnTp))
            {
                C.PersonalReturnUseDailyRoutinesTP = usePersonalReturnTp;
                C.Save();
            }

            bool useHubReturnTp = C.HubReturnUseDailyRoutinesTP;
            if (ImGui.Checkbox(T("Return after shopping"), ref useHubReturnTp))
            {
                C.HubReturnUseDailyRoutinesTP = useHubReturnTp;
                C.Save();
            }

            ImGui.SameLine();
            bool useDroneTp = C.Cosmodrone_UseDailyRoutinesTP;
            if (ImGui.Checkbox(T("Drone Missions"), ref useDroneTp))
            {
                C.Cosmodrone_UseDailyRoutinesTP = useDroneTp;
                C.Save();
            }

            if (C.FishingUseDailyRoutinesTP || C.GatherUseDailyRoutinesTP || C.PersonalReturnUseDailyRoutinesTP || C.HubReturnUseDailyRoutinesTP || C.Cosmodrone_UseDailyRoutinesTP)
            {
                ImGui.TextWrapped(T("Tip: Ensure Daily Routines 'Quick Teleport Panel' is enabled. If teleport fails, it automatically falls back to normal navigation."));
            }
        }
        private static void FishingLocations()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Fish, T("Personalized Fishing Spots"));
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, T("A way for you to save your own positions if you choose to not use a randomized spot that's included in the plugin\nYou don't have to use this, it will just use a random spot if:\n1: A position is saved:\n2: A random spot even is saved"), false);
            ImGui.Dummy(new Vector2(0, 5));

            var currentTerritory = Player.Territory.RowId;

            if (GatheringUtil.MoonFishingLocations.TryGetValue(currentTerritory, out var fishingHoles))
            {
                ImGui.Text(T("Planet: {0}", Player.Territory.Value.PlaceName.Value.Name.ToString()));
                ImGui.Checkbox(T("Show fishing spot raycast"), ref _fishingDebug.ShowFishRay);
                if (PlayerHelper.LocalPlayer is { } player && _fishingDebug.ShowFishRay)
                {
                    _fishingDebug.Draw();
                }

                ImGui.Separator();

                foreach (var hole in fishingHoles.Keys)
                {
                    // Find existing entry for this zone + map coord, or creating a new one if one doesn't exist
                    var entry = C.Personal_FishLocation.FirstOrDefault(f => f.ZoneId == currentTerritory && f.MapCoords == hole);

                    if (entry == null)
                    {
                        entry = new FishingLocations
                        {
                            ZoneId = currentTerritory,
                            X = hole.X,
                            Y = hole.Y,
                            WorldPosition = null
                        };
                        C.Personal_FishLocation.Add(entry);
                        C.SaveDebounced();
                    }

                    ImGui.PushID($"{hole}_Flag");

                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Flag, T("  X: {0:N2} Y: {1:N2}", hole.X, hole.Y)))
                    {
                        var mission = CosmicHelper.SheetMissionDict.Where(x => x.Value.MapPosition == hole).FirstOrDefault();
                        Utils.SetGatheringRing(mission.Value.TerritoryId, (int)hole.X, (int)hole.Y, mission.Value.Radius, $"{hole.X:N2} {hole.Y:N2}");
                    }
                    ImGui.SameLine();

                    string currentPos = entry.WorldPosition == null ? T("Add New") : T("Remove");

                    if (ImGui.Button(currentPos))
                    {
                        entry.WorldPosition = entry.WorldPosition == null ? Player.Position : null;
                        C.Save();
                    }

                    if (entry.WorldPosition != null)
                    {
                        ImGui.SameLine();
                        ImGui.Text($"{entry.WorldPosition.Value:N2}");
                    }

                    ImGui.PopID();
                }
            }
            else
            {
                ImGui.Text(T("Current planet has no stored fishing holes in the sheets. (Might need to be added?)"));
            }
        }
    }
}
