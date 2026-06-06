using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ICE.Utilities.GatheringHelper.RouteLoader;

public static class GatheringRouteLoader
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new Vector3Converter() }
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new Vector3Converter() }
    };

    public static Dictionary<uint, GatheringRoute> LoadedRoutes = new();


    // ── Loading ──────────────────────────────────────────────────────────────

    public static void LoadAllRoutes()
    {

        var _cache = new Dictionary<uint, GatheringRoute>();

        LoadEmbeddedRoutes(_cache);
        LoadDiskRoutes(_cache);

        PluginLog.Information($"Loaded {_cache.Count} gathering routes");
        LoadedRoutes = _cache;
    }

    private static void LoadEmbeddedRoutes(Dictionary<uint, GatheringRoute> target)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.Contains("GatherRoutes") && r.EndsWith(".json"));

        foreach (var resourceName in resources)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream);
                var route = JsonSerializer.Deserialize<GatheringRoute>(reader.ReadToEnd(), ReadOptions);

                if (route == null) continue;

                if (target.ContainsKey(route.RouteId))
                {
                    PluginLog.Warning($"Duplicate route_id {route.RouteId} in {resourceName}, skipping");
                    continue;
                }

                target[route.RouteId] = route;
                PluginLog.Verbose($"Embedded route {route.RouteId} loaded");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load embedded route {resourceName}: {ex.Message}");
            }
        }
    }

    private static void LoadDiskRoutes(Dictionary<uint, GatheringRoute> target)
    {
        var basePath = GetBasePath();
        if (!Directory.Exists(basePath))
            return;

        var files = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
        int loaded = 0, overridden = 0;

        foreach (var file in files)
        {
            try
            {
                var route = JsonSerializer.Deserialize<GatheringRoute>(
                    File.ReadAllText(file), ReadOptions);

                if (route == null) continue;

                bool isOverride = target.ContainsKey(route.RouteId);
                target[route.RouteId] = route;

                if (isOverride) overridden++;
                else loaded++;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load route {file}: {ex.Message}");
            }
        }

        PluginLog.Information($"Loaded {loaded} disk routes, {overridden} overrides from {basePath}");
    }

    // ── Saving ───────────────────────────────────────────────────────────────

    public static void SaveRoute(GatheringRoute route)
    {
        if (!CosmicMoonRegistry.TryGetMoon(route.TerritoryId, out var moon))
        {
            PluginLog.Error($"SaveRoute: unknown territory {route.TerritoryId} for route {route.RouteId}");
            return;
        }

        var dir = Path.Combine(GetBasePath(), $"{moon.TerritoryId}_{moon.DisplayName}");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"route_{route.RouteId}.json");
        route.DateModified = DateTime.UtcNow;

        File.WriteAllText(path, JsonSerializer.Serialize(route, WriteOptions));
        LoadedRoutes[route.RouteId] = route;

        PluginLog.Verbose($"Saved route {route.RouteId} -> {path}");
        LoadAllRoutes();
    }

    // ── Stubs ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates stub route files for any gathering missions in SheetMissionDict
    /// that don't already have a route file. Node is left null until captured in-mission.
    /// </summary>
    public static HashSet<uint> CreateMissingStubs(bool dryRun = false)
    {
        var routes = LoadedRoutes;
        var created = new HashSet<uint>();

        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (!info.Jobs.Contains(16) && !info.Jobs.Contains(17))
                continue;

            if (routes.ContainsKey(info.Gather_MapKey))
                continue;

            uint jobId = info.Jobs.Contains(17) ? 17u : 16u;

            IceLogging.Info($"Missing stub: route {info.Gather_MapKey}, territory {info.TerritoryId}, job {jobId}");

            var stub = new GatheringRoute
            {
                RouteId = info.Gather_MapKey,
                TerritoryId = info.TerritoryId,
                GatheringJobId = jobId,
                Author = string.IsNullOrWhiteSpace(C.AuthorName) ? "Ice" : C.AuthorName,
                Nodes = null
            };

            if (!dryRun)
                SaveRoute(stub);
            created.Add(info.Gather_MapKey);
        }

        IceLogging.Info(dryRun
            ? $"Dry run: {created.Count} stubs would be created"
            : $"Created {created.Count} stub routes");

        return created;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public static GatheringRoute? GetRoute(uint routeId)
    {
        var routes = LoadedRoutes;
        return routes.TryGetValue(routeId, out var route) ? route : null;
    }

    public static bool HasNode(uint routeId) => GetRoute(routeId)?.Nodes is { Count: > 0 };

    public static List<GatheringRoute> GetRoutesForTerritory(uint territoryId) =>
        LoadedRoutes.Values.Where(r => r.TerritoryId == territoryId).ToList();

    public static List<GatheringRoute> GetRoutesForJob(uint jobId) =>
        LoadedRoutes.Values.Where(r => r.GatheringJobId == jobId).ToList();

    public static List<GatheringRoute> GetIncompleteRoutes() =>
        LoadedRoutes.Values.Where(r => r.Nodes is null or { Count: 0 }).ToList();

    public static List<uint> AddedNodes()
    {
        List<uint> nodeIds = new();

        foreach (var route in LoadedRoutes)
        {
            if (route.Value.Nodes != null)
            {
                foreach (var node in route.Value.Nodes)
                {
                    if (!nodeIds.Contains(node.NodeId))
                        nodeIds.Add(node.NodeId);
                }
            }
        }
        return nodeIds;
    }

    // ── Internals ────────────────────────────────────────────────────────────

    private static string GetBasePath() =>
        !string.IsNullOrEmpty(C.CustomRoutePath)
            ? C.CustomRoutePath
            : Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "GatheringRoutes");
}
