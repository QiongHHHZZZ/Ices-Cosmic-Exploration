using ICE.Utilities.Cosmic_Helper;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_CustomItems
    {
        public static void Draw()
        {
            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.Borders |
                                         ImGuiTableFlags.SizingFixedFit |
                                         ImGuiTableFlags.Resizable; // Allow column resizing

            var GatheringItems = CosmicHelper.GatheringItems
                                .OrderBy(kvp => kvp.Value.Type)
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (ImGui.BeginTable("Item List: Craft + Gathering", 3, tableFlags))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Ids");
                ImGui.TableSetupColumn("Kind");

                ImGui.TableHeadersRow();

                foreach (var item in GatheringItems)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{item.Key}");

                    ImGui.TableNextColumn();
                    var idsText = item.Value.itemIds.Count > 0
                        ? string.Join(", ", item.Value.itemIds)
                        : "No items";
                    ImGui.Text(idsText);

                    ImGui.TableNextColumn();
                    ImGui.Text($"{Type(item.Value.Type)}");
                }

                ImGui.EndTable();
            }
        }

        private static string Type(uint kind)
        {
            string type = string.Empty;

            switch (kind)
            {
                case 1:
                    type = "Raw Materials";
                    break;
                case 2:
                    type = "Seafood";
                    break;
                case 3:
                    type = "Crafting Material";
                    break;
                case 4:
                    type = "Handicraft";
                    break;
                case 5:
                    type = "Bait";
                    break;
                case 7:
                    type = "Items";
                    break;
                default:
                    type = "???";
                    break;
            }

            return type;
        }
    }
}
