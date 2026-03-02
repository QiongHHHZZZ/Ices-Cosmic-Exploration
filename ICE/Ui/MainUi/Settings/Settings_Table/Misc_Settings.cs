using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Utilities.ImGuiTools;
using Lumina.Excel.Sheets;
using Pictomancy;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;

namespace ICE.Ui.MainUi.Settings.Settings_Table
{
    internal class Misc_Settings
    {
        public static void Draw()
        {
            OverlaySettings();
            Separator();
            AutoUse();
            Separator();
            RepairSettings();
            Separator();
            SafetySettings.Draw();
            Separator();
            ArtisanSettingsV2();
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

        private static void RepairSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Hammer, "Repair Settings");
            ImGui.Dummy(new Vector2(0, 5));

            bool repairAtVendor = C.RepairAtVendor;
            if (ImGui.Checkbox("Repair at Vendor", ref repairAtVendor))
            {
                C.RepairAtVendor = repairAtVendor;
                C.Save();
            }

            using (ImRaii.Disabled(repairAtVendor))
            {
                bool selfRepairGather = C.SelfRepairGather;
                if (ImGui.Checkbox("Self Repair Gather", ref selfRepairGather))
                {
                    C.SelfRepairGather = selfRepairGather;
                    C.Save();
                }

                bool selfRepairCrafter = C.SelfRepairCrafter;
                if (ImGui.Checkbox("Self Repair Crafter", ref selfRepairCrafter))
                {
                    C.SelfRepairCrafter= selfRepairCrafter;
                    C.Save();
                }
            }

