using System.Collections.Generic;
using System.Linq;

namespace ICE.Utilities.Cosmic_Helper;

public static class CosmicMissionLists
{
    public static HashSet<uint> UnlockMissionIds { get; private set; } = [];
    public static HashSet<uint> QuickLevelMissionIds { get; private set; } = [];
    public static HashSet<uint> ManualUnlockAdditions { get; } = [];
    public static HashSet<uint> ManualQuickLevelAdditions { get; } =
    [
        // Sinus
        3, 8, 19,      // CRP
        48, 53, 64,    // ARM
        93, 98, 109,   // BSM
        138, 143, 154, // GSM
        183, 188, 199, // LTW
        228, 233, 244, // WVR
        273, 278, 289, // ALC
        318, 323, 334, // CUL
        365, 369, 374, // MIN
        410, 414, 419, // BTN
        453, 458, 465, // FSH

        // Phaenna
        545, 556, 561, // CRP
        587, 598, 603, // BSM
        629, 640, 645, // ARM
        671, 682, 687, // GSM
        713, 724, 729, // LTW
        755, 766, 771, // WVR
        797, 808, 813, // ALC
        839, 850, 855, // CUL
        883, 903, 886, // MIN
        925, 945, 928, // BTN
        967, 973, 979, // FSH
        1040, 1045, 1048,
        1068, 1073, 1076,
        1096, 1101, 1104,
        1124, 1129, 1132,
        1152, 1157, 1160,
        1180, 1185, 1188,
        1208, 1213, 1216,
        1236, 1241, 1244,
        1266, 1270, 1274,
        1294, 1298, 1301,
        1321, 1327, 1331,

        // Auxesia (+330 from Oizys — verify in-game when SPM is known)
        1370, 1375, 1378,
        1398, 1403, 1406,
        1426, 1431, 1434,
        1454, 1459, 1462,
        1482, 1487, 1490,
        1510, 1515, 1518,
        1538, 1543, 1546,
        1566, 1571, 1574,
        1596, 1600, 1604,
        1624, 1628, 1631,
        1651, 1657, 1661,
    ];

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
