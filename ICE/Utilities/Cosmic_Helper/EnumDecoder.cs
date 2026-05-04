using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public static string PlaylistOptionString(PlaylistOptions option)
    {
        return option switch
        {
            PlaylistOptions.None => T("None"),
            PlaylistOptions.SinusMax => T("Max Sinus Relic [Lv. 9]"),
            PlaylistOptions.PhaennaMax => T("Max Phaenna Relic [Lv. 14]"),
            PlaylistOptions.OizysMax => T("Max Oizys Relic [Lv. 17]"),
            PlaylistOptions.SelectedRelicLv => T("Selected Relic Level"),
            PlaylistOptions.CreditAmount => T("Credit Amount"),
            PlaylistOptions.PlanetAmount => T("Planetary Credit Amount"),
            PlaylistOptions.DronebitAmount => T("Planetary Dronebit Amount"),
            PlaylistOptions.ClassLevel => T("Class Level"),
            PlaylistOptions.ClassScore => T("Class Score"),
            // PlaylistOptions.GoldClassMissions => "All Missions Golded",
            PlaylistOptions.ToolMaxExp => T("Max Tool Exp"),
            _ => T("???")
        };
    }
}
