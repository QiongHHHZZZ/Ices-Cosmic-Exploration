using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.ConfigFiles;

public partial class Config
{
    public class CharacterOverride
    {
        public bool? SelfRepairGather { get; set; } = null;
        public bool? SelfRepairCrafter { get; set; } = null;
        public bool? RepairAtVendor { get; set; } = null;
        public int? RepairPercent { get; set; } = null;
        public bool? Spiritbond_Remove { get; set; } = null;
        public bool? RepairAllGear { get; set; } = null;
        public int? Minimum_DarkMatter { get; set; } = null;

        public Global_Artisan? Artisan_GlobalStandard { get; set; } = null;
        public Global_Artisan? Artisan_GlobalExpert { get; set; } = null;

        public uint? MountId { get; set; } = null;
        public string? MountName { get; set; } = null;

        public bool? Relic_SwapJob { get; set; } = null;
        public uint? Relic_BattleJob { get; set; } = null;
        public bool? Relic_Stylist { get; set; } = null;
    }

    public Dictionary<ulong, CharacterOverride> CharacterOverrides { get; set; } = new();
    public Dictionary<ulong, string> KnownCharacters { get; set; } = new();

    public T Resolve<T>(ulong cid, Func<CharacterOverride, T?> overrideProp, Func<Config, T> globalProp) where T : struct
        => CharacterOverrides.TryGetValue(cid, out var ov) && overrideProp(ov) is { } val
            ? val
            : globalProp(this);

    // Overload for reference types (string, Global_Artisan, etc.)
    public T ResolveRef<T>(ulong cid, Func<CharacterOverride, T?> overrideProp, Func<Config, T> globalProp) where T : class
        => CharacterOverrides.TryGetValue(cid, out var ov) && overrideProp(ov) is { } val
            ? val
            : globalProp(this);
}
