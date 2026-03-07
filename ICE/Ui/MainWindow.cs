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
using ICE.Ui.SettingTabs;
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

        private static readonly Dictionary<string, Action> SelectedView = new()
        {
            // Cosmic Helper
            ["modeSelect_MissionSetup"] = () =>
            {
                if (C.ShowCompletionWindow)
                {
                    C.ShowCompletionWindow = false;
                    C.Save();
                }
                modeSelect_Standard.Draw();
            },
            ["modeSelect_Completion"] = () =>
            {
                if (!C.ShowCompletionWindow)
                {
                    C.ShowCompletionWindow = true;
                    C.Save();
                }
                modeSelect_Standard.Draw();
            },
            ["modeSelect_CosmicAgenda"] = () => modeSelect_Agenda.Draw(),

            // Settings
            ["setting_StopWhen"] = () => StopWhen.Draw(),
            ["setting_GatheringProfile"] = () => GatherSettings.Draw(),
            ["setting_MissionPriority"] = () => Priority_Settings.Draw(),
            ["setting_Misc"] = () => Misc_Settings.Draw(),
            ["setting_Travel"] = () => TravelSettings.Draw(),

            // Hub Activities
            ["hubActivities_CreditShopping"] = () => ShoppingTab.Draw(),
            ["hubActivites_GambaSetting"] = () => GambaWheel.Draw(),
            ["hubActivies_DroneSetting"] = () => Shop_Dronebit.Draw(),

            // Help Section
            ["help_PluginInstall"] = () => helpSelect_Required.Draw(),
            ["help_PluginLogs"] = () => helpSelect_Logs.Draw_Helper(),
        };

        private static void MainBody()
        {
            var selectedWindow = C.MainUi_SelectedWindow;

            if (SelectedView.TryGetValue(selectedWindow, out var drawAction))
            {
                drawAction();
            }
            else
            {
                ImGui.Text(T("Hehe"));
            }
        }
    }
}
