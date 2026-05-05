using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings
{
    internal class ShoppingTab
    {
        private static string ItemSearch = string.Empty;
        private static ImGuiEx.RealtimeDragDrop<uint> MaterialDragDrop = new("MaterialShop", (id) => id.ToString());
        private static ImGuiEx.RealtimeDragDrop<uint> GearDragDrop = new("GearShop", (id) => id.ToString());

        public static unsafe void Draw()
        {
            float minContentWidth = 920 * ImGuiHelpers.GlobalScale;
            var availWidth = ImGui.GetContentRegionAvail().X;
            if (availWidth < minContentWidth)
                ImGui.SetNextWindowContentSize(new Vector2(minContentWidth, 0));

            using var scrollChild = ImRaii.Child("##shoppingTabScroll", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);
            if (!scrollChild.Success) return;

            bool BuyItems = C.BuyItems;

            if (ImGui.Checkbox(T("Buy Items"), ref BuyItems))
            {
                C.BuyItems = BuyItems;
                C.StopOnceHitCosmoCredits = false;
                C.Save();
            }
            ImGui.SameLine();
            ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(T("This is your personalized shopping list that you can create that it will run when you hit a certain amount of credits."));
                ImGui.Text(T("Here's what each of the following does:"));
                ImGui.BulletText(T("Keep: Will buy up to that many items to make sure that you have in your inventory. This count doesn't go down between runs.\n" +
                                  "Useful for things like cordials where you want to always have a certain amount on hand"));
                ImGui.BulletText(T("Buy: Will buy X amount of those items, as it buys it from the vendor, the number will decrease until it hits 0.\n" +
                                  "Good for one off buys, or something that you only need a particular amount of"));
                ImGui.BulletText(T("Keep Buying: Once the other 2 have been met (Keep/Buy), it will constantly buy this item if it has the credits to do so.\n" +
                                  "This can only be set to 1 item, and gererally used for things you want to just spend your credits on"));
                ImGui.EndTooltip();
            }
            ImGui.NewLine();

            int buyAtAmount = C.CosmoBuyAtAmount;
            int CosmoKeepAmount = C.CosmoKeepAmount;

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt(T("Go buy items when you reach"), ref buyAtAmount, 0, 30000))
            {
                C.CosmoBuyAtAmount = buyAtAmount;
                C.SaveDebounced();
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.SliderInt(T("Keep this much Cosmocredits"), ref CosmoKeepAmount, 0, buyAtAmount))
            {
                C.CosmoKeepAmount = CosmoKeepAmount;
                C.SaveDebounced();
            }

            CheckConfigState();
            if (Task_BuyCosmoItems.CanPurchaseAnyItem())
            {
                ImGui.Text(T("You can buy cosmocredit items from the list!"));
            }
            else
            {
                ImGui.Text(T("You can't buy any items with your current credit value/items (tis fine, this just a test)"));
            }

            if (ImGui.Button(T("Add Material/Dyes/Items")))
            {
                ImGui.OpenPopup("CosmocreditMateriaPopup");
            }

            ImGui.SameLine();

            if (ImGui.Button(T("Add Armor/Housing/Mounts")))
            {
                ImGui.OpenPopup("Cosmocredit_MountArmorPopup");
            }

            ImGui.SameLine();
            
            if (ImGui.Button(T("Clear shopping list")))
            {
                C.CosmoShopping.Clear();
                C.CosmoShoppingOrder.Clear();
                C.CosmoShoppingOrder_Gear.Clear();

                C.Save();
            }

            DrawAddItemPopups();

            ImGui.Separator();
            ImGui.NewLine();

            DrawShoppingTable(T("Armor/Housing/Mounts"), "ArmorHousingMounts", Shop_Cosmocredits.Shop_MountsCards, C.CosmoShoppingOrder_Gear, GearDragDrop);

            // Draw separate tables for each shop type

            ImGui.NewLine();

            DrawShoppingTable(T("Materials/Dyes/Items"), "MaterialsDyesItems", Shop_Cosmocredits.Shop_MateriaDye, C.CosmoShoppingOrder, MaterialDragDrop);
        }

        private static void DrawAddItemPopups()
        {
            ImGui.SetNextWindowSize(new Vector2(400, 0), ImGuiCond.Appearing);

            if (ImGui.BeginPopup("CosmocreditMateriaPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Materia Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    ImGui.TableSetupColumn(T("Icons"), ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn(T("Names"), ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MateriaDye)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopup("Cosmocredit_MountArmorPopup"))
            {
                ImGui.SetNextItemWidth(380);
                ImGui.InputText("##Item Search2", ref ItemSearch, 256);

                ImGui.Spacing();

                if (ImGui.BeginTable("Cosmo Gear Shop", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg, new Vector2(0, 250)))
                {
                    ImGui.TableSetupColumn(T("Icons"), ImGuiTableColumnFlags.WidthFixed, 20);
                    ImGui.TableSetupColumn(T("Names"), ImGuiTableColumnFlags.WidthStretch);

                    foreach (var item in Shop_Cosmocredits.Shop_MountsCards)
                    {
                        DrawShopItemRow(item.Key, C.CosmoShoppingOrder_Gear);
                    }
                    ImGui.EndTable();
                }

                ImGui.EndPopup();
            }
        }

        private static void DrawShopItemRow(uint id, List<uint> orderList)
        {
            if (Svc.Data.GetExcelSheet<Item>().TryGetRow(id, out var itemInfo))
            {
                var name = itemInfo.Name.ToString();

                if (!ItemSearch.IsNullOrWhitespace() && !name.ToLower().Contains(ItemSearch.ToLower()))
                {
                    return;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.PushID(id);

                if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
                {
                    ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                }

                ImGui.TableNextColumn();
                ImGui.Text($"{itemInfo.Name}");

                if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    AddItemToList(id, orderList);
                    C.Save();
                }

                ImGui.PopID();
            }
        }

        private static void DrawShoppingTable(string tableName, string tableId, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            if (orderList.Count == 0)
            {
                ImGui.TextDisabled(T("No items in {0} shopping list", tableName));
                return;
            }

            ImGui.Text(T("{0} ({1} items)", tableName, orderList.Count));

            dragDrop.Begin();

            if (ImGui.BeginTable($"Shopping_{tableId}", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn(T("Order"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Name"));
                ImGui.TableSetupColumn(T("Have"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Cost"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Kind"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Unlocked"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Keep"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Buy"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(T("Keep Buying"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);

                ImGui.TableHeadersRow();

                for (int i = 0; i < orderList.Count; i++)
                {
                    uint itemId = orderList[i];
                    DrawShoppingItemRow(itemId, shopData, i, orderList, dragDrop);
                }

                ImGui.EndTable();
            }

            dragDrop.End();
        }

        private static void DrawShoppingItemRow(uint itemId, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData, int index, List<uint> orderList, ImGuiEx.RealtimeDragDrop<uint> dragDrop)
        {
            var setting = C.CosmoShopping[itemId];
            var itemInfo = Svc.Data.GetExcelSheet<Item>().GetRow(itemId);

            ImGui.TableNextRow();
            dragDrop.NextRow();
            dragDrop.SetRowColor(itemId);

            ImGui.PushID(itemId);

            // Drag/Drop Handle - MUCH SIMPLER NOW!
            ImGui.TableSetColumnIndex(0);
            dragDrop.DrawButtonDummy(itemId, orderList, index);

            // Name
            ImGui.TableNextColumn();
            if (itemInfo.Icon is { } itemIcon && Svc.Texture.TryGetFromGameIcon((int)itemIcon, out var texture))
            {
                ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                ImGui.SameLine();
            }
            ImGui.Text($"{itemInfo.Name}");

            // Have
            ImGui.TableNextColumn();
            PlayerHelper.GetItemCount(itemId, out var count);
            ImGui.Text($"{count}");

            // Cost
            ImGui.TableNextColumn();
            if (shopData.TryGetValue(itemId, out var shopInfo))
            {
                ImGui.Text($"{shopInfo.Cost:N0}");
            }

            // Kind
            ImGui.TableNextColumn();
            string kind = itemInfo.ItemUICategory.Value.Name.ToString();
            ImGui.Text(kind);

            // Unlocked (for consumable items like mounts, orchestrion rolls, cards, etc.)
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(UnlockState.IsItemUnlockable(itemInfo) ? UnlockState.IsItemUnlocked(itemInfo) ? T("Yes") : T("No") : "-");

            // Keep Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var keepAmount = setting.KeepAmount;
            if (ImGui.InputInt($"##keep_{itemId}", ref keepAmount))
            {
                setting.KeepAmount = Math.Max(0, keepAmount);
                C.SaveDebounced();
            }

            // Buy Amount
            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(80);
            var buyAmount = setting.BuyAmount;
            if (ImGui.InputInt($"##buy_{itemId}", ref buyAmount))
            {
                setting.BuyAmount = Math.Max(0, buyAmount);
                C.SaveDebounced();
            }

            // Keep Buying
            ImGui.TableNextColumn();
            var keepBuying = setting.KeepBuying;
            if (ImGui.Checkbox($"##keepbuying_{itemId}", ref keepBuying))
            {
                foreach (var enabled in C.CosmoShopping)
                {
                    enabled.Value.KeepBuying = false;
                }

                setting.KeepBuying = keepBuying;
                C.Save();
            }

            // Remove Button
            ImGui.TableNextColumn();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, $"##remove_{itemId}"))
            {
                RemoveItem(itemId, orderList);
                C.Save();
            }

            ImGui.PopID();
        }

        private static void AddItemToList(uint itemId, List<uint> orderList)
        {
            if (C.CosmoShopping.ContainsKey(itemId))
                return;

            C.CosmoShopping[itemId] = new CosmoShoppingList();
            orderList.Add(itemId);
        }

        private static void RemoveItem(uint itemId, List<uint> orderList)
        {
            C.CosmoShopping.Remove(itemId);
            orderList.Remove(itemId);
        }

        public static void CheckConfigState()
        {
            if (C.CosmoShopping == null)
            {
                C.CosmoShopping = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder == null)
            {
                C.CosmoShoppingOrder = new();
                C.Save();
            }
            if (C.CosmoShoppingOrder_Gear == null)
            {
                C.CosmoShoppingOrder_Gear = new();
                C.Save();
            }
        }
    }
}
