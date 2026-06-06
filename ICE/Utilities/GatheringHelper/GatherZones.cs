using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.GatheringHelper;

public static unsafe partial class GatheringUtil
{
    public class MapInfo
    {
        public List<uint> MissionIds { get; set; } = new();
        public uint TerritoryId { get; set; } = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Radius { get; set; } = 0;
        public uint IconId { get; set; } = 0;
        public List<uint> JobId { get; set; } = new();
    }

    public static Dictionary<uint, MapInfo> GatherSpots = new();
    public static Dictionary<uint, MapInfo> CriticalSpots = new();
}
