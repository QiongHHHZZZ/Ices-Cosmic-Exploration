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

    public static readonly List<uint> Ranks = [1, 2, 3, 4];
    public static readonly List<uint> ARankIds = [4, 5, 6];

    public static readonly List<uint> CrafterJobList = [8, 9, 10, 11, 12, 13, 14, 15];
    public static readonly List<uint> GatheringJobList = [16, 17, 18];
    public static readonly List<uint> SupportedJobs = [8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18];

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

    public static List<int> GreyIconList = new List<int>() { 91031, 91032, 91033, 91034, 91035, 91036, 91037, 91038, 91039, 91040, 91041 };
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

    public static readonly int MinimumLevel = 10;
    public static readonly int MaximumLevel = Player.MaxLevel;

    public static readonly int MaxRelicLevel = 17;
    public static readonly float MaxRelicExpStatus = 17.6f;

    #region Dictionaries

    public class CraftingInfo
    {
        public uint ItemId { get; set; } 
        public int RequiredAmount { get; set; } 
        public uint RecipeId { get; set; }
        public RecipeInfo RecipeInfo { get; set; } = new();
        public bool ExpertCraft { get; set; } = false;
        public Dictionary<uint, int> RequiredItems { get; set; } = new();
        public int IconId { get; set; } = 0;
        public string ItemName { get; set; } = "???";
    }

    public enum CompletionStatus
    {
        None,
        Completed,
        Gold,
    }

    public class CosmicInfo
    {
        // - - - Crafter Specific - - - //
        /// <summary>
        /// Key = What's used in the config per recipe. This keeps track of it on a per-recipe basis
        /// </summary>
        public Dictionary<ushort, CraftingInfo> Crafts_Main { get; set; } = new();
        public Dictionary<ushort, CraftingInfo> Crafts_Pre { get; set; } = new();
        public bool IsExpert { get; set; } = false;

        // - - - BTN | MIN Specific - - - //
        public Dictionary<uint, int> Gathering_Min { get; set; } = new();

        // - - - FSH Specific - - - //
        public int Fish_AmountRequired { get; set; } = 0;
        public int Fish_VarietyAmount { get; set; } = 0;
        public List<string> Fish_Presets { get; set; } = new();

        // - - - Map Related - - - // 
        public Vector2 MapPosition { get; set; } = new();
        public int Radius { get; set; } = 0;
        public uint TerritoryId { get; set; }
        public uint MarkerId { get; set; }

        // - - - Exp Modifier Section - - - // 

        public uint ExpModifier_1 { get; set; } = 0;
        public uint ExpModifier_2 { get; set; } = 0;
        public uint ExpModifier_3 { get; set; } = 0;

        // - - - Universal Info - - - //
        public string Name { get; set; }
        public List<uint> Jobs { get; set; } = new();
        public uint ToDoId { get; set; } = 0;
        public uint Rank { get; set; } = 1;
        public MissionClass Rankv2 { get; set; } = MissionClass.D;
        public uint Level { get; set; } = 0;
        public MissionAttributes Attributes { get; set; }
        public CosmicWeather Weather { get; set; }
        public uint StartTime { get; set; }
        public uint EndTime { get; set; }
        public uint ClassScore { get; set; } = 0;
        public uint CosmoCredit { get; set; } = 0;
        public uint LunarCredit { get; set; } = 0;
        public uint RewardItem { get; set; } = 0;
        public uint RewardItemAmount { get; set; } = 0;
        public uint DronebitReward { get; set; } = 0;
        public uint PreviousMissionId { get; set; } = new();
        public Dictionary<int, int> RelicXpInfo { get; set; } = new();
        public uint BronzeScore { get; set; } = 0;
        public uint SilverScore { get; set; } = 0;
        public uint GoldScore { get; set; } = 0;
        public uint TemporaryActionId { get; set; } = 0;
        public uint TemporaryActionCount { get; set; } = 0;
        public CompletionStatus MissionStatus { get; set; } = CompletionStatus.None;
        public List<uint> SequenceMissions_Previous { get; set; } = new();
        public List<uint> SequenceMissions_Next { get; set; } = new();

        public bool IsProvisional => Attributes.HasFlag(MissionAttributes.ProvisionalWeather) 
            || Attributes.HasFlag(MissionAttributes.ProvisionalSequential) 
            || Attributes.HasFlag(MissionAttributes.ProvisionalTimed);

        public bool IsCritical => Attributes.HasFlag(MissionAttributes.Critical);
        public bool IsWeather => Attributes.HasFlag(MissionAttributes.ProvisionalWeather);
        public bool IsTimed => Attributes.HasFlag(MissionAttributes.ProvisionalTimed);
        public bool IsSequence => Attributes.HasFlag(MissionAttributes.ProvisionalSequential);
        public bool ARank => Rank is 5 or 4;
        public bool BRank => Rank is 3;
        public bool CRank => Rank is 2;
        public bool Drank => Rank is 1;

    }

    public static Dictionary<uint, CosmicInfo> SheetMissionDict = new();

    public class GatheringInfo
    {
        public Dictionary<uint, int> MinGatherItems = [];
    }

    public static Dictionary<uint, GatheringInfo> GatheringItemDict = new();

    public static Dictionary<uint, ISharedImmediateTexture> GreyTexture = new Dictionary<uint, ISharedImmediateTexture>();

    public static Dictionary<uint, ISharedImmediateTexture> JobIconDict = new Dictionary<uint, ISharedImmediateTexture>();
    public static Dictionary<CosmicWeather, ISharedImmediateTexture> WeatherIconDict = new();

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

    public class GatherItemInfo
    {
        public HashSet<uint> itemIds { get; set; } = new();
        public uint Type { get; set; } = 0;
    }
    public static Dictionary<string, GatherItemInfo> GatheringItems = new();

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

    #endregion
}
