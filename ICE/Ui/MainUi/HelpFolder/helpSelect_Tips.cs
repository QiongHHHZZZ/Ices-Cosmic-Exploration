using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.HelpFolder.Tips_Folder;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.HelpFolder
{
    internal class helpSelect_Tips
    {
        public enum Help_Selection
        {
            Welcome,
            ModeSelection,
            AgendaMode, 

        }

        private static Help_Selection selectedMode = Help_Selection.Welcome;

        private static string EnumString(Help_Selection tipSelected)
        {
            return tipSelected switch
            {
                Help_Selection.Welcome => "Welcome",
                Help_Selection.ModeSelection => "Mode Selection",
                Help_Selection.AgendaMode => "Cosmic Agenda",
                _ => tipSelected.ToString()
            };
        }

        private static readonly Dictionary<Help_Selection, Action> TipViews = new()
        {
            [Help_Selection.Welcome] = () => Welcome.Draw(),
            [Help_Selection.ModeSelection] = () => ModeSelection.Draw(),
        };

        public static void Draw()
        {
            float spacing = 10f;
            float leftPanelWidth = 200f;
            float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
            float childHeight = ImGui.GetContentRegionAvail().Y;

            if (ImGui.BeginChild("Tip Selector", new Vector2(leftPanelWidth, childHeight), true))
            {
                foreach (Help_Selection tip in Enum.GetValues<Help_Selection>())
                {
                    bool isSelected = tip == selectedMode;
                    if (ImGui.Selectable($"{EnumString(tip)}", isSelected))
                    {
                        selectedMode = tip;
                    }
                }
            }
            ImGui.EndChild();

            ImGui.SameLine(0, spacing);
            if (ImGui.BeginChild("DebugContent", new System.Numerics.Vector2(rightPanelWidth, childHeight), true))
            {
                if (TipViews.TryGetValue(selectedMode, out var drawAction))
                {
                    drawAction();
                }
                else
                {
                    ImGui.Text(T("Unknown Tip View"));
                }
            }
            ImGui.EndChild();
        }

        private static void ScoreMax()
        {
            ImGui.TextWrapped(T("Each planet has a dedicated set of missions are deemed the most \"Optimal\" when it comes to farming score.\nThere's certain missions that are worth grinding more than others. Weather/time also plays a part of it all. Below is what I would recommend on a per class basis."));
            ImGui.TextWrapped("");
        }
    }
}

