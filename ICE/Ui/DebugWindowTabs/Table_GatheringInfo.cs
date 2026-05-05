using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using Lumina.Excel.Sheets;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_GatheringInfo
    {
        private static string MissionSearchText = "";

        public static unsafe void Draw()
        {
            ImGui.SetNextItemWidth(250);
            ImGui.InputText(T("Search by Name"), ref MissionSearchText, 100);

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.SizingFixedFit |
                            ImGuiTableFlags.Resizable |           // Allow column resizing
                            ImGuiTableFlags.Reorderable |         // Allow column reordering
                            ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

            if (ImGui.BeginTable("Mission_GatheringInfo", 10, tableFlags))
            {
                ImGui.TableSetupColumn(T("Key"));
                ImGui.TableSetupColumn(T("Mission Name"));
                for (int i = 1; i < 4; i++)
                {
                    ImGui.TableSetupColumn($"Gather Item [{i}]");
                    ImGui.TableSetupColumn($"Amount [{i}]");
                }
                ImGui.TableSetupColumn(T("Mission Radius"));
                ImGui.TableSetupColumn(T("Critical Location"));
                ImGui.TableHeadersRow();

                foreach (var entry in CosmicHelper.SheetMissionDict.Where(x => x.Value.Jobs.Intersect(CosmicHelper.GatheringJobList).Any()))
                {
                    if (!string.IsNullOrEmpty(MissionSearchText) && !entry.Value.Name.Replace(" ", "").Contains(MissionSearchText.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"[{entry.Key}]");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.Name}");

                    foreach (var item in entry.Value.Gathering_Min)
                    {
                        ImGui.TableNextColumn();
                        var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name;
                        ImGui.Text($"{itemName}");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text(T("Id: {0}", item.Key));
                            ImGui.EndTooltip();
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{item.Value}");
                    }

                    ImGui.TableSetColumnIndex(8);
                    if (entry.Value.MarkerId != 0)
                    {
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Text(FontAwesomeIcon.Flag.ToIconString());
                        ImGui.PopFont();
                        if (ImGui.IsItemClicked())
                        {
                            var missionInfo = entry.Value;
                            Utils.SetGatheringRing(missionInfo.TerritoryId, (int)missionInfo.MapPosition.X, (int)missionInfo.MapPosition.Y, missionInfo.Radius, missionInfo.Name);
                        }
                    }

                    ImGui.TableNextColumn();
                    if (CosmicHelper.CriticalLocations.TryGetValue(entry.Key, out var critical))
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.FlagCheckered);
                        if (ImGui.IsItemClicked())
                        {
                            if (P.Navmesh.Installed)
                            {
                                if (P.Navmesh.IsReady())
                                {
                                    var missionInfo = entry.Value;
                                    IceLogging.DestinationLogs.Log(critical.RawLocation);
                                    P.Navmesh.PathfindAndMoveTo(critical.RawLocation, false);
                                }
                                else
                                {
                                    Utils.VnavBuildInfo();
                                }
                            }
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
