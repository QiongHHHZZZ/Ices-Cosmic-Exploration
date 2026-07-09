using System.Collections.Generic;

namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// Per-moon settings that are not on WKSMissionUnit (hand-tuned in the plugin).
/// </summary>
public sealed class CosmicMoonDefinition
{
    /// <summary>TerritoryType row ID — same value Player.Territory uses in-zone (e.g. 1319 = Auxesia).</summary>
    public required uint TerritoryId { get; init; }
    public required string DisplayName { get; init; }
    public required string IconResource { get; init; }
    public required ItemFilter PlanetFilter { get; init; }
    public required uint PlanetCreditItemId { get; init; }
    public uint? TokenId { get; set; }
    public uint? BookletId { get; set; } 
    public uint? MountId { get; set; }
    /// <summary>Hub stellar return / navmesh anchor for this moon.</summary>
    public required Vector3 HubCenter { get; init; }
    /// <summary>Oizys and Auxesia have the cosmodrome / drone vendor; Sinus and Phaenna do not.</summary>
    public bool HasCosmodrome { get; init; }
    public uint? DronebitCreditId { get; init; }
    public uint? DronebitBoxId { get; init; }
    /// <summary>Max relic research stage on this hub (agenda playlist goals).</summary>
    public int MaxRelicStage { get; init; }
    /// <summary>Expedition log tab order (0 = Sinus … 3 = Auxesia).</summary>
    public int ExpeditionTabIndex { get; init; }
    /// <summary>First WKSMissionUnit row ID on this hub.</summary>
    public uint MissionRowIdStart { get; init; }
    /// <summary>Embedded gathering route folder under Resources/GatheringRoutes/.</summary>
    public required string GatheringRoutesFolder { get; init; }
    /// <summary>Agenda playlist option for max relic on this hub.</summary>
    public PlaylistOptions MaxRelicPlaylistOption { get; init; }
    /// <summary>Seed value for navmesh aethernet unlock count until WKSHistoryBoard is read.</summary>
    public uint DefaultAethernetLogLevel { get; init; }
    /// <summary>Expedition log research icon under Resources/ResearchIcons.</summary>
    public string? ResearchIconResource { get; init; }
}

/// <summary>
/// One place to define every cosmic hub (Sinus through Auxesia — final moon for this expansion).
/// </summary>
/// <remarks>
/// Auxesia was the last moon added, but this registry is the uniform contract for <b>all</b> hubs:
/// UI filters, agenda warnings, overlay planets, debug tables, credits, hub centers, and cosmodrome checks
/// should go through here instead of hardcoding 1237 / 1291 / 1310 / 1319.
/// <para>
/// Mission territory still comes from <see cref="CosmicTerritoryResolver"/> (game PlaceName), not row-id bands.
/// Hand-authored data (NPCs, gather YAML, fish holes, MissionScores) uses each moon's TerritoryId from here.
/// </para>
/// <para>
/// Infra pass: call TryGetPlanetCreditItemId / TryGetHubCenter / TryGetDronebit instead of the legacy
/// dict aliases on CosmicHelper and Mission_Info. Relic stage caps live on each moon (Auxesia = 20).
/// Startup logs from CosmicMoonContent.ValidateRegistry() show what's still missing per hub.
/// </para>
/// </remarks>
public static class CosmicMoonRegistry
{
    public static readonly CosmicMoonDefinition Sinus = new()
    {
        TerritoryId = 1237,
        DisplayName = "Sinus Ardorum",
        IconResource = "ICE.Resources.Sinus_Ardorum.png",
        PlanetFilter = ItemFilter.Sinus,
        PlanetCreditItemId = 45691,
        HubCenter = new(2.84f, 1.55f, -0.06f),
        MaxRelicStage = 9,
        ExpeditionTabIndex = 0,
        MissionRowIdStart = CosmicMissionBlocks.SinusStart,
        GatheringRoutesFolder = "1237_Sinus Ardorum",
        MaxRelicPlaylistOption = PlaylistOptions.SinusMax,
        DefaultAethernetLogLevel = 15,
        ResearchIconResource = "ICE.Resources.ResearchIcons.novice.png",
    };

    public static readonly CosmicMoonDefinition Phaenna = new()
    {
        TerritoryId = 1291,
        DisplayName = "Phaenna",
        IconResource = "ICE.Resources.Phaenna.png",
        PlanetFilter = ItemFilter.Phaenna,
        PlanetCreditItemId = 48146,
        TokenId = 47594,
        BookletId = 47343,
        MountId = 47336,
        HubCenter = new(339.90f, 52.60f, -412.10f),
        MaxRelicStage = 14,
        ExpeditionTabIndex = 1,
        MissionRowIdStart = CosmicMissionBlocks.PhaennaStart,
        GatheringRoutesFolder = "1291_Phaenna",
        MaxRelicPlaylistOption = PlaylistOptions.PhaennaMax,
        DefaultAethernetLogLevel = 15,
        ResearchIconResource = "ICE.Resources.ResearchIcons.intermediate.png",
    };

