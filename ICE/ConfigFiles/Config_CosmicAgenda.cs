using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    public class AgendaInfo
    {
        public PlaylistOptions SelectedOption { get; set; } = PlaylistOptions.None;
        public ModeSelect SelectedMode { get; set; } = ModeSelect.Standard;
        public uint SelectedJob { get; set; } = 8;
        public int SelectedRelicLevel { get; set; } = 17;
        public int CreditAmount { get; set; } = 30_000;
        public int PlanetAmount { get; set; } = 10_000;
        public int DronebitAmount { get; set; } = 5_000;
        public int ClassLevel { get; set; } = 100;
        public int ClassScore { get; set; } = 500_000;
    }

    public List<AgendaInfo> Cosmic_Agenda { get; set; } = new();

    public class AgendaProfileInfo
    {
        public string Name { get; set; } = "New Profile";
        public string Description { get; set; } = "";
        public List<AgendaInfo> MissionList { get; set; } = new();
    }

    public List<AgendaProfileInfo> Agenda_Profiles { get; set; } = new();
}
