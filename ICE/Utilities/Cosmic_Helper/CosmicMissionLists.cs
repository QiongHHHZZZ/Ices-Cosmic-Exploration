using System.Collections.Generic;
using System.Linq;

namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// Unlock + quick-level mission IDs, rebuilt from the sheet on startup.
/// The old giant static lists in CustomNotes.cs are gone — if a patch adds rows the heuristics miss,
/// drop the row ID into ManualUnlockAdditions or ManualQuickLevelAdditions below.
/// </summary>
public static class CosmicMissionLists
{
    public static HashSet<uint> UnlockMissionIds { get; private set; } = [];
    public static HashSet<uint> QuickLevelMissionIds { get; private set; } = [];

    // Patch gap filler — verify in the mission UI before copying +330 offsets from Oizys.
    public static HashSet<uint> ManualUnlockAdditions { get; } = [];
    public static HashSet<uint> ManualQuickLevelAdditions { get; } = [];

    public static void BuildFromSheet()
    {
        UnlockMissionIds.Clear();
        QuickLevelMissionIds.Clear();

        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (!CosmicMoonRegistry.IsKnownCosmicTerritory(info.TerritoryId))
                continue;

            if (info.IsProvisional || info.IsCritical)
                continue;

            // Unlock chain: rank 1–5 standard missions on any cosmic hub
            if (info.Rank is >= 1 and <= 5)
                UnlockMissionIds.Add(missionId);

            // Level mode: A-rank missions at the 10 / 50 / 90 tiers
            if (info.Level is 10 or 50 or 90 && info.ARank)
                QuickLevelMissionIds.Add(missionId);
        }

        foreach (var id in ManualUnlockAdditions)
            UnlockMissionIds.Add(id);

        foreach (var id in ManualQuickLevelAdditions)
            QuickLevelMissionIds.Add(id);
    }

    public static bool IsUnlockMission(uint missionId) => UnlockMissionIds.Contains(missionId);

    public static bool IsQuickLevelMission(uint missionId) => QuickLevelMissionIds.Contains(missionId);

    public static IEnumerable<uint> UnlockMissionList => UnlockMissionIds;

    public static IEnumerable<uint> QuickLevelList => QuickLevelMissionIds;

    public static bool HasUnlockContent(uint territoryId) =>
        UnlockMissionIds.Any(id =>
            CosmicHelper.SheetMissionDict.TryGetValue(id, out var info) && info.TerritoryId == territoryId);

    public static bool HasLevelingContent(uint territoryId) =>
        QuickLevelMissionIds.Any(id =>
            CosmicHelper.SheetMissionDict.TryGetValue(id, out var info) && info.TerritoryId == territoryId);
}
