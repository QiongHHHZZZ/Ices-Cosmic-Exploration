using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Scheduler.Handlers.PictoStuff;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.GatheringHelper.RouteLoader;
using KamiToolKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_GatherEditor
    {
        private static int _selectedPlanetIndex = 0;
        private static uint _selectedRoute = 0;
        private static string _routeSearch = string.Empty;
        private static NodeInfo selectedNode = new();

        private static FileDialogManager fileDialogManager = new FileDialogManager();

        private static int count = 0;

        public static void Draw()
        {
            count = GatheringUtil.GatherSpots
                .Where(x => x.Value.JobId.Contains(16) || x.Value.JobId.Contains(17))
                .Count();

            ImGui.Text($"Total: {count}");
            ImGui.SameLine();
            if (ImGui.Button("Set Save Location"))
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
            ImGui.Text($"{C.CustomRoutePath}");

            for (int i = 0; i < CosmicMoonRegistry.All.Length; i++)
            {
                var moon = CosmicMoonRegistry.All[i];
                ImGui.RadioButton(moon.DisplayName, ref _selectedPlanetIndex, i);

                if (i < CosmicMoonRegistry.All.Length - 1)
                    ImGui.SameLine();
            }

            var selected = CosmicMoonRegistry.All[_selectedPlanetIndex];
            
            if (ImGui.BeginTable("Gather Route Editor Table", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit, ImGui.GetContentRegionAvail()))
            {
                ImGui.TableSetupColumn("Route Selector", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Route Editor", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                RouteSelector();

                ImGui.TableNextColumn();
                RouteInfo();

                ImGui.EndTable();
            }

            fileDialogManager.Draw();
        }

        private static void RouteSelector()
        {
            var planet = CosmicMoonRegistry.All[_selectedPlanetIndex];

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##routeSearch", "Search...", ref _routeSearch, 64);

            var routes = GatheringUtil.GatherSpots
                .Where(x => x.Value.TerritoryId == planet.TerritoryId)
                .Where(x => x.Value.JobId.Contains(16) || x.Value.JobId.Contains(17))
                .Where(x => string.IsNullOrEmpty(_routeSearch) ||
                            $"{x.Key} | X: {x.Value.X} Y:{x.Value.Y}"
                                .Contains(_routeSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();

            using (var child = ImRaii.Child("##routeList", new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y - 20), false))
            {
                if (!child) return;

                foreach (var route in routes)
                {
                    var label = $"{route.Key} | X: {route.Value.X:N0} Y:{route.Value.Y:N0}";
                    if (ImGui.Selectable(label, _selectedRoute == route.Key))
                        _selectedRoute = route.Key;
                }
            }
        }

        private static void RouteInfo()
        {
            void AddNode(GatheringRoute route, IGameObject target)
            {
                route.Nodes ??= [];
                var playerPos = Player.Position;
                if (route.Nodes.Any(x => x.NodeId == target.BaseId))
                    return;

                var newNode = new NodeInfo()
                {
                    NodeId = target.BaseId,
                    Position = target.Position,
                    LandZone = playerPos  // temporary fallback
                };
                route.Nodes.Add(newNode);
                selectedNode = newNode;

                // Fire-and-forget: generate fan then pick landing zone
                _ = GenerateFanThenPickLandZone(newNode);
            }

            if (!GatheringUtil.GatherSpots.TryGetValue(_selectedRoute, out var mapInfo))
                return;

            if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"{_selectedRoute}_{mapInfo.X}_{mapInfo.Y}"))
            {
                Utils.SetGatheringRing(mapInfo.TerritoryId, mapInfo.X, mapInfo.Y, mapInfo.Radius, $"Route {_selectedRoute}");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Territory: {mapInfo.TerritoryId}");
                ImGui.Text($"Location: {mapInfo.X}, {mapInfo.Y}");
                ImGui.Text($"Radius: {mapInfo.Radius}");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Running, "Move To Navmesh"))
            {
                Svc.Commands.ProcessCommand("/vnav moveflag");
            }


            using (var child = ImRaii.Child("##missionList", new Vector2(-1, 5 * ImGui.GetFrameHeightWithSpacing()), false))
            {
                if (!child) return;

                foreach (var mission in mapInfo.MissionIds)
                {
                    if (!CosmicHelper.SheetMissionDict.TryGetValue(mission, out var sheetInfo))
                        continue;

                    for (int i = 0; i < sheetInfo.Jobs.Count; i++)
                    {
                        ImGui.Image(CosmicHelper.ClassInfoDict[sheetInfo.Jobs[i]].JobIcon.GetWrapOrEmpty().Handle, new(20, 20));
                        ImGui.SameLine();
                    }
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"[{mission}] - {sheetInfo.Name}");
                }
            }

            if (GatheringRouteLoader.LoadedRoutes.TryGetValue(_selectedRoute, out var routeInfo))
            {
                if (ImGui.Button("Save Route"))
                {
                    GatheringRouteLoader.SaveRoute(routeInfo);
                }

                if (ImGui.BeginChild("Node Selection", new(200, 200), true))
                {
                    if (Player.Available)
                    {
                        if (Svc.Objects.LocalPlayer.TargetObject != null)
                        {
                            var lastTarget = Svc.Objects.LocalPlayer.TargetObject;
                            if (lastTarget.ObjectKind == ObjectKind.GatheringPoint)
                            {
                                if (ImGui.Button($"Add Node: {lastTarget.BaseId}"))
                                {
                                    AddNode(routeInfo, lastTarget);
                                }
                            }
                        }

                        var objectList = Svc.Objects.Where(x => x.ObjectKind == ObjectKind.GatheringPoint)
                            .OrderBy(x => Player.DistanceTo(x.Position)).ToList();
                        foreach (var node in objectList)
                        {
                            if (!GatheringRouteLoader.AddedNodes().Contains(node.BaseId))
                            {
                                ImGui.PushID($"{node.BaseId}##{node.BaseId}_{node.Position}");
                                if (ImGui.Button($"{node.BaseId} | {Player.DistanceTo(node.Position):N2}"))
                                {
                                    AddNode(routeInfo, node);
                                }
                                ImGui.PopID();
                            }
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.SameLine();
                if (ImGui.BeginChild("Node Editor", new Vector2(200, 200), true))
                {
                    if (routeInfo.Nodes != null)
                    {
                        for (int i = 0; i < routeInfo.Nodes.Count; i++)
                        {
                            var node = routeInfo.Nodes[i];

                            // Up button (disabled on first item)
                            ImGui.BeginDisabled(i == 0);
                            if (ImGui.ArrowButton($"##up_{i}", ImGuiDir.Up))
                            {
                                (routeInfo.Nodes[i - 1], routeInfo.Nodes[i]) = (routeInfo.Nodes[i], routeInfo.Nodes[i - 1]);
                            }
                            ImGui.EndDisabled();

                            ImGui.SameLine();

                            // Down button (disabled on last item)
                            ImGui.BeginDisabled(i == routeInfo.Nodes.Count - 1);
                            if (ImGui.ArrowButton($"##down_{i}", ImGuiDir.Down))
                            {
                                (routeInfo.Nodes[i + 1], routeInfo.Nodes[i]) = (routeInfo.Nodes[i], routeInfo.Nodes[i + 1]);
                            }
                            ImGui.EndDisabled();

                            ImGui.SameLine();
                            if (selectedNode == node)
                            {
                                if (ImGui.Button($"-> {node.NodeId}"))
                                {
                                    selectedNode = node;
                                }
                            }
                            else
                            {
                                if (ImGui.Button($"{node.NodeId}"))
                                {
                                    selectedNode = node;
                                }
                            }
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.Separator();

                var nodeInfo = routeInfo.Nodes?.FirstOrDefault(x => x == selectedNode);
                if (nodeInfo is not null)
                {
                    ImGui.Text($"Node: {nodeInfo.NodeId}");
                    ImGui.Text($"X: {nodeInfo.Position.X:N2} | Y: {nodeInfo.Position.Y:N2} | Z: {nodeInfo.Position.Z:N2}");

                    if (ImGui.Button("Nav Move To"))
                    {
                        P.Navmesh.PathfindAndMoveTo(nodeInfo.LandZone, false);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Move To [Fan]"))
                    {
                        Task_NavmeshMove.ResetGatherMove();
                        P.TaskManager.Enqueue(() => Task_NavmeshMove.Task_GatherMove(nodeInfo, stayMounted: true));
                    }

                    ImGui.Dummy(new(0, 5));
                    if (ImGui.Button($"Player Start: {nodeInfo.LandZone}"))
                    {
                        nodeInfo.LandZone = Player.Position;
                    }

                    var fanStart = nodeInfo.RadiusStart;
                    var fanEnd = nodeInfo.RadiusEnd;
                    var fanMin = nodeInfo.MinDistance;
                    var fanMax = nodeInfo.MaxDistance;
                    var height = nodeInfo.FanHeight;

                    using (var disabled = ImRaii.Disabled(_isGeneratingFan || !ImGui.IsKeyDown(ImGuiKey.LeftShift)))
                    {
                        if (ImGui.Button(T("Generate Fan from Navmesh")))
                        {
                            _ = GenerateFanForNode(nodeInfo);
                        }
                    }


                    ImGui.SetNextItemWidth(100);
                    if (ImGui.DragFloat("Fan Start", ref fanStart, 1, 0, 360))
                    {
                        nodeInfo.RadiusStart = fanStart;
                    }
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.DragFloat("Fan End", ref fanEnd, 1, 0, 360))
                    {
                        nodeInfo.RadiusEnd = fanEnd;
                    }

                    ImGui.SetNextItemWidth(100);
                    if (ImGui.DragFloat("Min Distance", ref fanMin, 1, 1, 4))
                    {
                        nodeInfo.MinDistance = fanMin;
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.DragFloat("Max Distance", ref fanMax, 1, 1, 4))
                    {
                        nodeInfo.MaxDistance = fanMax;
                    }

                    ImGui.SetNextItemWidth(100);
                    if (ImGui.DragFloat("Fan Height", ref height, 0.1f, 0, 3))
                    {
                        nodeInfo.FanHeight = height;
                    }
                }

                ImGui.Text($"{_fanGenStatus}");
                var gatherFan = C.Picto_GatherFan;
                if (ImGui.ColorEdit4("Gather Fan", ref gatherFan))
                {
                    C.Picto_GatherFan = gatherFan;
                    C.SaveDebounced();
                }
                var selectedFan = C.Picto_SelectedFan;
                if (ImGui.ColorEdit4("Selected Fan", ref selectedFan))
                {
                    C.Picto_SelectedFan = selectedFan;
                    C.SaveDebounced();
                }


                if (routeInfo.Nodes?.Count > 0)
                {
                    foreach (var node in routeInfo.Nodes)
                    {
                        PictoManager.DrawGatheringFan(node, selectedNode.Position);
                    }
                }
            }
            else
            {
                ImGui.Text("No route file exist. Do you want to create one?");
                if (ImGui.Button("Create files"))
                {
                    GatheringRouteLoader.CreateMissingStubs();
                }
            }
        }

        private static bool _isGeneratingFan = false;
        private static string _fanGenStatus = "";
        private static async Task GenerateFanThenPickLandZone(NodeInfo node)
        {
            await GenerateFanForNode(node);

            // If fan gen failed or produced no arc, leave LandZone as-is
            if (node.RadiusStart == 0 && node.RadiusEnd == 0)
                return;

            await PickLandZoneFromFan(node);
        }

        private static async Task PickLandZoneFromFan(NodeInfo node)
        {
            const float snapToleranceXZ = 0.5f;
            const float snapToleranceY = 5f;

            Vector3 nodePos = node.Position;

            // Mid-angle of the fan arc (in degrees, FFXIV space where 0=North)
            float arcLength = node.RadiusEnd >= node.RadiusStart
                ? node.RadiusEnd - node.RadiusStart
                : (360f - node.RadiusStart) + node.RadiusEnd;

            float midAngleDeg = (node.RadiusStart + arcLength / 2f) % 360f;
            float midDist = (node.MinDistance + node.MaxDistance) / 2f;

            // FFXIV 0=North → standard math angle: standardAngle = 180 - ffxivAngle
            float standardAngle = 180f - midAngleDeg;
            float rad = standardAngle * (MathF.PI / 180f);

            Vector3 candidate = new Vector3(
                nodePos.X + midDist * MathF.Sin(rad),
                nodePos.Y + node.FanHeight,
                nodePos.Z + midDist * MathF.Cos(rad)
            );

            var nearest = await Task.Run(() =>
                P.Navmesh.NearestPointReachable(candidate, snapToleranceXZ, snapToleranceY));

            if (nearest.HasValue)
            {
                node.LandZone = nearest.Value;
                _fanGenStatus += $" | LandZone: {nearest.Value.X:F1}, {nearest.Value.Y:F1}, {nearest.Value.Z:F1}";
            }
            else
            {
                _fanGenStatus += " | LandZone pick failed, kept player pos";
            }
        }
        private static async Task GenerateFanForNode(NodeInfo route)
        {
            _isGeneratingFan = true;
            _fanGenStatus = string.Empty;

            try
            {
                Vector3 nodePos = route.Position;

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
                        bool allDistancesValid = true;
                        var distancesForAngle = new List<float>();
                        float highestY = float.MinValue;

                        for (float dist = testDistanceMin; dist <= testDistanceMax; dist += distanceStep)
                        {
                            float standardAngle = 180f - angleDeg;
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

                bool[] valid = new bool[360];
                foreach (var kvp in validDistances)
                    valid[kvp.Key] = true;

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

                float fanHeight = 0f;
                if (arcMaxY != float.MinValue && arcMaxY > nodePos.Y)
                    fanHeight = MathF.Round((arcMaxY - nodePos.Y) + 0.2f, 2);

                route.RadiusStart = ffxivStart;
                route.RadiusEnd = ffxivEnd;
                route.MinDistance = MathF.Round(allMin, 1);
                route.MaxDistance = MathF.Round(allMax, 1);
                route.FanHeight = fanHeight;

                _fanGenStatus = $"Generated! Angles: {ffxivStart}→{ffxivEnd} (arc {bestLen}°), Distance: {allMin:F1}→{allMax:F1}, Height: {fanHeight:F2}";
                IceLogging.Info($"[FanGen] Node {route.Position}: FFXIV {ffxivStart}→{ffxivEnd}, dist {allMin:F1}→{allMax:F1}, height {fanHeight:F2}");
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

