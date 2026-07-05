using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;
using Callback = ECommons.Automation.Callback;

namespace ICE.Utilities;

public unsafe class ShopExchangeItem : AddonMasterBase<AtkUnitBase>
{
    public ShopExchangeItem(nint addon) : base(addon) { }

    public ShopExchangeItem(void* addon) : base(addon) { }

    public uint NumEntries => Addon->AtkValues[3].UInt;
    public class ShopItemInfo(ShopExchangeItem master)
    {
        public uint ItemId;
        public uint CategoryId;
        public uint Quantity;
        public uint Index;
        public List<ExchangableItems> ExchangeItems;
        public void Select(int amount = 1)
        {
            Callback.Fire(master.Base, true, 0, Index, amount);
        }
    }

    public class ExchangableItems
    {
        public uint ItemId;
        public uint RequiredAmount;
    }

    public ShopItemInfo[] ItemInfo
    {
        get
        {
            var ret = new List<ShopItemInfo>();
            for (int i = 0; i < NumEntries; i++)
            {
                var itemId = Addon->AtkValues[1066 + (i * 1)].UInt;

                var categoryIconId = Addon->AtkValues[212 + (i * 1)].UInt;
                var quantity = Addon->AtkValues[700 + (i * 1)].UInt;
                var index = Addon->AtkValues[1310 + (i * 1)].UInt;

                List<ExchangableItems> reqItems = new();
                for (int x = 0; x < 3; x++)
                {
                    int location = (i * 3) + x;

                    var tradeItemId = Addon->AtkValues[3141 + location].UInt;
                    if (tradeItemId != 0)
                    {
                        var requiredAmount = Addon->AtkValues[2775 + location].UInt;
                        reqItems.Add(new() { ItemId = tradeItemId, RequiredAmount = requiredAmount });
                    }
                }
                var newEntry = new ShopItemInfo(this)
                {
                    ItemId = itemId,
                    CategoryId = categoryIconId,
                    Quantity = quantity,
                    Index = index,
                    ExchangeItems = [.. reqItems]
                };
                ret.Add(newEntry);
            }
            return [.. ret];
        }
    }

    public override string AddonDescription { get; } = "Item Exchange Window";
}