    public static readonly CosmicMoonDefinition Oizys = new()
    {
        TerritoryId = 1310,
        DisplayName = "Oizys",
        IconResource = "ICE.Resources.Oizys.png",
        PlanetFilter = ItemFilter.Oizys,
        PlanetCreditItemId = 48147,
        TokenId = 49802,
        BookletId = 50829,
        MountId = 50442,
        HubCenter = new(-180.02f, 0.50f, 129.25f),
        HasCosmodrome = true,
        DronebitCreditId = 49170,
        DronebitBoxId = 50414,
        MaxRelicStage = 17,
        ExpeditionTabIndex = 2,
        MissionRowIdStart = CosmicMissionBlocks.OizysStart,
        GatheringRoutesFolder = "1310_Oizys",
        MaxRelicPlaylistOption = PlaylistOptions.OizysMax,
        DefaultAethernetLogLevel = 17,
        ResearchIconResource = "ICE.Resources.ResearchIcons.advance.png",
    };

    public static readonly CosmicMoonDefinition Auxesia = new()
    {
        TerritoryId = 1319,
        DisplayName = "Auxesia",
        IconResource = "ICE.Resources.Auxesia.png",
        PlanetFilter = ItemFilter.Auxesia,
        PlanetCreditItemId = 48148,
        TokenId = 52092,
        BookletId = 52091,
        MountId = 52268,
        HubCenter = new(291.00f, 205.78f, 376.02f),
        HasCosmodrome = true,
        DronebitCreditId = 49171,
        DronebitBoxId = 50415,
        MaxRelicStage = 20,
        ExpeditionTabIndex = 3,
        MissionRowIdStart = CosmicMissionBlocks.AuxesiaStart,
        GatheringRoutesFolder = "1319_Auxesia",
        MaxRelicPlaylistOption = PlaylistOptions.AuxesiaMax,
        DefaultAethernetLogLevel = 2,
        ResearchIconResource = "ICE.Resources.ResearchIcons.expert.png",
    };

    public static readonly CosmicMoonDefinition[] All = [Sinus, Phaenna, Oizys, Auxesia];

    public static readonly IReadOnlyList<uint> TerritoryIds = All.Select(m => m.TerritoryId).ToArray();

    public static readonly IReadOnlyDictionary<uint, CosmicMoonDefinition> ByTerritoryId =
        All.ToDictionary(m => m.TerritoryId);

    public static readonly IReadOnlyDictionary<PlaylistOptions, CosmicMoonDefinition> ByMaxRelicPlaylistOption =
        All.ToDictionary(m => m.MaxRelicPlaylistOption);

    public static readonly IReadOnlyList<PlaylistOptions> MaxRelicPlaylistOptions =
        All.Select(m => m.MaxRelicPlaylistOption).ToArray();

    // Legacy dictionary aliases — prefer TryGetPlanetCreditItemId / TryGetHubCenter / TryGetDronebit.
    public static readonly Dictionary<uint, uint> PlanetCredits =
        All.ToDictionary(m => m.TerritoryId, m => m.PlanetCreditItemId);

    public static readonly Dictionary<uint, Vector3> HubCenters =
        All.ToDictionary(m => m.TerritoryId, m => m.HubCenter);

    // Only moons with a cosmodrome populate this — do not index blindly on Sinus/Phaenna
    public static readonly Dictionary<uint, CosmicHelper.Dronebit> Dronebits = All
        .Where(m => m.DronebitCreditId is uint creditId && m.DronebitBoxId is uint boxId)
        .ToDictionary(
            m => m.TerritoryId,
            m => new CosmicHelper.Dronebit 
            { 
                creditId = m.DronebitCreditId!.Value, 
                boxId = m.DronebitBoxId!.Value 
            });

    // Returns a dictionary that only contains the tokens and booklets for everything but sinus
    public static readonly Dictionary<uint, CosmicHelper.TokenInfo> TokenIds =
        All.Where(m => m.TokenId is uint tokenId && m.BookletId is uint bookletId)
        .ToDictionary(
            m => m.TerritoryId,
            m => new CosmicHelper.TokenInfo 
            { 
                tokenId = m.TokenId!.Value, 
                bookletId = m.BookletId!.Value,
                mountId = m.MountId!.Value
            });

    public static bool TryGetMoon(uint territoryId, out CosmicMoonDefinition moon) =>
        ByTerritoryId.TryGetValue(territoryId, out moon!);

