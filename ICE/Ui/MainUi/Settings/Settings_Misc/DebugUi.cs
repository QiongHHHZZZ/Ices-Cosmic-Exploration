using Dalamud.Game.Text;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string DebugCategory = "Debug";

    private static readonly SettingEntry Debug_Forecast = new()
    {
        Label = T("Weather Forecast"),
        Category = DebugCategory,
        Keywords = new[] { "Weather", "Forecast", "Hub", "Refresh", "Echo", "Chat" },
        Draw = () =>
        {
            if (ImGui.Button(T("Get current hub forecast")))
                PrintHubForecast();

            using (ImRaii.Disabled(!PlayerHelper.IsInCosmicZone()))
            {
                if (ImGui.Button(T("Refresh Forecast")))
                    WeatherForecastHandler.GetForecast();
            }
        }
    };

    private static readonly SettingEntry Debug_GatherInfo = Toggle(
        "Show Gather Debug Info", DebugCategory,
        new[] { "Gather", "Debug", "Info" },
        () => C.ShowDebugGatherInfo,
        v => C.ShowDebugGatherInfo = v);

    private static readonly SettingEntry Debug_HighlightMissions = Toggle(
        "Highlight Visible Missions", DebugCategory,
        new[] { "Highlight", "Visible", "Missions", "Table" },
        () => C.HighlightVisibleMissions,
        v => C.HighlightVisibleMissions = v);

    private static readonly SettingEntry Debug_OnlyGrab = Toggle(
        "Only grab mission", DebugCategory,
        new[] { "Only", "Grab", "Mission" },
        () => C.OnlyGrabMission_Debug,
        v => C.OnlyGrabMission_Debug = v);

    private static void PrintHubForecast()
    {
        var territoryId = PlayerHelper.IsInCosmicZone()
            ? Player.Territory.RowId
            : CosmicMoonRegistry.Sinus.TerritoryId;

        List<WeatherForecast> forecast = WeatherForecastHandler.GetTerritoryForecast((ushort)territoryId);
        if (forecast.Count == 0)
            return;

        var hubName = CosmicMoonRegistry.GetDisplayName(territoryId);
        Echo($"{hubName} Weather - {forecast[0].Name}");

        // i < forecast.Count: keep looping while i is still smaller than Count.
        // Starts at 1 because index 0 was the header line above.
        for (var i = 1; i < forecast.Count; i++)
        {
            var time = WeatherForecastHandler.FormatForecastTime(forecast[i].Time);
            Echo($"{forecast[i].Name} In {time}");
        }
    }

    private static void Echo(string message)
    {
        Svc.Chat.Print(new XivChatEntry
        {
            Message = message,
            Type = XivChatType.Echo,
        });
    }
}

