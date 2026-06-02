using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings;

public static class Settings_TableColumns
{
    private static string[] missionSortOptions = 
        ["Id", "Name", "Cosmo Credits", "Lunar Credits", 
        "Exp I", "Exp II", "Exp III", "Exp IV", "Exp V", 
        "Map Location", "Class Score", "Class Exp"];

    public static void ColumnSettings()
    {
        int missionSelectedOption = C.TableSortOption;
 if (ImGui.BeginCombo(T("Sort By"), T(missionSortOptions[missionSelectedOption])))
        {
            for (int i = 0; i < missionSortOptions.Length; i++)
            {
                bool isSelected = (i == missionSelectedOption);
                if (ImGui.Selectable(T(missionSortOptions[i]), isSelected))
                {
                    missionSelectedOption = i;
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
                if (missionSelectedOption != C.TableSortOption)
                {
                    C.TableSortOption = missionSelectedOption;
                    C.Save();
                }
            }
            ImGui.EndCombo();
        }

        bool hideUnsupported = C.HideUnsupportedMissions;
if (ImGui.Checkbox(T("Hide Unsupported Missions"), ref hideUnsupported))
        {
            C.HideUnsupportedMissions = hideUnsupported;
            C.Save();
        }

        bool grindAllProvisionals = C.GrindAllProvisionals;
if (ImGui.Checkbox(T("Allow All Provisional Kinds"), ref grindAllProvisionals))
        {
            C.GrindAllProvisionals = grindAllProvisionals;
            C.Save();
        }
        ImGuiEx.HelpMarker(T(
            "Enabling this will show you all weather/timed/sequence missions that you can grind, \n" +
            "ON TOP OF doing the normal missions for whichever class you start on.\n" +
            "If you just want to focus one specific class, set this to false\n" +
            "Do note: this replaced provisional grinding, due to just being built into the standard mode now (finally)"));

        bool autoShowToken = C.Auto_ShowTokens;
if (ImGui.Checkbox(T("Auto Hide/Show Planet Tokens"), ref autoShowToken))
        {
            C.Auto_ShowTokens = autoShowToken;
            C.Save();
        }

        bool allowCriticalsAllClass = C.GrindOffClassRedAlert;
        if (ImGui.Checkbox(T("Allow Criticals for all Classes"), ref allowCriticalsAllClass))
        {
            C.GrindOffClassRedAlert = allowCriticalsAllClass;
            C.Save();
        }
        ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.InfoCircle,
            T("This will allow you to grind other classes for criticals/red alerts. (So if you're on crp, but a bsm red alert pops up)"));

        bool showManualMode = C.ShowManualMode;
        if (!showManualMode)
        {
            using (ImRaii.Disabled(!(ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))))
            {
                if (ImGui.Checkbox(T("Show Manual Mode Column"), ref showManualMode))
                {
                    C.ShowManualMode = showManualMode;
                    if (!showManualMode)
                    {
                        foreach (var mission in C.MissionConfig)
                            mission.Value.ManualMode = false;
                    }
                    C.Save();
                }
            }
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.BeginTooltip();
                ImGui.Text(T("MAKE SURE TO READ THE INFO ON THE RIGHT !"));
                ImGui.Text(T("If you've done so, you can hold shift to allow enabling this"));
                ImGui.EndTooltip();
            }
        }
        else
        {
            if (ImGui.Checkbox(T("Show Manual Mode Column"), ref showManualMode))
            {
                C.ShowManualMode = showManualMode;
                if (!showManualMode)
                {
                    foreach (var mission in C.MissionConfig)
                        mission.Value.ManualMode = false;
                }
                C.Save();
            }
        }

        ImGuiEx.HelpMarker(T("Only enable this if you want plan on doing missions YOURSELF. AND NOT AUTOMATING IT. " +
                             "Or if you're letting a different plugin do all the automating of turning in, craftings, gathering... and not letting I.C.E. handle interacting with those plugins"));
    }

    private static bool ApplyToAllClasses = true;
    private static bool ApplyToSpecicClass = false;
    private static int SpecificClass = 8;
    private static int selectedClassIndex = 0;

    private static readonly string[] classOptionsKeys =
    {
        "Carpenter",      // 0
        "Blacksmith",     // 1
        "Armorer",        // 2
        "Goldsmith",      // 3
        "Leatherworker",  // 4
        "Weaver",         // 5
        "Alchemist",      // 6
        "Culinarian",     // 7
        "Miner",          // 8
        "Botanist",       // 9
        "Fisher"          // 10
    };

    private static string[] classOptionsDisplay = [];
    private static bool classOptionsDisplayIsZh;

    private static string[] GetClassOptionsDisplay()
    {
        // Rebuild only when language toggle flips.
        bool isZh = C.UseChineseUi;
        if (classOptionsDisplay.Length == 0 || classOptionsDisplayIsZh != isZh)
        {
            classOptionsDisplayIsZh = isZh;
            classOptionsDisplay = new string[classOptionsKeys.Length];
            for (int i = 0; i < classOptionsKeys.Length; i++)
                classOptionsDisplay[i] = T(classOptionsKeys[i]);
        }

        return classOptionsDisplay;
    }

    private static readonly int[] classIds = new[]
    {
        8,  // Carpenter
        9,  // Blacksmith
        10, // Armorer
        11, // Goldsmith
        12, // Leatherworker
        13, // Weaver
        14, // Alchemist
        15, // Culinarian
        16, // Miner
        17, // Botanist
        18  // Fisher
    };

    private static TurninState HighestTurnin = TurninState.Gold;

    private static bool AnyTurnin = true;
    private static bool TurninGold = false;
    private static bool TurninSilver = false;
    private static bool TurninBronze = false;

    public static void GeneralMissionSettings()
    {
        bool removeGold = C.RemoveAfterGold;
if (ImGui.Checkbox(T("Remove Mission Upon Gold Completion"), ref removeGold))
        {
            C.RemoveAfterGold = removeGold;
            C.Save();
        }
        using (ImRaii.Disabled(!removeGold))
        {
            bool keepARanks = C.KeepARanks;
            if (ImGui.Checkbox(T("Keep \"A Rank\" missions and below"), ref keepARanks))
            {
                C.KeepARanks = keepARanks;
                C.Save();
            }
        }

ImGui.Checkbox(T("Stop after current mission"), ref Mission_Settings.StopAfterCurrent);
        bool relicTurnin = C.TurninRelic;
        if (ImGui.Checkbox(T("Turnin if relic is complete##RelicTurnin_GeneralSetting"), ref relicTurnin))
        {
            C.TurninRelic = relicTurnin;
            C.Save();
        }
        ImGui.SameLine();
        ImGui.TextDisabled("?");
        if (ImGui.IsItemHovered())
        {
 ImGui.SetTooltip(
     T("THIS IS YOUR HEADS UP ON HOW THIS WORKS. If I change this in the future, this tooltip will also change.\n") +
     T("1: This will check for your current CLASS [not menu class, actual current class] for relic turnin.\n") +
     T("2: You must not have the tool eqipped for this to run full auto. \n") +
     T("\t- This is due to the fact that I cba coding this in at this time. (might change my mind in the future *shrugs*)\n") +
     T("3: This will take prio over \"Stop @ Relic Turnin\", in the sense that if you have both enabled, it will turnin vs stop. And continue about it's day\n") +
     T("4: If you're on a crafting class, it will return you back to the stop you were crafting post turnin. \n") +
     T("\t- This is optional, you can disable it at your own free will, I just like this so I can just go back to an isolated area of my choosing"));
        }
if (ImGui.Button(T("Quick Apply Turnins")))
        {
            ImGui.OpenPopup("Quick Apply_Mission Turnins");
        }

        if (ImGui.BeginPopup("Quick Apply_Mission Turnins"))
        {
if (ImGui.RadioButton(T("Apply to all classes"), ApplyToAllClasses))
            {
                ApplyToAllClasses = true;
                ApplyToSpecicClass = false;
            }

if (ImGui.RadioButton(T("Apply to specific class"), ApplyToSpecicClass))
            {
                ApplyToAllClasses = false;
                ApplyToSpecicClass = true;
            }
            var classOptions = GetClassOptionsDisplay();
            if (ImGui.Combo("##ClassSelector", ref selectedClassIndex, classOptions, classOptions.Length))
            {
                // Update SpecificClass when selection changes
                SpecificClass = classIds[selectedClassIndex];
                IceLogging.Debug($"Selected class: {classOptions[selectedClassIndex]}, ID: {SpecificClass}");
            }
            ImGui.Separator();
ImGui.Text(T("Select Turnin Options"));
            ImGui.Dummy(new Vector2(0, 2));

            if (ImGui.RadioButton(T("Gold"), HighestTurnin is TurninState.Gold))
            {
                HighestTurnin = TurninState.Gold;
            }
            if (ImGui.RadioButton(T("Silver"), HighestTurnin is TurninState.Silver))
            {
                HighestTurnin = TurninState.Silver;
            }
            if (ImGui.RadioButton(T("Bronze"), HighestTurnin is TurninState.Bronze))
            {
                HighestTurnin = TurninState.Bronze;
            }

            ImGui.Separator();

if (ImGui.Button(T("Apply")))
            {
                var amountApplied = 0;
                foreach (var mission in C.MissionConfig)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var sheetInfo))
                    {
                        if (ApplyToSpecicClass && !sheetInfo.Jobs.Contains((uint)SpecificClass))
                            continue;

                        if (sheetInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining))
                            continue;

                        if (C.MissionConfig.TryGetValue(mission.Key, out var config))
                        {
                            config.TurninGoal = HighestTurnin;
                        }
                        amountApplied += 1;
                    }
                }
                C.SaveDebounced();

                Notify.Success(T("Applied settings to: {0} missions.", amountApplied));
                ImGui.CloseCurrentPopup();
            }


            ImGui.EndPopup();
        }
    }
}
