using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;

namespace ICE.Utilities.GatheringHelper;

public static partial class GatheringUtil
{
    public static Dictionary<uint, List<string>> FishingPreset = new();

    private static readonly Dictionary<uint, Action> FishingRegistrars = new()
    {
        [CosmicMoonRegistry.Sinus.TerritoryId] = RegisterSinus,
        [CosmicMoonRegistry.Phaenna.TerritoryId] = RegisterPhaenna,
        [CosmicMoonRegistry.Oizys.TerritoryId] = RegisterOizys,
        [CosmicMoonRegistry.Auxesia.TerritoryId] = RegisterAuxesia,
    };

    public static void RegisterPresets()
    {
        foreach (var moon in CosmicMoonRegistry.All)
        {
            if (FishingRegistrars.TryGetValue(moon.TerritoryId, out var register))
            {
                IceLogging.Verbose($"Registering Presets for: {moon.TerritoryId}");
                register();
            }
            else
                PluginLog.Warning($"[FishingPresets] No registrar for {moon.DisplayName} ({moon.TerritoryId})");
        }
    }

    internal static bool HasFishingRegistrar(uint territoryId) =>
        FishingRegistrars.ContainsKey(territoryId);
}
