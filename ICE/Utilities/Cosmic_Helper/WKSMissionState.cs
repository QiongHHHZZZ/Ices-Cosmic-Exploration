using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using System.Runtime.InteropServices;

namespace ICE.Utilities.Cosmic_Helper;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct MissionStateCorrect
{
    [FieldOffset(0)]
    public ushort MissionUnitRowId;

    [FieldOffset(12)]
    public ushort Score;

    [FieldOffset(12)]
    public uint ScoreUInt;

    [FieldOffset(16)]
    public WKSMissionModule.MissionRank Rank;

    [FieldOffset(22)]
    public ushort CollectedTotal;

    [FieldOffset(24)]
    public byte CollectedIndividual;

    public uint EffectiveScore => ScoreUInt > ushort.MaxValue ? ScoreUInt : Score;
}