    public static string GetDisplayName(uint territoryId) =>
        TryGetMoon(territoryId, out var moon) ? moon.DisplayName : $"Zone_{territoryId}";

    public static bool IsKnownCosmicTerritory(uint territoryId) =>
        ByTerritoryId.ContainsKey(territoryId);

    /// <summary>Moons that have the cosmodrome / drone vendor (Oizys, Auxesia).</summary>
    public static IEnumerable<CosmicMoonDefinition> WithCosmodrome =>
        All.Where(m => m.HasCosmodrome);

    public static string GetIconResource(uint territoryId) =>
        TryGetMoon(territoryId, out var moon) ? moon.IconResource : Sinus.IconResource;

    public static bool ItemFilterIncludesTerritory(ItemFilter filter, uint territoryId) =>
        TryGetMoon(territoryId, out var moon) && filter.HasFlag(moon.PlanetFilter);

    /// <summary>
    /// Standard-mode missions (rank &lt; 6) enabled for a job on a given hub — same rules for every moon.
    /// </summary>
    public static int CountEnabledStandardMissions(uint territoryId, uint jobId)
    {
        var count = 0;
        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (info.TerritoryId != territoryId || !info.Jobs.Contains(jobId) || info.IsProvisional)
                continue;

            if (C.MissionConfig.TryGetValue(missionId, out var cfg) && cfg.Enabled)
                count++;
        }

        return count;
    }

    /// <summary>Enabled missions on a hub (any rank) — used for startup logging.</summary>
    public static int CountEnabledMissions(uint territoryId)
    {
        var count = 0;
        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (info.TerritoryId != territoryId)
                continue;

            if (C.MissionConfig.TryGetValue(missionId, out var cfg) && cfg.Enabled)
                count++;
        }

        return count;
    }

    public static CosmicMoonDefinition? GetMoonForTerritory(uint territoryId) =>
        TryGetMoon(territoryId, out var moon) ? moon : null;

    public static bool TryGetPlanetCreditItemId(uint territoryId, out uint itemId) =>
        PlanetCredits.TryGetValue(territoryId, out itemId);

    // Hub lookups — use these at call sites instead of indexing dicts directly.
    public static bool TryGetHubCenter(uint territoryId, out Vector3 hubCenter) =>
        HubCenters.TryGetValue(territoryId, out hubCenter);

    public static bool TryGetDronebit(uint territoryId, out CosmicHelper.Dronebit dronebit) =>
        Dronebits.TryGetValue(territoryId, out dronebit);

    // Per-moon relic caps (Sinus 9 → Auxesia 20). UI/agenda should use these, not a flat 17.
    public static int HighestMaxRelicStage => All.Max(m => m.MaxRelicStage);

    /// <summary>Relic XP bar cap for max-stage sub-progress (was hardcoded 17.6).</summary>
    public static float MaxRelicExpBarCap => HighestMaxRelicStage + 0.6f;

    public static int GetMaxRelicStage(uint territoryId) =>
        TryGetMoon(territoryId, out var moon) ? moon.MaxRelicStage : HighestMaxRelicStage;

    /// <summary>Target relic stage for max-relic playlist goals.</summary>
    public static int GetMaxRelicGoal(PlaylistOptions option) =>
        TryGetMoonForMaxRelicOption(option, out var moon) ? moon.MaxRelicStage : 0;

    public static bool IsMaxRelicPlaylistGoal(PlaylistOptions option) =>
        TryGetMoonForMaxRelicOption(option, out _);

    public static bool TryGetMoonForMaxRelicOption(PlaylistOptions option, out CosmicMoonDefinition moon) =>
        ByMaxRelicPlaylistOption.TryGetValue(option, out moon!);

    /// <summary>Legacy row-id band when PlaceName data is missing (see CosmicTerritoryResolver).</summary>
    public static uint ResolveTerritoryFromMissionRowId(uint missionRowId)
    {
        if (missionRowId < CosmicMissionBlocks.PhaennaStart)
            return Sinus.TerritoryId;
        if (missionRowId < CosmicMissionBlocks.OizysStart)
            return Phaenna.TerritoryId;
        if (missionRowId < CosmicMissionBlocks.AuxesiaStart)
            return Oizys.TerritoryId;
        if (missionRowId < CosmicMissionBlocks.UnknownMissionStart)
            return Auxesia.TerritoryId;

        return 0;
    }

    public static bool HasUnlockContent(CosmicMoonDefinition moon) =>
        CosmicMissionLists.HasUnlockContent(moon.TerritoryId);

    /// <summary>True when QuickLevelList has at least one mission on this hub.</summary>
    public static bool HasLevelingContent(CosmicMoonDefinition moon) =>
        CosmicMissionLists.HasLevelingContent(moon.TerritoryId);
}

