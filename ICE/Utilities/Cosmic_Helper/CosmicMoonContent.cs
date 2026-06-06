using ICE.Utilities.GatheringHelper;
using System.Collections.Generic;
using ICE.Utilities.GatheringHelper.RouteLoader;

namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// Content availability checks per hub — no in-game coords, only what is already in the repo/runtime cache.
/// </summary>
public static class CosmicMoonContent
{
    public static bool HasGatheringRoutes(uint territoryId)
    {
        if (!GatheringRouteLoader.LoadedRoutes.TryGetValue(territoryId, out var flags))
            return false;

        return flags.Nodes.Count > 0;
    }

    public static bool HasFishingHoles(uint territoryId) =>
        GatheringUtil.MoonFishingLocations.ContainsKey(territoryId);

    public static bool HasFishingPresets(uint territoryId)
    {
        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (info.TerritoryId != territoryId || !info.Jobs.Contains(18))
                continue;

            if (GatheringUtil.FishingPreset.TryGetValue(missionId, out var presets) && presets.Count > 0)
                return true;
        }

        return false;
    }

    public static bool HasNpcData(uint territoryId) =>
        NpcData.MoonNpcs.ContainsKey(territoryId);

    public static bool HasPlanetAethernet(uint territoryId) =>
        Task_NavmeshMove.PlanetAethernet.ContainsKey(territoryId)
        && Task_NavmeshMove.PlanetAethernet[territoryId].Count > 0;

    public static bool HasRedAlertAnnouncements(uint territoryId) =>
        AnnouncementHandlers.HasRedAlertData(territoryId);

    public static int CountCriticalMissions(uint territoryId) =>
        CosmicHelper.SheetMissionDict.Count(x =>
            x.Value.TerritoryId == territoryId && x.Value.IsCritical);

    public static int CountCriticalMissionsWithCoords(uint territoryId) =>
        CosmicHelper.SheetMissionDict.Count(x =>
            x.Value.TerritoryId == territoryId
            && x.Value.IsCritical
            && CosmicHelper.CriticalLocations.ContainsKey(x.Key));

    public static (int withRoutes, int total) CountGatherMissionsWithRoutes(uint territoryId)
    {
        var routes = GatheringRouteLoader.LoadedRoutes;
        var total = 0;
        var withRoutes = 0;

        foreach (var (_, info) in CosmicHelper.SheetMissionDict)
        {
            if (info.TerritoryId != territoryId)
                continue;
            if (!info.Jobs.Contains(16) && !info.Jobs.Contains(17))
                continue;

            total++;
            if (routes.TryGetValue(territoryId, out var zoneRoutes)
                && zoneRoutes.RouteId == info.Gather_MapKey)
                withRoutes++;
        }

        return (withRoutes, total);
    }

    /// <summary>Logs one line per hub after dictionary build — highlights missing authored content.</summary>
    public static void LogContentSummary()
    {
        foreach (var moon in CosmicMoonRegistry.All)
        {
            var territoryId = moon.TerritoryId;
            var tags = new List<string>();

            if (CosmicMoonRegistry.HasLevelingContent(moon))
                tags.Add("leveling");
            if (CosmicMoonRegistry.HasUnlockContent(moon))
                tags.Add("unlock list");
            if (HasPlanetAethernet(territoryId))
                tags.Add("aethernet");
            if (HasGatheringRoutes(territoryId))
                tags.Add("gather routes");
            if (HasFishingHoles(territoryId))
                tags.Add("fish holes");
            if (HasFishingPresets(territoryId))
                tags.Add("fish presets");
            if (HasNpcData(territoryId))
                tags.Add("NPCs");
            if (HasRedAlertAnnouncements(territoryId))
                tags.Add("red-alert hints");

            var criticalTotal = CountCriticalMissions(territoryId);
            var criticalMapped = CountCriticalMissionsWithCoords(territoryId);
            if (criticalTotal > 0)
                tags.Add($"red-alert coords {criticalMapped}/{criticalTotal}");

            var (gatherRoutes, gatherTotal) = CountGatherMissionsWithRoutes(territoryId);
            if (gatherTotal > 0 && gatherRoutes < gatherTotal)
                tags.Add($"gather YAML {gatherRoutes}/{gatherTotal}");

            var missingScores = MissionScoresGenerator.CountMissing(territoryId);
            if (missingScores > 0)
                tags.Add($"MissionScores missing {missingScores}");

            var summary = tags.Count > 0 ? string.Join(", ", tags) : "registry only (no authored routes/fish/red-alert yet)";
            IceLogging.Info($"[CosmicMoonContent] {moon.DisplayName} ({territoryId}): {summary}");
        }

        ValidateRegistry();
    }

    /// <summary>
    /// Startup sanity check: every registry moon should have NPCs, aethernet, etc.
    /// Logs warnings for missing authored content (gather YAML, fish holes, red-alert coords).
    /// Watch the [CosmicMoonContent] line on plugin load — Auxesia will look thin until you fill data.
    /// </summary>
    public static void ValidateRegistry()
    {
        NpcData.NpcType[] standardNpcTypes =
        [
            NpcData.NpcType.Repair,
            NpcData.NpcType.Credit,
            NpcData.NpcType.Relic,
            NpcData.NpcType.Gamba,
            NpcData.NpcType.RedAlert,
        ];

        foreach (var moon in CosmicMoonRegistry.All)
        {
            var territoryId = moon.TerritoryId;

            if (!HasNpcData(territoryId))
            {
                IceLogging.Warning($"[CosmicMoonRegistry] {moon.DisplayName} ({territoryId}) has no NpcInfo entry");
                continue;
            }

            foreach (var npcType in standardNpcTypes)
            {
                if (!NpcData.TryGetNpc(territoryId, npcType, out _))
                    IceLogging.Warning($"[CosmicMoonRegistry] {moon.DisplayName} missing NpcInfo.{npcType}");
            }

            if (moon.HasCosmodrome && !NpcData.TryGetNpc(territoryId, NpcData.NpcType.Drone, out _))
                IceLogging.Warning($"[CosmicMoonRegistry] {moon.DisplayName} has cosmodrome but no Drone NPC");

            if (!HasPlanetAethernet(territoryId))
                IceLogging.Warning($"[CosmicMoonRegistry] {moon.DisplayName} ({territoryId}) has no PlanetAethernet entries");

            // Colleague-owned gaps — info only, not a hard failure
            if (!HasFishingHoles(territoryId) && CosmicHelper.SheetMissionDict.Values.Any(x => x.TerritoryId == territoryId && x.Jobs.Contains(18)))
                IceLogging.Info($"[CosmicMoonRegistry] {moon.DisplayName} has fisher missions but no MoonFishingLocations entry yet");

            if (!HasGatheringRoutes(territoryId) && CosmicHelper.SheetMissionDict.Values.Any(x =>
                    x.TerritoryId == territoryId && (x.Jobs.Contains(16) || x.Jobs.Contains(17))))
                IceLogging.Info($"[CosmicMoonRegistry] {moon.DisplayName} has gather missions but no gathering route YAML yet");

            if (!GatheringUtil.HasFishingRegistrar(territoryId))
                IceLogging.Info($"[CosmicMoonRegistry] {moon.DisplayName} has no fishing preset registrar yet");

            var criticalTotal = CountCriticalMissions(territoryId);
            var criticalMapped = CountCriticalMissionsWithCoords(territoryId);
            if (criticalTotal > 0 && criticalMapped < criticalTotal)
            {
                IceLogging.Info(
                    $"[CosmicMoonRegistry] {moon.DisplayName} red-alert turn-in coords: {criticalMapped}/{criticalTotal} mapped");
            }
        }

        // Catch stray territory IDs that aren't in CosmicMoonRegistry.All
        foreach (var territoryId in NpcData.MoonNpcs.Keys)
        {
            if (!CosmicMoonRegistry.IsKnownCosmicTerritory(territoryId))
                IceLogging.Warning($"[CosmicMoonRegistry] NpcInfo has territory {territoryId} but it is not in CosmicMoonRegistry.All");
        }

        foreach (var territoryId in Task_NavmeshMove.PlanetAethernet.Keys)
        {
            if (!CosmicMoonRegistry.IsKnownCosmicTerritory(territoryId))
                IceLogging.Warning($"[CosmicMoonRegistry] PlanetAethernet has territory {territoryId} but it is not in CosmicMoonRegistry.All");
        }

        foreach (var territoryId in GatheringUtil.MoonFishingLocations.Keys)
        {
            if (!CosmicMoonRegistry.IsKnownCosmicTerritory(territoryId))
                IceLogging.Warning($"[CosmicMoonRegistry] MoonFishingLocations has territory {territoryId} but it is not in CosmicMoonRegistry.All");
        }
    }
}