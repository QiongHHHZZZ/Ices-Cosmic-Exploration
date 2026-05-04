using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static ICE.ConfigFiles.Config;

namespace ICE.OldYamlConfig
{
    public class MissionConfigs : IYamlConfig
    {
        // Last edited version: 1
        public int ConfigVersion { get; set; } = 9;

        #region Safety Settings
        public bool StopOnAbort { get; set; } = true;
        public bool RejectUnknownYesno { get; set; } = true;
        public bool DelayGrabMission { get; set; } = true;
        public int DelayIncrease { get; set; } = 500;
        public bool DelayCraft { get; set; } = true;
        public int DelayCraftIncrease { get; set; } = 2500;
        public bool AnimationLockAbandon { get; set; } = true;
        public bool JumpIfStuck { get; set; } = false;

        #endregion

        #region Main Window

        public bool ShowInfoButton { get; set; } = true;
        public float MiddleColumnWidth { get; set; } = 1000f;
        public uint SelectedJob { get; set; } = 8;
        public bool XPRelicGrind { get; set; } = false;
        public bool XPLeveling_Mode { get; set; } = false;
        public bool XPRelicIgnoreManual { get; set; } = false;
        public bool XPRelicOnlyEnabled { get; set; } = false;
        public bool ShowCritical { get; set; } = true;
        public bool ShowSequential { get; set; } = true;
        public bool ShowWeather { get; set; } = true;
        public bool ShowTimeRestricted { get; set; } = true;
        public bool ShowClassA { get; set; } = true;
        public bool ShowClassB { get; set; } = true;
        public bool ShowClassC { get; set; } = true;
        public bool ShowClassD { get; set; } = true;

        #endregion

        #region Overlay Settings

        public bool ShowOverlay { get; set; } = false;
        public bool ShowSeconds { get; set; } = false;
        public bool ShowTotalScore { get; set; } = true;
        public bool ShowExpBars { get; set; } = true;

        #endregion

        #region MissionSettings

        public bool OnlyGrabMission { get; set; } = false;
        public int TargetLevel { get; set; } = 10;
        public bool StopWhenLevel { get; set; } = false;
        public bool StopOnceHitCosmoCredits { get; set; } = false;
        public int CosmoCreditsCap { get; set; } = 30000;
        public bool StopOnceHitLunarCredits { get; set; } = false;
        public int LunarCreditsCap { get; set; } = 10000;
        public bool StopOnceHitCosmicScore { get; set; } = false;
        public int CosmicScoreCap { get; set; } = 500000;
        public bool StopOnceRelicFinished { get; set; } = false;
        public byte SequenceMissionPriority { get; set; } = 1;
        public byte WeatherMissionPriority { get; set; } = 2;
        public byte TimedMissionPriority { get; set; } = 3;
        public List<ProvisionalTypes> MissionPrio { get; set; } = new()
        {
            ProvisionalTypes.ProvisionalWeather,
            ProvisionalTypes.ProvisionalSequential,
            ProvisionalTypes.ProvisionalTimed
        };
        public bool GrindProvisionals { get; set; } = false;
        public bool GrindAllProvisionals { get; set; } = true;
        public List<uint> JobPrio { get; set; } = new()
        {
            8, 9, 10, 11, 12, 13, 14, 15,  // Crafters: CRP, BSM, ARM, GSM, LTW, WVR, ALC, CUL
            16, 17, 18                     // Gatherers: MIN, BTN, FSH
        };
        public bool AutoSelectMoon { get; set; } = true;
        public bool ShowSinusMissions { get; set; } = true;
        public bool ShowPhaennaMissions { get; set; } = true;
        public bool RemoveAfterGold { get; set; } = false;
        public bool ShowExtraMissionInfo { get; set; } = true;
        public Dictionary<uint, uint> ScoreKeeper { get; set; } = new();

        #endregion

        #region Table Settings

        public int TableSortOption { get; set; } = 0;
        public bool HideUnsupportedMissions { get; set; } = false;
        public bool AutoPickCurrentJob { get; set; } = false;
        public bool ShowCompletionWindow { get; set; } = false;
        public bool ShowCompletionOnlyJob { get; set; } = false;
        public bool ShowSelectedJobOnly { get; set; } = false;
        public bool ShowCompletion_MissingGold { get; set; } = false;
        public bool ShowManualMode { get; set; } = false;
        public bool Auto_ShowTokens { get; set; } = true;

        #endregion

        #region Repair Settings

        public bool SelfRepairGather { get; set; } = true;
        public bool SelfRepairCrafter { get; set; } = false;
        public bool RepairAtVendor { get; set; } = false;
        public int RepairPercent { get; set; } = 50;
        public bool SelfSpiritbondGather { get; set; } = true;

        #endregion

        #region Gathering Settings
        public int SelectedGatherIndex { get; set; } = 0;
        public bool UseGatheringFood { get; set; } = false;
        public uint GatheringFood { get; set; } = 0;

        #region Cordial Settings

        public bool AutoCordial { get; set; } = false;
        public bool inverseCordialPrio { get; set; } = false;
        public int CordialMinGp { get; set; } = 0;
        public bool UseOnFisher { get; set; } = false;
        public bool PreventOvercap { get; set; } = false;
        public bool UseOnlyInMission { get; set; } = false;

        #endregion

        public List<GatherProfile> GatherSettings { get; set; } = new()
        {
            new GatherProfile { Id = 0, Name = "Defualt"},
        };

