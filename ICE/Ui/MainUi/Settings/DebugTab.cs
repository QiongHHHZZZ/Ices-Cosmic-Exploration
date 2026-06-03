using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Scheduler.Handlers;
using ICE.Utilities;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class DebugTab
    {
        public static void Draw()
        {
            ImGui.Checkbox(T("Force OOM Main"), ref SchedulerMain.DebugOOMMain);
            ImGui.Checkbox(T("Force OOM Sub"), ref SchedulerMain.DebugOOMSub);

            if (ImGui.Button("Get current hub forecast"))
            {
                // Same fallback as other debug tabs: current hub, or Sinus when not in cosmic.
                var territoryId = PlayerHelper.IsInCosmicZone()
                    ? Player.Territory.RowId
                    : CosmicMoonRegistry.Sinus.TerritoryId;
                List<WeatherForecast> forecast = WeatherForecastHandler.GetTerritoryForecast((ushort)territoryId);
                Func<WeatherForecast, string> formatTime = (forecast) => WeatherForecastHandler.FormatForecastTime(forecast.Time);
                var hubName = CosmicMoonRegistry.GetDisplayName(territoryId);

                Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry()
                {
                    Message = $"{hubName} Weather - {forecast[0].Name}",
                    Type = Dalamud.Game.Text.XivChatType.Echo,
                });
                for (int i = 1; i < forecast.Count; i++)
                {
                    Svc.Chat.Print(new Dalamud.Game.Text.XivChatEntry()
                    {
                        Message = $"{forecast[i].Name} In {formatTime(forecast[i])}",
                        Type = Dalamud.Game.Text.XivChatType.Echo,
                    });
                }
            }

            using (ImRaii.Disabled(!PlayerHelper.IsInCosmicZone()))
            {
                if (ImGui.Button(T("Refresh Forecast")))
                {
                    WeatherForecastHandler.GetForecast();
                }
            }
            bool gatherDebug = C.ShowDebugGatherInfo;
            if (ImGui.Checkbox(T("Show Gather Debug Info"), ref gatherDebug))
            {
                C.ShowDebugGatherInfo = gatherDebug;
                C.Save();
            }

            bool highlightTable = C.HighlightVisibleMissions;
            if (ImGui.Checkbox(T("Highlight Visible Missions"), ref highlightTable))
            {
                C.HighlightVisibleMissions = highlightTable;
                C.Save();
            }

            bool onlyGrabMission = C.OnlyGrabMission_Debug;
            if (ImGui.Checkbox($"Only grab mission", ref onlyGrabMission))
            {
                C.OnlyGrabMission_Debug = onlyGrabMission;
                C.Save();
            }
        }
    }
}

