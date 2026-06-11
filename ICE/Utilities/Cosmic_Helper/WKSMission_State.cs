using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using System.Runtime.InteropServices;

namespace ICE.Utilities.Cosmic_Helper;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct MissionStateEx
{
    [FieldOffset(0)]
    public ushort MissionUnitRowId;

    [FieldOffset(12)]
    public uint Score;  // Corrected from ushort

    [FieldOffset(16)]
    public WKSMissionModule.MissionRank Rank;

    [FieldOffset(20)]
    private byte Unk14;

    [FieldOffset(22)]
    public ushort CollectedTotal;

    [FieldOffset(24)]
    public byte CollectedIndividual;
}