        public Dictionary<int, GatherProfile> GatherProfiles { get; set; } = new()
        {
            [0] = new GatherProfile() 
            { 
                Name = "Default",
            },
        };

        #endregion

        #region Gamba Settings

        // Gamba settings
        public List<Gamba> GambaItemWeights { get; set; } = new();
        public bool GambaEnabled { get; set; } = false;
        public bool GambaPreferSmallerWheel { get; set; } = false;
        public int GambaCreditsMinimum { get; set; } = 0;
        public int GambaDelay { get; set; } = 250;
        public bool GambaBetweenRuns = false;
        public int GambaAtAmount { get; set; } = 1000;

        #endregion

        #region Misc

        public bool MoonSprint { get; set; } = true;
        public uint MountId { get; set; } = 0;
        public string MountName { get; set; } = "Mount Roulette";
        public float MountRadius { get; set; } = 15.0f;
        public float DismountRadius { get; set; } = 7.0f;
        public bool UseMountOutsideMission { get; set; } = true;
        public bool UseMountInMission { get; set; } = true;
        public float LeftColumnWidth { get; set; } = 300f;
        public bool PlaySoundAlert { get; set; } = false;
        public float SoundVolume { get; set; } = 0.5f;
        public int TimeHistoryLimit { get; set; } = 100;
        public bool RemoveStellarStatus { get; set; } = false;
        public bool ShowSPM { get; set; } = false;
        public bool StartUponEnterMoon { get; set; } = false;
        public bool PersonalReturnSpot { get; set; } = false;
        public Dictionary<uint, Vector3> CrafterLocations { get; set; } = new();

        #endregion

        #region Relic Settings
        
        public bool TurninRelic { get; set; } = false;
        public Dictionary<uint, bool> RelicJobs { get; set; } = new()
        {
            [8] = true,
            [9] = true,
            [10] = true,
            [11] = true,
            [12] = true,
            [13] = true,
            [14] = true,
            [15] = true,
            [16] = true,
            [17] = true,
            [18] = true
        };
        public bool FarmAllRelics { get; set; } = false;
        public bool Stop_AllRelicsComplete { get; set; } = false;

        #endregion

        #region Shopping List

        public Dictionary<uint, CosmoShoppingList> CosmoShopping { get; set; } = new();
        public List<uint> CosmoShoppingOrder { get; set; } = new();
        public bool BuyItems { get; set; } = false;
        public int CosmoBuyAtAmount { get; set; } = 10000;

        #endregion

        public Dictionary<uint, MissionSettings> MissionConfig { get; set; } = new();

        public List<MissionCommand> PostMissionCommands { get; set; } = new();

        #region Tab Hider

        public bool Show_StopWhen { get; set; } = true;
        public bool Show_GatheringProfile { get; set; } = true;
        public bool Show_MissionPriority { get; set; } = true;
        public bool Show_MiscSettings { get; set; } = true;
        public bool Show_HubActivities { get; set; } = true;

        #endregion

        #region Debug

        public bool FailsafeRecipeSelect { get; set; } = false;
        public bool UseDummyXp { get; set; } = false;
        public Dictionary<int, CosmicHelper.XPType> DummyXP { get; set; } = new()
        {
            { 1, new CosmicHelper.XPType { CurrentXP = 0, NeededXP = 100} },
            { 2, new CosmicHelper.XPType { CurrentXP = 50, NeededXP = 200} },
            { 3, new CosmicHelper.XPType { CurrentXP = 100, NeededXP = 300} },
            { 4, new CosmicHelper.XPType { CurrentXP = 150, NeededXP = 400} },
            { 5, new CosmicHelper.XPType { CurrentXP = 200, NeededXP = 500} },
        };
        public uint PictoColor_Circle { get; set; } = 2616716297;
        public uint PictoColor_Dot { get; set; } = 2616716297;
        public uint PictoColor_Cone { get; set; } = 0;
        public bool UseDummyRanks { get; set; } = false;
        public bool ShowDummyA { get; set; } = false;
        public bool ShowDummyB { get; set; } = false;
        public bool ShowDummyC { get; set; } = false;
        public bool ShowDummyD { get; set; } = false;

        public bool DisablePathfindingToRedAlert { get; set; } = false;
        public bool ShowDebugGatherInfo { get; set; } = false;
        public string AuthorName { get; set; } = "Puni.sh Community";
        public string CustomRoutePath { get; set; } = string.Empty;
        public bool DisableHudClipping { get; set; } = false;

        public bool HighlightVisibleMissions { get; set; } = false;


        #endregion

        #region Yaml Save Stuff

        public static string ConfigPath => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "Mission Config.yaml");
        private static CancellationTokenSource? _saveCts;
        private static readonly object _saveLock = new();

        // Standard async save (fire-and-forget)
        public void Save()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await SaveAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Failed to save MissionConfigs: {ex}");
                }
            });
        }

        // Debounced save for rapid operations
        public void SaveDebounced(int delayMs = 500)
        {
            lock (_saveLock)
            {
                _saveCts?.Cancel();
                _saveCts = new CancellationTokenSource();
                var cts = _saveCts;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delayMs, cts.Token);
                        await SaveAsync().ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Newer save cancelled this one
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Failed to save MissionConfigs: {ex}");
                    }
                });
            }
        }

        // Core async implementation
        public async Task SaveAsync() => await YamlConfig.SaveAsync(this, ConfigPath);

        // Synchronous for migrations/critical paths
        public void SaveSync() => YamlConfig.SaveSync(this, ConfigPath);

        #endregion
    }
}
