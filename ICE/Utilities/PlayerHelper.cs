using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Utilities.Cosmic_Helper;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ICE.Utilities;

public class PlayerHelper
{
    // A lot of these functions are dupes to what is in Ecommons: GameHelper.Player
    // Which means that a lot of these can get depreciated becuase they are either:
    // -> Safer in how they are grabbed
    // -> Less Reduntant in code
    // -> Just genereally better 

    public static bool UsingSupportedJob()
    {
        var jobId = (uint)Player.Job;
        return (CosmicHelper.CrafterJobList.Contains(jobId) || CosmicHelper.GatheringJobList.Contains(jobId));
    }

    public static bool IsInCosmicZone() => IsInSinusArdorum() || IsInPhaenna() || IsInOizys() || IsInAuxesia();
    public static bool IsInSinusArdorum() => IsInZone(1237);
    public static bool IsInPhaenna() => IsInZone(1291);
    public static bool IsInOizys() => IsInZone(1310);
    public static bool IsInAuxesia() => IsInZone(1319);
    public static bool IsInZone(uint zoneID) => Svc.ClientState.TerritoryType == zoneID;
    public static IPlayerCharacter? LocalPlayer => Svc.Objects.LocalPlayer;
    private static unsafe float AnimationLock => *(float*)((nint)ActionManager.Instance() + 8);
    public static bool IsAnimationLocked => AnimationLock > 0;
    public static bool CustomIsBusy => GenericHelpers.IsOccupied() || LocalPlayer.IsCasting || IsAnimationLocked;
    public static bool IsScreenReady()
    {
        return !Svc.Condition[ConditionFlag.BetweenAreas] &&
               !Svc.Condition[ConditionFlag.BetweenAreas51] &&
               !Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent] &&
               !Svc.Condition[ConditionFlag.WatchingCutscene] &&
               !Svc.Condition[ConditionFlag.WatchingCutscene78];
    }
    public static unsafe bool HasStatusId(params uint[] statusIDs)
    {
        if (LocalPlayer == null)
            return false;

        var statusID = LocalPlayer.StatusList
            .Select(se => se.StatusId)
            .ToList().Intersect(statusIDs)
            .FirstOrDefault();

        return statusID != default;
    }
    public static int GetGp()
    {
        uint gp = LocalPlayer.CurrentGp;
        return (int)gp;
    }
    public static int MaxGp()
    {
        var maxGp = LocalPlayer.MaxGp;
        return (int)maxGp;
    }
    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);
    public static unsafe bool GetItemCount(uint itemID, out int count, bool includeHq = true, bool includeNq = true)
    {
        try
        {
            itemID = itemID >= 1_000_000 ? itemID - 1_000_000 : itemID;
            count = 0;
            if (includeHq)
                count += InventoryManager.Instance()->GetInventoryItemCount(itemID, true);
            if (includeNq)
                count += InventoryManager.Instance()->GetInventoryItemCount(itemID, false);
            count += InventoryManager.Instance()->GetInventoryItemCount(itemID + 500_000);
            return true;
        }
        catch
        {
            count = 0;
            return false;
        }
    }
    public static bool HasFoodRunning()
    {
        if (!C.UseGatheringFood || C.GatheringFood == 0)
            return true;

        var foodBuff = LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48 && x.RemainingTime > 10f);
        if (foodBuff == null)
            return false;
        if (Svc.Data.GetExcelSheet<Item>().TryGetRow(C.GatheringFood, out var itemInfo))
        {
            var desiredFood = itemInfo.ItemAction.Value;
            if (foodBuff.Param == desiredFood.DataHQ[1] + 10000)
                return true;
            if (foodBuff.Param == desiredFood.Data[1])
                return true;
        }

        return false;
    }
    public static unsafe bool NeedsRepair(float below = 0)
    {
        string tag = "Needs Repair";

        var im = InventoryManager.Instance();
        if (im == null)
        {
            IceLogging.Error("InventoryManager was null");
            return false;
        }

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
        {
            IceLogging.Error("InventoryContainer was null", tag);
            return false;
        }

        if (!equipped->IsLoaded)
        {
            IceLogging.Error($"InventoryContainer is not loaded");
            return false;
        }

        for (var i = 0; i < equipped->Size; i++)
        {
            var item = equipped->GetInventorySlot(i);
            if (item == null)
                continue;

            var itemCondition = Convert.ToInt32(Convert.ToDouble(item->Condition) / 30000.0 * 100.0);

            if (itemCondition <= below)
            {
                IceLogging.Debug($"Found an item that needed repair. Condition: {itemCondition}");
                return true;
            }
        }

        return false;
    }

    public static unsafe bool AnyNeedsRepair(float below = 0)
    {
        string tag = "All Repair Check";

        var im = InventoryManager.Instance();
        if (im == null)
        {
            IceLogging.Error("Inventory Manager was null, so can't check for repair status", tag);
            return false;
        }

        List<InventoryType> listInventory = new()
        {
            InventoryType.ArmoryMainHand,
            InventoryType.ArmoryOffHand,
            InventoryType.ArmoryHead,
            InventoryType.ArmoryBody,
            InventoryType.ArmoryHands,
            InventoryType.ArmoryLegs,
            InventoryType.ArmoryFeets,
            InventoryType.ArmoryEar,
            InventoryType.ArmoryNeck,
            InventoryType.ArmoryWrist,
            InventoryType.ArmoryRings,
            InventoryType.EquippedItems
        };

        foreach (var type in listInventory)
        {
            var inventory = im->GetInventoryContainer(type);
            if (inventory == null)
            {
                IceLogging.Error($"{type} has returned null, going to skip this for the check");
                continue;
            }

            if (!inventory->IsLoaded)
            {
                IceLogging.Error($"Inventory {type} is reporting not loaded, skipping", tag);
                continue;
            }

            for (var i = 0; i < inventory->Size; i++)
            {
                var item = inventory->GetInventorySlot(i);
                if (item == null)
                    continue;

                var itemCondition = Convert.ToInt32(Convert.ToDouble(item->Condition) / 30000.0 * 100.0);

                if (itemCondition <= below)
                {
                    IceLogging.Debug($"Found an item that needed repair. Condition: {itemCondition}", tag);
                    return true;
                }
            }

        }

        IceLogging.Debug("Repair all check has concluded, no item can be repaired", tag);
        return false;
    }

    public class ManipInfo
    {
        public uint ActionId { get; set; }
        public bool HasUnlocked { get; set; }
    }
    public static Dictionary<uint, ManipInfo> ManipClassInfo = new()
    {
        [8] = new ManipInfo { ActionId = 4574, HasUnlocked = true },
        [9] = new ManipInfo { ActionId = 4575, HasUnlocked = true },
        [10] = new ManipInfo { ActionId = 4576, HasUnlocked = true },
        [11] = new ManipInfo { ActionId = 4577, HasUnlocked = true },
        [12] = new ManipInfo { ActionId = 4578, HasUnlocked = true },
        [13] = new ManipInfo { ActionId = 4579, HasUnlocked = true },
        [14] = new ManipInfo { ActionId = 4580, HasUnlocked = true },
        [15] = new ManipInfo { ActionId = 4581, HasUnlocked = true },
    };
    public static unsafe void UpdateHasManip()
    {
        if (Player.IsBusy)
            return;

        if (Player.Mounted || Player.IsJumping)
            return;

        if (Svc.Condition[ConditionFlag.Crafting] || Svc.Condition[ConditionFlag.ExecutingCraftingAction] || Svc.Condition[ConditionFlag.PreparingToCraft])
            return;

        if (Svc.Condition[ConditionFlag.Gathering] || Svc.Condition[ConditionFlag.ExecutingGatheringAction])
            return;

        foreach (var jobId in CosmicHelper.CrafterJobList)
        {
            if (ManipClassInfo.TryGetValue(jobId, out var info))
            {
                info.HasUnlocked = ActionManager.Instance()->GetActionStatus(ActionType.Action, info.ActionId, checkRecastActive: false, checkCastingActive: false) is 574 or 586;
            }
        }
    }
}
