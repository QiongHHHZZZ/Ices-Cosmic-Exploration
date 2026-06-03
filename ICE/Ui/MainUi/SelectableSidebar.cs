using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using System.Reflection;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi
{
    internal class SelectableSidebar
    {
        public static void Draw()
        {
            var scale = ImGuiHelpers.GlobalScale;
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

                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Cosmic Helper"), SidebarTabs.CosmicHelper, icon: FontAwesomeIcon.ListAlt))
                {
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.List, T("Mission Setup"), WindowSelection.MissionSetup);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.ClipboardList, T("Cosmic Agenda"), WindowSelection.CosmicAgenda);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Trophy, T("Expedition Log"), WindowSelection.ExpeditionLogs);
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Planet Selection"), SidebarTabs.PlanetSelection, FontAwesomeIcon.Moon))
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

                    var moons = new (string Name, string Asset, ItemFilter planetFilter)[]
                    {
                            (T("Sinus Ardorum"), "ICE.Resources.Sinus_Ardorum.png", ItemFilter.Sinus),
                            (T("Phaenna"), "ICE.Resources.Phaenna.png", ItemFilter.Phaenna),
                            (T("Oizys"), "ICE.Resources.Oizys.png", ItemFilter.Oizys), 
                            (T("Auxesia"), "ICE.Resources.Auxesia.png", ItemFilter.Auxesia)
                    };

                    for (int i = 0; i < moons.Length; i++)
                    {
                        if (i > 0) ImGui.SameLine(0, iconSpacing);

                        var moon = moons[i];
                        bool isEnabled = C.ItemFilter.HasFlag(moon.planetFilter);
                        var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), moon.Asset).GetWrapOrEmpty();

                        if (ImGui_Ice.DrawStyledImageButton(texture, new Vector2(iconSize, iconSize), isEnabled))
                        {
                            C.ItemFilter = isEnabled
                                ? C.ItemFilter & ~moon.planetFilter  // was on → turn off 
                                : C.ItemFilter | moon.planetFilter;  // was off → turn on
                            C.AutoSelectMoon = false;
                            if (Mission_Setup.MissionTable != null)
                                Mission_Setup.MissionTable.SetFilterDirty();

                            C.Save();
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(moon.Name);
                        }
                    }
                }
                var currentClass = C.SelectedJob;
                var classIcon = ImGui_Ice.GetGreyscaleJob(currentClass);
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Select Class"), SidebarTabs.ClassSelection, imageTexture: classIcon))
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

                        ImGui_Ice.DrawJobButtons(i, CosmicHelper.ClassInfoDict[i]);

                        currentItem++;

                        if (currentItem % itemsPerRow != 0 && i != 18)
                            ImGui.SameLine(0, iconSpacing);
                    }
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Hub Activities"), SidebarTabs.HubActivites, icon: FontAwesomeIcon.Home))
                {
                    ImGui_Ice.DrawSelectable_Image(65112, T("Credit Shopping"), WindowSelection.CreditShopping);
                    ImGui_Ice.DrawSelectable_Image(65127, T("Gambling Settings"), WindowSelection.GambaShopping);
                    ImGui_Ice.DrawSelectable_Image(65138, T("Dronebit Settings"), WindowSelection.DroneShopping);
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Settings"), SidebarTabs.Settings, icon: FontAwesomeIcon.Cog))
                {
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Stop, T("Stop When..."), WindowSelection.StopWhen);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Leaf, T("Gathering Profile"), WindowSelection.GatheringProfiles);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.SortAmountUp, T("Mission Priority"), WindowSelection.MissionPriority);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Route, T("Travel & Pathfinding"), WindowSelection.TravelSettings);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.PersonBurst, T("Character Settings"), WindowSelection.CharacterSettings);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.UserCog, T("Misc Settings"), WindowSelection.MiscSettings);
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Current Tool XP"), SidebarTabs.ExpInfo, FontAwesomeIcon.ArrowUpRightDots))
                {
                    ImGui_Ice.Draw_ExpTable(currentClass);
                }
                if (ImGui_Ice.Sidebar_CollaspableHeader(T("Need Help?"), SidebarTabs.HelpInfo, FontAwesomeIcon.QuestionCircle))
                {
                 // ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.HandHoldingHand, T("Plugin Tips"), WindowSelection.Plugin_Tips);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.QuestionCircle, T("Plugin Requirements"), WindowSelection.Plugin_Install);
                    ImGui_Ice.DrawSelectable_Icon(FontAwesomeIcon.Book, T("Plugin Logs"), WindowSelection.Plugin_Logs);
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Toolbox, T("Refresh Class info"), size: new(ImGui.GetContentRegionAvail().X, 30)))
                    {
                        CosmicHelper.Task_UpdateRelicMissionInfo();
                    }
                }
#if DEBUG

#endif
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
                    if (Window_ExternalDetails.jokeId == -1)
                    {
                        var random = new Random();
                        Window_ExternalDetails.jokeId = random.Next(0, Window_ExternalDetails.JokeList.Count);
                    }
                    ImGui.SetTooltip(T(Window_ExternalDetails.JokeList[Window_ExternalDetails.jokeId]));
                }
                else
                {
                    Window_ExternalDetails.jokeId = -1;
                }

                // Add spacing after image
                ImGui.Dummy(new Vector2(0, 10));
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, 10));
            }
        }
        public static void AutoSelectMoonUpdate(bool autoSelectMoon)
        {
            if (!autoSelectMoon) return;

            var moonFlags = new (Func<bool> IsInZone, ItemFilter Flag)[]
            {
                (PlayerHelper.IsInSinusArdorum, ItemFilter.Sinus),
                (PlayerHelper.IsInPhaenna,      ItemFilter.Phaenna),
                (PlayerHelper.IsInOizys,        ItemFilter.Oizys),
                (PlayerHelper.IsInAuxesia,      ItemFilter.Auxesia)
            };

            var planetFlags = ItemFilter.Sinus | ItemFilter.Phaenna | ItemFilter.Oizys | ItemFilter.Auxesia;

            foreach (var (IsInZone, Flag) in moonFlags)
            {
                if (!IsInZone()) continue;

                var desired = (C.ItemFilter & ~planetFlags) | Flag;
                if (C.ItemFilter != desired)
                {
                    C.ItemFilter = desired;
                    if (Mission_Setup.MissionTable != null)
                        Mission_Setup.MissionTable.SetFilterDirty();
                    C.SaveDebounced();
                }
                return;
            }
        }
        public static void AutoSelectClass(bool autoSelectClass)
        {
            var jobId = (uint)Player.Job;

            if (!autoSelectClass) return;
            if (!CosmicHelper.ClassInfoDict.TryGetValue(jobId, out var jobClass)) return;

            var currentFlag = jobClass.JobFlag;

            // Check if any flags other than the current job are active
            bool needsUpdated = (C.JobFilter & ~currentFlag) != JobFilter.None
                             || (C.JobFilter & currentFlag) == JobFilter.None;

            if (needsUpdated)
            {
                C.JobFilter = currentFlag;
                C.Save();
                if (Mission_Setup.MissionTable != null)
                    Mission_Setup.MissionTable.SetFilterDirty();
            }
        }
    }
}
