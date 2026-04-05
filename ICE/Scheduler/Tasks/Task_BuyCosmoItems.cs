using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ICE.ConfigFiles;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.ConfigFiles.Config;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_BuyCosmoItems
    {
        public static void Enqueue()
        {
            P.TaskManager.Enqueue(Credit_PathToVendor, "Pathing to the credit vendor");

            if (CanPurchaseFromShop(C.CosmoShoppingOrder_Gear, Shop_Cosmocredits.Shop_MountsCards))
            {
                P.TaskManager.EnqueueMulti
                    (
                        new(TalkToCreditNPC, "Npc Talk: Cosmogear"),
                        new(() => SelectShop(0), "Selecting Cosmocredit Exchange"),
                        new(BuyGearItems, "Buying items from the vendor"),
                        new(CloseShop, "Closing the shop menu"),
                        new(DelayInsert, "Delaying between next interaction")
                    );
            }

            if (CanPurchaseFromShop(C.CosmoShoppingOrder, Shop_Cosmocredits.Shop_MateriaDye))
            {
                P.TaskManager.EnqueueMulti
                    (
                        new(TalkToCreditNPC, "Npc Talk: Material Exchange"),
                        new(() => SelectShop(1), "Selecting Material Exchange"),
                        new(BuyMaterialItems, "Buying items from the vendor", Utils.TaskConfig),
                        new(CloseShop, "Closing the shop menu")
                    );
            }
        }

        private static bool CanPurchaseFromShop(List<uint> shoppingOrder, Dictionary<uint, Shop_Cosmocredits.ItemInfo> shopData)
        {
            PlayerHelper.GetItemCount(45690, out var currencyAmount);
            currencyAmount -= C.CosmoKeepAmount;

            foreach (var itemId in shoppingOrder)
            {
                if (!C.CosmoShopping.TryGetValue(itemId, out var item))
                    continue;

                if (!shopData.TryGetValue(itemId, out var shopItem))
                    continue;

                // Check BuyAmount
                if (item.BuyAmount > 0 && currencyAmount >= shopItem.Cost)
                    return true;

                // Check KeepAmount
                PlayerHelper.GetItemCount(itemId, out int currentCount);
                if (item.KeepAmount > currentCount && currencyAmount >= shopItem.Cost)
                    return true;

                // Check KeepBuying
                if (item.KeepBuying && currencyAmount >= shopItem.Cost)
                    return true;
            }

            return false;
        }
        private static bool? Credit_PathToVendor()
        {
            string handle = "[Task_Credits: PathTo]";
            var zoneId = Player.Territory.RowId;
            var npcEntry = NpcData.MoonNpcs[zoneId].Where(x => x.type == NpcData.NpcType.Credit).FirstOrDefault();

            if (npcEntry != null)
            {
                Vector3 randomPos = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, 0.5f);
                if (!Task_NavmeshMove.Task_NavTo(randomPos, distance: 6, npcLoc: npcEntry.Location_Npc).Value)
                {
                    if (EzThrottler.Throttle("Repair move message", 1000))
                        IceLogging.Verbose($"Pathing to repair NPC. Current distance: {Player.DistanceTo(npcEntry.Location_Npc)}", handle);
                }
                else
                {
                    IceLogging.Debug("We're close enough to the repair npc! Continuing on", handle);
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("Error message: NPC", 5000))
                    IceLogging.Error("Hey! We don't have this npc coded yet, which means I forgot bout it, could you let me know\n" +
                                     $"Planet Territory ID: {Player.Territory.RowId}", handle);
            }

            return false;
        }
        private static unsafe bool? TalkToCreditNPC()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectIconString>("SelectIconString", out var iconString) && iconString.IsAddonReady)
            {
                IceLogging.Info("Icon string is visible! Time to shop");
                return true;
            }
            else
            {
                var researchId = NpcData.MoonNpcs[Player.Territory.RowId].Where(x => x.type == NpcData.NpcType.Credit).FirstOrDefault().NpcId;

                Utils.TryGetObjectByDataId(researchId, out var researchNpc);
                if (EzThrottler.Throttle("Interacting with researchingway"))
                {
                    Utils.TargetgameObject(researchNpc);
                    Utils.InteractWithObject(researchNpc);
                }

                if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Closing shop menu"))
                    {
                        ECommons.Automation.Callback.Fire(shopExchange.Base, true, -1);
                    }
                }
            }

            return false;
        }
        private static bool? SelectShop(int shopSelection)
        {
            // Something to consider here... there's 2 different shops. Probably going to need to add a check to see which one we're going to select because I *-know-* people are going to ask about it >.>
            // For now, just going to support the one shop
            if (GenericHelpers.TryGetAddonMaster<SelectIconString>("SelectIconString", out var iconString) && iconString.IsAddonReady)
            {
                if (EzThrottler.Throttle($"Selecting Shop Selection: {shopSelection}"))
                {
                    var select = iconString.Entries[shopSelection];
                    IceLogging.Debug($"Selecting: {select.Text}");
                    select.Select();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                return true;
            }

            return false;
        }
        private static unsafe bool? CloseShop()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("ShopExchangeCurrency", out var shopAddon) && shopAddon->IsReady)
            {
                if (EzThrottler.Throttle("Close Shop"))
                    shopAddon->Close(true);
                return false;
            }
            else
                return true;
        }

        private static int delay = 0;
        private static bool? DelayInsert()
        {
            if (delay > 1)
            {
                delay = 0;
                return true;
            }
            else
            {
                if (FrameThrottler.Throttle("Throttling the frames", 16))
                {
                    delay += 1;
                }
            }
            return false;
        }

        private static int BuyAmount = 0;
        private static int KeepAmount = 0;

        /*
        private static unsafe bool? BuyGearItems()
        {
            bool TryPurchaseGearItem(ShopExchangeCurrency shopExchange, uint currencyAmount, Func<CosmoShoppingList, uint, int> getTargetAmount, Action<int> setAmount)
            {
                if (ItemId != 0)
                {
                    if (PlayerHelper.GetItemCount(ItemId, out var currentAmount) && currencyAmount == previousItemCount)
                        return true;
                    else
                    {
                        if (C.CosmoShopping.TryGetValue(ItemId, out var config))
                        {
                            config.BuyAmount -= BuyAmount;
                            if (config.BuyAmount <= 0)
                                config.BuyAmount = 0;
                            C.Save();
                        }

                        setAmount(1);
                        ItemId = 0;
                        previousItemCount = -1;
                        return true;
                    }
                }

                foreach (var itemId in C.CosmoShoppingOrder_Gear)
                {
                    if (!C.CosmoShopping.TryGetValue(itemId, out var item))
                        continue;

                    int targetAmount = getTargetAmount(item, itemId);
                    if (targetAmount <= 0)
                        continue;

                    var shopExchangeItem = shopExchange.BasicShopItems.FirstOrDefault(x => x.ItemId == itemId);
                    if (shopExchangeItem == null)
                    {
                        if (Shop_Cosmocredits.Shop_MountsCards.TryGetValue(itemId, out var shopInfo))
                        {
                            if (EzThrottler.Throttle("Callback fire"))
                                ECommons.Automation.Callback.Fire(shopExchange.Base, true, 4, -1, 1, shopInfo.Tab);

                            return true;
                        }
                    }

                    int maxAffordable = (int)(currencyAmount / shopExchangeItem.CostAmount);
                    if (maxAffordable <= 0)
                        continue;

                    int buyAmount = 1;

                    if (EzThrottler.Throttle("Selecting Item to Buy"))
                    {
                        shopExchangeItem.Select(buyAmount);
                        ItemId = itemId;
                        PlayerHelper.GetItemCount(ItemId, out previousItemCount);
                    }
                    return true;
                }
                return false;
            }

            if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var YesNo) && YesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle($"Updating Item Count: {ItemId}", 3000))
                    PlayerHelper.GetItemCount(ItemId, out previousItemCount);

                if (EzThrottler.Throttle("Buy Item", 500))
                {
                    YesNo.Yes();
                    if (BuyAmount != 0)
                    {
                        if (C.CosmoShopping.TryGetValue(ItemId, out var config))
                        {
                            config.BuyAmount -= BuyAmount;
                            if (config.BuyAmount <= 0)
                                config.BuyAmount = 0;
                            C.Save();
                        }
                        BuyAmount = 0;
                        KeepAmount = 0;
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                var currencyAmount = shopExchange.CurrencyAmount - (uint)C.CosmoKeepAmount;

                // Then try KeepAmount (accounting for what player already has)
                if (TryPurchaseGearItem(shopExchange, currencyAmount,
                    (item, itemId) =>
                    {
                        PlayerHelper.GetItemCount(itemId, out int currentCount);
                        return Math.Max(0, item.KeepAmount - currentCount);
                    },
                    (amount) => KeepAmount = amount))
                    return false;

                // Try BuyAmount first
                if (TryPurchaseGearItem(shopExchange, currencyAmount, (item, itemId) => item.BuyAmount, (amount) => BuyAmount = amount))
                    return false;

                // Finally try KeepBuying (buy max affordable)
                if (TryPurchaseGearItem(shopExchange, currencyAmount, (item, itemId) => item.KeepBuying ? int.MaxValue : 0, (amount) => KeepAmount = amount))
                    return false;

                return true;
            }

            return false;
        }
        */

        private static uint previousItemId = 0;
        private static int previousItemCount = -1;

        private static unsafe bool? BuyGearItems()
        {
            string tag = "Shopping: Gear Items";

            bool TryPurchaseGearItem(ShopExchangeCurrency shopExchange, uint amountAvailable, bool singleBuy = false, bool keepAmount = false, bool keepBuying = false)
            {
                if (previousItemId != 0 && singleBuy)
                {
                    if (C.CosmoShopping.TryGetValue(previousItemId, out var config))
                    {
                        // We have an item that we're expecting to change +1, so we're going to check that
                        if (PlayerHelper.GetItemCount(previousItemId, out var currentCount))
                        {
                            if (EzThrottler.Throttle($"Item Check Count Log {previousItemId}", 1000))
                            {
                                IceLogging.Verbose($"Checking itemId: {previousItemId}. Last Count: {previousItemCount}", tag);
                            }
                            if (previousItemCount < currentCount)
                            {
                                IceLogging.Verbose("We got an increase in the current count. Time to update the shopping list", tag);
                                if (config.BuyAmount != 0 && singleBuy)
                                {
                                    config.BuyAmount -= 1;
                                    if (config.BuyAmount < 0)
                                        config.BuyAmount = 0;
                                    C.Save();
                                }
                                previousItemId = 0;
                                previousItemCount = -1;
                                IceLogging.Verbose("All Need to keep stuff is reset", tag);
                            }

                            return true;
                        }
                        else
                        {
                            if (EzThrottler.Throttle($"Warning itemID not found: {previousItemId}"))
                                IceLogging.Error($"Hey! {previousItemId} wasn't found... which shouldn't be the case", tag);

                            return false;
                        }
                    }
                    else
                    {
                        IceLogging.Error("Somehow... we're here? Which means something got pulled from the config wrong... We shouldn't be, so we're just going to reset everything/not change values");
                        previousItemId = 0;
                        previousItemCount = -1;
                        return true;
                    }
                }

                foreach (var itemId in C.CosmoShoppingOrder_Gear)
                {
                    if (C.CosmoShopping.TryGetValue(itemId, out var config))
                    {
                        PlayerHelper.GetItemCount(itemId, out var currentCount);

                        IceLogging.Debug($"Checking for ItemID: {itemId}");

                        var shopItem = shopExchange.BasicShopItems.FirstOrDefault(x => x.ItemId == itemId);
                        if (shopItem == null)
                        {
                            if (Shop_Cosmocredits.Shop_MountsCards.TryGetValue(itemId, out var shopInfo))
                            {
                                if (EzThrottler.Throttle("Callback fire"))
                                    ECommons.Automation.Callback.Fire(shopExchange.Base, true, 4, -1, 1, shopInfo.Tab);

                                IceLogging.Verbose("Selecting tab for item", tag);

                                return true;
                            }
                        }

                        var maxAfordable = (int)(amountAvailable / shopItem.CostAmount);
                        if (maxAfordable == 0)
                        {
                            IceLogging.Verbose($"Skipping: {itemId} due to not being able to buy amount: {maxAfordable} | Current Amount: {currentCount}");
                            continue;
                        }

                        bool buyItem = false;

                        if (singleBuy && config.BuyAmount != 0)
                            buyItem = true;
                        else if (config.KeepAmount != 0 && currentCount < config.KeepAmount && keepAmount)
                            buyItem = true;
                        else if (config.KeepBuying && keepBuying)
                            buyItem = true;

                        IceLogging.Debug($"Buy Check: {buyItem} | Single Buy: {singleBuy} | Keep Amount: {keepAmount} | Keep Buying: {keepBuying}");

                        if (buyItem)
                        {
                            if (EzThrottler.Throttle("Selecting item", 500))
                            {
                                shopItem.Select();
                                previousItemId = itemId;
                                previousItemCount = currentCount;
                            }

                            return true;
                        }
                    }
                    else
                    {
                        IceLogging.Error($"No item exist in the gear shop: {itemId}", tag);
                    }
                }

                return false;
            }

            if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var YesNo) && YesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("SelectYesNo"))
                    YesNo.Yes();
            }
            else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                var currencyAmount = shopExchange.CurrencyAmount - (uint)C.CosmoKeepAmount;

                if (TryPurchaseGearItem(shopExchange, currencyAmount, singleBuy: true))
                    return false;

                if (TryPurchaseGearItem(shopExchange, currencyAmount, keepAmount: true))
                    return false;

                if (TryPurchaseGearItem(shopExchange, currencyAmount, keepBuying: true))
                    return false;

                IceLogging.Debug("Gear shop is finished, continuing", tag);
                return true;
            }

            return false;
        }

        private static uint ItemId = 0;

        private static bool? BuyMaterialItems()
        {
            bool TryPurchaseMaterialItem(ShopExchangeCurrency shopExchange, uint currencyAmount, Func<CosmoShoppingList, uint, int> getTargetAmount, Action<int> setAmount)
            {
                foreach (var itemId in C.CosmoShoppingOrder)
                {
                    if (!C.CosmoShopping.TryGetValue(itemId, out var item))
                        continue;

                    int targetAmount = getTargetAmount(item, itemId);
                    if (targetAmount <= 0)
                        continue;

                    var shopExchangeItem = shopExchange.BasicShopItems.FirstOrDefault(x => x.ItemId == itemId);
                    if (shopExchangeItem == null)
                        continue;

                    int maxAffordable = (int)(currencyAmount / shopExchangeItem.CostAmount);
                    if (maxAffordable <= 0)
                        continue;

                    int buyAmount = Math.Min(maxAffordable, targetAmount);
                    if (buyAmount > 99) buyAmount = 99;

                    if (EzThrottler.Throttle("Selecting Item to Buy"))
                    {
                        shopExchangeItem.Select(buyAmount);
                        setAmount(buyAmount);
                        ItemId = itemId;
                    }
                    return true;
                }
                return false;
            }

            if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var YesNo) && YesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("Buy Item", 500))
                {
                    YesNo.Yes();
                    if (BuyAmount != 0)
                    {
                        if (C.CosmoShopping.TryGetValue(ItemId, out var config))
                        {
                            config.BuyAmount -= BuyAmount;
                            if (config.BuyAmount <= 0)
                                config.BuyAmount = 0;
                            C.Save();
                        }
                        BuyAmount = 0;
                        KeepAmount = 0;
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                var currencyAmount = shopExchange.CurrencyAmount - (uint)C.CosmoKeepAmount;

                // Then try KeepAmount (accounting for what player already has)
                if (TryPurchaseMaterialItem(shopExchange, currencyAmount,
                    (item, itemId) =>
                    {
                        PlayerHelper.GetItemCount(itemId, out int currentCount);
                        return Math.Max(0, item.KeepAmount - currentCount);
                    },
                    (amount) => KeepAmount = amount))
                    return false;

                // Try BuyAmount first
                if (TryPurchaseMaterialItem(shopExchange, currencyAmount,
                    (item, itemId) => item.BuyAmount,
                    (amount) => BuyAmount = amount))
                    return false;

                // Finally try KeepBuying (buy max affordable)
                if (TryPurchaseMaterialItem(shopExchange, currencyAmount,
                    (item, itemId) => item.KeepBuying ? int.MaxValue : 0,
                    (amount) => KeepAmount = amount))
                    return false;

                return true;
            }

            return false;
        }
        public static bool CanPurchaseAnyItem()
        {
            // Get current currency amount (you'll need to determine how to get this without the shop window)

            PlayerHelper.GetItemCount(45690, out var currencyAmount);
            currencyAmount -= C.CosmoKeepAmount;

            // Try BuyAmount first
            if (CanPurchaseItem(currencyAmount,
                (item, itemId) => item.BuyAmount))
                return true;

            // Then try KeepAmount (accounting for what player already has)
            if (CanPurchaseItem(currencyAmount,
                (item, itemId) =>
                {
                    PlayerHelper.GetItemCount(itemId, out int currentCount);
                    return Math.Max(0, item.KeepAmount - currentCount);
                }))
                return true;

            // Finally try KeepBuying (buy max affordable)
            if (CanPurchaseItem(currencyAmount,
                (item, itemId) => item.KeepBuying ? int.MaxValue : 0))
                return true;

            // Nothing can be purchased
            return false;
        }
        private static bool CanPurchaseItem(int currencyAmount, Func<CosmoShoppingList, uint, int> getTargetAmount)
        {
            foreach (var GearItemId in C.CosmoShoppingOrder_Gear)
            {
                if (!C.CosmoShopping.TryGetValue(GearItemId, out var item))
                    continue;

                int targetAmount = getTargetAmount(item, GearItemId);
                if (targetAmount <= 0)
                    continue;

                // Check if item exists in the cosmocredit shop dictionary
                if (!Shop_Cosmocredits.Shop_MountsCards.TryGetValue(GearItemId, out var shopItem))
                    continue;

                int maxAffordable = (int)(currencyAmount / shopItem.Cost);
                if (maxAffordable <= 0)
                    continue;

                // We can afford to buy at least one of this item
                return true;
            }

            foreach (var itemId in C.CosmoShoppingOrder)
            {
                if (!C.CosmoShopping.TryGetValue(itemId, out var item))
                    continue;

                int targetAmount = getTargetAmount(item, itemId);
                if (targetAmount <= 0)
                    continue;

                // Check if item exists in the cosmocredit shop dictionary
                if (!Shop_Cosmocredits.Shop_MateriaDye.TryGetValue(itemId, out var shopItem))
                    continue;

                int maxAffordable = (int)(currencyAmount / shopItem.Cost);
                if (maxAffordable <= 0)
                    continue;

                // We can afford to buy at least one of this item
                return true;
            }
            return false;
        }
    }
}
