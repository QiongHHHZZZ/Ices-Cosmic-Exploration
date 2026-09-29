using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string SafetyCategory = "Safety Settings";

    private static readonly SettingEntry Safety_MissionDelay = new()
    {
        Label = T("Add delay to mission menu"),
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Mission", "Menu", "Grab", "Safety", "Animation Lock", "ms" },
        Draw = () =>
        {
            var enabled = C.DelayGrabMission;
            if (ImGui.Checkbox(T("Add delay to mission menu"), ref enabled))
            {
                C.DelayGrabMission = enabled;
                C.Save();
            }

            ImGuiEx.HelpMarker(
                T("This is here for safety! If you want to decrease the delay between missions be my guest.\n") +
                T("Safety is around... 250? If you're having animation locks you can absolutely increase it higher\n") +
                T("Or if you're feeling daredevil. Lower it. I'm not your dad (will tell dad jokes though."));

            if (!enabled)
                return;

            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            var amount = C.DelayIncrease;
            if (ImGui.SliderInt("ms###Mission", ref amount, 0, 1000))
            {
                C.DelayIncrease = amount;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_CraftDelay = new()
    {
        Label = T("Add delay to crafting menu"),
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Craft", "Crafting", "Menu", "Turnin", "Safety", "Animation Lock", "ms" },
        Draw = () =>
        {
            var enabled = C.DelayCraft;
            if (ImGui.Checkbox(T("Add delay to crafting menu"), ref enabled))
            {
                C.DelayCraft = enabled;
                C.Save();
            }

            ImGuiEx.HelpMarker(
                T("This is here for safety! If you want to decrease the delay before turnin be my guest.\n") +
                T("Safety is around... 2500? If you're having animation locks you can absolutely increase it higher\n") +
                T("Or if you're feeling daredevil. Lower it. I'm not your dad (will tell dad jokes though."));

            if (!enabled)
                return;

            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            var amount = C.DelayCraftIncrease;
            if (ImGui.SliderInt("ms###Crafting", ref amount, 500, 5000))
            {
                C.DelayCraftIncrease = amount;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_RelicDelay = new()
    {
        Label = T("Delay Post Relic Turnin"),
        Category = SafetyCategory,
        Keywords = new[] { "Delay", "Relic", "Turnin", "Post", "Safety", "ms" },
        Draw = () =>
        {
            var delay = C.DelayPostRelic;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt(T("Delay Post Relic Turnin"), ref delay, 0, 5000))
            {
                C.DelayPostRelic = delay;
                C.SaveDebounced();
            }
        }
    };

    private static readonly SettingEntry Safety_GatherDelay = Toggle(
        T("Add delay to gather"), SafetyCategory,
        new[] { "Delay", "Gather", "Gathering", "Safety" },
        () => C.Delay_Gather,
        v => C.Delay_Gather = v);

    private static readonly SettingEntry Safety_CloseReward = Toggle(
        T("Auto Close Reward Popups"), SafetyCategory,
        new[] { "Reward", "Popup", "Close", "Hide", "Window", "Auto" },
        () => C.HideRewardWindow,
        v => C.HideRewardWindow = v);
}
