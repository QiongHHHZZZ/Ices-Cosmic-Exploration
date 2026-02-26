using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_MainMoon
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var HudAddon))
            {
                if (ImGui.Button(T("Mission")))
                {
                    HudAddon.Mission();
                }

                ImGui.SameLine();

                if (ImGui.Button(T("Mech")))
                {
                    HudAddon.Mech();
                }

                ImGui.SameLine();

                if (ImGui.Button(T("Steller")))
                {
                    HudAddon.Steller();
                }

                ImGui.SameLine();

                if (ImGui.Button(T("Infrastructor")))
                {
                    HudAddon.Infrastructor();
                }

                ImGui.SameLine();

                if (ImGui.Button(T("Research")))
                {
                    HudAddon.Research();
                }

                ImGui.SameLine();

                if (ImGui.Button(T("ClassTracker")))
                {
                    HudAddon.ClassTracker();
                }
            }
            else
            {
                ImGui.Text(T("Waiting for \"WKSHud\" to be visible"));
            }
        }
    }
}
