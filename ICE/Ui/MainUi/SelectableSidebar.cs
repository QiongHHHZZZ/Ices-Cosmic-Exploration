using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Utilities.ImGuiTools;
using SharpDX.Direct2D1.Effects;
using System.Collections.Generic;
using System.Reflection;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi
{
    internal class SelectableSidebar
    {
        public static void Draw()
        {
            var scale = ImGuiHelpers.GlobalScaleSafe;
            int baseSize = 200;
            var scaledWidth = baseSize * scale;
            var height = ImGui.GetContentRegionAvail().Y;

            using (var MainUi_Sidebar = ImRaii.Child("MainUi_Sidebar", new Vector2(scaledWidth, height), true))
            {
                PluginIcon();

                // this is here to be running globally while window is loaded vs only while expanded
                bool autoSelectMoon = C.AutoSelectMoon;
                AutoSelectMoonUpdate(autoSelectMoon);

                bool autoSelectedJob = C.AutoPickCurrentJob;
                AutoSelectClass(autoSelectedJob);

                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Cosmic Helper"), icon: FontAwesomeIcon.ListAlt))
                {
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.List, T("Mission Setup"), "modeSelect_MissionSetup");
                    // ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Trophy, "Complete Overview", "modeSelect_Completion");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.ClipboardList, T("Cosmic Agenda"), "modeSelect_CosmicAgenda");
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Planet Selection"), FontAwesomeIcon.Moon))
                {
                    if (ImGui_Ice.SliderButton("AutoSelectMoon", T("Auto Select"), ref autoSelectMoon))
                    {
                        C.AutoSelectMoon = autoSelectMoon;
                        C.Save();
                    }
                    ImGui.PushFont(UiBuilder.IconFont);
                    var infoIconWidth = ImGui.CalcTextSize(FontAwesomeIcon.InfoCircle.ToIconString()).X;
                    ImGui.PopFont();
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X + ImGui.GetCursorPosX() - infoIconWidth);
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
                    ImGui.PopFont();
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
ImGui.Text(T("Filters which planets appear in the\nmission list and the overlay."));
                        ImGui.EndTooltip();
                    }
                    ImGui.Dummy(new(0, 3));

                    float iconSize = 26 * scale;
                    float iconSpacing = 4;
                    float leftOffset = 10f; // Simple offset from the current position

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + leftOffset);

                    var moons = new (string Name, string Asset, Func<bool> GetEnabled, Action<bool> SetEnabled)[]
                    {
                            ("Sinus Ardorum", "ICE.Resources.Sinus_Ardorum.png", () => C.ShowSinusMissions, val => C.ShowSinusMissions = val),
                            ("Phaenna", "ICE.Resources.Phaenna.png", () => C.ShowPhaennaMissions, val => C.ShowPhaennaMissions = val),
                            ("Oizys", "ICE.Resources.Oizys.png", () => C.ShowOizysMissions, val => C.ShowOizysMissions = val)
                    };

                    for (int i = 0; i < moons.Length; i++)
                    {
                        if (i > 0) ImGui.SameLine(0, iconSpacing);

                        var moon = moons[i];
                        bool isEnabled = moon.GetEnabled();
                        var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), moon.Asset).GetWrapOrEmpty();

                        if (ImGui_Ice.DrawStyledImageButton(texture, new Vector2(iconSize, iconSize), isEnabled))
                        {
                            moon.SetEnabled(!isEnabled);
                            C.AutoSelectMoon = false;
                            C.Save();
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(T(moon.Name));
                        }
                    }
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Hub Activities"), icon: FontAwesomeIcon.Home))
                {
                    ImGui_Ice.DrawSelectable_Image(65112, T("Credit Shopping"), "hubActivities_CreditShopping");
                    ImGui_Ice.DrawSelectable_Image(65127, T("Gambling Settings"), "hubActivites_GambaSetting");
                    ImGui_Ice.DrawSelectable_Image(65138, T("Dronebit Settings"), "hubActivies_DroneSetting");
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Settings"), icon: FontAwesomeIcon.Cog))
                {
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Stop, T("Stop When..."), "setting_StopWhen");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Leaf, T("Gathering Profile"), "setting_GatheringProfile");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.SortAmountUp, T("Mission Priority"), "setting_MissionPriority");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Route, T("Travel & Pathfinding"), "setting_Travel");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.UserCog, T("Misc Settings"), "setting_Misc");
                }
                var currentClass = C.SelectedJob;
                var classIcon = ImGui_Ice.GetGreyscaleJob(currentClass);
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Select Class"), imageTexture: classIcon))
                {
                    int itemsPerRow = 4;
                    int currentItem = 0;

                    float iconSize = 26 * scale;
                    float iconSpacing = 4;
                    float leftOffset = 10f; // Simple offset from the current position

                    if (ImGui_Ice.SliderButton("AutoSelectJob", T("Auto Select Job"), ref autoSelectedJob))
                    {
                        C.AutoPickCurrentJob = autoSelectedJob;
                        C.Save();
                    }

                    ImGui.Dummy(new(0, 3));

                    for (uint i = 8; i < 19; ++i)
                    {
                        // Set cursor position at start of each new row
                        if (currentItem % itemsPerRow == 0)
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + leftOffset);

                        ImGui_Ice.DrawJobButtons(i, CosmicHelper.GetJobName(i));

                        currentItem++;

                        if (currentItem % itemsPerRow != 0 && i != 18)
                            ImGui.SameLine(0, iconSpacing);
                    }
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Current Tool XP"), FontAwesomeIcon.ArrowUpRightDots))
                {
                    ImGui_Ice.Draw_ExpTable(currentClass);
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Need Help?"), FontAwesomeIcon.QuestionCircle))
                {
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.QuestionCircle, T("Plugin Requirements"), "help_PluginInstall");
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Book, T("Plugin Logs"), "help_PluginLogs");
                }
            }
        }


        private static void PluginIcon()
        {
            string PluginIcon = "ICE.Resources.Icon.png";

            // Image/Icon of the plugin
            var pluginIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), PluginIcon).GetWrapOrEmpty();

            if (pluginIcon != null)
            {
                // Getting the image size via the texture wrap (using the * after to manipulate the size cause, she big)
                Vector2 imageSize = new Vector2(pluginIcon.Width, pluginIcon.Height) * 0.35f;

                // Calculate the offset/centering here
                float sidebarWidth = ImGui.GetContentRegionAvail().X;
                float offsetX = (sidebarWidth - imageSize.X) / 2.0f;

                // Center and drawing the image now
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offsetX);
                ImGui.Image(pluginIcon.Handle, imageSize);
                if (ImGui.IsItemHovered())
                {
                    if (modeSelect_TableInfo.jokeId == -1)
                    {
                        var random = new Random();
                        modeSelect_TableInfo.jokeId = random.Next(0, modeSelect_TableInfo.JokeList.Count);
                    }
                    ImGui.SetTooltip(modeSelect_TableInfo.JokeList[modeSelect_TableInfo.jokeId]);
                }
                else
                {
                    modeSelect_TableInfo.jokeId = -1;
                }

                // Add spacing after image
                ImGui.Dummy(new Vector2(0, 10));
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, 10));
            }
        }
        public static void AutoSelectMoonUpdate(bool autoSelectMoon)
        {
            bool NeedsUpdate(bool sinus, bool phaenna, bool oizys)
            {
                return C.ShowSinusMissions != sinus ||
                       C.ShowPhaennaMissions != phaenna ||
                       C.ShowOizysMissions != oizys;
            }

            void SetMoonVisibility(bool sinus, bool phaenna, bool oizys)
            {
                C.ShowSinusMissions = sinus;
                C.ShowPhaennaMissions = phaenna;
                C.ShowOizysMissions = oizys;
                C.Save();
            }

            if (autoSelectMoon)
            {
                if (PlayerHelper.IsInSinusArdorum() && NeedsUpdate(true, false, false))
                    SetMoonVisibility(sinus: true, phaenna: false, oizys: false);
                else if (PlayerHelper.IsInPhaenna() && NeedsUpdate(false, true, false))
                    SetMoonVisibility(sinus: false, phaenna: true, oizys: false);
                else if (PlayerHelper.IsInOizys() && NeedsUpdate(false, false, true))
                    SetMoonVisibility(sinus: false, phaenna: false, oizys: true);
            }
        }
        public static void AutoSelectClass(bool autoSelectClass)
        {
            var jobId = (uint)Player.Job;

            bool needsUpdated = autoSelectClass && C.SelectedJob != jobId && CosmicHelper.SupportedJobs.Contains(jobId);
            if (needsUpdated)
            {
                C.SelectedJob = (uint)Player.Job;
                C.Save();
            }
        }
    }
}
