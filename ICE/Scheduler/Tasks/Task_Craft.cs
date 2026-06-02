using Dalamud.Game.ClientState.Conditions;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_Craft
    {
        public static void Enqueue()
        {
            if (P.Artisan.IsBusy())
            {
                P.TaskManager.Enqueue(() => WaitingForArtisan(), "Waiting for artisan to finish crafting");
                P.TaskManager.Enqueue(() => Task_CheckScore.Enqueue(), "Checking score");
            }
            else
            {
                P.TaskManager.Enqueue(() => Task_CheckScore.Enqueue(), "Checking Score");
                P.TaskManager.Enqueue(() => CheckMaterials(), "Checking materials", Utils.TaskConfig);
            }
        }

        private static bool? WaitingForArtisan()
        {
            if (!P.Artisan.IsBusy())
            {
                IceLogging.Info("Artisan is no longer running, continuing the process");
                return true;
            }
            else
            {
                if (Svc.Condition[ConditionFlag.ExecutingCraftingAction])
                {
                    // Need to add a timer check here. Make it configuarable maybe... 10s?
                    // If the timer exceeds 10 seconds, then that means we're stuck in an animation lock
                    // then need to cancel them all and just force abandon lock failsafe
                }
                if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var moonHud))
                {
                    if (!AddonHelper.IsAddonActive("WKSMissionInfomation"))
                    {
                        if (EzThrottler.Throttle("Opening mission scoring info"))
                        {
                            moonHud.Mission();
                        }
                    }
                }
            }
            return false;
        }
        private static uint throttleCounter = 0;
        private static void InsertArtisanWait(KeyValuePair<ushort, CosmicHelper.CraftingInfo> item, int amount)
        {
            P.TaskManager.InsertMulti(
                new(() => ThrottleArtisanTaskV2(item, amount), "Telling artisan to craft"),
                new(() => WaitingForArtisan(), "Waiting for artisan")
            );
        }

        private static bool? ThrottleArtisanTaskV2(KeyValuePair<ushort, CosmicHelper.CraftingInfo> item, int amount)
        {
            int delay = C.DelayCraft ? C.DelayCraftIncrease : 25;

            var craftId = item.Key;
            var recipeId = item.Value.RecipeId;
            var itemId = item.Value.ItemId;
            var expert = item.Value.ExpertCraft;

            var missionId = CosmicHelper.CurrentLunarMission;
            var missionConfig = C.MissionConfig[missionId];

            if (missionConfig.CraftSettings.TryGetValue(recipeId, out var recipeConfig))
            {
                if (EzThrottler.Throttle("Applying Config States", 1000))
                {
                    IceLogging.Info($"Applying config states for the following recipeID: {recipeId}");
                    P.Artisan.CheckArtisanSettings((ushort)recipeId, CosmicHelper.CurrentLunarMission, expert, Mission_Settings.Mode == ModeSelect.LevelMode);
                }
            }
            else
            {
                missionConfig.CraftSettings[recipeId] = new();
                C.Save();
            }

            if (EzThrottler.Throttle("Waiting X Amount of seconds for artisan", delay))
            {
                throttleCounter += 1;
            }

            if (throttleCounter >= 3)
            {
                if (EzThrottler.Throttle("Artisan Crafting Task"))
                {
                    IceLogging.Debug($"Telling Artisan to craft: {itemId} -> {amount} times");
                    P.Artisan.CraftItem(craftId, amount);
                }

                if (P.TaskManager.IsBusy)
                {
                    throttleCounter = 0;
                    return true;
                }
            }

            return false;
        }

        private static bool? CheckMaterials()
        {
            var id = CosmicHelper.CurrentLunarMission;
            var mission = CosmicHelper.SheetMissionDict[id];

            bool provisional = mission.IsProvisional;

            if (!P.Artisan.IsBusy())
            {
                if (mission.Crafts_Pre.Count > 0)
                {
                    // Mission has pre-crafts that are required. 
                    // Checking to see if you have enough pre-crafts first
                    var preCraft = mission.Crafts_Pre.FirstOrDefault();
                    var mainCraft = mission.Crafts_Main.FirstOrDefault();

                    var preItemId = preCraft.Value.ItemId;
                    var mainItemId = mainCraft.Value.ItemId;

                    PlayerHelper.GetItemCount(preCraft.Value.ItemId, out var preItemAmount);
                    PlayerHelper.GetItemCount(preCraft.Value.RequiredItems.FirstOrDefault().Key, out var moonCrateCount);
                    PlayerHelper.GetItemCount(mainCraft.Value.ItemId, out var mainItemCount);

                    if (preItemAmount >= mainCraft.Value.RequiredItems[preItemId])
                    {
                        IceLogging.Info($"Required pre-Item count: {mainCraft.Value.RequiredItems[preItemId]} | amount necessary: {preItemAmount}");

                        // There's enough items to craft the mainhand. Telling it to craft it instead. 
                        if (mainItemCount < mainCraft.Value.RequiredAmount)
                        {
                            // you don't have enough of the pre-crafts to craft the main item. 
                            // going to tell artisan to just kick it into gear
                            bool SpecialExpert = mainCraft.Value.ExpertCraft && provisional;
                            var craftAmount = mainCraft.Value.RequiredAmount - mainItemCount;
                            InsertArtisanWait(mainCraft, craftAmount);
                            IceLogging.Info($"Telling artisan to craft: {mainCraft.Value.ItemId} -> {craftAmount}", "[Task Craft: Check Materials]");
                            return true;
                        }
                        else
                        {
                            // you have enough of the main hand item. But you still are crafting. So time to just craft 1 more
                            bool SpecialExpert = mainCraft.Value.ExpertCraft && provisional;
                            InsertArtisanWait(mainCraft, 1);
                            IceLogging.Info($"Current item count of: {mainCraft.Value.ItemId} | {mainItemCount}");
                            IceLogging.Info($"Telling artisan to craft: {mainCraft.Value.ItemId} -> 1", "[Task Craft: Check Materials]");
                            return true;
                        }

                    }
                    else if (moonCrateCount >= preCraft.Value.RequiredAmount)
                    {
                        // You should have enough to make this pre-craft. Initiating the thing now.
                        var craftAmount = preCraft.Value.RequiredAmount - preItemAmount;

                        if (mainCraft.Value.RequiredAmount > 1 && mainItemCount == 0)
                        {
                            craftAmount = mainCraft.Value.RequiredAmount * (preCraft.Value.RequiredAmount - preItemAmount);
                        }
                        if (craftAmount < 1)
                            craftAmount = 1;

                        bool SpecialExpert = preCraft.Value.ExpertCraft && provisional;
                        InsertArtisanWait(preCraft, craftAmount);
                        IceLogging.Info($"Found a material that still needed to be crafted", "[Task Craft: Check Materials]");
                        return true;
                    }
                    else
                    {
                        IceLogging.Info($"Somehow, out of mats. Need to exit. And either attempt to turnin, or just straight up abandon.", "[Task Craft: Check Materials]");
                        SchedulerMain.State = IceState.AbandonMission;
                        P.TaskManager.Tasks.Clear();
                        return true;
                    }
                }
                else
                {
                    // This is the case when you need multiple items, or even just a single item.
                    foreach (var craft in mission.Crafts_Main)
                    {
                        PlayerHelper.GetItemCount(craft.Value.ItemId, out var reqAmount);
                        if (reqAmount < craft.Value.RequiredAmount)
                        {
                            // If you need less than what is necessary, this should change the count to be proper
                            reqAmount = craft.Value.RequiredAmount - reqAmount;

                            // Found an item that needs to be crafted. Time to check if you have enough of the material
                            var craftMaterial = ExcelHelper.RecipeSheet.GetRow(craft.Key).Ingredient[0].RowId;
                            if (PlayerHelper.GetItemCount(craftMaterial, out var itemAmount) && itemAmount >= reqAmount)
                            {
                                bool SpecialExpert = craft.Value.ExpertCraft && provisional;
                                InsertArtisanWait(craft, reqAmount);
                                IceLogging.Info($"Telling artisan to craft: {craft.Value.ItemId} -> {reqAmount}", "[Craft: No Pre-Mats]");
                                return true;
                            }
                            else
                            {
                                // You don't have enough to craft this for the mission. Exiting out and checking for score/force abandon
                                IceLogging.Info("You have no remaining items to craft the main crafting items. Going to abandon the mission now", "[Crafts: No Pre-Mats]");
                                SchedulerMain.State = IceState.AbandonMission;
                                P.TaskManager.Tasks.Clear();
                                return true;
                            }
                        }
                    }

                    var moreCraft = mission.Crafts_Main.FirstOrDefault();
                    // If you've gotten this far, that means you still need scoring. Just going to queue up the first mission (if possible)
                    // If you need less than what is necessary, this should change the count to be proper
                    var AdditionalItem = 1;

                    // Found an item that needs to be crafted. Time to check if you have enough of the material
                    var moreCraftMaterial = ExcelHelper.RecipeSheet.GetRow(moreCraft.Key).Ingredient[0].RowId;
                    if (PlayerHelper.GetItemCount(moreCraftMaterial, out var moreItemAmount) && moreItemAmount >= AdditionalItem)
                    {
                        InsertArtisanWait(moreCraft, AdditionalItem);
                        IceLogging.Info($"Telling artisan to craft: {moreCraft.Value.ItemId} -> {AdditionalItem}", "[Craft: No Pre-Mats]");
                        return true;
                    }
                    else
                    {
                        // You don't have enough to craft this for the mission. Exiting out and checking for score/force abandon
                        SchedulerMain.State = IceState.AbandonMission;
                        IceLogging.Info("You have no remaining items to craft the pre-crafts. Going to abandon the mission now", "[Crafts: No Pre-Mats]");
                        P.TaskManager.Tasks.Clear();
                        return true;
                    }

                }
            }
            else
            {
                if (EzThrottler.Throttle("Artisan Busy Log", 3000))
                {
                    IceLogging.Debug("Artisan is currently busy... so we're properly waiting for it to finish");
                }
            }

            return false;
        }
    }
}
