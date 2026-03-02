using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Resources.GatheringRoutes;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_GatherRoute_Editor
    {
        private static Vector2 selectedRoute = Vector2.Zero;
        private static uint selectedZone = 0;
        private static uint selectedNode = 0;

        private static bool showOnlyVisibleNodes = false;
        private static float maxDistance = 75.0f;

        private static bool showSelectedNode = false;
        private static bool showRouteBetween = true;

        private static bool _isGeneratingFan = false;
        private static string _fanGenStatus = string.Empty;

        private static FileDialogManager fileDialogManager = new FileDialogManager();

        public static unsafe async Task Draw()
        {
            var routes = GatheringRouteLoader.LoadAllRoutes();

            // used for picto drawing here
            List<(uint nodeId, Vector3 position)> AllNodes = new();

            // end picto stuff

            using (var quickAccess = ImRaii.Child("Quick Access Routes", new Vector2(200, 120), true))
            {
                if (!quickAccess.Success)
                    return;

                ImGui.Text("Export Settings");
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, 5));

                // Author Name Input
                ImGui.Text("Author Name:");
                ImGui.SetNextItemWidth(200);
                string authorName = C.AuthorName;
                if (ImGui.InputText("##AuthorName", ref authorName, 100))
                {
                    C.AuthorName = authorName;
                    C.SaveDebounced();
                }
            }

            ImGui.SameLine(0, 10);

            using (var quickAccess2 = ImRaii.Child("Quick Access Routes 2", new Vector2(300, 120), true))
            {
                if (!quickAccess2.Success)
                    return;

                // Custom Path Display
                ImGui.Text("Export Location:");
                string displayPath = string.IsNullOrEmpty(C.CustomRoutePath)
                    ? "Using default plugin config folder"
                    : C.CustomRoutePath;

                ImGui.TextWrapped(displayPath);

                ImGui.Dummy(new Vector2(0, 5));

                // Browse button to set custom path
                if (ImGui.Button("Browse for Export Folder"))
                {
                    fileDialogManager.OpenFolderDialog("Select Export Folder", (success, path) =>
                    {
                        if (success && !string.IsNullOrEmpty(path))
                        {
                            C.CustomRoutePath = path;
                            C.Save();
                            PluginLog.Information($"Export path set to: {path}");
                        }
                    });
                }

                ImGui.SameLine();

                // Clear custom path button
                if (!string.IsNullOrEmpty(C.CustomRoutePath))
                {
                    if (ImGui.Button("Use Default"))
                    {
                        C.CustomRoutePath = string.Empty;
                        C.Save();
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip("Clear custom path and use default plugin config folder");
                    }
                }
            }

            ImGui.SameLine(0, 10);

            var remainingQuickAccess = ImGui.GetContentRegionAvail();
            using (var quickAccess3 = ImRaii.Child("Quick Access 3", new Vector2(remainingQuickAccess.X, 120), true))
            {
                if (!quickAccess3.Success)
                    return;

                GatheringRouteExportUI.DrawExportAllButton();

                ImGui.Dummy(new Vector2(0, 5));

                if (selectedRoute != Vector2.Zero)
                {
                    GatheringRouteExportUI.DrawExportSelectedButton(selectedZone, selectedRoute);
                }

                if (ImGui.Button("Add missing routes"))
                {
                    try
                    {
                        var createdRoutes = GatheringRouteLoader.CreateMissingRoutes();

                        if (createdRoutes.Count > 0)
                        {
                            PluginLog.Information($"Successfully created {createdRoutes.Count} new route files:");
                            foreach (var route in createdRoutes)
                            {
                                PluginLog.Information($"  - {route}");
                            }
                        }
                        else
                        {
                            PluginLog.Information("No new routes needed - all routes already exist");
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Failed to create missing routes: {ex.Message}");
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Scan CosmicHelper missions and create YAML files for any missing gathering routes (MIN/BTN only)");
                }
            }

            using (var routeSelectorUi = ImRaii.Child("Route Selector Window", new Vector2(200, 0), true))
            {
                if (!routeSelectorUi.Success)
                    return;

                foreach (var planet in routes)
                {
                    var planetTerritory = planet.Key;
                    ImGui.Text($"Zone: {planetTerritory}");
                    foreach (var mapLocation in planet.Value)
                    {
                        var location = mapLocation.Key;

                        ImGui.PushID($"{location}");

                        bool isSelected = selectedRoute == location;
                        string selectable = isSelected ? $"-> X: {location.X}, Y: {location.Y}" : $"X: {location.X}, Y: {location.Y}";

                        var jobId = GetJobIdForFlag(planetTerritory, location);

                        ImGui.Image(CosmicHelper.JobIconDict[jobId].GetWrapOrEmpty().Handle, new Vector2(20, 20));
                        ImGui.SameLine(0, 4);

                        float availableWidth = ImGui.GetContentRegionAvail().X;

                        if (ImGui.Selectable(selectable, isSelected, ImGuiSelectableFlags.None, new Vector2(availableWidth, 20)))
                        {
                            selectedRoute = location;
                            selectedZone = planetTerritory;
                        }

                        ImGui.PopID();
                    }
                }
            }

            ImGui.SameLine(0, 5);

            var windowSizeRemaining = ImGui.GetContentRegionAvail();
            using (var routeEditorUi = ImRaii.Child("Route Editor Window", windowSizeRemaining, true))
            {
                if (!routeEditorUi.Success)
                    return;

                if (selectedRoute == Vector2.Zero)
                {
                    ImGui.Text("No route is selected");
                }
                else
                {
                    string missions = string.Join(", ", GetMissionsForFlag(selectedZone, selectedRoute));
                    ImGui.Text($"Location: {MoonName(selectedZone)}");
                    ImGui.Text($"Missions: {missions}");
                    ImGui.Text($"Map Zone: X:{selectedRoute.X}, Z: {selectedRoute.Y}");

                    ImGui.Dummy(new Vector2(0, 5));

                    ImGui.Separator();

                    ImGui.Dummy(new Vector2(0, 5));

                    var routeList = GatheringRouteLoader.GetRoute(selectedZone, selectedRoute);

                    using (var routeListUi = ImRaii.Child("Route List Ui Selector", new Vector2(150, 200), true))
                    {
                        if (!routeListUi.Success)
                            return;

                        for (int i = 0; i < routeList.Count; i++)
                        {
                            var routeItem = routeList[i];
                            var nodeId = routeItem.NodeId;

                            if (!AllNodes.Contains((nodeId, routeItem.Position)))
                            {
                                AllNodes.Add((nodeId, routeItem.Position));
                            }

                            ImGui.PushID($"{nodeId}_routeViewer");

                            var isSelected = nodeId == selectedNode;
                            if (ImGui.Selectable($"{nodeId}", isSelected))
                            {
                                selectedNode = nodeId;
                            }
                            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsItemHovered())
                            {
                                ImGui.OpenPopup("Options for node");
                            }

                            if (ImGui.BeginPopup("Options for node"))
                            {
                                if (ImGui.Selectable("Remove Node"))
                                {
                                    routeList.Remove(routeItem);
                                }
                                if (ImGui.Selectable("Path to node"))
                                {
                                    P.TaskManager.Enqueue(() => Task_NavmeshMove.Task_NavTo(routeItem.LandZone, stayMounted: true), Utils.TaskConfig);
                                }

                                ImGui.EndPopup();
                            }

                            // Drag source
                            if (ImGui.BeginDragDropSource())
                            {
                                unsafe
                                {
                                    int dragIndex = i;
                                    byte* dataPtr = (byte*)&dragIndex;
                                    ReadOnlySpan<byte> data = new ReadOnlySpan<byte>(dataPtr, sizeof(int));
                                    ImGui.SetDragDropPayload("ROUTE_REORDER", data);
                                }
                                ImGui.Text($"Moving: {nodeId}");
                                ImGui.EndDragDropSource();
                            }

                            // Drop target
                            if (ImGui.BeginDragDropTarget())
                            {
                                unsafe
                                {
                                    var payload = ImGui.AcceptDragDropPayload("ROUTE_REORDER");
                                    if (!payload.IsNull)
                                    {
                                        int sourceIndex = *(int*)payload.Data;

                                        // Reorder the list
                                        if (sourceIndex != i && sourceIndex >= 0 && sourceIndex < routeList.Count)
                                        {
                                            var item = routeList[sourceIndex];
                                            routeList.RemoveAt(sourceIndex);
                                            routeList.Insert(i, item);
                                        }
                                    }
                                }
                                ImGui.EndDragDropTarget();
                            }

                            ImGui.PopID();
                        }
                    }

                    ImGui.SameLine(0, 5);

                    using (var allNodeViewer = ImRaii.Child("All Node Ui Viewer", new Vector2(200, 200), true))
                    {
                        if (!allNodeViewer.Success)
                            return;

                        ImGui.Text("All Node Viewer");

                        foreach (var x in Svc.Objects.Where(x => x.ObjectKind == ObjectKind.GatheringPoint && Player.DistanceTo(x.Position) <= maxDistance)
                                                     .OrderBy(x => Player.DistanceTo(x.Position)))
                        {
                            ImGui.PushID($"{x.BaseId}_{x.Position}_NodeViewer");

                            ImGui.Text($"Id: {x.BaseId} | Distance: {Player.DistanceTo(x.Position):N2}");
                            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && ImGui.IsItemHovered())
                            {
                                ImGui.OpenPopup("Node Viewer Popup");
                            }

                            if (ImGui.BeginPopup("Node Viewer Popup"))
                            {
                                if (ImGui.Selectable("Add node to list"))
                                {
                                    routeList.Add(new Resources.GatheringRoutes.GathNodeInfo()
                                    {
                                        NodeId = x.BaseId,
                                        Position = x.Position,
                                        LandZone = Player.Position,
                                    });

                                    ImGui.CloseCurrentPopup();
                                }

                                ImGui.EndPopup();
                            }

                            ImGui.PopID();
                        }
                    }

                    ImGui.SameLine(0, 5);
                    using (var nodeSelector = ImRaii.Child("Specific Node Viewer", new Vector2(200, 200), true))
                    {
                        if (nodeSelector.Success)
                        {
                            foreach (var x in Svc.Objects.Where(x => x.ObjectKind == ObjectKind.GatheringPoint && Player.DistanceTo(x.Position) <= maxDistance)
                             .OrderBy(x => Player.DistanceTo(x.Position)))
                            {
                                var localPlayer = Svc.Objects.LocalPlayer;
                                if (localPlayer != null && localPlayer.TargetObject != null)
                                {
                                    if (localPlayer.TargetObject.BaseId == x.BaseId && localPlayer.TargetObject.ObjectKind == ObjectKind.GatheringPoint)
                                    {
                                        ImGui.Text($"ID: {x.BaseId}");
                                        ImGui.Text($"Position: {x.Position}");
                                        if (ImGui.Button($"Add"))
                                        {
                                            routeList.Add(new Resources.GatheringRoutes.GathNodeInfo()
                                            {
                                                NodeId = x.BaseId,
                                                Position = x.Position,
                                                LandZone = Player.Position,
                                            });

                                            selectedNode = x.BaseId;
                                            IceLogging.Info($"Added node {x.BaseId} to route");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var nodeEditorSpace = ImGui.GetContentRegionAvail();
                    using (var nodeEditorUi = ImRaii.Child("Node Editor Ui", nodeEditorSpace))
                    {
                        if (!nodeEditorUi.Success)
                            return;

                        if (ImGui.Button("Generate Path Nodes"))
                        {
                            UpdateCache(routeList);
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Clear Path"))
                        {
                            cachedWaypointPath = null;
                        }

                        var route = routeList.Where(x => x.NodeId == selectedNode).FirstOrDefault();
                        if (route != null)
                        {
                            ImGui.Text($"Node Id: {route.NodeId}");
                            ImGui.Text($"Node Position: {route.Position}");

                            // Player Land Zone (currently static, might change this later)
                            Vector3 playerLandZone = route.LandZone;
                            ImGui.Text("Player Land Zone");
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.InputFloat3("##Player Land Zone", ref playerLandZone))
                            {
                                route.LandZone = playerLandZone;
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Set to current position"))
                            {
                                route.LandZone = Player.Position;
                            }

                            // Radius Start/End
                            ImGui.Text("Radius Info");
                            float radiusStart = route.Radius_Start;
                            float radiusEnd = route.Radius_End;
                            float height = route.FanHeight;

                            ImGui.SetNextItemWidth(100);
                            if (ImGui.DragFloat("Start##radiusStart", ref radiusStart, 1, 0, 360))
                            {
                                route.Radius_Start = radiusStart;
                            }

                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.DragFloat("End##radiusEnd", ref radiusEnd, 1, 0, 360))
                            {
                                route.Radius_End = radiusEnd;
                            }

                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.DragFloat("Height", ref height, 0.1f, 0, 3))
                            {
                                route.FanHeight = height;
                            }

                            // Min/Max Distance
                            ImGui.Text("Distance to Node");
                            float minDistance = route.Distance_Min;
                            float maxDistance = route.Distance_Max;

                            ImGui.SetNextItemWidth(100);
                            if (ImGui.DragFloat("Start##minDistance", ref minDistance, 0.1f, 0, 5))
                            {
                                route.Distance_Min = minDistance;
                            }

                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.DragFloat("End##maxDistance", ref maxDistance, 0.1f, 0, 5))
                            {
                                route.Distance_Max = maxDistance;
                            }

                            if (ImGui.Button("Path to node"))
                            {
                                P.TaskManager.Enqueue(() => Task_NavmeshMove.Task_GatherMove(route, stayMounted: true), Utils.TaskConfig);
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Test Massive Pathfinding"))
                            {
                                Task_NavmeshMove.Enqueue_NavmeshTask(route.LandZone);
                            }

                            if (ImGui.Button("Test Path to all Nodes"))
                            {
                                var firstPosition = Vector3.Zero;
                                foreach (var routeItem in routeList)
                                {
                                    if (firstPosition == Vector3.Zero)
                                        firstPosition = routeItem.LandZone;

                                    P.TaskManager.Enqueue(() => Task_NavmeshMove.Task_GatherMove(routeItem, stayMounted: true), Utils.TaskConfig);
                                }
                                P.TaskManager.Enqueue(() => Task_NavmeshMove.Task_NavTo(firstPosition, stayMounted: true), Utils.TaskConfig);
                            }

                            if (ImGui.Button("Stop Task"))
                            {
                                P.TaskManager.Tasks.Clear();
                                P.TaskManager.Abort();
                                P.Navmesh.Stop();
                            }

                            ImGui.Dummy(new Vector2(0, 5));
                            ImGui.Separator();
                            ImGui.Text("Fan Auto-Generation");

                            using (var disabled = ImRaii.Disabled(_isGeneratingFan))
                            {
                                if (ImGui.Button("Generate Fan from Navmesh"))
                                {
                                    _ = GenerateFanForNode(route);
                                }
                            }

                            if (_isGeneratingFan)
                            {
                                ImGui.SameLine();
                                ImGui.TextColored(new Vector4(1f, 1f, 0f, 1f), "Sampling...");
                            }

                            if (!string.IsNullOrEmpty(_fanGenStatus))
                            {
                                ImGui.TextWrapped(_fanGenStatus);
                            }
                        }
                    }

                    if (showRouteBetween)
                    {
                        PictoManager.DrawGatherNodes(routeList, selectedNode, cachedWaypointPath);
                    }
                }
            }

            fileDialogManager.Draw();
        }

        private static uint GetJobIdForFlag(uint territoryId, Vector2 map)
        {
            var missionEntry = CosmicHelper.SheetMissionDict.Where(x => x.Value.MapPosition == map
                                                                && x.Value.TerritoryId == territoryId).FirstOrDefault();

            if (missionEntry.Key != 0)
            {
                if (missionEntry.Value.Jobs.Contains(17))
                    return 17;
                else if (missionEntry.Value.Jobs.Contains(16))
                    return 16;
                else
                    return 8;
            }
            else
                return 8;
        }

        private static List<uint> GetMissionsForFlag(uint territoryId, Vector2 map)
        {
            List<uint> missions = new();
            foreach (var mission in CosmicHelper.SheetMissionDict.Where(x => x.Value.MapPosition == map && x.Value.TerritoryId == territoryId))
            {
                missions.Add(mission.Key);
            }

            return missions;
        }

        private static string MoonName(uint territoryId)
        {
            if (territoryId == 1237)
                return "Sinus Ardorum";
            else if (territoryId == 1291)
                return "Phaenna";
            else
            {
                return "???";
            }
        }

        public static class GatheringRouteExportUI
        {
            private static string? _lastExportMessage = null;
            private static DateTime _lastExportMessageTime = DateTime.MinValue;
            private static bool _lastExportSuccess = false;

            public static void DrawExportAllButton()
            {
                if (ImGui.Button("Export All Routes"))
                {
                    try
                    {
                        GatheringRouteLoader.ExportAllRoutes();
                        var exportPath = GetDefaultExportPath();
                        _lastExportMessage = $"Successfully exported all routes to:\n{exportPath}";
                        _lastExportSuccess = true;
                        _lastExportMessageTime = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Export failed: {ex.Message}");
                        _lastExportMessage = $"Export failed: {ex.Message}";
                        _lastExportSuccess = false;
                        _lastExportMessageTime = DateTime.Now;
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Export all routes to plugin config folder");
                }

                DrawExportMessage();
            }

            public static void DrawExportSelectedButton(uint zoneId, Vector2 flag)
            {
                if (ImGui.Button("Export Selected Route"))
                {
                    try
                    {
                        GatheringRouteLoader.ExportRoute(zoneId, flag);
                        var exportPath = GetDefaultExportPath();
                        _lastExportMessage = $"Successfully exported route to:\n{exportPath}";
                        _lastExportSuccess = true;
                        _lastExportMessageTime = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Export failed: {ex.Message}");
                        _lastExportMessage = $"Export failed: {ex.Message}";
                        _lastExportSuccess = false;
                        _lastExportMessageTime = DateTime.Now;
                    }
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Export this route to plugin config folder");
                }

                DrawExportMessage();
            }

            public static void DrawExportAllButtonWithCustomPath()
            {
                if (ImGui.Button("Export All Routes (Choose Location)"))
                {
                    // TODO: Add file picker dialog integration
                    ImGui.OpenPopup("export_path_picker");
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Export all routes to a custom location");
                }

                // Placeholder for file picker popup
                if (ImGui.BeginPopup("export_path_picker"))
                {
                    ImGui.Text("File picker not yet implemented");
                    ImGui.Text("Use default location button for now");
                    ImGui.EndPopup();
                }
            }

            public static void DrawExportSelectedButtonWithCustomPath(uint zoneId, Vector2 flag)
            {
                if (ImGui.Button("Export Selected Route (Choose Location)"))
                {
                    // TODO: Add file picker dialog integration
                    ImGui.OpenPopup("export_path_picker_selected");
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Export this route to a custom location");
                }

                // Placeholder for file picker popup
                if (ImGui.BeginPopup("export_path_picker_selected"))
                {
                    ImGui.Text("File picker not yet implemented");
                    ImGui.Text("Use default location button for now");
                    ImGui.EndPopup();
                }
            }

            private static void DrawExportMessage()
            {
                if (_lastExportMessage != null && (DateTime.Now - _lastExportMessageTime).TotalSeconds < 5)
                {
                    var color = _lastExportSuccess
                        ? new Vector4(0.0f, 1.0f, 0.0f, 1.0f)  // Green
                        : new Vector4(1.0f, 0.0f, 0.0f, 1.0f); // Red

                    ImGui.TextColored(color, _lastExportMessage);
                }
            }

            private static string GetDefaultExportPath()
            {
                var configDir = Svc.PluginInterface.GetPluginConfigDirectory();
                return Path.Combine(configDir, "ExportedRoutes");
            }
        }

        private static List<Vector3>? cachedWaypointPath = null;
        private static async void UpdateCache(List<GathNodeInfo> routeList)
        {
            cachedWaypointPath = await GenerateWaypointPathAsync(routeList);
        }

        public static async Task<List<Vector3>?> GenerateWaypointPathAsync(List<GathNodeInfo> routeItem)
        {
            try
            {
                var waypoints = new List<Vector3>();

                // Add player position as starting point
                if (Player.Available)
                {
                    waypoints.Add(Player.Position);
                }
                else
                {
                    IceLogging.Error("Player not available for path generation");
                    return null;
                }

                // Generate path through all land zones
                for (int i = 0; i < routeItem.Count; i++)
                {
                    var currentLandZone = routeItem[i].LandZone;

                    IceLogging.Info($"Generating path to node {i + 1}/{routeItem.Count}");

                    // Get path from previous position to current land zone
                    Vector3 startPos = waypoints[^1];

                    var pathToNode = await Task.Run(() => P.Navmesh.Pathfind(startPos, currentLandZone, false));

                    if (pathToNode != null && pathToNode.Count > 0)
                    {
                        // Skip first point if it's too close to the last waypoint
                        int startIndex = Vector3.Distance(waypoints[^1], pathToNode[0]) < 0.5f ? 1 : 0;

                        for (int j = startIndex; j < pathToNode.Count; j++)
                        {
                            waypoints.Add(pathToNode[j]);
                        }
                    }
                    else
                    {
                        IceLogging.Warning($"Pathfinding failed for node {i}, using direct line");
                        waypoints.Add(currentLandZone);
                    }
                }

                // Path back to the first node to complete the loop
                if (routeItem.Count > 0)
                {
                    IceLogging.Info("Generating path back to first node to complete loop");

                    Vector3 lastPos = waypoints[^1];
                    Vector3 firstLandZone = routeItem[0].LandZone;

                    var pathBackToStart = await Task.Run(() => P.Navmesh.Pathfind(lastPos, firstLandZone, false));

                    if (pathBackToStart != null && pathBackToStart.Count > 0)
                    {
                        // Skip first point if it's too close to the last waypoint
                        int startIndex = Vector3.Distance(waypoints[^1], pathBackToStart[0]) < 0.5f ? 1 : 0;

                        for (int j = startIndex; j < pathBackToStart.Count; j++)
                        {
                            waypoints.Add(pathBackToStart[j]);
                        }
                    }
                    else
                    {
                        IceLogging.Warning("Pathfinding failed for loop closure, using direct line");
                        waypoints.Add(firstLandZone);
                    }
                }

                IceLogging.Info($"Path generation complete! Generated {waypoints.Count} waypoints");
                return waypoints;
            }
            catch (Exception ex)
            {
                IceLogging.Error($"Error generating waypoint path: {ex.Message}");
                return null;
            }
        }

        private static async Task GenerateFanForNode(GathNodeInfo route)
        {
            _isGeneratingFan = true;
            _fanGenStatus = string.Empty;

            try
            {
                Vector3 nodePos = route.Position;

                // Sampling config
                const float snapToleranceXZ = 0.5f;
                const float snapToleranceY = 5f;
                const float testDistanceMin = 1.0f;
                const float testDistanceMax = 2.4f;
                const float distanceStep = 0.5f;
                const int angleSamples = 360;

                var validDistances = new Dictionary<int, List<float>>();
                var validYHeights = new Dictionary<int, float>();

                await Task.Run(() =>
                {
                    for (int angleDeg = 0; angleDeg < angleSamples; angleDeg++)
                    {
                        float ffxivAngle = angleDeg;
                        bool allDistancesValid = true;
                        var distancesForAngle = new List<float>();
                        float highestY = float.MinValue;

                        for (float dist = testDistanceMin; dist <= testDistanceMax; dist += distanceStep)
                        {
                            float standardAngle = 180f - ffxivAngle;
                            float rad = standardAngle * (MathF.PI / 180f);
                            Vector3 candidate = new Vector3(
                                nodePos.X + dist * MathF.Sin(rad),
                                nodePos.Y,
                                nodePos.Z + dist * MathF.Cos(rad)
                            );

                            var nearest = P.Navmesh.NearestPointReachable(candidate, snapToleranceXZ, snapToleranceY);
                            if (nearest.HasValue)
                            {
                                float xzDist = MathF.Sqrt(
                                    MathF.Pow(nearest.Value.X - candidate.X, 2) +
                                    MathF.Pow(nearest.Value.Z - candidate.Z, 2)
                                );
                                float yDist = MathF.Abs(nearest.Value.Y - candidate.Y);

                                if (xzDist <= snapToleranceXZ && yDist <= snapToleranceY)
                                {
                                    distancesForAngle.Add(dist);
                                    if (nearest.Value.Y > highestY)
                                        highestY = nearest.Value.Y;
                                }
                                else
                                {
                                    allDistancesValid = false;
                                    break;
                                }
                            }
                            else
                            {
                                allDistancesValid = false;
                                break;
                            }
                        }

                        if (allDistancesValid && distancesForAngle.Count > 0)
                        {
                            validDistances[angleDeg] = distancesForAngle;
                            validYHeights[angleDeg] = highestY;
                        }
                    }
                });

                if (validDistances.Count == 0)
                {
                    _fanGenStatus = "No reachable points found around this node.";
                    return;
                }

                // Build bool array of valid angles
                bool[] valid = new bool[360];
                foreach (var kvp in validDistances)
                    valid[kvp.Key] = true;

                // Find largest contiguous arc (handles wraparound by doubling the array)
                int bestStart = 0, bestLen = 0;
                int currentStart = 0, currentLen = 0;

                for (int i = 0; i < 720; i++)
                {
                    if (valid[i % 360])
                    {
                        if (currentLen == 0)
                            currentStart = i;
                        currentLen++;

                        if (currentLen > bestLen)
                        {
                            bestLen = currentLen;
                            bestStart = currentStart;
                        }
                    }
                    else
                    {
                        currentLen = 0;
                    }

                    if (currentLen >= 360)
                        break;
                }

                if (bestLen == 0)
                {
                    _fanGenStatus = "Could not find a contiguous arc of reachable angles.";
                    return;
                }

                int ffxivStart = bestStart % 360;
                int ffxivEnd = (bestStart + bestLen - 1) % 360;

                // Convert back to Pictomancy space
                float pictoStart = (ffxivStart + 180f) % 360f;
                float pictoEnd = (ffxivEnd + 180f) % 360f;

                // Derive min/max distance and max Y within the arc
                float allMin = float.MaxValue, allMax = float.MinValue;
                float arcMaxY = float.MinValue;

                foreach (var kvp in validDistances)
                {
                    int normalizedAngle = ((kvp.Key - ffxivStart) % 360 + 360) % 360;
                    if (normalizedAngle < bestLen)
                    {
                        foreach (var d in kvp.Value)
                        {
                            if (d < allMin) allMin = d;
                            if (d > allMax) allMax = d;
                        }
                    }
                }

                foreach (var kvp in validYHeights)
                {
                    int normalizedAngle = ((kvp.Key - ffxivStart) % 360 + 360) % 360;
                    if (normalizedAngle < bestLen && kvp.Value > arcMaxY)
                        arcMaxY = kvp.Value;
                }

                // Compute fan height offset
                float fanHeight = 0f;
                if (arcMaxY != float.MinValue && arcMaxY > nodePos.Y)
                    fanHeight = MathF.Round((arcMaxY - nodePos.Y) + 0.2f, 2);

                route.Radius_Start = pictoStart;
                route.Radius_End = pictoEnd;
                route.Distance_Min = MathF.Round(allMin, 1);
                route.Distance_Max = MathF.Round(allMax, 1);
                route.FanHeight = fanHeight;

                _fanGenStatus = $"Generated! Angles: {pictoStart:F0}→{pictoEnd:F0} (arc {bestLen}°), Distance: {allMin:F1}→{allMax:F1}, Height: {fanHeight:F2}";
                IceLogging.Info($"[FanGen] Node {route.NodeId}: Picto {pictoStart:F0}→{pictoEnd:F0}, dist {allMin:F1}→{allMax:F1}, height {fanHeight:F2}");
            }
            catch (Exception ex)
            {
                _fanGenStatus = $"Error: {ex.Message}";
                IceLogging.Error($"[FanGen] Failed: {ex.Message}");
            }
            finally
            {
                _isGeneratingFan = false;
            }
        }
    }
}
