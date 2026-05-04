using ICE.Utilities.Cosmic_Helper;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_MoonRecipies
    {
        private static string RecipeTableSearchText = "";

        public static unsafe void Draw()
        {
            ImGui.SetNextItemWidth(250);
            ImGui.InputText("Search by Name", ref RecipeTableSearchText, 100);

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.SizingFixedFit |
                            ImGuiTableFlags.Resizable |           // Allow column resizing
                            ImGuiTableFlags.Reorderable |         // Allow column reordering
                            ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

            if (ImGui.BeginTable("Mission Info List", 14, tableFlags))
            {
                ImGui.TableSetupColumn("Key");
                ImGui.TableSetupColumn("Mission Name");
                ImGui.TableSetupColumn("Main-Craft 1");
                ImGui.TableSetupColumn("Amount [1]");
                ImGui.TableSetupColumn("Main-Craft 2");
                ImGui.TableSetupColumn("Amount [2]");
                ImGui.TableSetupColumn("Main-Craft 3");
                ImGui.TableSetupColumn("Amount [3]");
                ImGui.TableSetupColumn("Pre-Craft [1]");
                ImGui.TableSetupColumn("Amount [1]");
                ImGui.TableSetupColumn("Pre-Craft [2]");
                ImGui.TableSetupColumn("Amount [2]");
                ImGui.TableSetupColumn("Pre-Craft [3]");
                ImGui.TableSetupColumn("Amount [3]");

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
                        ImGui.Text($"{entry.Key}");

                        ImGui.TableNextColumn();
                        var missionName = CosmicHelper.SheetMissionDict.First(x => x.Key == entry.Key).Value.Name;
                        ImGui.Text($"{missionName}");

                        // Column #2
                        foreach (var mainCraft in entry.Value.Crafts_Main)
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text($"{mainCraft.Value.ItemId}");
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"RecipeID: {mainCraft.Key}");
                                string itemName = ExcelHelper.ItemSheet.GetRow(mainCraft.Value.ItemId).Name.ToString();
                                ImGui.Text($"Item Name: {itemName}");
                                ImGui.Separator();
                                ImGui.Text($"Item ID: {mainCraft.Value.ItemId}");
                                ImGui.Text($"Necessary Amount: {mainCraft.Value.RequiredAmount}");
                                ImGui.Text($"Recipe ID: {mainCraft.Value.RecipeId}");
                                ImGui.Text($"Expert Craft: {mainCraft.Value.ExpertCraft}");
                                ImGui.Separator();
                                ImGui.Text($"Required Item");
                                foreach (var item in mainCraft.Value.RequiredItems)
                                {
                                    ImGui.Text($"Id: {item.Key}");
                                    ImGui.Text($"Amount: {item.Value}");
                                }

                                ImGui.EndTooltip();
                            }

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mainCraft.Value.RequiredAmount}");
                        }

                        ImGui.TableSetColumnIndex(7);
                        if (entry.Value.Crafts_Pre.Count > 0)
                        {
                            foreach (var preCraft in entry.Value.Crafts_Pre)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text($"{preCraft.Value.ItemId}");
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.Text($"RecipeID: {preCraft.Key}");
                                    string itemName = ExcelHelper.ItemSheet.GetRow(preCraft.Value.ItemId).Name.ToString();
                                    ImGui.Text($"Item Name: {itemName}");
                                    ImGui.Separator();
                                    ImGui.Text($"Item ID: {preCraft.Value.ItemId}");
                                    ImGui.Text($"Necessary Amount: {preCraft.Value.RequiredAmount}");
                                    ImGui.Text($"Recipe ID: {preCraft.Value.RecipeId}");
                                    ImGui.Text($"Expert Craft: {preCraft.Value.ExpertCraft}");
                                    ImGui.Separator();
                                    ImGui.Text($"Required Item");
                                    foreach (var item in preCraft.Value.RequiredItems)
                                    {
                                        string itemNameC = ExcelHelper.ItemSheet.GetRow(item.Key).Name.ToString();
                                        ImGui.Text($"{itemNameC}");
                                        ImGui.Text($"Id: {item.Key}");
                                        ImGui.Text($"Amount: {item.Value}");
                                    }

                                    ImGui.EndTooltip();
                                }

                                ImGui.TableNextColumn();
                                ImGui.Text($"{preCraft.Value.RequiredAmount}");
                            }
                        }
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
