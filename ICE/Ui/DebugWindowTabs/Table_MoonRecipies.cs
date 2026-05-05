using ICE.Utilities.Cosmic_Helper;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_MoonRecipies
    {
        private static string RecipeTableSearchText = "";

        public static unsafe void Draw()
        {
            ImGui.SetNextItemWidth(250);
            ImGui.InputText(T("Search by Name"), ref RecipeTableSearchText, 100);

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.SizingFixedFit |
                            ImGuiTableFlags.Resizable |           // Allow column resizing
                            ImGuiTableFlags.Reorderable |         // Allow column reordering
                            ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

            if (ImGui.BeginTable(T("Mission Info List"), 14, tableFlags))
            {
                ImGui.TableSetupColumn(T("Key"));
                ImGui.TableSetupColumn(T("Mission Name"));
                ImGui.TableSetupColumn(T("Main-Craft 1"));
                ImGui.TableSetupColumn(T("Amount [1]"));
                ImGui.TableSetupColumn(T("Main-Craft 2"));
                ImGui.TableSetupColumn(T("Amount [2]"));
                ImGui.TableSetupColumn(T("Main-Craft 3"));
                ImGui.TableSetupColumn(T("Amount [3]"));
                ImGui.TableSetupColumn(T("Pre-Craft [1]"));
                ImGui.TableSetupColumn(T("Amount [1]"));
                ImGui.TableSetupColumn(T("Pre-Craft [2]"));
                ImGui.TableSetupColumn(T("Amount [2]"));
                ImGui.TableSetupColumn(T("Pre-Craft [3]"));
                ImGui.TableSetupColumn(T("Amount [3]"));

                ImGui.TableHeadersRow();

                foreach (var entry in CosmicHelper.SheetMissionDict)
                {
                    if (entry.Value.Jobs.Any(x => CosmicHelper.CrafterJobList.Contains(x)))
                    {
                        if (!string.IsNullOrEmpty(RecipeTableSearchText) &&
                            !entry.Value.Name.ToLower().Contains(RecipeTableSearchText.ToLower()))
                            continue;

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(T("{0}", entry.Key));

                        ImGui.TableNextColumn();
                        var missionName = CosmicHelper.SheetMissionDict.First(x => x.Key == entry.Key).Value.Name;
                        ImGui.Text(T("{0}", missionName));

                        // Column #2
                        foreach (var mainCraft in entry.Value.Crafts_Main)
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text(T("{0}", mainCraft.Value.ItemId));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("RecipeID: {0}", mainCraft.Key));
                                string itemName = ExcelHelper.ItemSheet.GetRow(mainCraft.Value.ItemId).Name.ToString();
                                ImGui.Text(T("Item Name: {0}", itemName));
                                ImGui.Separator();
                                ImGui.Text(T("Item ID: {0}", mainCraft.Value.ItemId));
                                ImGui.Text(T("Necessary Amount: {0}", mainCraft.Value.RequiredAmount));
                                ImGui.Text(T("Recipe ID: {0}", mainCraft.Value.RecipeId));
                                ImGui.Text(T("Expert Craft: {0}", mainCraft.Value.ExpertCraft));
                                ImGui.Separator();
                                ImGui.Text(T("Required Item"));
                                foreach (var item in mainCraft.Value.RequiredItems)
                                {
                                    ImGui.Text(T("Id: {0}", item.Key));
                                    ImGui.Text(T("Amount: {0}", item.Value));
                                }

                                ImGui.EndTooltip();
                            }

                            ImGui.TableNextColumn();
                            ImGui.Text(T("{0}", mainCraft.Value.RequiredAmount));
                        }

                        ImGui.TableSetColumnIndex(7);
                        if (entry.Value.Crafts_Pre.Count > 0)
                        {
                            foreach (var preCraft in entry.Value.Crafts_Pre)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text(T("{0}", preCraft.Value.ItemId));
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text(T("RecipeID: {0}", preCraft.Key));
                                    string itemName = ExcelHelper.ItemSheet.GetRow(preCraft.Value.ItemId).Name.ToString();
                                    ImGui.Text(T("Item Name: {0}", itemName));
                                    ImGui.Separator();
                                    ImGui.Text(T("Item ID: {0}", preCraft.Value.ItemId));
                                    ImGui.Text(T("Necessary Amount: {0}", preCraft.Value.RequiredAmount));
                                    ImGui.Text(T("Recipe ID: {0}", preCraft.Value.RecipeId));
                                    ImGui.Text(T("Expert Craft: {0}", preCraft.Value.ExpertCraft));
                                    ImGui.Separator();
                                    ImGui.Text(T("Required Item"));
                                    foreach (var item in preCraft.Value.RequiredItems)
                                    {
                                        string itemNameC = ExcelHelper.ItemSheet.GetRow(item.Key).Name.ToString();
                                        ImGui.Text(T("{0}", itemNameC));
                                        ImGui.Text(T("Id: {0}", item.Key));
                                        ImGui.Text(T("Amount: {0}", item.Value));
                                    }

                                    ImGui.EndTooltip();
                                }

                                ImGui.TableNextColumn();
                                ImGui.Text(T("{0}", preCraft.Value.RequiredAmount));
                            }
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
