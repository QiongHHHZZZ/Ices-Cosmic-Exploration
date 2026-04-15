using Dalamud.Interface;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Dronebit
    {
        public static void Draw()
        {
if (ImGui.Button(T("Run Drone Finder")))
            {
                SchedulerMain.State = IceState.ArtifactSearch;
            }

if (ImGui.Button(T("Stop")))
            {
                SchedulerMain.DisablePlugin();
            }

            bool buyDrones = C.Cosmodrone_Buy;
if (ImGui.Checkbox(T("Buy Drones"), ref buyDrones))
            {
                C.Cosmodrone_Buy = buyDrones;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                T("Do you want to buy drones? If yes, enable this")
                );

            int drone_buyAtAmount = C.Cosmodrone_BuyAt;
            ImGui.SetNextItemWidth(200);
if (ImGui.SliderInt(T("Buy At Amount"), ref drone_buyAtAmount, 200, 5000))
            {
                drone_buyAtAmount = (int)Math.Round(drone_buyAtAmount / 200.0) * 200;
                C.Cosmodrone_BuyAt = drone_buyAtAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, 
                T("When do you wanna buy drones from the vendor?\n" +
                  "Set in incriments of 200, max of 5,000")
                );

            int maxCrateAmount = C.Cosmodrone_MaxKeep;
            ImGui.SetNextItemWidth(200);
if (ImGui.InputInt(T("Maximum Drones"), ref maxCrateAmount))
            {
                if (maxCrateAmount < 0)
                    maxCrateAmount = 0;
                C.Cosmodrone_MaxKeep = maxCrateAmount;
                C.SaveDebounced();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                T("What's the maximum amount of drones you wanna keep?\n" +
                  "0 = will just keep buying\n" +
                  "Anything above 0 will just be a hard cap and will stop buying if it reaches this")
                
                );

            bool runDroneFinder = C.Cosmodrone_Run;
if (ImGui.Checkbox(T("Automate cosmodrone"), ref runDroneFinder))
            {
                C.Cosmodrone_Run = runDroneFinder;
                C.Save();
            }
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                T("Do you want to run the automated drone finding? If yes, enable this\n" +
                  "PLEASE NOTE. DO. NOT. LEAVE. THIS. ALONE. This is still being worked on heavily"));
        }
    }
}
