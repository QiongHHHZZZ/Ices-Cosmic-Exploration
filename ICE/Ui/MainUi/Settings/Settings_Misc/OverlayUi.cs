using Dalamud.Interface.Utility;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string OverlayCategory = "Overlay Window";

    private static readonly Dictionary<uint, string> OverlayClassNames = new()
    {
        [8] = "刻木匠",
        [9] = "锻铁匠",
        [10] = "铸甲匠",
        [11] = "雕金匠",
        [12] = "革匠",
        [13] = "裁缝",
        [14] = "炼金术士",
        [15] = "烹调师",
        [16] = "采矿工",
        [17] = "园艺工",
        [18] = "捕鱼人",
    };

    private static readonly SettingEntry Overlay_AutoOpen = new()
    {
        Label = T("Auto-Open Overlay"),
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Open", "Auto" },
        Draw = () =>
        {
            var showOverlay = C.ShowOverlay;
            if (ImGui.Checkbox(T("Auto-Open Overlay"), ref showOverlay))
            {
                C.ShowOverlay = showOverlay;
                C.Save();
            }

            ImGui.SameLine();
            if (ImGui.Button(T("Open Overlay")) && !P.overlayWindow.IsOpen)
                P.overlayWindow.IsOpen = true;
        }
    };

    private static readonly SettingEntry Overlay_CogsIcon = Toggle(
        T("Use cogs button instead of home"), OverlayCategory,
        new[] { "Overlay", "Cogs", "Home", "Button", "Icon" },
        () => C.Overlay_UseCogsIcon,
        v => C.Overlay_UseCogsIcon = v);

    private static readonly SettingEntry Overlay_ShowSeconds = Toggle(
        T("Show Seconds"), OverlayCategory,
        new[] { "Overlay", "Seconds", "Time" },
        () => C.ShowSeconds,
        v => C.ShowSeconds = v);

    private static readonly SettingEntry Overlay_ExpBars = new()
    {
        Label = T("Show Experience Bars on Overlay"),
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Experience", "Exp", "XP", "Bars", "Maxed" },
        Draw = () =>
        {
            var showExp = C.ShowExpBars;
            if (ImGui.Checkbox(T("Show Experience Bars on Overlay"), ref showExp))
            {
                C.ShowExpBars = showExp;
                C.Save();
            }

            if (!showExp)
                return;

            ImGui.SameLine();
            var hideWhenMaxed = C.ShowExpBars_HideWhenMaxed;
            if (ImGui.Checkbox(T("Until maxed only"), ref hideWhenMaxed))
            {
                C.ShowExpBars_HideWhenMaxed = hideWhenMaxed;
                C.Save();
            }
        }
    };

    private static readonly SettingEntry Overlay_Scores = new()
    {
        Label = T("Score Display"),
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Score", "Class", "Total", "Mastery", "Current" },
        Draw = () =>
        {
            var showClassScore = C.ShowCurrentScore;
            if (ImGui.Checkbox(T("Show Current Class Score"), ref showClassScore))
            {
                C.ShowCurrentScore = showClassScore;
                C.Save();
            }

            ImGui.SameLine();
            var showTotalScore = C.ShowTotalScore;
            if (ImGui.Checkbox(T("Show Total Score"), ref showTotalScore))
            {
                C.ShowTotalScore = showTotalScore;
                C.Save();
            }

            ImGui.SameLine();
            var showMasteryScore = C.ShowMasteryScore;
            if (ImGui.Checkbox(T("Show Mastery Score"), ref showMasteryScore))
            {
                C.ShowMasteryScore = showMasteryScore;
                C.Save();
            }
        }
    };

    private static readonly SettingEntry Overlay_AutoResize = Toggle(
        T("Auto Resize Overlay"), OverlayCategory,
        new[] { "Overlay", "Resize", "Size" },
        () => C.Overlay_AutoResize,
        v => C.Overlay_AutoResize = v);

    private static readonly SettingEntry Overlay_HighlightWeather = Toggle(
        T("Highlight EX+ token weathers"), OverlayCategory,
        new[] { "Overlay", "Highlight", "Weather", "Token", "EX+" },
        () => C.Overlay_HighlightTokenWeather,
        v => C.Overlay_HighlightTokenWeather = v);

    private static readonly SettingEntry Overlay_WeatherSelected = Toggle(
        T("Show enabled missions on weather hover"), OverlayCategory,
        new[] { "Overlay", "Weather", "Hover", "Missions", "Enabled", "Selected" },
        () => C.Overlay_WeatherSelected,
        v => C.Overlay_WeatherSelected = v);

    private static readonly SettingEntry Overlay_JobFilter = new()
    {
        Label = T("Filter by current job only"),
        Category = OverlayCategory,
        Keywords = new[] { "Overlay", "Filter", "Job", "Class", "Current" },
        Draw = () =>
        {
            var filterByCurrentJob = C.Overlay_FilterByCurrentJob;
            if (ImGui.Checkbox(T("Filter by current job only"), ref filterByCurrentJob))
            {
                C.Overlay_FilterByCurrentJob = filterByCurrentJob;
                C.Save();
            }

            if (filterByCurrentJob)
                return;

            var iconSize = 26 * ImGuiHelpers.GlobalScale;
            const float iconSpacing = 4;

            foreach (var (jobId, name) in OverlayClassNames)
            {
                var isSelected = C.Overlay_FilterJobs.Contains(jobId);
                var icon = isSelected
                    ? CosmicHelper.ClassInfoDict.TryGetValue(jobId, out var tex) ? tex.JobIcon.GetWrapOrEmpty() : null
                    : ImGui_Ice.GetGreyscaleJob(jobId);

                if (icon != null && ImGui_Ice.DrawStyledImageButton(icon, new Vector2(iconSize, iconSize), isSelected))
                {
                    if (isSelected)
                        C.Overlay_FilterJobs.Remove(jobId);
                    else
                        C.Overlay_FilterJobs.Add(jobId);
                    C.Save();
                }

                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(name);

                ImGui.SameLine(0, iconSpacing);
            }
            ImGui.NewLine();
        }
    };

    private static readonly SettingEntry Overlay_HudClipping = Toggle(
        T("Disable HUD Clipping"), OverlayCategory,
        new[] { "Overlay", "HUD", "Clipping", "Native", "UI" },
        () => C.DisableHudClipping,
        v => C.DisableHudClipping = v,
        T("When enabled, overlays will render over the native UI elements"));
}

