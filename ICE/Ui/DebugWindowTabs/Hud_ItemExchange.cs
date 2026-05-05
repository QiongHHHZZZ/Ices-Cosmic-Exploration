using Lumina.Excel.Sheets;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_ItemExchange
    {
        private static int Tab = 0;

        public static unsafe void Draw()
        {
            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                             ImGuiTableFlags.Borders |
                             ImGuiTableFlags.SizingFixedFit |
                             ImGuiTableFlags.Resizable |           // Allow column resizing
                             ImGuiTableFlags.Reorderable |         // Allow column reordering
                             ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

            if (GenericHelpers.TryGetAddonMaster<ECommons.UIHelpers.AddonMasterImplementations.AddonMaster.InclusionShop>("InclusionShop", out var itemExchange) && itemExchange.IsAddonReady)
            {

                ImGui.Text(T("Currency Amount: {0}", itemExchange.CurrencyAmount));

                if (ImGui.BeginTable("Item Exchange Window", 3, tableFlags))
                {
                    ImGui.TableSetupColumn("##ItemId");
                    ImGui.TableSetupColumn("##ItemName");
                    ImGui.TableSetupColumn("##Cost1", ImGuiTableColumnFlags.WidthStretch);

                    for (int i = 0; i < itemExchange.NumEntries; i++)
                    {
                        var entry = itemExchange.ShopItems[i];
                        var itemId = entry.ItemId;
                        var currencyId = entry.CurrencyId;
                        var cost = entry.Cost;

                        var sheet = Svc.Data.GetExcelSheet<Item>();
                        var itemName = sheet.GetRow(itemId).Name.ToString();

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{itemId}");

                        ImGui.TableNextColumn();
                        ImGui.Text(itemName);

                        ImGui.TableNextColumn();
                        var currencyIcon = sheet.GetRow(currencyId).Icon;
                        if (currencyIcon is { } icon)
                        {
                            if (Svc.Texture.TryGetFromGameIcon((int)icon, out var texture))
                            {
                                ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                                ImGui.SameLine();
                            }
                        }
                        ImGui.Text($"{cost}");
                        ImGui.SameLine();
                        if (ImGui.Button(T("Buy Item")))
                        {
                            entry.Select();
                        }
                    }
                    ImGui.EndTable();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                var sheet = Svc.Data.GetExcelSheet<Item>();

                var currencyIcon = sheet.GetRow(shopExchange.CurrencyId).Icon;
                Svc.Texture.TryGetFromGameIcon((int)currencyIcon, out var texture);
                ImGui.Text($"{shopExchange.CurrencyAmount}");
                ImGui.InputInt($"{T("Tab #")}##Tab", ref Tab);
                if (ImGui.Button(T("Copy Item List")))
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < shopExchange.NumEntries; i++)
                    {
                        var entry = shopExchange.BasicShopItems[i];
                        var itemId = entry.ItemId;
                        var itemName = sheet.GetRow(itemId).Name.ToString();

                        sb.AppendLine($"[{itemId}] = new ItemInfo");
                        sb.AppendLine($"\t{{");
                        sb.AppendLine($"\t\tName = \"{itemName}\",");
                        sb.AppendLine($"\t\tCost = {entry.CostAmount},");
                        sb.AppendLine($"\t\tTab = {Tab},");
                        sb.AppendLine($"\t\tIndex = {entry.Index},");
                        sb.AppendLine($"\t}},");
                    }
                    ImGui.SetClipboardText(sb.ToString());
                }

                ImGui.Text(T("Number of entries: {0}", shopExchange.NumEntries));

                if (ImGui.BeginTable("Item Exchange Window", 5, tableFlags))
                {
                    ImGui.TableSetupColumn("##ItemId");
                    ImGui.TableSetupColumn("##ItemName");
                    ImGui.TableSetupColumn("##Cost1");
                    ImGui.TableSetupColumn("##Cost2");
                    ImGui.TableSetupColumn("##Cost3");

                    for (int i = 0; i < shopExchange.NumEntries; i++)
                    {
                        var entry = shopExchange.BasicShopItems[i];
                        var itemId = entry.ItemId;
                        var cost = entry.CostAmount;

                        var itemName = sheet.GetRow(itemId).Name.ToString();

                        ImGui.PushID(itemId);

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{itemId}");

                        ImGui.TableNextColumn();
                        ImGui.Text(itemName);

                        ImGui.TableNextColumn();
                        ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                        ImGui.SameLine();
                        ImGui.Text($"{entry.CostAmount}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button(T("Buy 1 Item")))
                        {
                            entry.Select();
                        }

                        ImGui.TableNextColumn();
                        if (ImGui.Button(T("Buy Max")))
                        {
                            if (EzThrottler.Throttle("Buying from shop throttle"))
                            {
                                var amount = shopExchange.CurrencyAmount;
                                var buyAmount = amount / cost;
                                entry.Select((int)buyAmount);
                            }
                        }

                        ImGui.PopID();
                    }
                    ImGui.EndTable();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<Shop>("Shop", out var Shop) && Shop.IsAddonReady)
            {
                var sheet = Svc.Data.GetExcelSheet<Item>();

                var currencyIcon = 65002;
                Svc.Texture.TryGetFromGameIcon(currencyIcon, out var texture);
                PlayerHelper.GetItemCount(1, out var amount);
                ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(26, 26));
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{amount}");

                if (ImGui.Button(T("Copy Item List")))
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < Shop.NumEntries; i++)
                    {
                        var entry = Shop.ShopItems[i];
                        var itemId = entry.ItemId;
                        var itemName = sheet.GetRow(itemId).Name.ToString();

                        sb.AppendLine($"[{itemId}] = new ItemInfo");
                        sb.AppendLine($"\t{{");
                        sb.AppendLine($"\t\tName = \"{itemName}\",");
                        sb.AppendLine($"\t\tCost = {entry.CostAmount},");
                        sb.AppendLine($"\t}},");
                    }
                    ImGui.SetClipboardText(sb.ToString());
                }

                if (ImGui.BeginTable("Item Exchange Window", 5, tableFlags))
                {
                    ImGui.TableSetupColumn("##ItemId");
                    ImGui.TableSetupColumn("##ItemName");
                    ImGui.TableSetupColumn("##Cost1");
                    ImGui.TableSetupColumn("##Cost2");
                    ImGui.TableSetupColumn("##Cost3");

                    for (int i = 0; i < Shop.NumEntries; i++)
                    {
                        var entry = Shop.ShopItems[i];
                        var itemId = entry.ItemId;
                        var cost = entry.CostAmount;

                        var itemName = sheet.GetRow(itemId).Name.ToString();

                        ImGui.PushID(itemId);

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{itemId}");

                        ImGui.TableNextColumn();
                        ImGui.Text(itemName);

                        ImGui.TableNextColumn();
                        ImGui.Image(texture.GetWrapOrEmpty().Handle, new Vector2(20, 20));
                        ImGui.SameLine();
                        ImGui.Text($"{entry.CostAmount}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button(T("Buy 1 Item")))
                        {
                            entry.Select();
                        }

                        ImGui.TableNextColumn();
                        if (ImGui.Button(T("Buy Max")))
                        {
                            if (EzThrottler.Throttle("Buying from shop throttle"))
                            {
                                var buyAmount = amount / cost;
                                entry.Select((int)buyAmount);
                            }
                        }

                        ImGui.PopID();
                    }
                    ImGui.EndTable();
                }
            }
            else
            {
                ImGui.Text(T("Waiting for a shop exchange window to be open"));
            }
        }
    }
}
