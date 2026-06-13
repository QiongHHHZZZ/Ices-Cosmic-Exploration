using ECommons.GameHelpers;
using Pictomancy;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.Debug_Tabs.Debug_Ui
{
    internal class Ui_NavmeshTesting
    {
        private static List<Vector3> finalPath = new List<Vector3>();
        private static Vector3 currentPos = new Vector3();
        private static List<Vector3> wholePath = new List<Vector3>();

        // Picto Stuff
        private static float lineWidth = 0;
        private static float dotRadius = 4.2f;
        private static uint LineColor = 804847871;
        private static uint WPColor = 4294180358;
        private static uint TextCol = 2667577343;

        public static void Draw()
        {
            if (Player.Object != null)
                currentPos = Player.Object.Position;
            else
                currentPos = new Vector3(0, 0, 0);

            ImGui.Text(T("Current pos: {0:N2} | {1:N2} | {2:N2}", currentPos.X, currentPos.Y, currentPos.Z));

            if (ImGui.Button(T("Add Position")))
            {
                finalPath.Add(currentPos);
            }
            for (int i = 0; i < finalPath.Count; i++)
            {
                var entry = finalPath[i];

                ImGui.Text(T("X: {0:N2}, Y: {1:N2}, Z: {2:N2}", entry.X, entry.Y, entry.Z));
                ImGui.SameLine();
                if (ImGui.Button($"Adjust###Adjust_{entry}_{i}"))
                {
                    entry = currentPos;
                }
                ImGui.SameLine();
                if (ImGui.Button($"Remove###Remove_{entry}_{i}"))
                {
                    finalPath.Remove(entry);
                    i--;
                }
            }

            using (var drawList = PctService.Draw())
            {
                int wpNumber = 0;

                if (drawList == null)
                    return;

                for (int x = 0; x < wholePath.Count; x++)
                {
                    var wp = wholePath[x];
                    if (x <  wholePath.Count - 1)
                    {
                        var nextWp = wholePath[x + 1];
                        drawList.AddLine(wp, nextWp, lineWidth, LineColor);
                    }

                    drawList.AddDot(wp, dotRadius, WPColor);
                    Vector3 wpText = new Vector3(wp.X, wp.Y + 5, wp.Z);
                    wpNumber++;
                    drawList.AddText(wpText, TextCol, $"{wpNumber}", 0);
                }
            }
        }
    }
}
