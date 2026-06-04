using System.Collections.Generic;

namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// Some mission rows land on the same map flag. We bump them a tile so gather-route YAML keys stay unique.
/// Hard overrides are in Overrides; Oizys rows 1317–1319 get a +1 nudge in TryGetOverlapNudge.
/// If Auxesia has stacked markers too, add another override entry.
/// </summary>
internal static class CosmicMapMarkerNudges
{
    private static readonly Dictionary<uint, Vector2> Overrides = new()
    {
        [1272] = new(-340, 870),
        [1264] = new(-573, 3),
        [1296] = new(-514, 232),
    };

    public static bool TryGetOverride(uint missionRowId, out Vector2 mapFlag)
    {
        if (Overrides.TryGetValue(missionRowId, out mapFlag))
            return true;

        mapFlag = default;
        return false;
    }

    // Mission row IDs 1317–1319 on Oizys — not territory 1319 (Auxesia).
    public static bool TryGetOverlapNudge(uint missionRowId, Vector2 mapFlag, out Vector2 nudgedFlag)
    {
        List<uint> MissionNudges = new()
        {
            // Oizys ones that I decided to just nudge
            1317, 1318, 1319,

            // Auxesia 
            1607, // 8 Node BTN - A Rank
            1641, 1642, 1643, 1644, // BTN Nodes that overlap with the MIN nodes in the same area
            1647, 1648, 1649, // BTN Master missions overlapping the MIN Master nodes at (-633, 246)
            1599, 1600, 1601, // MIN Missions, Overlaps w/ 8 node one (didn't feel like remaking it)
            1602, // MIN, 8 Node
            1635, // BTN, 8 Node
            1594, // MIN, 8 Node
        };

        if (MissionNudges.Contains(missionRowId))
        {
            nudgedFlag = new(mapFlag.X + 1, mapFlag.Y + 1);
            return true;
        }

        nudgedFlag = default;
        return false;
    }
}
