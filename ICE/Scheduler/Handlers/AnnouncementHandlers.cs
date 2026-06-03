using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;

using JobPairs = (string job, uint territoryId, float x, float y);
namespace ICE.Scheduler.Handlers
{
    using LocationEntry = KeyValuePair<string, (JobPairs first, JobPairs second)[]>;
    internal static unsafe class AnnouncementHandlers
    {
        private static readonly string Announcement = "WKSAnnounce";

        // Red-alert chat hints when WKS announce fires. Sinus only for now.
        // Phaenna / Oizys / Auxesia: copy the SinusRedAlert pattern and register in RedAlertByTerritory.
        private static readonly Dictionary<string, (JobPairs first, JobPairs second)[]> SinusRedAlert = new()
        {
            {
                "meteorite shower",
                new(JobPairs first, JobPairs second)[]
                {
                    (
                        ("ARM/GSM", CosmicMoonRegistry.Sinus.TerritoryId, 22.6f, 14.9f),
                        ("BSM/LTW/MIN", CosmicMoonRegistry.Sinus.TerritoryId, 16.3f, 24.4f)
                    )
                }
            },
            {
                "sporing mist",
                new(JobPairs first, JobPairs second)[]
                {
                    (
                        ("CUL/BTN/FSH", CosmicMoonRegistry.Sinus.TerritoryId, 24.5f, 16.8f),
                        ("CRP/LTW/WVR", CosmicMoonRegistry.Sinus.TerritoryId, 29.0f, 35.4f)
                    ),
                    (
                        ("BSM/ALC", CosmicMoonRegistry.Sinus.TerritoryId, 32.2f, 22.2f),
                        ("CRP/WVR/BTN", CosmicMoonRegistry.Sinus.TerritoryId, 36.0f, 23.4f)
                    )
                }
            },
            {
                "astromagnetic storm",
                new(JobPairs first, JobPairs second)[]
                {
                    (
                        ("ARM/GSM/ALC", CosmicMoonRegistry.Sinus.TerritoryId, 24.9f, 33.1f),
                        ("MIN/FSH", CosmicMoonRegistry.Sinus.TerritoryId, 19.2f, 15.0f)
                    ),
                    (
                        ("CRP/GSM/WVR", CosmicMoonRegistry.Sinus.TerritoryId, 19.8f, 36.8f),
                        ("CUL/MIN/FSH", CosmicMoonRegistry.Sinus.TerritoryId, 12.3f, 20.0f)
                    )
                }
            },
        };

        private static readonly Dictionary<string, (JobPairs first, JobPairs second)[]> EmptyRedAlert = new();

        private static readonly Dictionary<uint, Dictionary<string, (JobPairs first, JobPairs second)[]>> RedAlertByTerritory =
            CosmicMoonRegistry.All.ToDictionary(
                m => m.TerritoryId,
                m => m.TerritoryId == CosmicMoonRegistry.Sinus.TerritoryId ? SinusRedAlert : EmptyRedAlert);

        internal static bool HasRedAlertData(uint territoryId) =>
            RedAlertByTerritory.TryGetValue(territoryId, out var data) && data.Count > 0;

        internal static LocationEntry CheckForRedAlert()
        {
            if (!PlayerHelper.IsInCosmicZone()) return default;
            try
            {
                if (AddonHelper.IsAddonActive(Announcement))
                {
                    if (AddonHelper.GetAtkTextNode(Announcement, 48)->IsVisible()) // Red Alert Preparation
                    {
                        var description = AddonHelper.GetNodeText(Announcement, 47).ToLower();
                        var territoryId = Player.Territory.RowId;

                        if (!RedAlertByTerritory.TryGetValue(territoryId, out var redAlert))
                            return default;

                        return redAlert.FirstOrDefault(location => description.Contains(location.Key));
                    }
                    else
                    {
                        return default;
                    }
                }
                else
                {
                    return default;
                }
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
