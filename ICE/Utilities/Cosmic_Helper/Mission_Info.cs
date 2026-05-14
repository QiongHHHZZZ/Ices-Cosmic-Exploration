using System.Collections.Generic;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using ICE.Utilities.Cosmic_Helper;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Utilities.Cosmic_Helper;

public static partial class CosmicHelper
{
    public static CosmicInfo CurrentMissionInfo => SheetMissionDict[CurrentLunarMission];

    /// <summary>
    /// Gives the current mission that is active
    /// </summary>
    public static unsafe uint CurrentLunarMission
    {
        get
        {
            try
            {
                var manager = WKSManager.Instance();
                if (manager == null)
                    return 0; // or some default value

                return manager->CurrentMissionUnitRowId;
            }
            catch (AccessViolationException)
            {
                IceLogging.Error("We're currently getting access violations with this, so returning 0");
                return 0;
            }
            catch (Exception)
            {
                IceLogging.Error("Welp. Somehow not getting it still. Exception exit");
                return 0;
            }
        }
    }
    public static unsafe uint? CurrentBait => WKSManager.Instance()->FishingBait;
    // public static unsafe uint CurrentLunarDevelopment => ExcelHelper.DevGrade.GetRow(WKSManager.Instance()->DevGrade).Unknown6;
    public static unsafe uint CurrentLunarDevelopment = 0;

    public static int MaxXpKind = 6;

    public static Dictionary<int, string> ExpDictionary = new()
    {
        { 1, "I" },
        { 2, "II" },
        { 3, "III" },
        { 4, "IV" },
        { 5, "V" },
        { 6, "VI" }
    };

    public static readonly Dictionary<uint, uint> PlanetCreditInfo = new()
    {
        [1237] = 45691, // sinus
        [1291] = 48146, // phaenna
        [1310] = 48147, // Oizys
        // [] = 48148, // moon 4
    };

    public class Dronebit
    {
        public uint creditId { get; set; } = 0;
        public uint boxId { get; set; } = 0;
    }

    public static readonly Dictionary<uint, Dronebit> DronebitInfo = new()
    {
        [1310] = new() // Oizys
        {
            creditId = 49170,
            boxId = 50414,
        }
        // [] = ???    // Next Planet (Maybe)
    };

