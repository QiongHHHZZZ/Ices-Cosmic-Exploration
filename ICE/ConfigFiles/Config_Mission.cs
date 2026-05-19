using ICE.OldYamlConfig;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace ICE.ConfigFiles;

public partial class Config
{
    public ModeSelect SelectedMode { get; set; } = ModeSelect.Standard;
    public bool OnlyGrabMission_Debug { get; set; } = false;
    public int TargetLevel { get; set; } = 100;
    public bool StopWhenLevel { get; set; } = false;
    public bool StopOnceHitCosmoCredits { get; set; } = false;
    public int CosmoCreditsCap { get; set; } = 30_000;
    public bool StopOnceHitLunarCredits { get; set; } = false;
    public int LunarCreditsCap { get; set; } = 10_000;
    public bool StopOnceHitCosmicScore { get; set; } = false;
    public int CosmicScoreCap { get; set; } = 500_000;
    public bool StopOnceRelicFinished { get; set; } = false;
    public List<ProvisionalTypes> MissionPrio { get; set; } = new()
    {
        ProvisionalTypes.ProvisionalWeather,
        ProvisionalTypes.ProvisionalSequential,
        ProvisionalTypes.ProvisionalTimed
    };
    public List<MissionTypes> MissionTypePrio { get; set; } = new()
    {
        MissionTypes.DroneSearch,
        MissionTypes.Critical,
        MissionTypes.Provisional,
        MissionTypes.Standard,
    };
    public List<uint> JobPrio { get; set; } = new()
    {
        8, 9, 10, 11, 12, 13, 14, 15,  // Crafters: CRP, BSM, ARM, GSM, LTW, WVR, ALC, CUL
        16, 17, 18                     // Gatherers: MIN, BTN, FSH
    };
    public bool AutoSelectMoon { get; set; } = true;
    public bool ShowSinusMissions { get; set; } = true;
    public bool ShowPhaennaMissions { get; set; } = true;
    public bool ShowOizysMissions { get; set; } = true;
    public bool RemoveAfterGold { get; set; } = false;
    public bool KeepARanks { get; set; } = false;
    public bool ShowExtraMissionInfo { get; set; } = true;
    public Dictionary<uint, uint> ScoreKeeper { get; set; } = new();
    public Dictionary<uint, MissionSettings> MissionConfig { get; set; } = new();
    public Dictionary<string, List<uint>> Mission_Playlist { get; set; } = new();

    public bool GrindAllProvisionals { get; set; } = true;
    public bool GrindOffClassRedAlert { get; set; } = false;
    public bool Relic_IncludeCriticals { get; set; } = true;
    public bool DisableHub_Critical { get; set; } = false;

    public class MissionSettings
    {
        public bool Enabled { get; set; } = false;
        public bool ManualMode { get; set; } = false;
        public int GProfileId { get; set; } = 0;
        public bool AutoTurnin { get; set; } = true;
        public bool TurninGold { get; set; } = false;
        public bool TurninSilver { get; set; } = false;
        public bool TurninBronze { get; set; } = false;
        public bool Use_BuildinPreset { get; set; } = false;
        public string AutoHookPresetName { get; set; } = string.Empty;
        public double BestTime { get; set; } = double.MaxValue;
        public double AverageTime { get; set; } = 0;
        public double AverageBronzeTime { get; set; } = 0;
        public double AverageSilverTime { get; set; } = 0;
        public double AverageGoldTime { get; set; } = 0;
        public double AverageCriticalTime { get; set; } = 0;
        public int TotalCompletions { get; set; } = 0;
        public int BronzeCompletion { get; set; } = 0;
        public int SilverCompletions { get; set; } = 0;
        public int GoldCompletions { get; set; } = 0;
        public int CriticalCompletions { get; set; } = 0;
        public int FailedCounters { get; set; } = 0;
        public int TotalAttempts { get; set; } = 0;
        public List<TurninData> TurninRecords { get; set; } = new();
        public Dictionary<uint, ArtisanSettings> CraftSettings { get; set; } = new();

        public class TurninData
        {
            public double Time { get; set; }
            public TurninState State { get; set; }
        }

        public class ArtisanSettings
        {
            public bool UseGlobal { get; set; } = true;
            public uint FoodId { get; set; } = 0;
            public bool FoodHQ { get; set; } = true;
            public uint PotionId { get; set; } = 0;
            public bool PotionHQ { get; set; } = false;
            public uint ManualId { get; set; } = 0;
            public uint SquadronManualId { get; set; } = 0;
            public ArtisanCraftType ArtisanSolverType { get; set; } = ArtisanCraftType.Default;
            public string MacroName { get; set; } = "";
            public int SkillUsageAmount { get; set; } = -1;
            public int MinStepsForMiracle { get; set; } = -1;
            public uint ExpertProfileId = 0;
        };
    }

    public class FishingLocations
    {
        public uint ZoneId { get; set; } = 0;
        public float X { get; set; } = 0.0f;
        public float Y { get; set; } = 0.0f;
        public Vector3? WorldPosition { get; set; } = null;

        [JsonIgnore]
        public Vector2 MapCoords => new(X, Y);
    }

    public List<FishingLocations> Personal_FishLocation = new();
}