            float repairAmount = C.RepairPercent;
            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderFloat("###Repair %", ref repairAmount, 0f, 99f, "%.0f%%"))
            {
                if (C.RepairPercent != repairAmount)
                {
                    C.RepairPercent = (int)repairAmount;
                    C.SaveDebounced();
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

        private static void ArtisanSettings()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Wrench, "Global Artisan Settings");
            ImGui.Dummy(new Vector2(0, 5));

            bool force_Raphael = C.Artisan_RaphaelForce;
            bool expertRaphael = C.Artisan_RaphaelMaster;

            if (ImGui.Checkbox("Enforce Raphael Solver", ref force_Raphael))
            {
                C.Artisan_RaphaelForce = force_Raphael;
                C.Save();
            }
            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"Will force all crafts while the plugin is running to use the raphael solver.");
                ImGui.Text($"This excludes the expert solver crafts due to their nature of how they function");
                ImGui.EndTooltip();
            }
            if (force_Raphael)
            {
                if (ImGui.Checkbox("Use Raphael Solver on Expert Recipe", ref expertRaphael))
                {
                    C.Artisan_RaphaelMaster = expertRaphael;
                    C.Save();
                }
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"Will force crafts that would normally use the Expert Solver to instead use Raphael.");
                    ImGuiEx.Icon(new Vector4(1.0f, 0.4f, 0.0f, 1.0f), FontAwesomeIcon.Diamond);
                    ImGui.SameLine();
                    ImGui.Text($"This is the icon within the recipe details btw");
                    ImGui.Text($"I would not recommend this on Oizys, it's not perfect and has been causing a lot of issues for peeps.");
                    ImGui.EndTooltip();
                }
            }

            ImGui.TextDisabled("More Coming Soon. . . ");
        }

        private static void ArtisanSettingsV2()
        {
            ImGuiEx.IconWithText(FontAwesomeIcon.Wrench, "Global Artisan Settings");
            ImGui.Dummy(new Vector2(0, 5));

            List<ArtisanCraftType> global_StandardModes = new()
            {
                ArtisanCraftType.Default,
                ArtisanCraftType.ProgressOnly,
                ArtisanCraftType.Standard,
                ArtisanCraftType.Raphael,
            };

            List<ArtisanCraftType> global_ExpertModes = new()
            {
                ArtisanCraftType.Default,
                ArtisanCraftType.Expert,
                ArtisanCraftType.Raphael,
            };

            if (ImGui.BeginTable("Global Artisan Settings", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("");
                ImGui.TableSetupColumn("Standard Craft Settings");
                ImGui.TableSetupColumn("Expert Craft Settings");

                ImGui.TableHeadersRow();

                var craft_Standard = C.Artisan_GlobalStandard;
                var craft_Expert = C.Artisan_GlobalExpert;

                #region Labels

                string GetSolverLabel(ArtisanCraftType type)
                {
                    return type switch
                    {
                        ArtisanCraftType.Default => "Default",
                        ArtisanCraftType.Raphael => "Raphael Solver",
                        ArtisanCraftType.ProgressOnly => "Progress Only Solver",
                        ArtisanCraftType.Standard => "Standard Solver",
                        ArtisanCraftType.Expert => "Expert Recipe Solver",
                        _ => "Unknown"
                    };
                }
                string GetFoodLable(uint foodId)
                {
                    if (foodId == 0) return "Default";
                    var item = ConsumableInfo.CrafterFood.FirstOrDefault(x => x.Id == foodId);
                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                    return BuildItemLabel(item.Name, nq, hq);
                }
                string GetPotionLable(uint potionId)
                {
                    if (potionId == 0) return "Default";
                    var item = ConsumableInfo.Pots.FirstOrDefault(x => x.Id == potionId);
                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                    PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                    return BuildItemLabel(item.Name, nq, hq);
                }
                string GetManualLabel(uint manualId)
                {
                    if (manualId == 0) return "Default";
                    var item = ConsumableInfo.Manuals.FirstOrDefault(x => x.Id == manualId);
                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                    return BuildItemLabel(item.Name, nq, 0);
                }
                string GetSquadronManualLabel(uint squadManualId)
                {
                    if (squadManualId == 0) return "Default";
                    var item = ConsumableInfo.SquadronManuals.FirstOrDefault(x => x.Id == squadManualId);
                    PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                    return BuildItemLabel(item.Name, nq, 0);
                }
                string BuildItemLabel(string name, int nqCount, int hqCount)
                {
                    var parts = new List<string>();
                    if (hqCount > 0) parts.Add($"{(char)0xE03C} {name} [x{hqCount}]");
                    if (nqCount > 0) parts.Add($"{name} [x{nqCount}]");
                    return string.Join(" / ", parts);
                }

                var standard_FoodLabel = GetFoodLable(craft_Standard.FoodId);
                var Expert_FoodLabel = GetFoodLable(craft_Expert.FoodId);

                var standard_PotionLabel = GetPotionLable(craft_Standard.PotionId);
                var expert_PotionLabel = GetPotionLable(craft_Expert.PotionId);

                var standard_ManualLabel = GetManualLabel(craft_Standard.ManualId);
                var expert_ManalLabel = GetManualLabel(craft_Expert.ManualId);

                var standard_SquadManualLabel = GetSquadronManualLabel(craft_Standard.SquadronManual);
                var expert_SquadManalLabel = GetSquadronManualLabel(craft_Expert.SquadronManual);

                var standardSolver = craft_Standard.SolverType.ToString();
                var expertSolver = craft_Expert.SolverType.ToString();

                float standard_ComboWidth = new[]
                {
                    standard_FoodLabel,
                    standard_PotionLabel,
                    standard_ManualLabel,
                    standard_SquadManualLabel,
                    GetSolverLabel(craft_Standard.SolverType),
                }.Max(label => ImGui.CalcTextSize(label).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

                float expert_ComboWidth = new[]
                {
                    Expert_FoodLabel,
                    expert_PotionLabel,
                    expert_ManalLabel,
                    expert_SquadManalLabel,
                    GetSolverLabel(craft_Expert.SolverType),
                }.Max(label => ImGui.CalcTextSize(label).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

                #endregion

                #region Solvers

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Solver Type");

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(standard_ComboWidth);
                if (ImGui.BeginCombo("##StandardSolverType", GetSolverLabel(craft_Standard.SolverType)))
                {
                    foreach (var type in global_StandardModes)
                    {
                        bool isSelected = craft_Standard.SolverType == type;
                        if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                        {
                            craft_Standard.SolverType = type;
                            C.Save();
                        }
                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }

                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(expert_ComboWidth);
                if (ImGui.BeginCombo("##ExpertSolverType", GetSolverLabel(craft_Expert.SolverType)))
                {
                    foreach (var type in global_ExpertModes)
                    {
                        bool isSelected = craft_Expert.SolverType == type;
                        if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                        {
                            craft_Expert.SolverType = type;
                            C.Save();
                        }
                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }

                #endregion

                bool supportedArtisan = P.Artisan.UpdatedArtisan();

                if (supportedArtisan)
                {
                    #region Food Column

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Food");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(standard_ComboWidth);
                    if (ImGui.BeginCombo("##StandardFood", standard_FoodLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Standard.FoodId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Standard.FoodId = 0;
                            craft_Standard.FoodHQ = false;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.CrafterFood)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                            if (nqCount == 0 && hqCount == 0) continue;

                            bool isSelected = craft_Standard.FoodId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Standard.FoodId = item.Id;
                                craft_Standard.FoodHQ = hqCount > 0;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(expert_ComboWidth);
                    if (ImGui.BeginCombo("##ExpertFood", GetFoodLable(craft_Expert.FoodId)))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Expert.FoodId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Expert.FoodId = 0;
                            craft_Expert.FoodHQ = false;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.CrafterFood)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                            if (nqCount == 0 && hqCount == 0) continue;

                            bool isSelected = craft_Expert.FoodId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Expert.FoodId = item.Id;
                                craft_Expert.FoodHQ = hqCount > 0;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    #endregion

                    #region Potion Row

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Potions");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(standard_ComboWidth);
                    if (ImGui.BeginCombo("##StandardPotion", standard_PotionLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Standard.PotionId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Standard.PotionId = 0;
                            craft_Standard.PotionHQ = false;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.Pots)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                            if (nqCount == 0 && hqCount == 0) continue;

                            bool isSelected = craft_Standard.PotionId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Standard.PotionId = item.Id;
                                craft_Standard.PotionHQ = hqCount > 0;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(expert_ComboWidth);
                    if (ImGui.BeginCombo("##ExpertPotion", expert_PotionLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Expert.PotionId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Expert.PotionId = 0;
                            craft_Expert.PotionHQ = false;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.Pots)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                            if (nqCount == 0 && hqCount == 0) continue;

                            bool isSelected = craft_Expert.PotionId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Expert.PotionId = item.Id;
                                craft_Expert.PotionHQ = hqCount > 0;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    #endregion

                    #region Manual

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Manual");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(standard_ComboWidth);
                    if (ImGui.BeginCombo("##StandardManual", standard_ManualLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Standard.ManualId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Standard.ManualId = 0;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.Manuals)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                            if (nqCount == 0) continue;

                            bool isSelected = craft_Standard.ManualId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Standard.ManualId = item.Id;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(expert_ComboWidth);
                    if (ImGui.BeginCombo("##ExpertManual", expert_ManalLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Expert.ManualId == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Expert.ManualId = 0;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.Manuals)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                            if (nqCount == 0) continue;

                            bool isSelected = craft_Expert.ManualId == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Expert.ManualId = item.Id;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    #endregion

                    #region Squadron Manual

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Squadron Manual");

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(standard_ComboWidth);
                    if (ImGui.BeginCombo("##StandardSquadManual", standard_SquadManualLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Standard.SquadronManual == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Standard.SquadronManual = 0;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.SquadronManuals)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                            if (nqCount == 0) continue;

                            bool isSelected = craft_Standard.SquadronManual == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Standard.SquadronManual = item.Id;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(expert_ComboWidth);
                    if (ImGui.BeginCombo("##ExpertSquadManual", expert_SquadManalLabel))
                    {
                        // Default option
                        bool isDefaultSelected = craft_Expert.SquadronManual == 0;
                        if (ImGui.Selectable("Default", isDefaultSelected))
                        {
                            craft_Expert.SquadronManual = 0;
                            C.Save();
                        }
                        if (isDefaultSelected)
                            ImGui.SetItemDefaultFocus();

                        ImGui.Separator();

                        foreach (var item in ConsumableInfo.SquadronManuals)
                        {
                            PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                            if (nqCount == 0) continue;

                            bool isSelected = craft_Expert.SquadronManual == item.Id;
                            string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                            if (ImGui.Selectable(label, isSelected))
                            {
                                craft_Expert.SquadronManual = item.Id;
                                C.Save();
                            }

                            if (isSelected)
                                ImGui.SetItemDefaultFocus();
                        }

                        ImGui.EndCombo();
                    }

                    #endregion
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
