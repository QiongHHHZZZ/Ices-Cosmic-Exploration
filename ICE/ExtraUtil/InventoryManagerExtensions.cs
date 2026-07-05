using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;

namespace ICE.ExtraUtil;

// Borrowed from this: https://github.com/Jaksuhn/clib/blob/3ed208db2a7c3dbf6509cb574bb8cbc25dcf3f35/Extensions/ClientStructs/InventoryManagerExtensions.cs#L7
public static unsafe class InventoryManagerExtensions
{
    extension(InventoryManager)
    {
        public static bool IsUpdating => RaptureAtkModule.Instance()->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate);
        public static int GetEmptySlots(params InventoryType[] inventories)
        => inventories.Length == 0 ? (int)InventoryManager.Instance()->GetEmptySlotsInBag()
            : inventories.ToList().Sum(i => InventoryManager.Instance()->GetInventoryItems(i).Count(item => item.Value->ItemId == 0));

        public static List<ItemHandle> GetHqItems(params InventoryType[] inventories)
            => inventories.Length == 0 ? [.. InventoryType.FullInventory.SelectMany(inv => InventoryManager.Instance()->GetInventoryItems(inv))
                .Where(item => item.Value->ItemId != 0 && item.Value->Flags == InventoryItem.ItemFlags.HighQuality)
                .Select(item => (ItemHandle)item)]
                : [.. inventories.SelectMany(inv => InventoryManager.Instance()->GetInventoryItems(inv))
                    .Where(item => item.Value->ItemId != 0 && item.Value->Flags == InventoryItem.ItemFlags.HighQuality)
                    .Select(item => (ItemHandle)item)];
    }

    public static Pointer<InventoryItem>[] GetInventoryItems(ref this InventoryManager instance, InventoryType container)
    {
        var inv = instance.GetInventoryContainer(container);
        if (inv == null) return [];
        var items = new Pointer<InventoryItem>[inv->Size];
        for (var i = 0; i < inv->Size; i++)
            items[i] = inv->GetInventorySlot(i);
        return items;
    }

    public static ItemHandle[] GetItems(ref this InventoryManager instance, InventoryType container)
    {
        var inv = instance.GetInventoryContainer(container);
        if (inv == null) return [];
        var items = new ItemHandle[inv->Size];
        for (var i = 0; i < inv->Size; i++)
            items[i] = inv->GetInventorySlot(i);
        return items;
    }

    public static int? GetFirstEmptySlot(ref this InventoryManager instance, InventoryType container)
    {
        var inv = instance.GetInventoryContainer(container);
        if (inv == null) return null;
        for (var i = 0; i < inv->Size; i++)
        {
            if (inv->GetInventorySlot(i) is var item && (item == null || item->IsEmpty()))
                return i;
        }
        return null;
    }
}

