using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.Ui.DebugWindowTabs.Ui_OyzinMap;

namespace ICE.Scheduler.Tasks
{
    internal class Task_ArtifactSearch
    {
        public static void EnqueueBuy()
        {
            P.TaskManager.EnqueueMulti
                (
                    new(Drone_PathToVendor, "Drone NPC: Path to"),
                    new(TalkToDroneNpc, "Drone NPC: Talk"),
                    new(SelectShop, "Drone NPC: Select Shop"),
                    new(BuyDroneCrates, "Drone NPC: Buying"),
                    new(ExitDroneShop, "Drone NPC: Leaving Shop")
                );
        }
        private static bool? Drone_PathToVendor()
        {
            string handle = "[Task_Artifact: PathTo]";
            var zoneId = Player.Territory.RowId;

            if (NpcData.TryGetNpc(zoneId, NpcData.NpcType.Drone, out var npcEntry))
            {
                Vector3 randomPos = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, 0.5f);
                if (!Task_NavmeshMove.Task_NavTo(randomPos, distance: 6, npcLoc: npcEntry.Location_Npc).Value)
                {
                    if (EzThrottler.Throttle("Drone Move Message", 1000))
                        IceLogging.Verbose($"Pathing to drone NPC. Current distance: {Player.DistanceTo(npcEntry.Location_Npc)}", handle);
                }
                else
                {
                    IceLogging.Debug("We're close enough to the drone npc! Continuing on", handle);
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
        private static bool? TalkToDroneNpc()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var iconString) && iconString.IsAddonReady)
            {
                IceLogging.Info("Icon string is visible! Time to shop");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Throttle talking", 100))
                    talk.Click();
            }
            else
            {
                if (NpcData.TryGetNpc(Player.Territory.RowId, NpcData.NpcType.Drone, out var droneInfo))
                {
                    Utils.TryGetObjectByDataId(droneInfo.NpcId, out var droneNpc);
                    if (EzThrottler.Throttle("Interacting with researchingway"))
                    {
                        Utils.TargetgameObject(droneNpc);
                        Utils.InteractWithObject(droneNpc);
                    }
                }
            }

            return false;
        }
        private static bool? SelectShop()
        {
            if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                IceLogging.Debug("Shop Exchange Currency Addon is Ready!");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting Materia Selection"))
                {
                    var select = selectString.Entries[0];
                    IceLogging.Verbose($"Selecting: {select.Text}");
                    select.Select();
                }
            }

