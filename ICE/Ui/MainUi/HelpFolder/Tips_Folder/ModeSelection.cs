using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class ModeSelection
    {
        public static void Draw()
        {
            ImGui.TextWrapped("There is 5 different modes that exist currently (as of writing this) that all serve minorly differently functions.");
            ImGui.TextWrapped("Depending on what you want / what your goal is, these all serve all different functions.");

            if (ImGui.BeginTabBar("Mode Selection Info"))
            {
                if (ImGui.BeginTabItem("Standard"))
                {
                    StandardMode();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Relic Grind"))
                {
                    RelicGrind();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Agenda Mode"))
                {
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        private static void StandardMode()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.List, "Standard");
            ImGui.TextWrapped(
                "The most straightforward mode. Standard runs only the missions you have enabled " +
                "for your current class — you pick what you want done, and it handles the rest.");
            ImGui.TextWrapped("This gives you full control over what missions to run, making it ideal for:");
            ImGui.BulletText("Score Farming — select specific high-value missions");
            ImGui.BulletText("Exp Grinding");
            ImGui.BulletText("Credit / Planetary Credits / Token farming");
            ImGui.TextWrapped(
                "Basic missions (Ranks D→A) will only pull from the class you started on. " +
                "Provisional missions (Weather, Timed, and Sequence) and Red Alerts will be picked up " +
                "between missions — enable the multi-class setting to allow switching classes for these as well.");
        }

        private static void RelicGrind()
        {
            ImGui.Dummy(new(0, 5));
            ImGuiEx.IconWithText(FontAwesomeIcon.ArrowUpRightDots, "Relic Grind");

            ImGui.TextWrapped(
                "A mode designed to automate mission selection for relic progression with minimal intervention. " +
                "It scans all available missions, evaluates the experience each one provides, and picks whichever " +
                "yields the most for your current level.");
            ImGui.TextWrapped(
                "If you're high enough level to need the next rank category but haven't unlocked it yet " +
                "(e.g. Rank D completed but Rank C not yet unlocked), make sure to unlock it manually before starting.");
            ImGui.TextWrapped("Note: this mode does not swap planets for you. Each planet also has an exp cap:");
            ImGui.BulletText("Sinus   — Rank IV Max");
            ImGui.BulletText("Phaenna — Rank V Max");
            ImGui.BulletText("Oizys   — Rank VI Max");
        }

        public static void GoldCompletion()
        {

        }
    }
}
