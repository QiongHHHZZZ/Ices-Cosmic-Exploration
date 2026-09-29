using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.ImGuiTools;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    private const string AutoUseCategory = "Auto-Use";
    private const string PostMissionCategory = "Post Mission Settings";
    private const string RecordCategory = "Record Settings";
    private const string PostCommandsCategory = "Post Mission Commands";
    private const string FunCategory = "Dev Favorites";
    private const string NotificationCategory = "Notification Master";

    #region Auto Use

    private static readonly SettingEntry AutoUse_RemoveStellar = new()
    {
        Label = T("Auto-Remove Stellar Status"),
        Category = AutoUseCategory,
        Keywords = new[] { "Stellar", "Status", "Remove", "Star Contributor", "Glow", "Buff" },
        Draw = () =>
        {
            var removeStellar = C.RemoveStellarStatus;
            if (ImGui.Checkbox(T("Auto-Remove Stellar Status"), ref removeStellar))
            {
                C.RemoveStellarStatus = removeStellar;
                C.Save();
            }

            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.InfoCircle,
                T("Automatically removes the Star Contributor visual effect (the glow you get for being a top contributor).\nThe buff restores itself when you re-enter the zone."));
        }
    };

    private static readonly SettingEntry AutoUse_StartOnMoon = new()
    {
        Label = T("Auto start upon entering a Cosmic Exploration area"),
        Category = AutoUseCategory,
        Keywords = new[] { "Auto", "Start", "Enter", "Moon", "Cosmic Exploration", "Zone" },
        Draw = () =>
        {
            var startOnMoon = C.StartUponEnterMoon;
            if (ImGui.Checkbox(T("Auto start upon entering a Cosmic Exploration area"), ref startOnMoon))
            {
                C.StartUponEnterMoon = startOnMoon;
                C.Save();
            }

            ImGui.SameLine();
            ImGuiEx.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                T("This will check to see if you're on a gathering/crafting class upon first entering the moon.\n") +
                T("If you are, it will automatically start as if you had pressed the start button yourself\n") +
                T("Really useful if you have a tool to auto-log you in/if you just want to enter the moon and go\n") +
                T("This will ONLY run upon first entry."));
        }
    };

    #endregion

    #region Post Mission Settings

    private static readonly SettingEntry Post_GoldRemover = new()
    {
        Label = T("Remove Mission Upon Gold Completion"),
        Category = PostMissionCategory,
        Keywords = new[] { "Remove", "Mission", "Gold", "Completion", "A Rank", "Keep" },
        Draw = () =>
        {
            var removeGold = C.RemoveAfterGold;
            if (ImGui.Checkbox(T("Remove Mission Upon Gold Completion"), ref removeGold))
            {
                C.RemoveAfterGold = removeGold;
                C.Save();
            }

            using (ImRaii.Disabled(!removeGold))
            {
                var keepARanks = C.KeepARanks;
                if (ImGui.Checkbox(T("Keep \"A Rank\" missions and below"), ref keepARanks))
                {
                    C.KeepARanks = keepARanks;
                    C.Save();
                }
            }
        }
    };

    #endregion

    #region Record Settings

    private static readonly SettingEntry Record_TimeHistory = new()
    {
        Label = T("Average Time History to keep"),
        Category = RecordCategory,
        Keywords = new[] { "Time", "History", "Average", "Records", "Logs", "Limit" },
        Draw = () =>
        {
            var timeHistory = C.TimeHistoryLimit;
            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt(T("Average Time History to keep"), ref timeHistory))
            {
                C.TimeHistoryLimit = timeHistory;
                C.Save();
            }

            ImGui.SameLine();
            ImGui.TextDisabled("?");
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(T("Anything below 0 to keep all logs\n") +
                                 T("Above 0 to keep a set limit"));
            }
        }
    };

    #endregion

    #region Post Mission Commands

    private static readonly SettingEntry Post_Commands = new()
    {
        Label = T("Post Mission Commands"),
        Category = PostCommandsCategory,
        Keywords = new[] { "Post", "Mission", "Commands", "Script", "Delay", "SND", "Macro" },
        Draw = DrawPostMissionCommands
    };

    private static void DrawPostMissionCommands()
    {
        ImGui.TextWrapped(T("Input below a list of commands that you would like to run after a run has been completed. \n") +
                          T("This is kind of my way of letting you somewhat script/set up a sequence of other things that you would like to do that might not be included in the plugin itself. \n") +
                          T("If you want something more complex, just make an SND script at that point. And have this run that script post lol."));

        if (ImGui.Button(T("Add New Command")))
        {
            C.PostMissionCommands.Add(new MissionCommand
            {
                command = "",
                Delay = 0,
            });
            C.Save();
        }

        if (!ImGui.BeginTable("Mission Commands", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            return;

        ImGui.TableSetupColumn(T("Command"));
        ImGui.TableSetupColumn(T("Delay"));
        ImGui.TableSetupColumn(T("Remove"));
        ImGui.TableHeadersRow();

        MissionCommand? toRemove = null;
        var entryCounter = 0;

        foreach (var entry in C.PostMissionCommands)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.SetNextItemWidth(200);

            ImGui.PushID($"{entryCounter}_MissionCommand");

            var command = entry.command;
            if (ImGui.InputText("##Command", ref command))
            {
                entry.command = command;
                C.SaveDebounced();
            }

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(100);
            var delay = entry.Delay;
            if (ImGui.InputInt("###Delay", ref delay))
            {
                entry.Delay = delay;
                C.SaveDebounced();
            }

            ImGui.TableNextColumn();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"remove{entryCounter}"))
                toRemove = entry;

            ImGui.PopID();
            entryCounter++;
        }

        if (toRemove != null)
        {
            C.PostMissionCommands.Remove(toRemove);
            C.Save();
        }

        ImGui.EndTable();
    }

    #endregion

    #region Dev Favorites

    private static readonly SettingEntry Fun_CrazyTaxi = Toggle(
        T("Show Crazy Taxi Arrow when navmeshing"), FunCategory,
        new[] { "Crazy Taxi", "Arrow", "Navmesh", "Fun" },
        () => C.CrazyTaxiArrow,
        v => C.CrazyTaxiArrow = v);

    private static readonly SettingEntry Fun_Placebo = new()
    {
        Label = T("Increase Gathering & Crafting Speed"),
        Category = FunCategory,
        Keywords = new[] { "Gathering", "Crafting", "Speed", "Placebo", "Fun" },
        Draw = () =>
        {
            var placebo = C.PlaceboCheckbox;
            if (ImGui.Checkbox(T("Increase Gathering & Crafting Speed"), ref placebo))
            {
                C.PlaceboCheckbox = placebo;
                C.Save();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                T("This does abosolutely nothing\n") +
                T("But I know there's going to be people who enable this and don't read, so it's a tehe.\n") +
                T("Thanks for using my plugin though, it means a lot <3"), false);
        }
    };

    private static readonly SettingEntry Fun_FakeFisher = new()
    {
        Label = T("Increase Fishing Speed"),
        Category = FunCategory,
        Keywords = new[] { "Fishing", "Speed", "Clown", "Glamourer", "Fun", "Joke" },
        Draw = () =>
        {
            var fakeFishing = C.FakeIncreaseFisher;
            if (ImGui.Checkbox(T("Increase Fishing Speed"), ref fakeFishing))
            {
                C.FakeIncreaseFisher = fakeFishing;
                C.SaveDebounced();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                T("This is your warning, this will just apply a clown head to you every minute or so from glamourer.\n") +
                T("100% a joke setting, don't take it seriously. I don't have the technology for this"), false);
        }
    };

    #endregion

    #region Notification Master IPC

    private static readonly SettingEntry Notification_Foreground = new()
    {
        Label = T("Notification Master: Foreground"),
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Foreground" },
        Draw = () =>
        {
            var v = C.Notification_Foreground;
            if (ImGui.Checkbox(T("Bring game to foreground"), ref v))
            {
                C.Notification_Foreground = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, T("Need to install Notification Master"), false);
            }
        }
    };

    private static readonly SettingEntry Notification_Toast = new()
    {
        Label = T("Notification Master: Toast"),
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Toast", "Notification" },
        Draw = () =>
        {
            var v = C.Notification_Toast;
            if (ImGui.Checkbox(T("Send a toast message when stopped"), ref v))
            {
                C.Notification_Toast = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, T("Need to install Notification Master"), false);
            }
        }
    };

    private static readonly SettingEntry Notification_Flash = new()
    {
        Label = T("Notification Master: Flash"),
        Category = NotificationCategory,
        Keywords = new[] { "Notification", "Master", "Notification Master", "Flash", "Notification" },
        Draw = () =>
        {
            var v = C.Notification_FlashTaskbar;
            if (ImGui.Checkbox(T("Flash the icon on your bar"), ref v))
            {
                C.Notification_FlashTaskbar = v;
                C.Save();
            }
            if (!P.NotificationIPC.Installed)
            {
                ImGui.SameLine();
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.ExclamationTriangle, T("Need to install Notification Master"), false);
            }
        }
    };

    #endregion
}

