using ICE.ExtraUtil;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_ShopExchange
    {
        private static int BuyAmount = 1;
        public static void Draw()
        {
            if (ImGui.Button("Merge Items"))
            {
                if (EzThrottler.Throttle("Merge Throttle"))
                    Task_BuyCosmoItems.MergeItems();
            }

            if (GenericHelpers.TryGetAddonMaster<ShopExchangeItem>(out var shopExchange) && shopExchange.IsAddonReady)
            {
                ImGui.SameLine();
                if (ImGui.Button("Queue Exchange Buy"))
                {
                    if (!P.TaskManager.IsBusy)
                        P.TaskManager.Enqueue(() => Task_BuyCosmoItems.BuyPlanetBoolets(), Utils.TaskConfig);
                }
                if (ImGui.BeginTable("Shop Exchange Items", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Item");
                    ImGui.TableSetupColumn("Have");
                    ImGui.TableSetupColumn("Required Items");
                    ImGui.TableSetupColumn("Buy Amount");

                    ImGui.TableHeadersRow();

                    foreach (var item in shopExchange.ItemInfo)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (ExcelHelper.ItemSheet.TryGetRow(item.ItemId, out var sheetInfo))
                        {
                            var itemName = sheetInfo.Name.ToString();

                            Svc.Texture.TryGetFromGameIcon((uint)sheetInfo.Icon, out var icon);
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), itemName, $"{item.ItemId}_{itemName}", new(24, 24));
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{item.Quantity}");

                        ImGui.TableNextColumn();
                        for (int i = 0; i < item.ExchangeItems.Count; i++)
                        {
                            var exchangeItem = item.ExchangeItems[i];
                            if (ExcelHelper.ItemSheet.TryGetRow(exchangeItem.ItemId, out var itemInfo))
                            {
                                if (i != 0)
                                    ImGui.SameLine();

                                Svc.Texture.TryGetFromGameIcon((uint)itemInfo.Icon, out var icon);
                                ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{exchangeItem.RequiredAmount}", $"{exchangeItem.ItemId}_{itemInfo.Name.ToString()}", new(24, 24));
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip($"{itemInfo.Name.ToString()}");
                                }
                            }
                        }

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(200);
                        ImGui.InputInt($"Buy Amount", ref BuyAmount);
                        ImGui.SameLine();
                        if (ImGui.Button($"Buy Item##{item.ItemId}_Buy"))
                        {
                            item.Select(BuyAmount);
                        }
                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
