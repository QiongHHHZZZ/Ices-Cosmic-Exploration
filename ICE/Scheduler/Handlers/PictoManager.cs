using ECommons.GameHelpers;
using ICE.Resources.GatheringRoutes;
using ICE.Utilities.Cosmic_Helper;
using Pictomancy;
using System.Collections.Generic;

namespace ICE.Scheduler.Handlers
{
    // TO ANYONE WONDERING WTF THIS IS.
    // This is a universal way of me being able to add things to picto drawing without having to add it constantly. 
    // This will run on framework thread -> just load whatever it's suppose to load
    // Which, is nice cause it means that I can add things from different windows here and run it all under the same code

    internal static class PictoManager
    {
        // Storage for draw commands - initialize inline to avoid any timing issues
        private static readonly List<Action<PctDrawList>> drawCommands = new();
        private static readonly object lockObject = new();

        // Add a draw command to be executed this frame
        public static void AddDrawCommand(Action<PctDrawList> drawAction)
        {
            if (drawAction == null) return;

            lock (lockObject)
            {
                drawCommands.Add(drawAction);
            }
        }

        // Main draw function - call this every frame
        public static void DrawPicto()
        {
            try
            {
                using (var pictoDraw = PictoService.Draw())
                {
                    if (pictoDraw == null)
                        return;

                    lock (lockObject)
                    {
                        // Execute all queued draw commands
                        foreach (var command in drawCommands)
                        {
                            try
                            {
                                command(pictoDraw);
                            }
                            catch (Exception ex)
                            {
                                IceLogging.Error($"Error executing draw command: {ex}");
                            }
                        }

                        // Clear the queue for next frame
                        drawCommands.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                IceLogging.Error($"Error in DrawPicto: {ex}");
            }
        }

        // Helper methods for common draws
        public static void DrawArrow(Vector3 targetPos, uint fillColor, uint outlineColor, bool scaleWithDistance = true)
        {
            AddDrawCommand(pictoDraw =>
            {
                Picto_TriangleRotate(pictoDraw, targetPos, fillColor, outlineColor, scaleWithDistance);
            });
        }

        public static void DrawConnectors(List<(uint nodeId, Vector3 position)> waypoints)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                var lineColor = C.PictoColor_Circle;
                var currentNode = waypoints[i];

                if (i != 0)
                {
                    var prevNode = waypoints[i-1];
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddLine(prevNode.position, currentNode.position, 0, lineColor);
                    });
                }