    // General use functions used across the codebase, specifically tied to cosmic related functions
    public static void OpenStellarMission()
    {
        if (GenericHelpers.TryGetAddonMaster<WKSHud>("WKSHud", out var hud) && hud.IsAddonReady)
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMissionInfomation>("WKSMissionInfomation", out var missionInfo) && missionInfo.IsAddonReady)
            {
                return;
            }
            else
            {
                if (EzThrottler.Throttle("Opening Stellar Missions"))
                {
                    IceLogging.Debug("Opening Mission Menu");
                    hud.Mission();
                }
            }
        }
    }

    public class ClassInfo
    {
        public int Score { get; set; } = 0;
        public int Stage_Current { get; set; } = 0;
        public int Stage_Next { get; set; } = 0;
        public Dictionary<int, ExpInfo> CurrentExp { get; set; } = new();
    }

    public class ExpInfo
    {
        public string Name { get; set; } = "";
        public int Current { get; set; } = 0;
        public int Needed { get; set; } = 0;
        public int Max { get; set; } = 0;
    }

    public static Dictionary<uint, ClassInfo> Cosmic_ClassInfo = new()
    {
        [8] = new(),
        [9] = new(),
        [10] = new(),
        [11] = new(),
        [12] = new(),
        [13] = new(),
        [14] = new(),
        [15] = new(),
        [16] = new(),
        [17] = new(),
        [18] = new(),
    };

    public static ulong _LastCid = 0;
    public static void CheckForUpdate()
    {
        var currentId = Player.CID;
        if (currentId != 0 && _LastCid != currentId && !P.TaskManager.IsBusy)
        {
            IceLogging.Verbose("Updating cosmic stats to reflect the currect stats", "Updating Mission Stats");
            P.TaskManager.Enqueue(() => SendCosmicUpdate(), "Updating stats for characters");
            _LastCid = currentId;
        }
    }


    public unsafe static void UpdateClassInfo()
    {
        Dictionary<uint, ClassInfo> cosmicClassInfo = new()
        {
            [8] = new(),
            [9] = new(),
            [10] = new(),
            [11] = new(),
            [12] = new(),
            [13] = new(),
            [14] = new(),
            [15] = new(),
            [16] = new(),
            [17] = new(),
            [18] = new(),
        };

        var wksManagerPtr = WKSManager.Instance();
        if (wksManagerPtr == null)
        {
            if (PlayerHelper.IsInCosmicZone())
            {
                if (EzThrottler.Throttle("Throttling log message", 3000))
                    IceLogging.Error("WKSManager returned null");
            }
            return;
        }

        var wks = wksManagerPtr;
        var researchModule = wks->ResearchModule;

        if (researchModule == null || !researchModule->IsLoaded)
        {
            if (EzThrottler.Throttle("Throttling log message", 3000))
                IceLogging.Error("Research Module has returned null");
            return;
        }

        // Use original pointer only for member function calls
        var researchModuleFuncs = researchModule;

        for (int i = 0; i < 11; i++)
        {
            uint jobId = (uint)i + 8;
            byte toolClassId = (byte)(jobId - 7);
            byte arrayIndex = (byte)(toolClassId - 1);

            var score = wks->Scores[arrayIndex];
            var currentStage = researchModule->CurrentStages[arrayIndex];
            var nextStage = currentStage == CosmicHelper.MaxRelicLevel
                ? CosmicHelper.MaxRelicLevel
                : (byte)(currentStage + 1);

            ClassInfo entry = new()
            {
                Score = score,
                Stage_Current = currentStage,
                Stage_Next = nextStage,
            };

            for (byte type = 1; type <= MaxXpKind; type++)
            {
                if (!researchModuleFuncs->IsTypeAvailable(toolClassId, type))
                {
                    IceLogging.Verbose($"Type wasn't available: {type}");
                    break;
                }

                entry.CurrentExp[type] = new()
                {
                    Name = ExpDictionary.TryGetValue(type, out var expName) ? expName : "???",
                    Needed = researchModuleFuncs->GetNeededAnalysis(toolClassId, type),
                    Current = researchModuleFuncs->GetCurrentAnalysis(toolClassId, type),
                    Max = researchModuleFuncs->GetMaxAnalysis(toolClassId, type),
                };
            }

            cosmicClassInfo[jobId] = entry;
        }

        Cosmic_ClassInfo = cosmicClassInfo;
    }
    public unsafe static void UpdateMissionStatus()
    {
        foreach (var mission in CosmicHelper.SheetMissionDict)
            mission.Value.MissionStatus = CosmicHandler.MissionStatus(mission.Key);
    }

    public static unsafe bool SendCosmicUpdate()
    {
        string tag = "Task: Update Cosmic Info";

        if (PlayerHelper.IsInCosmicZone())
        {
            if (PlayerHelper.IsScreenReady())
            {
                var wksManagerPtr = WKSManager.Instance();
                var researchModule = wksManagerPtr->ResearchModule;
                if (wksManagerPtr == null)
                {
                    if (EzThrottler.Throttle("Update Stats"))
                        IceLogging.Verbose("Waiting for the wksManager to be loaded", tag);

                    return false;
                }
                else if (researchModule == null || !researchModule->IsLoaded)
                {
                    if (EzThrottler.Throttle("Waiting for research"))
                        IceLogging.Verbose("Waiting for research module to be loaded", tag);

                    return false;
                }
                else
                {
                    UpdateClassInfo();
                    UpdateMissionStatus();
                    IceLogging.Verbose("Updated cosmic dictionary to have proper values", tag);
                    return true;
                }
            }
            else
            {
                IceLogging.Verbose("Waiting for screen to be ready...", tag);
                return false;
            }
        }
        else
        {
            IceLogging.Verbose("We're not in a cosmic area, so we're going to just exit this check", tag);
            return true;
        }
    }
}
