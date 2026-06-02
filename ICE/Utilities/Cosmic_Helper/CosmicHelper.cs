using Dalamud.Interface.Textures;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Enums;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public static readonly int MinimumLevel = 10;
    public static readonly int MaximumLevel = Player.MaxLevel;
    public static readonly float ImageSize = 24;

    public static readonly int MaxRelicLevel = 17;
    public static readonly float MaxRelicExpStatus = 17.6f;



    public static readonly List<uint> CrafterJobList = [8, 9, 10, 11, 12, 13, 14, 15];
    public static readonly List<uint> GatheringJobList = [16, 17, 18];
    public static readonly List<uint> SupportedJobs = [8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18];

    public static Dictionary<CosmicWeather, ISharedImmediateTexture> WeatherIconDict = new();

    public class JobClass
    {
        public JobFilter JobFlag { get; set; } = JobFilter.None;
        public string JobName { get; set; } = "???";
        public string shortName { get; set; } = "???";
        public ISharedImmediateTexture JobIcon { get; set; }
    }
    public static Dictionary<uint, JobClass> ClassInfoDict = new()
    {
        [8] = new() { JobFlag = JobFilter.CRP },
        [9] = new() { JobFlag = JobFilter.BSM },
        [10] = new() { JobFlag = JobFilter.ARM },
        [11] = new() { JobFlag = JobFilter.GSM },
        [12] = new() { JobFlag = JobFilter.LTW },
        [13] = new() { JobFlag = JobFilter.WVR },
        [14] = new() { JobFlag = JobFilter.ALC },
        [15] = new() { JobFlag = JobFilter.CUL },
        [16] = new() { JobFlag = JobFilter.MIN },
        [17] = new() { JobFlag = JobFilter.BTN },
        [18] = new() { JobFlag = JobFilter.FSH },
    };

    public static string GetJobName(uint jobId)
    {
        return jobId switch
        {
            8 => T("Carpenter"),
            9 => T("Blacksmith"),
            10 => T("Armorer"),
            11 => T("Goldsmith"),
            12 => T("Leatherworker"),
            13 => T("Weaver"),
            14 => T("Alchemist"),
            15 => T("Culinarian"),
            16 => T("Miner"),
            17 => T("Botanist"),
            18 => T("Fisher"),
            _ => T("Unknown")
        };
    }


    /// <summary>
    /// Currently contains all the WKSMissionLotterySpecialCond values that are weather based
    /// MAKE SURE. TO UPDATE THIS. COME NEW MOON
    /// </summary>
    public static readonly HashSet<uint> WeatherSelection = new() { 13, 14, 15, 16, 23, 24 };

    public static Dictionary<CosmicWeather, int> WeatherIds = new()
    {
        [CosmicWeather.UmbralWind] = 60219,
        [CosmicWeather.MoonDust] = 60222,
        [CosmicWeather.Clouds] = 60203,
        [CosmicWeather.Rain] = 60207,
        [CosmicWeather.ClearSkies] = 60201,
        [CosmicWeather.FairSkies] = 60202,
    };

    // WKSMissionLotterySpecialCond RowId -> WeatherRequired.Value.Name (client data)
    private static readonly Dictionary<CosmicWeather, uint> WeatherSpecialCondIds = new()
    {
        [CosmicWeather.UmbralWind] = 13,
        [CosmicWeather.MoonDust] = 14,
        [CosmicWeather.Clouds] = 15,
        [CosmicWeather.Rain] = 16,
        [CosmicWeather.ClearSkies] = 23,
        [CosmicWeather.FairSkies] = 24,
    };

    public static string GetCosmicWeatherName(CosmicWeather weather)
    {
        if (weather == CosmicWeather.None)
            return T("None");

        if (WeatherSpecialCondIds.TryGetValue(weather, out var condId))
        {
            var cond = Svc.Data.GetExcelSheet<WKSMissionLotterySpecialCond>().GetRow(condId);
            var weatherName = cond.WeatherRequired.Value.Name.ToString();
            if (!string.IsNullOrWhiteSpace(weatherName))
                return weatherName;
        }

        return weather.ToString();
    }

    public static Dictionary<uint, uint> MissionScoreDict = new(); // MissionID -> Score

    // Load the CSV file
    public static void LoadMissionScores()
    {
        MissionScoreDict.Clear();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "ICE.Resources.MissionScores.csv";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            PluginLog.Error($"Failed to find embedded CSV: {resourceName}");
            return;
        }

        using var reader = new StreamReader(stream);
        bool headerSkipped = false;
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!headerSkipped)
            {
                headerSkipped = true;
                continue; // Skip header
            }

            var parts = line.Split(',');
            if (parts.Length >= 4 &&
                uint.TryParse(parts[0].Trim(), out uint missionId) &&
                uint.TryParse(parts[3].Trim(), out uint score))
            {
                MissionScoreDict[missionId] = score;
            }
        }
    }

    public class XPType
    {
        public int CurrentXP { get; set; }
        public int NeededXP { get; set; }
    }

    public static Dictionary<uint, List<uint>> MissionUnlock = new()
    {
        [499] = new() { 82, 397 },
        [500] = new() { 217, 397 },
        [501] = new() { 262, 397 },
        [505] = new() { 37, 442 },
        [506] = new() { 127, 442 },
        [507] = new() { 307, 442 },
        [510] = new() { 172, 487 },
        [511] = new() { 352, 487 }
    };
    public static Dictionary<uint, Vector3> HubCenter = new()
    {
        [1237] = new(2.84f, 1.55f, -0.06f),
        [1291] = new(339.90f, 52.60f, -412.10f),
        [1310] = new(-180.02f, 0.50f, 129.25f)
    };
}
