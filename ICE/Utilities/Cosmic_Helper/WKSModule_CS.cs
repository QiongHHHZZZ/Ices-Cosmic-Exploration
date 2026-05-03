using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Utilities.Cosmic_Helper;
using System.Runtime.InteropServices;

namespace ICE.Utilities;

[StructLayout(LayoutKind.Explicit, Size = 0xF90)]
public unsafe struct WKSManagerCustom
{
    [FieldOffset(0xE80)] public ushort CurrentMissionId;

    [FieldOffset(0xED1)] public fixed byte MissionCompletionFlags[213];
    [FieldOffset(0xFA6)] public fixed byte MissionGoldFlags[213];

    [FieldOffset(0xE8C)] public uint CurrentScore;
    [FieldOffset(0xE90)] public MissionRank CurrentRank;
    [FieldOffset(0xE96)] public ushort CollectedTotal;
    [FieldOffset(0xE98)] public byte CollectedIndividual;

    [FieldOffset(0x107C)] public fixed int Scores[11];

    [FieldOffset(0x1140)] public WKSResearchModuleCorrect* ResearchModule;

    public enum MissionRank : ushort
    {
        None,
        Bronze,
        Silver,
        Gold,
        Depleted = 5,
    }

    public bool IsMissionCompleted(uint missionUnitId)
    {
        var group = (byte)(missionUnitId >> 3);
        var mask = 1 << ((int)missionUnitId & 7);
        return (mask & MissionCompletionFlags[group]) != 0;
    }

    public bool IsMissionGolded(uint missionUnitId)
    {
        var group = (byte)(missionUnitId >> 3);
        var mask = 1 << ((int)missionUnitId & 7);
        return (mask & MissionGoldFlags[group]) != 0;
    }
}