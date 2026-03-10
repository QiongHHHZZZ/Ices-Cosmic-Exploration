using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace ICE.Utilities;

public unsafe class WKSHistoryBoard : AddonMasterBase<AddonGuildLeve>
{
    public WKSHistoryBoard(nint addon) : base(addon) { }

    public WKSHistoryBoard(void* addon) : base(addon) { }

    public uint NumEntries => Addon->AtkValues[0].UInt;
    public override string AddonDescription { get; }
}
