using static ICE.Localization.L10n;
﻿using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_WheelofFortune
    {
        public static void Draw()
        {
            if (ImGui.Button($"Auto Gamba"))
            {
                Task_Gamba.Enqueue();
            }

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var lotto) && lotto.IsAddonReady)
            {
                ImGui.Text($"Lottery addon is visible!");

                if (ImGui.Button($"Left wheel select"))
                {
                    Task_Gamba.SelectWheelLeft(lotto);
                }
                ImGui.SameLine();

                if (ImGui.Button($"Right wheel select"))
                {
                    Task_Gamba.SelectWheelRight(lotto);
                }

                ImGui.SameLine();
                if (ImGui.Button($"Confirm"))
                {
                    lotto.ConfirmButton();
                }

                if (ImGui.Button(T("Auto Gamba")))
                {
                    Task_Gamba.Enqueue();
                }

                ImGui.Text(T("Items in left wheel"));
                foreach (var l in lotto.LeftWheelItems)
                {
                    ImGui.Text(T("Name: {0} | Id: {1} | Amount: {2}", l.itemName, l.itemId, l.itemAmount));
                }

                ImGui.Spacing();
                foreach (var m in lotto.RightWheelItems)
                {
                    ImGui.Text(T("Name: {0} | Id: {1} | Amount: {2}", m.itemName, m.itemId, m.itemAmount));
                }
            }
            else
            {
                ImGui.Text(T("Waiting for \"WKSLottery\" to be visible"));
            }
        }
    }
}
