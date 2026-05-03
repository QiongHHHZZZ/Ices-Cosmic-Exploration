using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;

namespace ICE.Utilities.Cosmic_Helper;

[StructLayout(LayoutKind.Explicit, Size = 0xC8)]
public unsafe struct WKSResearchModuleCorrect
{
    // 11 toolClasses * 7 types = 77 ushorts
    [FieldOffset(0x08)] public fixed ushort Analysis[77];       
    [FieldOffset(0xA2)] public fixed byte CurrentStages[11];    
    [FieldOffset(0xAD)] public fixed byte UnlockedStages[11];   
    [FieldOffset(0xB8)] public fixed ushort RatePercentages[2]; 
    [FieldOffset(0xC0)] public bool IsLoaded;
}