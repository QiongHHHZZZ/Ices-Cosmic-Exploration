using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    public bool FailsafeRecipeSelect { get; set; } = false;
    public bool UseDummyXp { get; set; } = false;
    public Dictionary<int, CosmicHelper.XPType> DummyXP { get; set; } = new()
    {
        { 1, new CosmicHelper.XPType { CurrentXP = 0, NeededXP = 100} },
        { 2, new CosmicHelper.XPType { CurrentXP = 50, NeededXP = 200} },
        { 3, new CosmicHelper.XPType { CurrentXP = 100, NeededXP = 300} },
        { 4, new CosmicHelper.XPType { CurrentXP = 150, NeededXP = 400} },
        { 5, new CosmicHelper.XPType { CurrentXP = 200, NeededXP = 500} },
    };
    public uint PictoColor_Circle { get; set; } = 2616716297;
    public uint PictoColor_Dot { get; set; } = 2616716297;
    public uint PictoColor_Cone { get; set; } = 0;
    public bool UseDummyRanks { get; set; } = false;
    public bool ShowDummyA { get; set; } = false;
    public bool ShowDummyB { get; set; } = false;
    public bool ShowDummyC { get; set; } = false;
    public bool ShowDummyD { get; set; } = false;

    public bool DisablePathfindingToRedAlert { get; set; } = false;
    public bool ShowDebugGatherInfo { get; set; } = false;
    public string AuthorName { get; set; } = "Puni.sh Community";
    public string CustomRoutePath { get; set; } = string.Empty;
    public bool DisableHudClipping { get; set; } = false;

    public bool HighlightVisibleMissions { get; set; } = false;
}
