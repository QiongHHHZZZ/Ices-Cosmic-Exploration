using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    public Dictionary<uint, CosmoShoppingList> CosmoShopping { get; set; } = new();
    public List<uint> CosmoShoppingOrder { get; set; } = new();
    public List<uint> CosmoShoppingOrder_Gear { get; set; } = new();

    public bool BuyItems { get; set; } = false;
    public int CosmoBuyAtAmount { get; set; } = 10000;
    public int CosmoKeepAmount { get; set; } = 0;

    public bool BookletBuy_Enable { get; set; } = false;
    public int BookletBuy_Amount { get; set; } = 900;

    public bool PlanetMount_Enable { get; set; } = false;
    public int PlanetMount_Amount { get; set; } = 60;

    public class CosmoShoppingList
    {
        public int KeepAmount { get; set; } = 0;
        public int BuyAmount { get; set; } = 0;
        public bool KeepBuying { get; set; } = false;
    }
}