                if (i == waypoints.Count - 1)
                {
                    var firstNode = waypoints[0];
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddLine(currentNode.position, firstNode.position, 0, lineColor);
                    });
                }

                AddDrawCommand(pictoDraw =>
                {
                    pictoDraw.AddText(new Vector3(currentNode.position.X, currentNode.position.Y + 2, currentNode.position.Z), C.PictoColor_Dot, $"Node: {currentNode.nodeId}");
                });

                AddDrawCommand(pictoDraw =>
                {
                    pictoDraw.AddDot(currentNode.position, 5, C.PictoColor_Dot);
                });
            }
        }

        public static void DrawGatherNodes(List<GathNodeInfo>? routeItem)
        {
            // Light colors with transparency (lighter alpha ~50%)
            uint lightBlue = 0x80ADD8E6;   // Light blue with 50% alpha
            uint lightPurple = 0x80DDA0DD; // Light purple (plum) with 50% alpha
            uint orange = 0xFFFFA500;      // Orange with 100% alpha (fully opaque)

            // Solid colors for distance checks (full alpha)
            uint green = 0xFF00FF00;       // Bright green, fully opaque
            uint red = 0xFFFF0000;         // Bright red, fully opaque

            float distanceThreshold = 5f; // Adjust this threshold as needed

            if (routeItem != null)
            {
                for (int i = 0; i < routeItem.Count; i++)
                {
                    var currentNode = routeItem[i];

                    // Draw line connecting to previous land zone
                    if (i != 0)
                    {
                        var prevNode = routeItem[i - 1];
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddLineFilled(prevNode.LandZone, currentNode.LandZone, 0.01f, lightBlue);
                        });
                    }

                    // Draw line connecting last land zone back to first (loop)
                    if (i == routeItem.Count - 1)
                    {
                        var firstNode = routeItem[0];
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddLineFilled(currentNode.LandZone, firstNode.LandZone, 0.01f, lightBlue);
                        });
                    }

                    // Draw circle around the node position
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddCircleFilled(currentNode.Position, 0.75f, lightPurple, lightBlue);
                    });

                    // Draw text label for node
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddText(new Vector3(currentNode.Position.X, currentNode.Position.Y + 2, currentNode.Position.Z),
                            0xFFFFFFFF, $"Node: {currentNode.NodeId}");
                    });

                    // Draw dot at landzone position
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddDot(currentNode.LandZone, 5, orange);
                    });

                    // Draw line between position and landzone with color based on distance
                    float distance = Vector3.Distance(currentNode.Position, currentNode.LandZone);
                    uint lineColor = distance <= distanceThreshold ? green : red;

                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddLine(currentNode.Position, currentNode.LandZone, 0.01f, lineColor);
                    });
                }
            }
        }
        public static void DrawGatherNodes(List<GathNodeInfo>? routeItem, uint selectedNode, List<Vector3>? waypointPath = null)
        {
            // Light colors with transparency (lighter alpha ~50%)
            uint lightBlue = 0x80ADD8E6;   // Light blue with 50% alpha
            uint lightPurple = 0x80DDA0DD; // Light purple (plum) with 50% alpha
            uint orange = 0xFFFFA500;      // Orange with 100% alpha (fully opaque)

            // Solid colors for distance checks (full alpha)
            uint green = 0xFF00FF00;       // Bright green, fully opaque
            uint red = 0xFFFF0000;         // Bright red, fully opaque
            uint waypointColor = 0xFF00FFFF; // Cyan for waypoint path

            float distanceThreshold = 5f; // Adjust this threshold as needed

            if (routeItem != null)
            {
                // Draw the waypoint path if it exists
                if (waypointPath != null && waypointPath.Count > 0)
                {
                    for (int i = 0; i < waypointPath.Count - 1; i++)
                    {
                        var currentWaypoint = waypointPath[i];
                        var nextWaypoint = waypointPath[i + 1];

                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddLine(currentWaypoint, nextWaypoint, 0.02f, waypointColor);
                        });

                        // Draw small dots at waypoints
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddDot(currentWaypoint, 3, waypointColor);
                        });
                    }

                    // Draw last waypoint dot
                    if (waypointPath.Count > 0)
                    {
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddDot(waypointPath[^1], 3, waypointColor);
                        });
                    }
                }

                // Draw the original gather nodes
                for (int i = 0; i < routeItem.Count; i++)
                {
                    var currentNode = routeItem[i];

                    // Draw line connecting to previous land zone
                    if (i != 0)
                    {
                        var prevNode = routeItem[i - 1];
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddLineFilled(prevNode.LandZone, currentNode.LandZone, 0.01f, lightBlue);
                        });
                    }

                    // Draw line connecting last land zone back to first (loop)
                    if (i == routeItem.Count - 1)
                    {
                        var firstNode = routeItem[0];
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddLineFilled(currentNode.LandZone, firstNode.LandZone, 0.01f, lightBlue);
                        });
                    }

                    // Draw circle around the node position
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddCircleFilled(currentNode.Position, 0.75f, lightPurple, lightBlue);
                    });

                    // Draw text label for node
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddText(new Vector3(currentNode.Position.X, currentNode.Position.Y + 2, currentNode.Position.Z),
                            0xFFFFFFFF, $"Node: {currentNode.NodeId}");
                    });

                    // Draw dot at landzone position
                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddDot(currentNode.LandZone, 5, orange);
                    });

                    var fanColor = lightPurple;
                    if (selectedNode == currentNode.NodeId)
                        fanColor = orange;

                    Vector3 position = new(currentNode.Position.X, currentNode.Position.Y + currentNode.FanHeight, currentNode.Position.Z);

                    if (currentNode.Radius_Start > currentNode.Radius_End)
                    {
                        AddDrawCommand(pictoDraw =>
                        {
                            // Draw from start up to 360
                            pictoDraw.AddFanFilled(
                                position,
                                currentNode.Distance_Min,
                                currentNode.Distance_Max,
                                DegreesToRadians(currentNode.Radius_Start),
                                DegreesToRadians(360),
                                fanColor);

                            // Draw from 0 up to end
                            pictoDraw.AddFanFilled(
                                position,
                                currentNode.Distance_Min,
                                currentNode.Distance_Max,
                                DegreesToRadians(0),
                                DegreesToRadians(currentNode.Radius_End),
                                fanColor);
                        });
                    }
                    else
                    {
                        // Normal case, start < end
                        AddDrawCommand(pictoDraw =>
                        {
                            pictoDraw.AddFanFilled(
                                position,
                                currentNode.Distance_Min,
                                currentNode.Distance_Max,
                                DegreesToRadians(currentNode.Radius_Start),
                                DegreesToRadians(currentNode.Radius_End),
                                fanColor);
                        });
                    }

                    // Draw line between position and landzone with color based on distance
                    float distance = Vector3.Distance(currentNode.Position, currentNode.LandZone);
                    uint lineColor = distance <= distanceThreshold ? green : red;

                    AddDrawCommand(pictoDraw =>
                    {
                        pictoDraw.AddLine(currentNode.Position, currentNode.LandZone, 0.01f, lineColor);
                    });
                }
            }
        }

        /// <summary>
        /// Fancy ass fucking triangle that might not be used? But also, it's neat so I'm keeping it. <br></br>
        /// Esentially just picto drawing a custom triangle in the 3d space
        /// </summary>
        /// <param name="pictoDraw"></param>
        /// <param name="pos"></param>
        /// <param name="fillColor"></param>
        /// <param name="outlineColor"></param>
        /// <param name="scaleWithDistance"></param>
        public static void Picto_TriangleRotate(PctDrawList pictoDraw, Vector3 pos, uint fillColor, uint outlineColor, bool scaleWithDistance = true)
        {
            var player = Player.Position;

            // Position the arrow at a fixed world position (arrow TIP will be at this position)
            var arrowPosition = pos;

            // Calculate direction from arrow to player (in XZ plane)
            var toPlayer = new Vector2(
                player.X - arrowPosition.X,
                player.Z - arrowPosition.Z
            );

            // Calculate the angle to rotate - NEGATE to fix rotation direction
            float angle = -MathF.Atan2(toPlayer.X, toPlayer.Y);

            // Calculate scale based on distance to maintain constant screen size
            float scale = 1.0f;
            if (scaleWithDistance)
            {
                float distance = Vector3.Distance(player, arrowPosition);
                // Scale proportionally to distance to maintain constant screen size
                // Clamp between min and max to prevent it from getting too small or too large
                scale = Math.Clamp(distance / 5f, 0.5f, 5.0f); // Min 0.5x, Max 5x
            }

            // Arrow dimensions (scaled)
            float shaftTopWidth = 0.4f * scale;
            float shaftBottomWidth = 0.25f * scale;
            float shaftHeight = 1.5f * scale;
            float headWidth = 0.8f * scale;
            float headHeight = 1.0f * scale;

            // Total arrow height
            float totalHeight = shaftHeight + headHeight;

            // Helper function to rotate a point around the arrow's center position
            Vector3 RotatePoint(float x, float y, float z)
            {
                // Rotate around Y axis
                float rotatedX = x * MathF.Cos(angle) - z * MathF.Sin(angle);
                float rotatedZ = x * MathF.Sin(angle) + z * MathF.Cos(angle);

                return new Vector3(
                    arrowPosition.X + rotatedX,
                    arrowPosition.Y + y,
                    arrowPosition.Z + rotatedZ
                );
            }

            // Define arrow shape in local space (tip is at origin, arrow extends upward)
            var shaftTopLeft = RotatePoint(-shaftTopWidth, totalHeight, 0);
            var shaftTopRight = RotatePoint(shaftTopWidth, totalHeight, 0);
            var shaftBottomLeft = RotatePoint(-shaftBottomWidth, headHeight, 0);
            var shaftBottomRight = RotatePoint(shaftBottomWidth, headHeight, 0);

            var headLeft = RotatePoint(-headWidth, headHeight, 0);
            var headRight = RotatePoint(headWidth, headHeight, 0);
            var arrowTip = RotatePoint(0, 0, 0);

            float lineHalfWidth = 0.05f * scale; // Scale line width too

            // FILL the shapes first (so outlines draw on top)
            // Fill shaft trapezoid
            pictoDraw.AddQuadFilled(shaftTopLeft, shaftTopRight, shaftBottomRight, shaftBottomLeft, fillColor);

            // Fill arrowhead triangle
            pictoDraw.AddTriangleFilled(headLeft, headRight, arrowTip, fillColor);

            // THEN draw the outlines
            // Draw shaft trapezoid outline
            pictoDraw.AddLine(shaftTopLeft, shaftTopRight, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(shaftTopRight, shaftBottomRight, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(shaftBottomRight, shaftBottomLeft, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(shaftBottomLeft, shaftTopLeft, lineHalfWidth, outlineColor, 2.0f);

            // Draw arrowhead outline
            pictoDraw.AddLine(shaftBottomLeft, headLeft, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(shaftBottomRight, headRight, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(headLeft, headRight, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(headLeft, arrowTip, lineHalfWidth, outlineColor, 2.0f);
            pictoDraw.AddLine(headRight, arrowTip, lineHalfWidth, outlineColor, 2.0f);
        }

        public static void DrawIcon(ImTextureID textureHandle, Vector3 worldPos, Vector2? size = null)
        {
            AddDrawCommand(_ =>
            {
                var iconSize = size ?? new Vector2(24, 24);
                if (!PictoService.GameGui.WorldToScreen(worldPos, out var screenPos)) return;

                var topLeft = screenPos - iconSize / 2;
                ImGui.GetForegroundDrawList().AddImage(textureHandle, topLeft, topLeft + iconSize);
            });
        }

        public static void DrawIcon(ImTextureID textureHandle, Vector3 worldPos, Vector2? size = null, float opacity = 1f, float? scaleByDistance = null)
        {
            AddDrawCommand(_ =>
            {
                if (!PictoService.GameGui.WorldToScreen(worldPos, out var screenPos)) return;

                var iconSize = size ?? new Vector2(24, 24);

                // Scale based on distance if a max distance is provided
                if (scaleByDistance != null)
                {
                    float distance = Vector3.Distance(Player.Position, worldPos);
                    // Normalize distance: 0 = right next to it (scale 1x), maxDist = far away (scale 4x)
                    float t = Math.Clamp(distance / scaleByDistance.Value, 0f, 1f);
                    float scaleFactor = 1f + (4f - 1f) * t;
                    iconSize *= scaleFactor;
                }

                // Convert opacity 0-1 to alpha byte in RGBA
                uint alpha = (uint)(Math.Clamp(opacity, 0f, 1f) * 255) << 24;
                uint tintColor = alpha | 0x00FFFFFF; // full white RGB, variable alpha

                var topLeft = screenPos - iconSize / 2;
                ImGui.GetForegroundDrawList().AddImage(textureHandle, topLeft, topLeft + iconSize, Vector2.Zero, Vector2.One, tintColor);
            });
        }

        public static float DegreesToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180f);
        }
    }
}
