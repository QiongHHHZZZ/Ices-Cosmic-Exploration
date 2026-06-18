using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    // Cordial Settings

    public bool AutoCordial { get; set; } = false;
    public bool inverseCordialPrio { get; set; } = false;
    public int CordialMinGp { get; set; } = 0;
    public bool UseOnFisher { get; set; } = false;
    public bool PreventOvercap { get; set; } = false;
    public bool UseOnlyInMission { get; set; } = false;
    public int CordialMinRank { get; set; } = 0; // 0=all, 1=D+, 2=C+, 3=B+, 4=A+, 5=Ex+, 6=Ex+ only
    public int SelectedGatherIndex { get; set; } = 0;
    public bool UseGatheringFood { get; set; } = false;
    public uint GatheringFood { get; set; } = 0;
    public int FoodMinRank { get; set; } = 0; // 0=all, 1=D+, 2=C+, 3=B+, 4=A+, 5=Ex+, 6=Ex+ only
    public int GatherFanSectionSize { get; set; } = 360;
    public bool Gather_NoNav { get; set; } = false;

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

    public class GatherProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MinimumGp { get; set; } = -1;
        public GatherBuffs GatherBuffs { get; set; } = new();
    }

    public class GatherBuff
    {
        public bool Enabled { get; set; } = false;
        public int MinGp { get; set; }
        public int MaxUse { get; set; } = -1;
        public int MinUsableDurability { get; set; } = 0;
    }

    public class GatherBuffs
    {
        public Dictionary<string, GatherBuff> Buffs { get; set; } = new()
        {
            ["BoonIncrease2"] = new() { MinGp = 100 },
            ["BoonIncrease1"] = new() { MinGp = 50 },
            ["Tidings"] = new() { MinGp = 200 },
            ["YieldII"] = new() { MinGp = 500 },
            ["YieldI"] = new() { MinGp = 400 },
            ["BountifulYieldII"] = new() { MinGp = 100 },
            ["BonusIntegrity"] = new() { MinGp = 300 },
            ["BonusIntegrityChance"] = new() { Enabled = true, MinGp = 0 },
            ["FieldMasteryIII"] = new() { MinGp = 250 },
            ["FieldMasteryII"] = new() { MinGp = 100 },
            ["FieldMasteryI"] = new() { MinGp = 50 },
            ["FieldMasteryTemp"] = new() { MinGp = 50 },
        };

        public int BountifulMinItem = 4;
    }
}
