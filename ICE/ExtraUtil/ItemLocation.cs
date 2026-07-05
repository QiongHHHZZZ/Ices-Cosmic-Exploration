using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.ExtraUtil;

// Borrowed from here: https://github.com/Jaksuhn/clib/blob/3ed208db2a7c3dbf6509cb574bb8cbc25dcf3f35/Utils/ItemLocation.cs#L5
// There's some aspect to just using the lib... but also need to just probably grab the code I need
public class ItemLocation
{
    public InventoryType Container { get; set; } = InventoryType.Invalid;
    public ushort Slot { get; set; } = 0;

    public ItemLocation() { }

    public ItemLocation(InventoryType container, ushort slot)
    {
        Container = container;
        Slot = slot;
    }

    public unsafe ItemLocation(InventoryItem* item) => SetInventoryItem(item);

    public void Clear()
    {
        Container = InventoryType.Invalid;
        Slot = 0;
    }

    public void SetContainerAndSlot(InventoryType container, ushort slot)
    {
        Clear();
        Container = container;
        Slot = slot;
    }

    public unsafe void SetInventoryItem(InventoryItem* item)
    {
        if (item == null)
            Clear();
        else
        {
            Container = item->GetInventoryType();
            Slot = item->GetSlot();
        }
    }

    public unsafe InventoryItem* GetInventoryItem()
    {
        if (Container == InventoryType.Invalid)
            return null;

        return InventoryManager.Instance()->GetInventorySlot(Container, Slot);
    }

    public unsafe bool IsEmpty
    {
        get
        {
            if (Container != InventoryType.Invalid)
                return false;

            var inventoryItem = GetInventoryItem();
            return inventoryItem == null || inventoryItem->IsEmpty();
        }
    }

    public override string ToString() => $"[c={Container} s={Slot}]";

    public ValueTuple<InventoryType, ushort> AsTuple() => (Container, Slot);
    public static unsafe implicit operator ItemLocation(InventoryItem* item) => new(item);
    public static implicit operator ItemLocation((InventoryType Container, ushort Slot) tuple) => new(tuple.Container, tuple.Slot);
    public static implicit operator ValueTuple<InventoryType, ushort>(ItemLocation itemLoc) => itemLoc.AsTuple();
}
