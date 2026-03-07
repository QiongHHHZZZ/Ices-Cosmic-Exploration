using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.ModeSelect_Modes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui
{
    internal class Window_ExternalDetails : Window
    {
        private int focusDelayFrames = 0;

        public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | Mission Details")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public void RequestFocus()
        {
            focusDelayFrames = 10;
        }

        public override void Draw()
        {
            if (focusDelayFrames > 0)
            {
                focusDelayFrames--;
                if (focusDelayFrames == 0)
                    BringToFront();
            }

            var selectedId = modeSelect_TableInfo.selectedMission;
            if (selectedId == 0)
            {
                P.externalDetails.IsOpen = false;
            }
            else
            {
                modeSelect_TableInfo.DrawMissionDetails();
            }
        }
    }
}
