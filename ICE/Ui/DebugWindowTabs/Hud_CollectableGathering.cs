using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_CollectableGathering
    {
        public static unsafe void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<GatheringMasterpiece>("GatheringMasterpiece", out var gatherCollect) && gatherCollect.IsAddonReady)
            {
                if (ImGui.Button(T("TryGather")))
                {
                    Task_Gather.CollectableGather(gatherCollect);
                }
                ImGui.SameLine();
                if (ImGui.Button(T("Reset Buff Check")))
                {
                    Mission_Settings.Collectable_BuffCount = GatheringUtil.CollectStandardCharges();
                }

                ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                             ImGuiTableFlags.Borders |
                             ImGuiTableFlags.SizingFixedFit |
                             ImGuiTableFlags.Resizable |           // Allow column resizing
                             ImGuiTableFlags.Reorderable |         // Allow column reordering
                             ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

                if (ImGui.BeginTable("Gathering_Collectable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("##AddonInfo");
                    ImGui.TableSetupColumn("##AddonValue");

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Item Name: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.ItemName}");

                    // Row 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Item ID: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.ItemID}");

                    // Row 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Current Collectability: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.CurrentCollectability}");

                    // Row 4
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Item Integrity: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.CurrentIntegrity} / {gatherCollect.TotalIntegrity}");

                    // Row 5
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Min Collectibility: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.MinCollectability}");

                    // Row 6
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Mid Collectibility: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.MidCollectability}");

                    // Row 7
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("High Collectibility: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.HighCollectability}");

                    // Row 8
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Max Collectibility: "));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.MaxCollectability}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Scour Amount"));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.ScourPower}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Brazen Power"));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.BrazenPowerMin} | {gatherCollect.BrazenPowerMax}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Meticulous Power"));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{gatherCollect.MeticulousPower}");

                    ImGui.EndTable();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<Gathering>("Gathering", out var gather) && gather.IsAddonReady)
            {
                if (ImGui.Button(T("Increase collectability")))
                {
                    foreach (var item in gather.GatheredItems)
                    {
                        if (item.ItemID != 0)
                        {
                            bool missingDur = gather.CurrentIntegrity < gather.TotalIntegrity;
                            bool useAction = Task_Gather.UseGatherAction(0, item.GatherChance, item.BoonChance, gather.CurrentIntegrity, gather.TotalIntegrity, PlayerHelper.GetGp());
                            IceLogging.Debug($"Used action: {useAction}");
                            break;
                        }
                    }
                }
            }
            else
            {
                ImGui.Text(T("Waiting for Gather Collectable window to be visible"));
            }
        }
    }
}
