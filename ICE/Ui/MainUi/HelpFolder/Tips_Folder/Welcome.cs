using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class Welcome
    {
        public static void Draw()
        {
            ImGui.TextWrapped($"Welcome! This is probably the most complicated plugin I've created so far.");
            ImGui.TextWrapped($"This plugin is designed for specifically for the use of Cosmic Exploration, and is kinda hefty. So I'm going to try and go through all the different tips / tricks");

            ImGui.Dummy(new(0, 5));

            ImGui.TextWrapped("To the side you'll find a couple of different tabs that will *try* and answer any question that you migth have.");
            ImGui.TextWrapped("PLEASE MAKE SURE TO CHECK THE REQUIREMENTS SECTION TO SEE WHAT YOU NEED FOR WHAT");
            ImGui.TextWrapped("Or just read a specific tab to find out. Probably would answer a lot of questions");
        }
    }
}
