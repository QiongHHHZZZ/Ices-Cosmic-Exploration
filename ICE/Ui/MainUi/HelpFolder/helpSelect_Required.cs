using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ECommons.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.HelpFolder
{
    internal class helpSelect_Required
    {
        private const string AutoHookRepo = "https://love.puni.sh/ment.json";
        private const string AutoHookPluginName = "AutoHook";
        private const string MissFisherRepo = "https://raw.githubusercontent.com/BlackCleaverLoli/MissFisher/refs/heads/main/MissFisher.json";
        private const string MissFisherPluginName = "MissFisher";

        public static void Draw()
        {
ImGui.TextWrapped(T("These are a list of the following plugins that are required for the plugin to function. If you don't have these installed, it will not function properly"));

            ImGui.Separator();
            ImGuiEx.IconWithText(FontAwesomeIcon.Hammer, T("Crafting"));
            HasPlugin("https://love.puni.sh/ment.json", "Artisan");

            ImGui.Separator();
            ImGuiEx.IconWithText(FontAwesomeIcon.Feather, T("Gathering"));
ImGui.Text(T("For botanist/miner/fisher"));
            HasPlugin("https://puni.sh/api/repository/veyn", "vnavmesh");
            ImGui.Dummy(new Vector2(0, 10));
            DrawFishingPluginRequirement();

            ImGui.Separator();
            ImGuiEx.IconWithText(FontAwesomeIcon.Running, T("Automating Hub Activities"));
            HasPlugin("https://puni.sh/api/repository/veyn", "vnavmesh");

            ImGui.Separator();
ImGui.TextWrapped(T("This isn't required, but highly recommended for leveling up characters. It will auto equip gear from your armory/inventory, and swap it out when running Leveling Grind Mode"));
            ImGuiEx.IconWithText(FontAwesomeIcon.Leaf, T("Stylist"));
            HasPlugin("https://raw.githubusercontent.com/NightmareXIV/MyDalamudPlugins/main/pluginmaster.json", "Stylist");
        }

        private static void DrawFishingPluginRequirement()
        {
            // CN-MAINT: Keep this UI in sync with Task_Fishing conflict policy (both installed => conflict warning).
            ImGui.Text(T("Fishing only (choose one: AutoHook or MissFisher)"));

            ImGui.TextDisabled(T("AutoHook"));
            HasPlugin(AutoHookRepo, AutoHookPluginName);

            ImGui.TextDisabled(T("MissFisher"));
            HasPlugin(MissFisherRepo, MissFisherPluginName);

            bool hasAutoHook = Utils.HasPlugin(AutoHookPluginName);
            bool hasMissFisher = Utils.HasPlugin(MissFisherPluginName);

            if (hasAutoHook && hasMissFisher)
            {
                ImGui.TextWrapped(T("Detected both AutoHook and MissFisher installed. Disable one."));
            }
            else if (!hasAutoHook && !hasMissFisher)
            {
                ImGui.TextWrapped(T("No fishing plugin detected. Install AutoHook or MissFisher."));
            }
            else if (hasAutoHook)
            {
                ImGui.TextWrapped(T("Fishing plugin check passed (current: AutoHook)."));
            }
            else
            {
                ImGui.TextWrapped(T("Fishing plugin check passed (current: MissFisher)."));
            }
        }

        public static void HasPlugin(string repo, string pluginName)
        {
            bool isInstalled = DalamudReflector.HasRepo($"{repo}");
            if (isInstalled)
            {
                FontAwesome.Print(EColor.Green, FontAwesome.Check);
                ImGui.SameLine();
                ImGui.Text(T("{0} Repo is Installed", pluginName));
            }
            else
            {
                FontAwesome.Print(EColor.Red, FontAwesome.Cross);
                ImGui.SameLine();
                if (ImGui.Button(T("Install {0} Repo", pluginName)))
                {
                    DalamudReflector.AddRepo(repo, true);
                    DalamudReflector.SaveDalamudConfig();
                }
            }

            bool hasPlugin = Utils.HasPlugin($"{pluginName}");

            if (hasPlugin)
            {
                FontAwesome.Print(EColor.Green, FontAwesome.Check);
                ImGui.SameLine();
                ImGui.Text(T("{0} is installed", pluginName));
            }
            else
            {
                FontAwesome.Print(EColor.Red, FontAwesome.Cross);
                ImGui.SameLine();
                using (ImRaii.Disabled(installingPlugin))
                {
                    if (ImGui.Button(T("Install {0}", pluginName)))
                    {
                        _ = InstallPlugin(repo, pluginName);
                    }
                }
            }
        }

        private static bool installingPlugin = false;
        private static async Task InstallPlugin(string repo, string pluginName)
        {
            if (installingPlugin) return; // Already installing

            installingPlugin = true;
            try
            {
                await DalamudReflector.AddPlugin(repo, pluginName);
                DalamudReflector.SaveDalamudConfig();
            }
            finally
            {
                installingPlugin = false;
            }
        }
    }
}
