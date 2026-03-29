using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.ConfigFiles;

public partial class Config
{
    public bool SelfRepairGather { get; set; } = true;
    public bool SelfRepairCrafter { get; set; } = false;
    public bool RepairAtVendor { get; set; } = false;
    public int RepairPercent { get; set; } = 50;
    public bool SelfSpiritbondGather { get; set; } = true;
    public bool RepairAllGear { get; set; } = true;
    public bool Stop_DarkMatter { get; set; } = true;
    public int Minimum_DarkMatter { get; set; } = 12;
}
