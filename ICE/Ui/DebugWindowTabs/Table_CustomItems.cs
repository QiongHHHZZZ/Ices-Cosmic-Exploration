using ICE.Utilities.Cosmic_Helper;
using static ICE.Localization.L10n;

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

            if (ImGui.BeginTable(T("Item List: Craft + Gathering"), 3, tableFlags))
            {
                ImGui.TableSetupColumn(T("Name"));
                ImGui.TableSetupColumn(T("Ids"));
                ImGui.TableSetupColumn(T("Kind"));

                ImGui.TableHeadersRow();

                foreach (var item in GatheringItems)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("{0}", item.Key));

                    ImGui.TableNextColumn();
                    var idsText = item.Value.itemIds.Count > 0
                        ? string.Join(", ", item.Value.itemIds)
                        : T("No items");
                    ImGui.Text(idsText);

                    ImGui.TableNextColumn();
                    ImGui.Text(Type(item.Value.Type));
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
                    type = T("Raw Materials");
                    break;
                case 2:
                    type = T("Seafood");
                    break;
                case 3:
                    type = T("Crafting Material");
                    break;
                case 4:
                    type = T("Handicraft");
                    break;
                case 5:
                    type = T("Bait");
                    break;
                case 7:
                    type = T("Items");
                    break;
                default:
                    type = T("???");
                    break;
            }

            return type;
        }
    }
}
