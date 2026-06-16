using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_Aethernet
    {
        public static void Draw()
        {
            List<uint> AetherIds = new()
            {
                16, 17, 18, 19, 20
            };

            foreach (var aethernet in AetherIds)
            {
                ImGui.Text($"[{aethernet}] 已解锁：{IsAetheryteUnlocked(aethernet, out var _)}");
            }
        }

        public unsafe static bool IsAetheryteUnlocked(uint aetheryteId, out byte subIndex)
        {
            subIndex = 0;

            UIState* uiState = UIState.Instance();
            return uiState != null && uiState->IsAetheryteUnlocked(aetheryteId);
        }
    }
}
