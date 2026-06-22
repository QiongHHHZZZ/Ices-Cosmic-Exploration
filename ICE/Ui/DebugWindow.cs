using Dalamud.Interface;
using ICE.Ui.Debug_Tabs.Debug_CS;
using ICE.Ui.Debug_Tabs.Debug_Hud;
using ICE.Ui.Debug_Tabs.Debug_Tables;
using ICE.Ui.Debug_Tabs.Debug_Ui;
using ICE.Ui.DebugWindowTabs;
using ICE.Ui.MainUi.HelpFolder;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static ICE.Localization.L10n;

namespace ICE.Ui;

internal class DebugWindow : Window
{
    private bool _showSidebar = true;

    public DebugWindow() :
        base(T("ICE {0} Debugger ###IceCosmicDebug1", P.GetType().Assembly.GetName().Version?.ToString() ?? "unknown"))
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(3000, 3000)
        };

        // Title-bar toggle for the left tab list, so the window can be shrunk down to just the content.
        TitleBarButtons.Add(new TitleBarButton
        {
            Icon = FontAwesomeIcon.Bars,
            IconOffset = new Vector2(2, 1),
            Click = _ => _showSidebar = !_showSidebar,
            ShowTooltip = () => ImGui.SetTooltip(_showSidebar ? T("Hide tab list") : T("Show tab list")),
        });

        P.windowSystem.AddWindow(this);
    }

    public void Dispose()
    {
        P.windowSystem.RemoveWindow(this);
    }

    private readonly Dictionary<string, Dictionary<string, Action>> DebugViewGroups = new()
    {
        ["Hud"] = new()
        {
            ["Shop"] = () => Hud_Shop.Draw(),
            ["Moon Main"] = () => Hud_MainMoon.Draw(),
            ["Mission"] = () => Hud_Mission.Draw(),
            ["Mission Info"] = () => Hud_MissionInfo.Draw(),
            ["Wheel of Fortune!"] = () => Hud_WheelofFortune.Draw(),
            ["Moon Recipe"] = () => Hud_MoonRecipe.Draw(),
            ["Gather Collectable"] = () => Hud_CollectableGathering.Draw(),
            ["Item Exchange"] = () => Hud_ItemExchange.Draw(),
            ["Aethernet"] = () => Hud_Aethernet.Draw(),
        },
        ["Table"] = new()
        {
            ["Mission Info"] = () => Table_MissionInfo.Draw(),
            ["Gathering Missions"] = () => Table_GatheringInfo.Draw(),
            ["Special Missions"] = () => Table_TimeWeather.Draw(),
            ["Mission Text"] = () => Table_MissionText.Draw(),
            ["Recipes"] = () => Table_MoonRecipies.Draw(),
            ["Fish Info"] = () => Table_FishInfo.Draw(),
            ["Leveling Missions"] = () => Table_LevelingMissions.Draw(),
            ["Mission Select"] = () => Table_MissionSelect.Draw(),
            ["Mission V3"] = () => Table_MissionsV3.Draw(),
        },
        ["Ui"] = new()
        {
            ["Select String"] = () => Ui_RedAlertString.Draw(),
            ["Fishing Hole Editor"] = () => Ui_Fish_HoleEditor.Draw(),
            ["Fishing Presets"] = () => Ui_FishPresets.Draw(),
            ["Gather Editor"] = () => Ui_GatherEditor.Draw(),
            ["Log Viewer"] = () => helpSelect_Logs.Draw_Debug(),
            ["Player Gearsets"] = () => Ui_Gearsets.Draw(),
            ["Player Info"] = () => Ui_PlayerInfo.Draw(),
            ["Relic Info"] = () => Ui_RelicInfo.Draw(),
            ["Relic Info V2"] = () => Ui_ClassInfo.Draw(),
            ["NPC Box Viewer"] = () => Ui_NpcViewer.Draw(),
            ["Oizyr Map Stuff"] = () => Ui_OyzinMap.Draw(),
            ["Aethernet Test"] = () => Ui_Aethernet.Draw(),
        },
        ["Misc"] = new()
        {
            ["CS: Timer Info"] = () => CS_TimerInfo.Draw(),
            ["CS: Available Missions"] = () => CS_Missions.Draw(),
            ["Test Buttons"] = () => Ui_TestButtons.Draw(),
            ["IPC Testing"] = () => Ui_IPCTesting.Draw(),
            ["IPC: Artisan"] = () => Ipc_Artisan.Draw(),
            ["Map Test"] = () => Ui_MapTesting.Draw(),
            ["Navmesh Testing"] = () => Ui_NavmeshTesting.Draw(),
            ["TaskManager Testing"] = () => Ui_TaskManagerInfo.Draw(),
            ["ImGui Testing"] = () => UI_Test.Draw(),
            ["Sheet: Mission Rewards"] = () => Sheet_MissionRewards.Draw(),
        },
    };

    private string _selectedGroup = "Hud";
    private string _selectedView = "Moon Main";

    public override void Draw()
    {
        float spacing = 10f;
        float leftPanelWidth = 160f;
        float childHeight = ImGui.GetContentRegionAvail().Y;

        // -- Left sidebar: one selectable per group --
        if (_showSidebar)
        {
            if (ImGui.BeginChild("DebugGroupSelector", new Vector2(leftPanelWidth, childHeight), true))
            {
                foreach (var groupName in DebugViewGroups.Keys)
                {
                    bool isSelected = (_selectedGroup == groupName);
                    if (ImGui.Selectable(groupName, isSelected))
                    {
                        if (_selectedGroup != groupName)
                        {
                            _selectedGroup = groupName;
                            // Auto-select first tab in the new group
                            _selectedView = DebugViewGroups[groupName].Keys.First();
                        }
                    }
                }
            }
            ImGui.EndChild();
            ImGui.SameLine(0, spacing);
        }

        // -- Right panel: tabs across the top, content below --
        float rightPanelWidth = ImGui.GetContentRegionAvail().X;
        if (ImGui.BeginChild("DebugRightPanel", new Vector2(rightPanelWidth, childHeight), false))
        {
            if (DebugViewGroups.TryGetValue(_selectedGroup, out var views))
            {
                if (ImGui.BeginTabBar("DebugTabs"))
                {
                    foreach (var (viewName, drawAction) in views)
                    {
                        var flags = (_selectedView == viewName && ImGui.GetFrameCount() <= 1)
                            ? ImGuiTabItemFlags.SetSelected
                            : ImGuiTabItemFlags.None;

                        if (ImGui.BeginTabItem(viewName, ref Unsafe.NullRef<bool>(), flags))
                        {
                            _selectedView = viewName;

                            if (ImGui.BeginChild("DebugContent", new Vector2(0, 0), true))
                                drawAction();
                            ImGui.EndChild();

                            ImGui.EndTabItem();
                        }
                    }
                    ImGui.EndTabBar();
                }
            }
            else
            {
                ImGui.Text(T("Unknown Debug View"));
            }
        }
        ImGui.EndChild();
    }
}
