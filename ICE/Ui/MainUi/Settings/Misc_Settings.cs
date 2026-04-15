using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.Settings.Settings_Table;
using ICE.Utilities.ImGuiTools;
using Lumina.Excel.Sheets;
using Pictomancy;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings
{
    internal class Misc_Settings
    {
        public static void Draw()
        {
            OverlaySettings();
            Separator();
            AutoUse();
            Separator();
            GoldMissionRemover();
            Separator();
            SafetySettings.Draw();
            Separator();
            Separator();
            TimeRecords();
            Separator();
            PostMissionCommands();
#if DEBUG
            Separator();
            DebugTab.Draw();
#endif
        }

        public static void OverlaySettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.WindowMaximize, "Overlay Window");
            ImGui.Dummy(new (0, 5));

            bool showOverlay = C.ShowOverlay;
            if (ImGui.Checkbox("Show Overlay", ref showOverlay))
            {
                C.ShowOverlay = showOverlay;
                C.Save();
            }
            ImGui.SameLine();
            bool useCogsIcon = C.Overlay_UseCogsIcon;
            if (ImGui.Checkbox("Use cogs button instead of home", ref useCogsIcon))
            {
                C.Overlay_UseCogsIcon = useCogsIcon;
                C.Save();
            }

            bool ShowSeconds = C.ShowSeconds;
            if (ImGui.Checkbox("Show Seconds", ref ShowSeconds))
            {
                C.ShowSeconds = ShowSeconds;
                C.Save();
            }

            bool showExpOverlay = C.ShowExpBars;
            if (ImGui.Checkbox("Show Experience Bars on Overlay", ref showExpOverlay))
            {
                C.ShowExpBars = showExpOverlay;
                C.Save();
            }
            if (showExpOverlay)
            {
                ImGui.SameLine();
                bool hideWhenMaxed = C.ShowExpBars_HideWhenMaxed;
                if (ImGui.Checkbox("Until maxed only", ref hideWhenMaxed))
                {
                    C.ShowExpBars_HideWhenMaxed = hideWhenMaxed;
                    C.Save();
                }
            }

            bool showClassScore = C.ShowCurrentScore;
            if (ImGui.Checkbox("Show Current Class Score", ref showClassScore))
            {
                C.ShowCurrentScore = showClassScore;
                C.Save();
            }
            ImGui.SameLine();
            bool showTotalScore = C.ShowTotalScore;
            if (ImGui.Checkbox("Show Total Score", ref showTotalScore))
            {
                C.ShowTotalScore = showTotalScore;
                C.Save();
            }

            bool AutoResize = C.Overlay_AutoResize;
            if (ImGui.Checkbox("Auto Resize Overlay", ref AutoResize))
            {
                C.Overlay_AutoResize = AutoResize;
                C.Save();
            }


            bool highlightTokenWeather = C.Overlay_HighlightTokenWeather;
            if (ImGui.Checkbox("Highlight EX+ token weathers", ref highlightTokenWeather))
            {
                C.Overlay_HighlightTokenWeather = highlightTokenWeather;
                C.Save();
            }

            bool showSelectedWeatherMissions = C.Overlay_WeatherSelected;
            if (ImGui.Checkbox($"Show enabled missions on weather hover", ref showSelectedWeatherMissions))
            {
                C.Overlay_WeatherSelected = showSelectedWeatherMissions;
                C.Save();
            }

            bool filterByCurrentJob = C.Overlay_FilterByCurrentJob;
            if (ImGui.Checkbox("Filter by current job only", ref filterByCurrentJob))
            {
                C.Overlay_FilterByCurrentJob = filterByCurrentJob;
                C.Save();
            }
            if (!filterByCurrentJob)
            {
                float scale = ImGuiHelpers.GlobalScaleSafe;
                float iconSize = 26 * scale;
                float iconSpacing = 4;
                var classDict = new Dictionary<uint, string>
                {
                    [8] = "CRP", [9] = "BSM", [10] = "ARM", [11] = "GSM",
                    [12] = "LTW", [13] = "WVR", [14] = "ALC", [15] = "CUL",
                    [16] = "MIN", [17] = "BTN", [18] = "FSH",
                };
                foreach (var (jobId, name) in classDict)
                {
                    bool isSelected = C.Overlay_FilterJobs.Contains(jobId);
                    var icon = isSelected
                        ? CosmicHelper.JobIconDict.TryGetValue(jobId, out var tex) ? tex.GetWrapOrEmpty() : null
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

            bool disableHudClipping = C.DisableHudClipping;
            if (ImGui.Checkbox("Disable HUD Clipping", ref disableHudClipping))
            {
                C.DisableHudClipping = disableHudClipping;
                C.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("When enabled, overlays will render over the native UI elements");
            }

        }

        private static void AutoUse()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.PersonRays, "Auto-Use");
            ImGui.Dummy(new Vector2(0, 5));

            bool DisableLunarAura = C.RemoveStellarStatus;
            if (ImGui.Checkbox("Auto-Remove Stellar Status", ref DisableLunarAura))
            {
                C.RemoveStellarStatus = DisableLunarAura;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                                   "Automatically removes the Star Contributor visual effect (the glow you get for being a top contributor).\n" +
                                   "The buff restores itself when you re-enter the zone.");

            bool autoStartOnMoonEnter = C.StartUponEnterMoon;
            if (ImGui.Checkbox("Auto start upon entering a Cosmic Exploration area", ref autoStartOnMoonEnter))
            {
                C.StartUponEnterMoon = autoStartOnMoonEnter;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                                   "This will check to see if you're on a gathering/crafting class upon first entering the moon.\n" +
                                   "If you are, it will automatically start as if you had pressed the start button yourself\n" +
                                   "Really useful if you have a tool to auto-log you in/if you just want to enter the moon and go\n" +
                                   "This will ONLY run upon first entry.");
            ImGui.Dummy(Vector2.Zero);
        }

        private static void GoldMissionRemover()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Medal, "Post Mission Settings");

            bool removeGold = C.RemoveAfterGold;
            if (ImGui.Checkbox("Remove Mission Upon Gold Completion", ref removeGold))
            {
                C.RemoveAfterGold = removeGold;
                C.Save();
            }

            using (ImRaii.Disabled(!removeGold))
            {
                bool keepARanks = C.KeepARanks;
                if (ImGui.Checkbox("Keep \"A Rank\" missions and below", ref keepARanks))
                {
                    C.KeepARanks = keepARanks;
                    C.Save();
                }
            }
        }

        private static void TimeRecords()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Clock, "Record Settings");
            ImGui.Dummy(new Vector2(0, 5));

            int TimeHistory = C.TimeHistoryLimit;
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("Average Time History to keep", ref TimeHistory))
            {
                C.TimeHistoryLimit = TimeHistory;
                C.Save();
            }
            ImGui.SameLine();
            ImGui.TextDisabled("?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Anything below 0 to keep all logs\n" +
                                 "Above 0 to keep a set limit");
            }
        }

        private static void PostMissionCommands()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Play, "Post Mission Commands");
            ImGui.Dummy(new Vector2(0, 5));

            ImGui.TextWrapped("Input below a list of commands that you would like to run after a run has been completed. \n" +
                              "This is kind of my way of letting you somewhat script/set up a sequence of other things that you would like to do that might not be included in the plugin itself. \n" +
                              "If you want something more complex, just make an SND script at that point. And have this run that script post lol.");

            if (ImGui.Button("Add New Command"))
            {
                C.PostMissionCommands.Add(new MissionCommand
                {
                    command = "",
                    Delay = 0,
                });
                C.Save();
            }

            MissionCommand? toRemove = null;
            int entryCounter = 0;

            if (ImGui.BeginTable("Mission Commands", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Command");
                ImGui.TableSetupColumn("Delay");
                ImGui.TableSetupColumn("Remove");

                ImGui.TableHeadersRow();

                foreach (var entry in C.PostMissionCommands)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.SetNextItemWidth(200);

                    ImGui.PushID($"{entryCounter}_MissionCommand");
                    string command = entry.command;
                    if (ImGui.InputText("##Command", ref command))
                    {
                        entry.command = command;
                        C.SaveDebounced();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(100);
                    int delay = entry.Delay;
                    if (ImGui.InputInt("###Delay", ref delay))
                    {
                        entry.Delay = delay;
                        C.SaveDebounced();
                    }

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"remove{C.PostMissionCommands.IndexOf(entry)}"))
                    {
                        toRemove = entry;
                    }
                    ImGui.PopID();
                    entryCounter += 1;
                }

                if (toRemove != null)
                {
                    C.PostMissionCommands.Remove(toRemove);
                    C.Save();
                }

                ImGui.EndTable();
            }
        }

        private static void Separator()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }
    }
}
