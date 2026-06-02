using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi;
using ICE.Ui.MainUi.HelpFolder;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.Settings;
using ICE.Ui.MainUi.Settings.Settings_Table;
using System.Collections.Generic;
using System.Reflection;
using static ICE.Localization.L10n;

namespace ICE.Ui
{
    internal class MainWindow : Window
    {
        public MainWindow() :
#if DEBUG
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} [Debug Build] ###ICEMainWindow2")
#else
        base($"Ice's Cosmic Exploration {P.GetType().Assembly.GetName().Version} ###ICEMainWindow2")
#endif
        {
            Flags = ImGuiWindowFlags.NoScrollbar;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500),
                MaximumSize = new Vector2(4000, 4000),
            };
            TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip(T("♥ Ko-fi (Buy me an ice coffee)")), Icon = FontAwesomeIcon.Heart, IconOffset = new(1, 1), Click = _ => GenericHelpers.ShellStart("https://ko-fi.com/ice643269") });

            P.windowSystem.AddWindow(this);

            AllowPinning = true;
            AllowClickthrough = true;
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void Draw()
        {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 10).Push(ImGuiStyleVar.ChildBorderSize, 1);

            SelectableSidebar.Draw();

            ImGui.SameLine(0, 5);

            var windowSizeRemaining = ImGui.GetContentRegionAvail();
            using (var mainBody = ImRaii.Child("mainBody_WindowV3", windowSizeRemaining, true))
            {
                if (!mainBody.Success) return;
                MainBody();
            }
        }

        private static readonly Dictionary<WindowSelection, Action> SelectedView = new()
        {
            // Cosmic Helper
            [WindowSelection.MissionSetup] = () =>  Mission_Setup.Draw(),
            [WindowSelection.CosmicAgenda] = () => Cosmic_Agenda.Draw(),
            [WindowSelection.ExpeditionLogs] = () => Expedition_Log.Draw(),

            // Settings
            [WindowSelection.StopWhen] = () => StopWhen.Draw(),
            [WindowSelection.GatheringProfiles] = () => GatherSettings.Draw(),
            [WindowSelection.MissionPriority] = () => Priority_Settings.Draw(),
            [WindowSelection.CharacterSettings] = () => Character_Settings.Draw(),
            [WindowSelection.MiscSettings] = () => Misc_Settings.Draw(),
            [WindowSelection.TravelSettings] = () => TravelSettings.Draw(),

            // Hub Activities
            [WindowSelection.CreditShopping] = () => ShoppingTab.Draw(),
            [WindowSelection.GambaShopping] = () => GambaWheel.Draw(),
            [WindowSelection.DroneShopping] = () => Shop_Dronebit.Draw(),

            // Help Section
            [WindowSelection.Plugin_Install] = () => helpSelect_Required.Draw(),
            [WindowSelection.Plugin_Logs] = () => helpSelect_Logs.Draw_Helper(),
            [WindowSelection.Plugin_Tips] = () => helpSelect_Tips.Draw(),
        };

        private static void MainBody()
        {
            var selectedWindow = C.SelectedTab;

            if (SelectedView.TryGetValue(selectedWindow, out var drawAction))
            {
                drawAction();
            }
            else
            {
                ImGui.Text(T("Hehe"));
            }
        }

        public static void ModeSelection()
        {
            bool standard = C.SelectedMode == ModeSelect.Standard;
            bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
            bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
            bool goldMode = C.SelectedMode == ModeSelect.MissionGoldMode;
            bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;

            ImGui.Text(T("Select Mode"));
            ImGui.Separator();

            if (ImGui.RadioButton(T("Standard"), standard))
            {
                C.SelectedMode = ModeSelect.Standard;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.Standard));
            if (ImGui.RadioButton(T("Relic Grind"), relicMode))
            {
                C.SelectedMode = ModeSelect.RelicMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.RelicMode));

            if (ImGui.RadioButton(T("Leveling Grind"), xpLeveling))
            {
                C.SelectedMode = ModeSelect.LevelMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.LevelMode));
            if (ImGui.RadioButton(T("Gold Completion Grind"), goldMode))
            {
                C.SelectedMode = ModeSelect.MissionGoldMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.MissionGoldMode));
            if (ImGui.RadioButton(T("Agenda Mode"), agendaMode))
            {
                C.SelectedMode = ModeSelect.AgendaMode;
                C.Save();
            }
            ImGuiEx.HelpMarker(HelpInfoText(ModeSelect.AgendaMode));
        }
        public static string HelpInfoText(ModeSelect mode)
        {
            return mode switch
            {
                ModeSelect.Standard =>
                    "Stand Mode \n" +
                    "-> Used to select which missions you want to grind. It'll priortize in the following order:\n" +
                    "-> Critical -> Provisional [Sequence/Timed/Weather] -> Standard [A->D]\n" +
                    "-> Select which missions you want to do, and go at it.",
                ModeSelect.LevelMode =>
                    "Leveling Grind\n" +
                    "-> Will automatically select which mission is the best for leveling your current class based on what level bracket you're in\n" +
                    "-> These are hand picked by me, and determined by the time it takes to complete it\n" +
                    "-> For crafters it's whatever missions take the least amount of progress" +
                    "-> For gathering, it's whatever is the least pain to do w/ the minimum amount of skills\n" +
                    "**These will automatically set settings for using these modes temporarily**",
                ModeSelect.RelicMode =>
                    "Relic Grind\n" +
                    "-> Automatically select which missions that are best to finish up your relic\n" +
                    "-> These are weighed based on what is needed to complete the tool to the next step\n" +
                    "-> If you want to only do certain missions, enable the option and select which ones you want to do",
                ModeSelect.AgendaMode =>
                    "This mode is if you want to do a series of things in a particular order. So for example, if you wanted to grind out all the relics on all the classes back to back\n" +
                    "Or if you wanted to do the relic on WVR -> Then farm score on BTN -> Farm credits on BSM\n" +
                    "Really is the \"I want to do this order of things\" kind of thing.\n" +
                    "Note. I'm not responsible if you leave this on and get banned for it. I'm not one for leaving things at their pc, but people are watching always. Keep this in mind",
                ModeSelect.MissionGoldMode =>
                    "Gold Completion Mode\n" +
                    "-> Will automatically pick all the missions that you do not have currently gold, AND ONLY THOSE MISSIONS.\n" +
                    "-> If it is apart of a sequence chain, it will grab the mission that are needed previously to help complete it, and the missions post if necessary\n" +
                    "-> If it runs out of missions to reroll, it will just continually swap tabs until the mission is available (via provisional or critical)\n" +
                    "**This will respect the want to grind off class provisionals, and criticals if you have those enabled",
                _ => "???? For some reason we're missing this. Please Report this to I"
            };
        }
    }
}
