using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder
{
    internal class helpSelect_Tips
    {
        private static List<string> TipOptions = new()
        {
            "Score Maximizing"
        };

        private static readonly Dictionary<string, Action> TipViews = new()
        {
            ["Score Maximizing"] = () => ScoreMax()
        };

        private static string selectedOption = TipOptions[0];

        public static void Draw()
        {
            float spacing = 10f;
            float leftPanelWidth = 200f;
            float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
            float childHeight = ImGui.GetContentRegionAvail().Y;

            if (ImGui.BeginChild("Tip Selector", new Vector2(leftPanelWidth, childHeight), true))
            {
                
            }
            ImGui.EndChild();

            ImGui.SameLine(0, spacing);
            if (ImGui.BeginChild("DebugContent", new System.Numerics.Vector2(rightPanelWidth, childHeight), true))
            {
                if (TipViews.TryGetValue(selectedOption, out var drawAction))
                {
                    drawAction();
                }
                else
                {
                    ImGui.Text("Unknown Tip View");
                }
            }
            ImGui.EndChild();
        }

        private static void ScoreMax()
        {
            ImGui.TextWrapped("Each planet has a dedicated set of missions are deemed the most \"Optimal\" when it comes to farming score." +
                "\nThere's certain missions that are worth grinding more than others. Weather/time also plays a part of it all. Below is what I would recommend on a per class basis.");
            ImGui.TextWrapped("");
        }
    }
}