            return false;
        }
        private static bool? BuyDroneCrates()
        {
            string tag = "[Task_ArtifactSearch: Buy Drones]";

            if (EzThrottler.Throttle("General Buying Throttle"))
            {
                if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var YesNo) && YesNo.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Buy Drone Boxes"))
                    {
                        YesNo.Yes();
                    }
                }
                else if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
                {
                    // There's only 1 item here. So we just need to make sure we have enough to buy it
                    var currency = shopExchange.CurrencyAmount;
                    var item = shopExchange.BasicShopItems[0];
                    var maxAmount = (int)(currency / item.CostAmount); // Maximum we could afford
                    PlayerHelper.GetItemCount(item.ItemId, out var currentAmount);

                    if (C.Cosmodrone_MaxKeep != 0)
                    {
                        var remainingSpace = C.Cosmodrone_MaxKeep - currentAmount;
                        if (remainingSpace <= 0)
                        {
                            IceLogging.Debug($"Already at or above max keep limit ({currentAmount}/{C.Cosmodrone_MaxKeep}), skipping purchase", tag);
                            return true;
                        }

                        maxAmount = Math.Min(maxAmount, remainingSpace);
                    }

                    if (maxAmount > 0 && item.CostAmount <= currency)
                    {
                        if (EzThrottler.Throttle("Selecting to buy this item", 1000))
                        {
                            item.Select(maxAmount);
                            IceLogging.Debug($"Purchasing {maxAmount} items (current: {currentAmount}, max keep: {C.Cosmodrone_MaxKeep})", tag);
                        }
                    }
                    else
                    {
                        IceLogging.Debug("We don't have any more currency to buy the dronebit, so continuing on", tag);
                        return true;
                    }
                }
            }

            return false;
        }
        private static bool? ExitDroneShop()
        {
            string tag = "[Task: ArtifactBuy | Exit Shop]";
            if (GenericHelpers.TryGetAddonMaster<ShopExchangeCurrency>("ShopExchangeCurrency", out var shopExchange) && shopExchange.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing the window"))
                {
                    IceLogging.Verbose("Closing the shop exchange window", tag);
                    GenericHandlers.FireCallback("ShopExchangeCurrency", true, -1);
                }
            }
            else
            {
                return true;
            }

            return false;
        }
        public static bool CanBuyDroneBoxes()
        {
            var territory = Player.Territory.RowId;
            if (!CosmicMoonRegistry.TryGetDronebit(territory, out var dronebitInfo))
                return false;

            bool shouldBuyItems = false;

            if (PlayerHelper.GetItemCount(dronebitInfo.creditId, out var bitCount))
            {
                var buyAt = C.Cosmodrone_BuyAt;
                if (buyAt <= bitCount)
                {
                    shouldBuyItems = true;
                }
            }

            if (PlayerHelper.GetItemCount(dronebitInfo.boxId, out var boxCount))
            {
                var maxBox = C.Cosmodrone_MaxKeep;
                if (maxBox != 0 && boxCount >= maxBox)
                {
                    shouldBuyItems = false;
                }
            }

            return shouldBuyItems;
        }

        // Going to drone locations
        private static Vector3 droneLoc = Vector3.Zero;
        public static void Enqueue_DroneCheck()
        {
            P.TaskManager.EnqueueMulti
                (
                    new(CloseMapInfo, "Making sure map is close"),
                    new(OpenMapInfo, "Re-opening map to refresh"),
                    new(CloseMapInfo, "Closing one more time cause we don't need it"),
                    new(CheckBoxStatus, "Checking Box Status")
                );
        }
        public class TempMapMarkerData
        {
            public Vector3 Position { get; set; }
            public uint IconId { get; set; }
        }
        public unsafe static List<TempMapMarkerData> GetAllEventMarkers()
        {
            var markers = new List<TempMapMarkerData>();
            var agentMap = AgentMap.Instance();

            if (agentMap == null) return markers;

            // Event markers (what you already have)
            foreach (var marker in agentMap->EventMarkers)
            {
                markers.Add(new TempMapMarkerData
                {
                    Position = marker.Position,
                    IconId = marker.IconId,
                });
            }

            return markers;
        }
        public static unsafe bool IsTreasureDetected()
        {
            var mapMarkers = GetAllEventMarkers();
            var marker = mapMarkers.Where(x => x.IconId == 63989).FirstOrDefault();

            return marker != null;
        }
        public static bool? RefreshMapInfo()
        {
            P.TaskManager.InsertMulti
                (
                    new(CloseMapInfo, "Making sure map is close"),
                    new(OpenMapInfo, "Re-opening map to refresh"),
                    new(CloseMapInfo, "Closing one more time cause we don't need it"),
                    new(CheckBoxStatus, "Checking Box Status")
                );
            return true;
        }
        public static unsafe bool? CheckBoxStatus()
        {
            droneLoc = Vector3.Zero;
            string tag = "[Task_Artifact: CheckBoxStatus]";

            var mapMarkers = GetAllEventMarkers();
            var marker = mapMarkers.Where(x => x.IconId == 63989).FirstOrDefault();
            if (!CosmicMoonRegistry.TryGetDronebit(Player.Territory.RowId, out var dronebit))
                return false;

            uint itemId = dronebit.boxId;

            if (marker != null)
            {
                if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var hud) && hud.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Close mission window"))
                        GenericHandlers.FireCallback("WKSMission", true, -1);
                }

                IceLogging.Debug("We've found the map flag! Setting it for us to travel to", tag);
                droneLoc = marker.Position;
                TryDailyRoutinesTeleport(droneLoc, tag);
                P.TaskManager.Insert(InteractWithDrone, "Interact with drone");
                Task_NavmeshMove.Enqueue_NavmeshTask(droneLoc, false, 3.5f);
                return true;
            }
            else
            {
                if (PlayerHelper.GetItemCount(itemId, out var count) && count > 0)
                {
                    IceLogging.Debug("We have a crate to use! Initiating the task to start using it", tag);
                    P.TaskManager.Insert(UseDroneBox, "Use Drone Box");
                    return true;
                }
                else
                {
                    IceLogging.Debug($"We are out of boxes, and we have no markers. So we're continuing on with the normal task");
                    if (SchedulerMain.State == IceState.ArtifactSearch)
                    {
                        SchedulerMain.State = IceState.Idle;
                        P.TaskManager.Tasks.Clear();
                    }
                    return true;
                }
            }
        }
        private static bool? InteractWithDrone()
        {
            string tag = "[Task_Artifact: Drone Interact]";

            var artifact = Svc.Objects.Where(x => x.BaseId == 2015138).FirstOrDefault();

            if (artifact != null)
            {
                if (Player.DistanceTo(artifact.Position) < 4)
                {
                    if (P.Navmesh.IsRunning())
                        P.Navmesh.Stop();

                    if (!Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
                    {
                        Utils.TargetgameObject(artifact);
                        Utils.InteractWithObject(artifact);
                        IceLogging.Verbose($"Drone has been found! Interacting with it", tag);
                    }
                }
            }
            else
            {
                P.TaskManager.Tasks.Clear();
                return true;
            }

            return false;
        }
        private static unsafe bool? UseDroneBox()
        {
            string tag = "[Task_ArtifactSearch: Use Drone Box]";

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("AreaMap", out var mapAddon) && GenericHelpers.IsAddonReady(mapAddon))
            {
                P.TaskManager.Insert(() => RefreshMapInfo(), "Task Artifact: Check Box Status", Utils.TaskConfig);
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var YesNo) && YesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes"))
                {
                    IceLogging.Verbose($"Text: {YesNo.Text}");
                    YesNo.Yes();
                }
            }
            else
            {
                if (Player.Mounted || Player.IsJumping)
                {
                    Utils.Dismount();
                    return false;
                }

                var actionManager = ActionManager.Instance();
                if (!CosmicMoonRegistry.TryGetDronebit(Player.Territory.RowId, out var dronebit))
                    return false;

                uint itemId = dronebit.boxId;

                var status = actionManager->GetActionStatus(ActionType.Item, itemId);

                if (status == 0)
                {
                    if (EzThrottler.Throttle("Use Drone", 1000))
                        UseDrone();
                }
                else
                {
                    if (EzThrottler.Throttle("Using drone throttle"))
                        IceLogging.Verbose("We're waiting for the addon map to be visible. If it's not then there's a problem\n" +
                            $"Status is currently: {status}", tag);
                }
            }
                
            return false;
        }

        private static void TryDailyRoutinesTeleport(Vector3 destination, string tag)
        {
            if (!C.Cosmodrone_UseDailyRoutinesTP)
                return;

            if (!Utils.HasPlugin("DailyRoutines"))
            {
                if (EzThrottler.Throttle("DroneMissingDailyRoutines", 8000))
                {
                    IceLogging.Warning("未检测到 Daily Routines，已回退原有寻路。", tag);
                }
                return;
            }

            if (EzThrottler.Throttle("DroneDailyRoutinesTeleport", 2500))
            {
                var command = string.Format(
                    CultureInfo.InvariantCulture,
                    "/pdrtp pos {0:F2} {1:F2} {2:F2}",
                    destination.X,
                    destination.Y,
                    destination.Z);

                Svc.Commands.ProcessCommand(command);
                IceLogging.Debug($"已尝试 Daily Routines 传送：{command}", tag);
            }
        }

        public static unsafe bool? OpenMapInfo()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("AreaMap", out var mapAddon) && GenericHelpers.IsAddonReady(mapAddon))
            {
                return true;
            }
            else
            {
                if (EzThrottler.Throttle("Opening map again", 500))
                {
                    var map = Player.Territory.Value.Map.Value;
                    var territoryid = Player.Territory.RowId;
                    var agent = AgentMap.Instance();

                    agent->OpenMap(map.RowId, territoryid);
                }
                return false;
            }
        }
        public static unsafe bool? CloseMapInfo()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("AreaMap", out var mapAddon) && GenericHelpers.IsAddonReady(mapAddon))
            {
                if (EzThrottler.Throttle("Closing map temp"))
                {
                    GenericHandlers.FireCallback("AreaMap", true, -1);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        private static unsafe void UseDrone()
        {
            if (!CosmicMoonRegistry.TryGetDronebit(Player.Territory.RowId, out var dronebit))
                return;

            uint itemId = dronebit.boxId;
            var inventoryManager = InventoryManager.Instance();

            // Array of inventory types to check
            var inventoryTypes = new[]
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4
            };

            foreach (var invType in inventoryTypes)
            {
                var container = inventoryManager->GetInventoryContainer(invType);
                if (container == null) continue;

                for (int i = 0; i < container->Size; i++)
                {
                    var item = container->GetInventorySlot(i);
                    if (item != null && item->ItemId == itemId)
                    {
                        // Use the item from inventory
                        if (EzThrottler.Throttle("Using item"))
                            IceLogging.Verbose($"Use Item: {itemId} | Inventory Type: {invType.ToString()} | Slot: {i}");
                        AgentInventoryContext.Instance()->UseItem(item->ItemId, invType, (uint)i, 0);
                        return;
                    }
                }
            }

            // If we get here, item wasn't found
            PluginLog.Warning($"Item {itemId} not found in any inventory container");
        }
    }
}
