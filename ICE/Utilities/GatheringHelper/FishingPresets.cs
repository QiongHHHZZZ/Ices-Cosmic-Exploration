using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Utilities.GatheringHelper;

public static partial class GatheringUtil
{
    public static Dictionary<uint, List<string>> FishingPreset = new();

    public static void RegisterPresets()
    {
        RegisterSinus();
        RegisterPhaenna();
        RegisterOizys();
    }
}
