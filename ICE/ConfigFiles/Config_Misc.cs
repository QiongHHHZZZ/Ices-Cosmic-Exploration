using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
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
    public bool ClosestNodeSelection { get; set; } = false;
    public bool AvoidStellarReturn { get; set; } = false;
    public bool AvoidStellarReturnExceptHub { get; set; } = false;
    public bool RandomizeWaypoints { get; set; } = false;
    public float RandomizeWaypointsRadius { get; set; } = 1.0f;
    public bool RandomizeWaypointsDebug { get; set; } = false;
    public Dictionary<uint, Vector3> CrafterLocations { get; set; } = new();
    public List<MissionCommand> PostMissionCommands { get; set; } = new();

    public bool UseHubReturn { get; set; } = true;
    public bool UseAethernet { get; set; } = true;
    public float HubReturn_Distance { get; set; } = 75f;
    public bool UseRedAlertNpc { get; set; } = false;
    public bool HideRewardWindow { get; set; } = true;
    public bool FakeIncreaseFisher { get; set; } = false;

    // CN-MAINT: Daily Routines TP integration switches.
    public bool FishingUseDailyRoutinesTP { get; set; } = false;
    public bool GatherUseDailyRoutinesTP { get; set; } = false;
    public bool PersonalReturnUseDailyRoutinesTP { get; set; } = false;
    public bool HubReturnUseDailyRoutinesTP { get; set; } = false;

    public class MissionCommand
    {
        public required string command { get; set; }
        public int Delay { get; set; } = 0;
    }

    // These are dumb options that do absolutely nothing for gameplay
    // And some of these are just for memes
    public bool CrazyTaxiArrow { get; set; } = false;
    public bool PlaceboCheckbox { get; set; } = false;
}
