using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Enums
{
    public enum CosmicWeather
    {
        None,        // Basic
        UmbralWind,  // WKSMissionUnit.Unknown7 13
        MoonDust,    // WKSMissionUnit.Unknown7 14
        Clouds,      // WKSMissionUnit.WKSMissionLotterySpecialCond.15
        Rain,        // WKSMissionUnit.WKSMissionLotterySpecialCond.16
        ClearSkies,  // WKSMissionUnit.WKSMissionLotterySpecialCond.23
        FairSkies,   // WKSMissionUnit.WKSMissionLotterySpecialCond.24
    }

    public enum ProvisionalTypes
    {
        ProvisionalTimed = 16384,            // Timed Mission
        ProvisionalWeather = 32768,          // Weather Mission
        ProvisionalSequential = 65536,       // Sequential Mission
    }

    public enum MissionTypes
    {
        DroneSearch,
        Critical,
        Provisional,
        Standard,
    }

    public enum TurninState
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Critical = 4,
        SequenceGold = 5,
    }

    public enum ArtisanCraftType
    {
        Default,
        Standard,
        ProgressOnly,
        Raphael,
        Expert,
        Macro
    }
}
