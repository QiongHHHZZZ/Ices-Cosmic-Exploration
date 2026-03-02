using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.ConfigFiles;

public partial class Config
{
    public bool Artisan_RaphaelForce { get; set; } = true;
    public bool Artisan_RaphaelMaster { get; set; } = false;
    public bool MigratedOldArtisan { get; set; } = false;

    public Global_Artisan Artisan_GlobalStandard { get; set; } = new();
    public Global_Artisan Artisan_GlobalExpert { get; set; } = new()
    {
        SolverType = ArtisanCraftType.Expert
    };

    public class Global_Artisan
    {
        public ArtisanCraftType SolverType { get; set; } = ArtisanCraftType.Standard;
        public uint FoodId { get; set; } = 0;
        public bool FoodHQ { get; set; } = true;
        public uint PotionId { get; set; } = 0;
        public bool PotionHQ { get; set; } = true;
        public uint ManualId { get; set; } = 0;
        public uint SquadronManual { get; set; } = 0;
    }
}
